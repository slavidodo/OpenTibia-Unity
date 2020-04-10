using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Inventory
{
    public class BodyContainer : Core.Components.Base.Module, IUseWidget, IMoveWidget, IWidgetContainerWidget
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.ItemPanel _head = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _torso = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _legs = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _feet = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _neck = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _leftHand = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _finger = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _backpack = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _rightHand = null;
        [SerializeField]
        private UI.Legacy.ItemPanel _hip = null;
        [SerializeField]
        private Texture2D _defaultsTexture = null;
        [SerializeField]
        private InventoryWidget _inventoryWidget = null;

        // fields
        private RenderTexture _renderTexture;
        private int _slotUnderMouse = -1;

        protected override void Awake() {
            base.Awake();

            // initialize render texture
            _renderTexture = new RenderTexture(Constants.FieldSize * (int)ClothSlots.Hip, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
            _renderTexture.filterMode = FilterMode.Bilinear;
            _renderTexture.Create();

            // initialize objects interaction
            ObjectDragHandler.RegisterHandler(this);
            ObjectMultiUseHandler.RegisterContainer(this);

            // setup events
            OpenTibiaUnity.ContainerStorage.BodyContainerView.onSlotChange.AddListener(OnInventorySlotChange);

            _head.onPointerEnter.AddListener(OnItemPointerEnter);
            _head.onPointerExit.AddListener(OnItemPointerExit);
            _neck.onPointerEnter.AddListener(OnItemPointerEnter);
            _neck.onPointerExit.AddListener(OnItemPointerExit);
            _backpack.onPointerEnter.AddListener(OnItemPointerEnter);
            _backpack.onPointerExit.AddListener(OnItemPointerExit);
            _torso.onPointerEnter.AddListener(OnItemPointerEnter);
            _torso.onPointerExit.AddListener(OnItemPointerExit);
            _rightHand.onPointerEnter.AddListener(OnItemPointerEnter);
            _rightHand.onPointerExit.AddListener(OnItemPointerExit);
            _leftHand.onPointerEnter.AddListener(OnItemPointerEnter);
            _leftHand.onPointerExit.AddListener(OnItemPointerExit);
            _legs.onPointerEnter.AddListener(OnItemPointerEnter);
            _legs.onPointerExit.AddListener(OnItemPointerExit);
            _feet.onPointerEnter.AddListener(OnItemPointerEnter);
            _feet.onPointerExit.AddListener(OnItemPointerExit);
            _finger.onPointerEnter.AddListener(OnItemPointerEnter);
            _finger.onPointerExit.AddListener(OnItemPointerExit);
            _hip.onPointerEnter.AddListener(OnItemPointerEnter);
            _hip.onPointerExit.AddListener(OnItemPointerExit);
        }

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.GameManager.GetModule<UI.Legacy.WorldMapWidget>().onInvalidateTRS.AddListener(OnInvalidateTRS);
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

        protected override void OnDestroy() {
            base.OnDestroy();

            _renderTexture.Release();
            if (OpenTibiaUnity.GameManager)
                OpenTibiaUnity.GameManager.GetModule<UI.Legacy.WorldMapWidget>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        private void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true);

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(_renderTexture);
            commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

            var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
            commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var @object = OpenTibiaUnity.ContainerStorage.BodyContainerView.Objects[i];
                if (@object) {
                    @object.SetClamping(true);
                    @object.Animate(OpenTibiaUnity.TicksMillis);
                    @object.Draw(commandBuffer, new Vector2Int(Constants.FieldSize * i, 0), 0, 0, 0);
                }
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();
        }

        private void OnInventorySlotChange(ClothSlots slot, ObjectInstance @object) {
            switch (slot) {
                case ClothSlots.Head: InitialiseObjectImage(slot, _head, @object); break;
                case ClothSlots.Neck: InitialiseObjectImage(slot, _neck, @object); break;
                case ClothSlots.Backpack: InitialiseObjectImage(slot, _backpack, @object); break;
                case ClothSlots.Torso: InitialiseObjectImage(slot, _torso, @object); break;
                case ClothSlots.RightHand: InitialiseObjectImage(slot, _rightHand, @object); break;
                case ClothSlots.LeftHand: InitialiseObjectImage(slot, _leftHand, @object); break;
                case ClothSlots.Legs: InitialiseObjectImage(slot, _legs, @object); break;
                case ClothSlots.Feet: InitialiseObjectImage(slot, _feet, @object); break;
                case ClothSlots.Finger: InitialiseObjectImage(slot, _finger, @object); break;
                case ClothSlots.Hip: InitialiseObjectImage(slot, _hip, @object); break;
            }
        }

        private void OnInvalidateTRS() {
            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var @object = OpenTibiaUnity.ContainerStorage.BodyContainerView.Objects[i];
                if (@object)
                    @object.InvalidateTRS();
            }
        }

        private void OnItemPointerEnter(ClothSlots slot) {
            _slotUnderMouse = (int)slot;
        }

        private void OnItemPointerExit(ClothSlots _) {
            _slotUnderMouse = -1;
        }

        public void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!_inventoryWidget.IsMouseOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;

            var @object = GetObjectUnderMouse();
            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, @object, applyAction, updateCursor);
            return action != AppearanceActions.None;
        }

        public AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, ObjectInstance @object, bool applyAction = false, bool updateCursor = false) {
            if (!@object)
                return AppearanceActions.None;

            if (updateCursor)
                updateCursor = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler.IsMouseButtonDragged(MouseButton.Left) || inputHandler.IsMouseButtonDragged(MouseButton.Right))
                return AppearanceActions.None;

            var action = AppearanceActions.None;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            if (optionStorage.MousePreset == MousePresets.LeftSmartClick) {
                if (eventModifiers == EventModifiers.None || eventModifiers == EventModifiers.Control) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                        var defaultAction = @object.Type.DefaultAction;
                        if (defaultAction == Protobuf.Shared.PlayerAction.Open)
                            action = AppearanceActions.Open;
                        else if (defaultAction == Protobuf.Shared.PlayerAction.Look)
                            action = AppearanceActions.Look;
                        else if (defaultAction == Protobuf.Shared.PlayerAction.Use)
                            action = AppearanceActions.Use;
                        else if (@object.Type.IsContainer)
                            action = AppearanceActions.Open;
                        else if (@object.Type.IsForceUse || @object.Type.IsMultiUse || @object.Type.IsUsable)
                            action = AppearanceActions.Use;
                        else
                            action = AppearanceActions.Look;
                    } else if (mouseButton == MouseButton.Right && eventModifiers == EventModifiers.None) {
                        action = AppearanceActions.ContextMenu;
                    }
                } else if (eventModifiers == EventModifiers.Shift) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None)
                        action = AppearanceActions.Look;
                }

            } else if (optionStorage.MousePreset == MousePresets.Classic) {
                if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                    if (eventModifiers == EventModifiers.Shift)
                        action = AppearanceActions.Look;
                    else if (eventModifiers == EventModifiers.Control)
                        action = AppearanceActions.ContextMenu;
                    else if (eventModifiers == EventModifiers.Alt)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                    else if (eventModifiers == EventModifiers.Control)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Both) {
                    action = AppearanceActions.Look;
                }
            } else if (optionStorage.MousePreset == MousePresets.Regular) {
                if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                    if (eventModifiers == EventModifiers.Shift)
                        action = AppearanceActions.Look;
                    else if (eventModifiers == EventModifiers.Control)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                    else if ((eventModifiers & EventModifiers.Alt) != 0 && (eventModifiers & EventModifiers.Shift) == 0)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = AppearanceActions.ContextMenu;
                    else if (eventModifiers == EventModifiers.Control)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                }
            }

            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(action, CursorPriority.Medium);

            if (applyAction) {
                var absolutePosition = new Vector3Int(65535, _slotUnderMouse, 0);
                switch (action) {
                    case AppearanceActions.None: break;
                    case AppearanceActions.ContextMenu:
                        OpenTibiaUnity.CreateObjectContextMenu(absolutePosition, @object, absolutePosition.z, @object, absolutePosition.z, null)
                            .Display(mousePosition);
                        break;
                    case AppearanceActions.Look:
                        new LookActionImpl(absolutePosition, @object.Type, absolutePosition.z).Perform();
                        break;
                    case AppearanceActions.Use:
                        if (@object.Type.IsMultiUse)
                            ObjectMultiUseHandler.Activate(absolutePosition, @object, absolutePosition.z);
                        else
                            new UseActionImpl(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        new UseActionImpl(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }

        private void InitialiseObjectImage(ClothSlots slot, UI.Legacy.ItemPanel itemView, ObjectInstance @object) {
            if (slot > ClothSlots.Hip)
                return;

            itemView.objectInstance = @object;
            if (!!@object) {
                itemView.objectAmount = (int)@object.Data;
                itemView.showAmount = itemView.objectAmount > 1;

                itemView.image.texture = _renderTexture;
                float w = (float)Constants.FieldSize / _renderTexture.width;
                itemView.image.uvRect = new Rect(w * ((int)slot - 1), 0, w, 1);
            } else {
                float singleWidth = 1f / (ClothSlots.Hip - ClothSlots.Head + 1);
                itemView.image.texture = _defaultsTexture;
                itemView.image.uvRect = new Rect(singleWidth * (slot - ClothSlots.Head), 0, singleWidth, 1);
            }
        }

        public void ToggleStyle(bool minimized) {
            _head.gameObject.SetActive(!minimized);
            _torso.gameObject.SetActive(!minimized);
            _legs.gameObject.SetActive(!minimized);
            _feet.gameObject.SetActive(!minimized);
            _backpack.gameObject.SetActive(!minimized);
            _rightHand.gameObject.SetActive(!minimized);
            _hip.gameObject.SetActive(!minimized);
            _neck.gameObject.SetActive(!minimized);
            _leftHand.gameObject.SetActive(!minimized);
            _finger.gameObject.SetActive(!minimized);
        }

        public ObjectInstance GetObjectUnderMouse() {
            if (_slotUnderMouse == -1)
                return null;

            return OpenTibiaUnity.ContainerStorage.BodyContainerView.GetObject((ClothSlots)_slotUnderMouse);
        }

        public int GetTopObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return _slotUnderMouse;
        }

        public int GetUseObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return _slotUnderMouse;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            if (!@object.Type.IsMultiUse) {
                @object = null;
                return -1;
            }

            return _slotUnderMouse;
        }

        public int GetMoveObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(_, out @object);
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 _) {
            if (_slotUnderMouse == -1)
                return null;

            return new Vector3Int(65535, _slotUnderMouse, 0);
        }

        public Vector3Int? MousePositionToMapPosition(Vector3 _) {
            return null;
        }
    }
}
