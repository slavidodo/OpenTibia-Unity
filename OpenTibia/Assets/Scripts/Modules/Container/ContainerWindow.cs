using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Container;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Container
{
    internal class ContainerWindow : Core.Components.Base.MiniWindow, IUseWidget, IMoveWidget, IWidgetContainerWidget
    {
        [SerializeField] private OTU_ScrollRect m_ItemsScrollRect = null;
        [SerializeField] private Button m_UpButton = null;

        private ContainerView m_ContainerView = null;
        private int m_Rows = -1;
        private int m_NumberOfSlots = 0;
        private int m_SlotUnderMouse = -1;

        private ItemView[] m_ItemViews;
        private RenderTexture m_SlotsRenderTexture;
        private ObjectDragImpl<ContainerWindow> m_DragHandler;

        protected override void Awake() {
            base.Awake();

            m_DragHandler = new ObjectDragImpl<ContainerWindow>(this);
            ObjectMultiUseHandler.RegisterContainer(this);

            OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utility.EventImplPriority.Default, OnMouseUp);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            var gameManager = OpenTibiaUnity.GameManager;
            InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true);

            if (!m_SlotsRenderTexture)
                return;

            Vector2 zoom = new Vector2(Screen.width / (float)m_SlotsRenderTexture.width, Screen.height / (float)m_SlotsRenderTexture.height);

            m_SlotsRenderTexture.Release();
            RenderTexture.active = m_SlotsRenderTexture;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < m_Rows; j++) {
                    int index = j * 4 + i;

                    if (index < m_ContainerView.NumberOfTotalObjects) {
                        var @object = m_ContainerView.GetObject(index + m_ContainerView.IndexOfFirstObject);
                        if (@object) {
                            @object.Animate(OpenTibiaUnity.TicksMillis);
                            @object.DrawTo(new Vector2(Constants.FieldSize * i, Constants.FieldSize * j), zoom, 0, 0, 0);
                        }
                    }
                }
            }
            
            RenderTexture.active = null;
        }

        protected void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!m_MouseCursorOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;

            var @object = GetObjectUnderMouse();
            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, @object, applyAction, updateCursor);
            return action != AppearanceActions.None;
        }

        protected AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, ObjectInstance @object, bool applyAction = false, bool updateCursor = false) {
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
                        action = AppearanceActions.Open;
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
                        action = AppearanceActions.Open;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = AppearanceActions.Open;
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
                        action = AppearanceActions.Open;
                    else if ((eventModifiers & EventModifiers.Alt) != 0 && (eventModifiers & EventModifiers.Shift) == 0)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = AppearanceActions.ContextMenu;
                    else if (eventModifiers == EventModifiers.Control)
                        action = AppearanceActions.Open;
                }
            }

            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(action, CursorPriority.Medium);

            if (applyAction) {
                var absolutePosition = new Vector3Int(65535, 64 + m_ContainerView.ID, m_SlotUnderMouse);
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
                            GameActionFactory.CreateUseAction(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        GameActionFactory.CreateUseAction(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }

        internal void UpdateProperties(ContainerView containerView) {
            containerView.onObjectAdded.AddListener(OnAddedObject);
            containerView.onObjectRemoved.AddListener(OnRemovedObject);

            m_NumberOfSlots = containerView.NumberOfSlotsPerPage;
            m_Rows = (int)Mathf.Ceil(m_NumberOfSlots / 4f);

            m_SlotsRenderTexture = new RenderTexture(Constants.FieldSize * 4, Constants.FieldSize * m_Rows, 0, RenderTextureFormat.ARGB32);

            m_ItemViews = new ItemView[m_NumberOfSlots];

            m_ContainerView = containerView;

            int paginationHeight = 0;
            if (containerView.IsPaginationEnabled)
                paginationHeight += 25;

            m_MinContentHeight = 34 + paginationHeight;
            m_MaxContentHeight = m_Rows * 34 + (m_Rows - 1) * 3 + paginationHeight;

            int activeRows = (containerView.NumberOfTotalObjects / 4);
            m_PreferredContentHeight = activeRows * 34 + (activeRows - 1) * 3 + paginationHeight;
            
            m_UpButton.gameObject.SetActive(containerView.IsSubContainer);

            m_TitleLabel.SetText(containerView.Name);

            for (int i = 0; i < m_NumberOfSlots; i++) {
                var itemView = Instantiate(ModulesManager.Instance.ItemViewPrefab, m_ItemsScrollRect.content);
                itemView.onPointerEnter.AddListener((ClothSlots _) => m_SlotUnderMouse = i);
                itemView.onPointerExit.AddListener((ClothSlots _) => m_SlotUnderMouse = -1);

                itemView.itemImage.enabled = false;
                itemView.itemImage.texture = m_SlotsRenderTexture;
                float w = (float)Constants.FieldSize / m_SlotsRenderTexture.width;
                float h = (float)Constants.FieldSize / m_SlotsRenderTexture.height;
                itemView.itemImage.uvRect = new Rect(w * (i % 4), 1 - h * (i / 4 + 1), w, h);
                m_ItemViews[i] = itemView;
            }

            UpdateLayout();
        }

        protected void OnAddedObject(ContainerView _, int index, ObjectInstance @object) {
            index -= m_ContainerView.IndexOfFirstObject;

            var itemView = m_ItemViews[index];

            itemView.itemImage.enabled = true;
            itemView.showAmount = @object.Data > 1;
            itemView.objectAmount = (int)@object.Data;
        }
        
        protected void OnRemovedObject(ContainerView _, int index, ObjectInstance appendObject) {
            index -= m_ContainerView.IndexOfFirstObject;
            m_ItemViews[index].itemImage.enabled = !!appendObject;
        }

        internal ObjectInstance GetObjectUnderMouse() {
            if (m_SlotUnderMouse == -1)
                return null;

            if (m_SlotUnderMouse >= m_ContainerView.NumberOfTotalObjects)
                return null;

            return m_ContainerView.GetObject(m_SlotUnderMouse + m_ContainerView.IndexOfFirstObject);
        }

        internal bool Matches(ContainerView containerView) {
            return m_ContainerView != null && m_ContainerView.ID == containerView.ID;
        }

        public int GetTopObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return m_SlotUnderMouse;
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return m_SlotUnderMouse;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            if (!@object.Type.IsMultiUse) {
                @object = null;
                return -1;
            }

            return m_SlotUnderMouse;
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(mousePosition, out @object);
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 mousePosition) {
            if (m_SlotUnderMouse == -1)
                return null;

            return new Vector3Int(65535, 64 + m_ContainerView.ID, m_SlotUnderMouse);
        }

        public Vector3Int? MousePositionToMapPosition(Vector3 mousePosition) {
            return null;
        }
    }
}
