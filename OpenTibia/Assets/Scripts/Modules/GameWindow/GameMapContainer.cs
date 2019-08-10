using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;

using PlayerAction = OpenTibiaUnity.Protobuf.Shared.PlayerAction;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    internal class GameMapContainer : GamePanelContainer, IMoveWidget, IUseWidget, IWidgetContainerWidget
    {
        [SerializeField] private GameWorldMap m_GameWorldMap = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_FramecounterText = null;
        
        private Rect m_CachedScreenRect = Rect.zero;
        private bool m_ScreenRectCached = false;
        private bool m_MouseCursorOverRenderer = false;

        private int m_LastScreenWidth = 0;
        private int m_LastScreenHeight = 0;
        private int m_LastFramerate = 0;
        private int m_LastPing = 9999;

        private ObjectDragImpl<GameMapContainer> m_DragHandler;
        
        private RectTransform worldMapRectTransform {
            get => m_GameWorldMap.rectTransform;
        }

        protected override void Start() {
            base.Start();

            m_DragHandler = new ObjectDragImpl<GameMapContainer>(this);
            ObjectMultiUseHandler.RegisterContainer(this);

            m_GameWorldMap.onPointerEnter.AddListener(OnWorldMapPointerEnter);
            m_GameWorldMap.onPointerExit.AddListener(OnWorldMapPointerExit);

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

        protected void OnWorldMapPointerEnter() {
            m_MouseCursorOverRenderer = true;
        }

        protected void OnWorldMapPointerExit() {
            m_MouseCursorOverRenderer = false;
            OpenTibiaUnity.WorldMapRenderer.HighlightTile = null;
            OpenTibiaUnity.WorldMapRenderer.HighlightObject = OpenTibiaUnity.CreatureStorage.Aim;
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.Medium);
        }

        protected void RenderWorldMap() {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused)
                return;
#endif

            if (!m_ScreenRectCached)
                CacheScreenRect();

            var gameManager = OpenTibiaUnity.GameManager;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            if (gameManager != null && worldMapStorage != null && gameManager.IsGameRunning && worldMapStorage.Valid) {
                if (m_MouseCursorOverRenderer && gameManager.GameCanvas.gameObject.activeSelf && !gameManager.GamePanelBlocker.gameObject.activeSelf) {
                    InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true, true);
                } else {
                    worldMapRenderer.HighlightTile = null;
                    worldMapRenderer.HighlightObject = null;
                }

                if (ContextMenuBase.CurrentContextMenu != null || ObjectDragImpl.AnyDraggingObject)
                    worldMapRenderer.HighlightTile = null;

                gameManager.WorldMapRenderingTexture.Release();
                RenderTexture.active = gameManager.WorldMapRenderingTexture;
                worldMapRenderer.Render(worldMapRectTransform.rect);
                RenderTexture.active = null;

                // setting the clip area
                m_GameWorldMap.rawImage.uvRect = worldMapRenderer.CalculateClipRect();

                if (worldMapRenderer.Framerate != m_LastFramerate) {
                    m_LastFramerate = worldMapRenderer.Framerate;
                    m_LastPing = protocolGame.Ping;
                    m_FramecounterText.text = string.Format("FPS: <color=#{0:X6}>{1}</color>\nPing:{2}", GetFramerateColor(m_LastFramerate), m_LastFramerate, m_LastPing);
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
        
        internal void CacheScreenRect() {
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

        internal void InvalidateScreenRect() => m_ScreenRectCached = false;

        internal void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!m_MouseCursorOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;
            
            var point = RawMousePositionToLocalMapPosition(mousePosition);
            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, point, applyAction, updateCursor, updateHighlight);
            return action != AppearanceActions.None;
        }
        
        internal AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, Vector2 point, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            if (updateCursor)
                updateCursor = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            if (updateHighlight)
                updateHighlight = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler.IsMouseButtonDragged(MouseButton.Left) || inputHandler.IsMouseButtonDragged(MouseButton.Right))
                return AppearanceActions.None;

            var gameManager = OpenTibiaUnity.GameManager;
            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var mapPosition = worldMapRenderer.PointToMap(point);

            if (gameManager.ClientVersion >= 1100)
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

                if (mouseButton == MouseButton.Right) {
                    var field = worldMapStorage.GetField(mapPosition.Value);
                    topLookObjectStackPos = field.GetTopLookObject(out topLookObject);
                    topUseObjectStackPos = field.GetTopUseObject(out topUseObject);
                    if (!!topLookObject && topLookObject.IsCreature)
                        creature = creatureStorage.GetCreature(topLookObject.Data);

                    if (!!topUseObject || !!topLookObject)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Left) {
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
                            } else if ((topLookObjectStackPos = field.GetTopLookObject(out topLookObject)) != -1 && !!topLookObject) {
                                action = AppearanceActions.Look;
                            } else {
                                // TODO (default action)
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
                        if (!!creature && creature.ID != player.ID && (!creature.IsNPC || gameManager.ClientVersion < 1000))
                            action = AppearanceActions.Attack;

                        }
                }
            } else if (optionStorage.MousePreset == MousePresets.Classic) {
                if (eventModifiers == EventModifiers.Alt) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                        creature = worldMapRenderer.PointToCreature(point, true);
                        if (!!creature && creature.ID != player.ID && (!creature.IsNPC || gameManager.ClientVersion < 1000))
                            action = AppearanceActions.Attack;
                    }
                } else if (eventModifiers == EventModifiers.Control) {
                    if (mouseButton != MouseButton.Both && mouseButton != MouseButton.Middle) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);
                        if (!!topLookObject && topLookObject.IsCreature)
                            creature = creatureStorage.GetCreature(topLookObject.Data);

                        if (!!topUseObject || !!topLookObject)
                            action = AppearanceActions.ContextMenu;
                    }
                } else if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                    if (eventModifiers == EventModifiers.None) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject) {
                            if (optionStorage.MouseLootPreset == MouseLootPresets.Left && topLookObject.Type.IsCorpse) {
                                topUseObject = topLookObject;
                                topUseObjectStackPos = topLookObjectStackPos;
                                action = AppearanceActions.Loot;
                            } else if (topLookObject.Type.DefaultAction == PlayerAction.AutowalkHighlight) {
                                action = AppearanceActions.AutoWalkHighlight;
                            } else {
                                action = AppearanceActions.AutoWalk;
                            }
                        }
                            
                    } else if (eventModifiers == EventModifiers.Shift) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject)
                            action = AppearanceActions.Look;
                    }
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None) {
                        creature = worldMapRenderer.PointToCreature(point, true);
                        if (!!creature && creature.ID != player.ID && (!creature.IsNPC || gameManager.ClientVersion < 1000)) {
                            action = AppearanceActions.Attack;
                        } else {
                            topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);
                            if (!!topUseObject) {
                                if (optionStorage.MouseLootPreset == MouseLootPresets.Right && topUseObject.Type.IsCorpse)
                                    action = AppearanceActions.Loot;
                                else if (topUseObject.Type.IsContainer)
                                    action = AppearanceActions.Open;
                                else
                                    action = AppearanceActions.Use;
                            }
                        }
                    } else if (eventModifiers == EventModifiers.Shift && optionStorage.MouseLootPreset == MouseLootPresets.ShiftPlusRight) {
                        topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);
                        if (!!topUseObject && topUseObject.Type.IsCorpse)
                            action = AppearanceActions.Loot;
                    }
                } else if (mouseButton == MouseButton.Both) {
                    if (eventModifiers == EventModifiers.None) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        if (!!topLookObject)
                            action = AppearanceActions.Look;
                    }
                }

            } else if (optionStorage.MousePreset == MousePresets.Regular) {
                // TODO
            }

            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(action, CursorPriority.Medium);

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
                        if (topUseObject.Type.IsMultiUse)
                            ObjectMultiUseHandler.Activate(absolutePosition, topUseObject, topUseObjectStackPos);
                        else
                            GameActionFactory.CreateUseAction(absolutePosition, topUseObject.Type, topUseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        GameActionFactory.CreateUseAction(absolutePosition, topUseObject, topUseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
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

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!m_MouseCursorOverRenderer) {
                @object = null;
                return - 1;
            }

            var mapPosition = MousePositionToMapPosition(mousePosition);
            if (!mapPosition.HasValue) {
                @object = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopMoveObject(mapPosition.Value, out @object);
        }

        public int GetTopObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!m_MouseCursorOverRenderer) {
                @object = null;
                return -1;
            }

            var mapPosition = MousePositionToMapPosition(mousePosition);
            if (!mapPosition.HasValue) {
                @object = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopLookObject(mapPosition.Value, out @object);
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!m_MouseCursorOverRenderer) {
                @object = null;
                return -1;
            }

            var mapPosition = MousePositionToMapPosition(mousePosition);
            if (!mapPosition.HasValue) {
                @object = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopUseObject(mapPosition.Value, out @object);
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!m_MouseCursorOverRenderer) {
                @object = null;
                return -1;
            }

            var mapPosition = MousePositionToMapPosition(mousePosition);
            if (!mapPosition.HasValue) {
                @object = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopMultiUseObject(mapPosition.Value, out @object);
        }

        public Vector3Int? MousePositionToMapPosition(Vector3 mousePosition) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToMap(RawMousePositionToLocalMapPosition(mousePosition));
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 mousePosition) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToAbsolute(RawMousePositionToLocalMapPosition(mousePosition));
        }

        private Vector2 RawMousePositionToLocalMapPosition(Vector3 mousePosition) {
            CacheScreenRect();

            var mousePoint = new Vector2(Input.mousePosition.x, m_LastScreenHeight - Input.mousePosition.y);
            return mousePoint - m_CachedScreenRect.position;
        }
    }
}
