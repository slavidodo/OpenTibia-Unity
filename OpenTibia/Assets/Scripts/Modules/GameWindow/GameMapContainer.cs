using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.InputManagment.GameAction;
using UnityEngine;
using UnityEngine.UI;

using PlayerAction = OpenTibiaUnity.Proto.Appearances.PlayerAction;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    public class GameMapContainer : GamePanelContainer, IMoveWidget, IUseWidget, IWidgetContainerWidget
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RawImage m_WorldMapRawImage;
        [SerializeField] private TMPro.TextMeshProUGUI m_FramecounterText;
#pragma warning restore CS0649 // never assigned to
        
        private Rect m_CachedScreenRect = Rect.zero;
        private bool m_ScreenRectCached = false;
        private bool m_MouseCursorOverRenderer = false;

        private int m_LastScreenWidth = 0;
        private int m_LastScreenHeight = 0;
        private int m_LastFramerate = 0;

        private ObjectDragImpl<GameMapContainer> m_DragHandler;
        
        private RectTransform worldMapRectTransform {
            get {
                return m_WorldMapRawImage.rectTransform;
            }
        }

        protected override void Start() {
            base.Start();

            m_DragHandler = new ObjectDragImpl<GameMapContainer>(this);

            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utility.EventImplPriority.Default, OnMouseUp);
        }

        protected void Update() {
            if (m_LastScreenWidth != Screen.width || m_LastScreenHeight != Screen.height) {
                InvalidateScreenRect();

                m_LastScreenWidth = Screen.width;
                m_LastScreenHeight = Screen.height;
            }
        }

        protected void OnGUI() {
            if (Event.current.type == EventType.Repaint)
                RenderWorldMap();
        }

        protected void RenderWorldMap() {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused)
                return;
#endif

            if (!m_ScreenRectCached)
                CacheScreenRect();

            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            if (worldMapRenderer != null && worldMapStorage.Valid) {
                InternalStartMouseAction(Input.mousePosition, MouseButtons.None, false, true, true);

                if (ContextMenuBase.CurrentContextMenu != null || ObjectDragImpl.AnyDraggingObject) {
                    worldMapRenderer.HighlightTile = null;
                } else if (!m_MouseCursorOverRenderer) {
                    worldMapRenderer.HighlightTile = null;
                    worldMapRenderer.HighlightObject = OpenTibiaUnity.CreatureStorage.Aim;
                    OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.Low);
                }
                
                OpenTibiaUnity.GameManager.WorldMapRenderingTexture.Release();
                RenderTexture.active = OpenTibiaUnity.GameManager.WorldMapRenderingTexture;
                worldMapRenderer.Render(worldMapRectTransform.rect);
                RenderTexture.active = null;

                // setting the clip area
                m_WorldMapRawImage.uvRect = worldMapRenderer.CalculateClipRect();

                if (worldMapRenderer.Framerate != m_LastFramerate) {
                    m_LastFramerate = worldMapRenderer.Framerate;
                    int? ping = -1;
                    m_FramecounterText.text = string.Format("FPS: <color=#{0:X6}>{1}</color>\nPing:{2}", GetFramerateColor(m_LastFramerate), m_LastFramerate, ping.HasValue ? ping.Value : -1);
                }
            } else {
                m_FramecounterText.text = "";
            }
        }

        protected override void OnDestroy() => base.OnDestroy();

        private static uint GetFramerateColor(int framerate) {
            if (framerate < 10)
                return 0xFFFF00;
            else if (framerate < 30)
                return 0xF55E5E;
            else if (framerate < 58)
                return 0xFE6500;

            return 0x00EB00;
        }
        
        public void CacheScreenRect() {
            if (!m_ScreenRectCached) {
                Vector2 size = Vector2.Scale(worldMapRectTransform.rect.size, worldMapRectTransform.lossyScale);
                m_CachedScreenRect = new Rect(worldMapRectTransform.position.x, Screen.height - worldMapRectTransform.position.y, size.x, size.y);
                m_CachedScreenRect.x -= worldMapRectTransform.pivot.x * size.x;
                m_CachedScreenRect.y -= (1.0f - worldMapRectTransform.pivot.y) * size.y;

                m_LastScreenWidth = Screen.width;
                m_LastScreenHeight = Screen.height;
                m_ScreenRectCached = true;
            }
        }

        public void InvalidateScreenRect() => m_ScreenRectCached = false;

        public void OnMouseUp(Event e, MouseButtons mouseButton, bool repeat) {
            if (UseActionImpl.ConcurrentMultiUse != null)
                return;

            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButtons mouseButton, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!gameManager || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;
            
            var point = RawMousePositionToLocalMapPosition(Input.mousePosition);

            m_MouseCursorOverRenderer = !(point.x < 0 || point.y < 0 || point.x > m_CachedScreenRect.size.x || point.y > m_CachedScreenRect.size.y);
            if (!m_MouseCursorOverRenderer) {
                return false;
            }

            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, point, applyAction, updateCursor, updateHighlight);
            return action != AppearanceActions.None;
        }
        
        public AppearanceActions DetermineAction(Vector3 mousePosition, MouseButtons mouseButton, EventModifiers eventModifiers, Vector2 point, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            if (updateCursor)
                updateCursor = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            if (updateHighlight)
                updateHighlight = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler.IsMouseButtonDragged(MouseButtons.Left) || inputHandler.IsMouseButtonDragged(MouseButtons.Right))
                return AppearanceActions.None;

            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var mapPosition = worldMapRenderer.PointToMap(point);

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1100)
                worldMapRenderer.HighlightTile = mapPosition;

            if (!mapPosition.HasValue)
                return AppearanceActions.None;
            
            var player = OpenTibiaUnity.Player;
            var action = AppearanceActions.None;
            Creature creature = null;
            ObjectInstance topLookObject = null;
            ObjectInstance topUseObject = null;
            int topLookObjectStackPos = -1;
            int topUseObjectStackPos = -1;

            var optionStorage = OpenTibiaUnity.OptionStorage;
            var absolutePosition = worldMapStorage.ToAbsolute(mapPosition.Value);
            
            if (optionStorage.MousePreset == MousePresets.LeftSmartClick) {
                bool forceLook = eventModifiers == EventModifiers.Shift;

                if (mouseButton == MouseButtons.Right) {
                    var field = worldMapStorage.GetField(mapPosition.Value);
                    topLookObjectStackPos = field.GetTopLookObject(out topLookObject);
                    topUseObjectStackPos = field.GetTopUseObject(out topUseObject);
                    if (!!topLookObject && topLookObject.IsCreature)
                        creature = creatureStorage.GetCreature(topLookObject.Data);

                    if (!!topUseObject || !!topLookObject)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButtons.Left) {
                    if (eventModifiers == EventModifiers.None) {
                        if (mapPosition.Value.z != worldMapStorage.PlayerZPlane) {
                            var field = worldMapStorage.GetField(mapPosition.Value);
                            var creatureObjectStackPos = field.GetTopCreatureObject(out ObjectInstance creatureObject);

                            action = AppearanceActions.SmartClick;
                            if (!!creatureObject && !!(creature = creatureStorage.GetCreature(creatureObject.Data))) {
                                if (creature.ID == player.ID || forceLook) {
                                    topLookObjectStackPos = creatureObjectStackPos;
                                    topLookObject = creatureObject;
                                    action = AppearanceActions.Look;
                                } else if (creature.IsNPC) {
                                    action = AppearanceActions.Talk;
                                } else {
                                    action = AppearanceActions.Attack;
                                }
                            } else if ((topUseObjectStackPos = field.GetTopUseObject(out topUseObject)) != -1 && !!topUseObject && topUseObject.Type.IsUsable) {
                                action = AppearanceActions.Use;
                            } else if ((topUseObjectStackPos = field.GetTopUseObject(out topUseObject)) != -1 && !!topUseObject && topUseObject.Type.IsUsable) {
                                action = AppearanceActions.Look;
                            } else {

                            }
                        } else {
                            action = AppearanceActions.AutoWalk;
                        }
                    } else if (eventModifiers == EventModifiers.Shift) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject)
                            action = AppearanceActions.Look;
                    } else if (eventModifiers == EventModifiers.Control) {
                        action = AppearanceActions.AutoWalk;
                    } else if (eventModifiers == EventModifiers.Alt) {
                        creature = worldMapRenderer.PointToCreature(point, true);
                        if (!!creature && creature.ID != player.ID && !creature.IsNPC)
                            action = AppearanceActions.Attack;
                    }
                }
            } else if (optionStorage.MousePreset == MousePresets.Regular) {

            } else if (optionStorage.MousePreset == MousePresets.Classic) {
                if (eventModifiers == EventModifiers.Alt) {
                    if (mouseButton == MouseButtons.Left || mouseButton == MouseButtons.None) {
                        creature = worldMapRenderer.PointToCreature(point, true);
                        if (!!creature && creature.ID != player.ID && !creature.IsNPC)
                            action = AppearanceActions.Attack;
                    }
                } else if (eventModifiers == EventModifiers.Control) {
                    if (mouseButton != MouseButtons.Both && mouseButton != MouseButtons.Middle) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);
                        if (!!topLookObject && topLookObject.IsCreature)
                            creature = creatureStorage.GetCreature(topLookObject.Data);

                        if (!!topUseObject || !!topLookObject)
                            action = AppearanceActions.ContextMenu;
                    }
                } else if (mouseButton == MouseButtons.Left || mouseButton == MouseButtons.None) {
                    if (eventModifiers == EventModifiers.Shift) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject)
                            action = AppearanceActions.Look;
                    } else {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject) // TODO: mouse loot preset
                            action = (topLookObject.Type.DefaultAction == PlayerAction.AutowalkHighlight) ? AppearanceActions.AutoWalkHighlight : AppearanceActions.AutoWalk;
                    }
                } else if (mouseButton == MouseButtons.Right) {
                    if (eventModifiers != EventModifiers.Shift) {
                        creature = worldMapRenderer.PointToCreature(point, true);
                        if (!!creature && creature.ID != player.ID && !creature.IsNPC) {
                            action = AppearanceActions.Attack;
                        } else {
                            topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);
                            if (!!topUseObject) // TODO: mouse loot preset
                                action = topUseObject.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                        }
                    }
                } else if (mouseButton == MouseButtons.Both) {
                    if (eventModifiers == EventModifiers.None) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject)
                            action = AppearanceActions.Look;
                    }
                }
            }
            
            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(GetCursorForAction(action), CursorPriority.Medium);

            if (updateHighlight && !OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf) {
                switch (action) {
                    case AppearanceActions.Talk:
                    case AppearanceActions.Attack:
                        worldMapRenderer.HighlightObject = creature;
                        break;

                    case AppearanceActions.Look:
                    case AppearanceActions.AutoWalkHighlight:
                        worldMapRenderer.HighlightObject = topLookObject;
                        break;
                        
                    case AppearanceActions.Use:
                    case AppearanceActions.Open:
                    case AppearanceActions.Loot:
                        worldMapRenderer.HighlightObject = topUseObject;
                        break;
                        
                    default:
                        worldMapRenderer.HighlightObject = null;
                        break;
                }
            } else if (updateHighlight) {
                worldMapRenderer.HighlightObject = null;
            }
            
            if (applyAction) {
                switch (action) {
                    case AppearanceActions.None: break;
                    case AppearanceActions.Attack:
                        if (!!creature && creature.ID != player.ID)
                            OpenTibiaUnity.CreatureStorage.ToggleAttackTarget(creature, true);
                        break;
                    case AppearanceActions.AutoWalk:
                    case AppearanceActions.AutoWalkHighlight:
                        player.StartAutowalk(worldMapStorage.ToAbsolute(mapPosition.Value), false, true);
                        break;
                    case AppearanceActions.ContextMenu:
                        OpenTibiaUnity.CreateObjectContextMenu(absolutePosition, topLookObject, topLookObjectStackPos, topUseObject, topUseObjectStackPos, creature)
                            .Display(mousePosition);
                        break;
                    case AppearanceActions.Look:
                        new LookActionImpl(absolutePosition, topLookObject, topLookObjectStackPos).Perform();
                        break;
                    case AppearanceActions.Use:
                    case AppearanceActions.Open:
                        GameActionFactory.CreateUseAction(absolutePosition, topUseObject, topUseObjectStackPos, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Talk:
                        GameActionFactory.CreateGreetAction(creature).Perform();
                        break;
                    case AppearanceActions.Loot:
                        // TODO: Loot action
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }

        private CursorState GetCursorForAction(AppearanceActions action) {
            switch (action) {
                case AppearanceActions.Attack:
                    return CursorState.Attack;

                case AppearanceActions.AutoWalk:
                case AppearanceActions.AutoWalkHighlight:
                    return CursorState.Walk;

                case AppearanceActions.Look:
                    return CursorState.Look;

                case AppearanceActions.Use:
                    return CursorState.Use;

                case AppearanceActions.Open:
                    return CursorState.Open;

                case AppearanceActions.Talk:
                    return CursorState.Talk;

                case AppearanceActions.Loot:
                    return CursorState.Loot;

                default:
                    return CursorState.Default;
            }
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            var mapPosition = PointToMap(mousePosition);
            if (!mapPosition.HasValue) {
                obj = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopMoveObject(mapPosition.Value, out obj);
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            var mapPosition = PointToMap(mousePosition);
            if (!mapPosition.HasValue) {
                obj = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopUseObject(mapPosition.Value, out obj);
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            var mapPosition = PointToMap(mousePosition);
            if (!mapPosition.HasValue) {
                obj = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopMultiUseObject(mapPosition.Value, out obj);
        }

        public Vector3Int? PointToMap(Vector3 mousePosition) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToMap(RawMousePositionToLocalMapPosition(mousePosition));
        }

        public Vector3Int? PointToAbsolute(Vector3 mousePosition) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToAbsolute(RawMousePositionToLocalMapPosition(mousePosition));
        }

        private Vector2 RawMousePositionToLocalMapPosition(Vector3 mousePosition) {
            CacheScreenRect();

            var mousePoint = new Vector2(Input.mousePosition.x, m_LastScreenHeight - Input.mousePosition.y);
            return mousePoint - m_CachedScreenRect.position;
        }
    }
}
