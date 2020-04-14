using System.IO;
using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;
using PlayerAction = OpenTibiaUnity.Protobuf.Shared.PlayerAction;
using System;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.RawImage))]
    public class WorldMapWidget : Core.Components.Base.Module, IPointerEnterHandler, IPointerExitHandler, IUseWidget, IMoveWidget, IWidgetContainerWidget
    {
		// serialized fields
        [SerializeField]
        private bool _shouldTakeScreenshot = false;
        [SerializeField]
        private TMPro.TextMeshProUGUI _framecounterText = null;
        [SerializeField]
        private UnityUI.RawImage _onscreenTextImage = null;

		// non-serialized fields
        [NonSerialized]
        public UnityEvent onInvalidateTRS = null;

        // fields
        private RenderTexture _onscreenTextRenderTexture = null;
        private Rect _cachedScreenRect = Rect.zero;
        private bool _screenRectDirty = false;
        private bool _mouseCursorOverRenderer = false;
        private int _lastScreenWidth = 0;
        private int _lastScreenHeight = 0;
        private int _lastFramerate = 0;
        private int _lastPing = 9999;

        // properties
        private UnityUI.RawImage _rawImage;
        public UnityUI.RawImage rawImage {
            get {
                if (!_rawImage)
                    _rawImage = GetComponent<UnityUI.RawImage>();
                return _rawImage;
            }
        }

        protected override void Awake() {
            base.Awake();

            onInvalidateTRS = new UnityEvent();

            ObjectDragHandler.RegisterHandler(this);
            ObjectMultiUseHandler.RegisterContainer(this);
        }

        protected void Update() {
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height) {
                _screenRectDirty = true;

                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;

                if (OpenTibiaUnity.WorldMapStorage != null)
                    OpenTibiaUnity.WorldMapStorage.InvalidateFieldsTRS();

                onInvalidateTRS.Invoke();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utils.EventImplPriority.Default, OnMouseUp);
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.RemoveMouseUpListener(OnMouseUp);
        }

        protected void OnGUI() {
            if (Event.current.type == EventType.Repaint)
                RenderWorldMap();
        }

        protected override void OnRectTransformDimensionsChange() {
            _screenRectDirty = true;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _mouseCursorOverRenderer = true;
        }

        public void OnPointerExit(PointerEventData _) {
            _mouseCursorOverRenderer = false;

            if (OpenTibiaUnity.WorldMapRenderer != null) {
                OpenTibiaUnity.WorldMapRenderer.HighlightTile = null;
                OpenTibiaUnity.WorldMapRenderer.HighlightObject = OpenTibiaUnity.CreatureStorage.Aim;
            }

            if (OpenTibiaUnity.GameManager != null)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.Medium);
        }

        protected void RenderWorldMap() {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused)
                return;
#endif

            bool shouldCreateOnscreenTexture = false;
            if (_screenRectDirty) {
                shouldCreateOnscreenTexture = true;
                CacheScreenRect();
            }

            var gameManager = OpenTibiaUnity.GameManager;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            if (gameManager != null && worldMapStorage != null && gameManager.IsGameRunning && worldMapStorage.Valid) {
                if (_mouseCursorOverRenderer && gameManager.GameCanvas.gameObject.activeSelf && !gameManager.GamePanelBlocker.gameObject.activeSelf) {
                    InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true, true);
                } else {
                    worldMapRenderer.HighlightTile = null;
                    worldMapRenderer.HighlightObject = null;
                }

                if (ContextMenuBase.CurrentContextMenu != null || ObjectDragHandler.AnyDraggingObject)
                    worldMapRenderer.HighlightTile = null;

                var error = worldMapRenderer.RenderWorldMap(rectTransform.rect, gameManager.WorldMapRenderTexture);

                // no point in rendering text if rendering worldmap failed
                if (error == RenderError.None) {
                    if (_onscreenTextRenderTexture == null || shouldCreateOnscreenTexture) {
                        _onscreenTextRenderTexture?.Release();
                        _onscreenTextRenderTexture = new RenderTexture(_lastScreenWidth, _lastScreenHeight, 0, RenderTextureFormat.ARGB32);
                        _onscreenTextRenderTexture.filterMode = FilterMode.Bilinear;
                        _onscreenTextRenderTexture.Create();
                        _onscreenTextImage.texture = _onscreenTextRenderTexture;
                    }

                    worldMapRenderer.RenderOnscreenText(_cachedScreenRect, _onscreenTextRenderTexture);

                    if (!_onscreenTextImage.enabled)
                        _onscreenTextImage.enabled = true;
                }

                // setting the clip area
                var clipRect = worldMapRenderer.CalculateClipRect();
                if (clipRect != rawImage.uvRect)
                    rawImage.uvRect = clipRect;

                bool supportsPing = gameManager.GetFeature(GameFeature.GameClientPing);
                if (worldMapRenderer.Framerate != _lastFramerate || (supportsPing && _lastPing != protocolGame.Latency)) {
                    _lastFramerate = worldMapRenderer.Framerate;
                    var fpsColor = GetFramerateColor(_lastFramerate);

                    string text = string.Format("FPS: <color=#{0:X6}>{1}</color>", fpsColor, _lastFramerate);
                    if (supportsPing) {
                        _lastPing = protocolGame.Latency;
                        var pingColor = GetPingColor(_lastPing);
                        text += string.Format("\nPing: <color=#{0:X6}>{1}</color>", pingColor, _lastPing);
                    }

                    _framecounterText.text = text;
                }

                // tho, this is a very effecient way of taking screenshots
                // it doesn't consider windows ontop of the gamewindow that
                // may be blocking it the view
                if (_shouldTakeScreenshot && error == RenderError.None) {
                    _shouldTakeScreenshot = false;

                    var tmpTex2D = ScreenCapture.CaptureScreenshotAsTexture();
                    tmpTex2D.Apply();

                    int startX = (int)_cachedScreenRect.x;
                    int startY = (int)(_lastScreenHeight - _cachedScreenRect.height - _cachedScreenRect.y);
                    int width = (int)_cachedScreenRect.width;
                    int height = (int)_cachedScreenRect.height;

                    var tex2D = new Texture2D((int)_cachedScreenRect.width, (int)_cachedScreenRect.height, TextureFormat.ARGB32, false);
                    tex2D.SetPixels(0, 0, tex2D.width, tex2D.height, tmpTex2D.GetPixels(startX, startY, width, height));
                    tex2D.Apply();
                    byte[] texBytes = tex2D.EncodeToPNG();

                    if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Screenshots")))
                        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Screenshots"));

                    var texPath = Path.Combine(Application.persistentDataPath, "Screenshots/Screenshot_" + protocolGame.CharacterName + "_" +
                        System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");

                    File.WriteAllBytes(texPath, texBytes);
                    Debug.Log($"Saved Screenshot to : {texPath}");
                } else if (_shouldTakeScreenshot) {
                    _shouldTakeScreenshot = false;
                }
            } else {
                _framecounterText.text = "";
            }
        }

        private static uint GetFramerateColor(int framerate) {
            if (framerate < 10)
                return Core.Chat.MessageColors.Red;
            else if (framerate < 30)
                return Core.Chat.MessageColors.Orange;
            else if (framerate < 58)
                return Core.Chat.MessageColors.Yellow;
            else
                return Core.Chat.MessageColors.Green;
        }

        private static uint GetPingColor(int ping) {
            if (ping < 200)
                return Core.Chat.MessageColors.Green;
            else if (ping < 500)
                return Core.Chat.MessageColors.Yellow;
            else if (ping < 1000)
                return Core.Chat.MessageColors.Orange;
            else
                return Core.Chat.MessageColors.Red;
        }

        public void CacheScreenRect() {
            if (_screenRectDirty) {
                _screenRectDirty = false;

                var position = rectTransform.position;
                var pivot = rectTransform.pivot;
                var size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);

                _cachedScreenRect = new Rect {
                    x = position.x - pivot.x * size.x,
                    y = Screen.height - position.y - (1.0f - pivot.y) * size.y,
                    size = size,
                };

                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;

                _onscreenTextImage.rectTransform.offsetMin = new Vector2 {
                    x = -_cachedScreenRect.x,
                    y = -(Screen.height - _cachedScreenRect.height - _cachedScreenRect.y),
                };
                _onscreenTextImage.rectTransform.offsetMax = new Vector2 {
                    x = Screen.width - _cachedScreenRect.width - _cachedScreenRect.x,
                    y = _cachedScreenRect.y
                };
            }
        }

        public void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!_mouseCursorOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;

            var point = RawMousePositionToLocalMapPosition(mousePosition);
            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, point, applyAction, updateCursor, updateHighlight);
            return action != AppearanceActions.None;
        }

        public AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, Vector2 point, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
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
            var mapPosition = worldMapRenderer.PointToMap(point, false);

            if (gameManager.ClientVersion >= 1100)
                worldMapRenderer.HighlightTile = mapPosition;

            if (!mapPosition.HasValue)
                return AppearanceActions.None;

            var player = OpenTibiaUnity.Player;
            var action = AppearanceActions.None;
            ObjectInstance topLookObject = null;
            ObjectInstance topUseObject = null;
            int topLookObjectStackPos = -1;
            int topUseObjectStackPos = -1;

            Creature creature = worldMapRenderer.PointToCreature(point, true);

            var optionStorage = OpenTibiaUnity.OptionStorage;
            var absolutePosition = worldMapStorage.ToAbsolute(mapPosition.Value);

            if (optionStorage.MousePreset == MousePresets.LeftSmartClick) {
                bool forceLook = eventModifiers == EventModifiers.Shift;
                if (mouseButton == MouseButton.Right) {
                    var field = worldMapStorage.GetField(mapPosition.Value);
                    topLookObjectStackPos = field.GetTopLookObject(out topLookObject);
                    topUseObjectStackPos = field.GetTopUseObject(out topUseObject);

                    if (!!topUseObject || !!topLookObject)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Left) {
                    if (eventModifiers == EventModifiers.None) {
                        if (mapPosition.Value.z != worldMapStorage.PlayerZPlane) {
                            var field = worldMapStorage.GetField(mapPosition.Value);

                            action = AppearanceActions.SmartClick;
                            if (!!creature) {
                                if (creature.Id == player.Id || forceLook) {
                                    topLookObjectStackPos = field.GetCreatureObjectForCreatureId(creature.Id, out ObjectInstance creatureObject);
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
                        if (!!creature && creature.Id != player.Id && (!creature.IsNPC || gameManager.ClientVersion < 1000))
                            action = AppearanceActions.Attack;
                    }
                }
            } else if (optionStorage.MousePreset == MousePresets.Classic) {
                if (eventModifiers == EventModifiers.Alt) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                        if (!!creature && creature.Id != player.Id && (!creature.IsNPC || gameManager.ClientVersion < 1000))
                            action = AppearanceActions.Attack;
                    }
                } else if (eventModifiers == EventModifiers.Control) {
                    if (mouseButton != MouseButton.Both && mouseButton != MouseButton.Middle) {
                        topLookObjectStackPos = worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                        topUseObjectStackPos = worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);

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
                        if (!!creature && creature.Id != player.Id && (!creature.IsNPC || gameManager.ClientVersion < 1000)) {
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
                        if (!!topUseObject && topUseObject.Type.IsCorpse && !topUseObject.Type.IsPlayerCorpse)
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
                        if (!!creature && creature.Id != player.Id)
                            OpenTibiaUnity.CreatureStorage.ToggleAttackTarget(creature, true);
                        break;
                    case AppearanceActions.AutoWalk:
                    case AppearanceActions.AutoWalkHighlight:
                        absolutePosition = worldMapRenderer.PointToAbsolute(RawMousePositionToLocalMapPosition(mousePosition), true).Value;
                        player.StartAutowalk(absolutePosition, false, true);
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
                            new UseActionImpl(absolutePosition, topUseObject.Type, topUseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        new UseActionImpl(absolutePosition, topUseObject, topUseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Talk:
                        new GreetAction(creature).Perform();
                        break;
                    case AppearanceActions.Loot:
                        new LootActionImpl(absolutePosition, topLookObject, topLookObjectStackPos).Perform();
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!_mouseCursorOverRenderer) {
                @object = null;
                return -1;
            }

            var mapPosition = MousePositionToMapPosition(mousePosition);
            if (!mapPosition.HasValue) {
                @object = null;
                return -1;
            }

            return OpenTibiaUnity.WorldMapStorage.GetTopMoveObject(mapPosition.Value, out @object);
        }

        public int GetTopObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            if (!_mouseCursorOverRenderer) {
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
            if (!_mouseCursorOverRenderer) {
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
            if (!_mouseCursorOverRenderer) {
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
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToMap(RawMousePositionToLocalMapPosition(mousePosition), false);
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 mousePosition) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToAbsolute(RawMousePositionToLocalMapPosition(mousePosition), false);
        }

        private Vector2 RawMousePositionToLocalMapPosition(Vector3 mousePosition) {
            CacheScreenRect();

            var mousePoint = new Vector2(Input.mousePosition.x, _lastScreenHeight - Input.mousePosition.y);
            return mousePoint - _cachedScreenRect.position;
        }
    }
}
