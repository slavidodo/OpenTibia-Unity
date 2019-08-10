using OpenTibiaUnity.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Hotkeys
{
    internal class HotkeysWindow : Core.Components.Base.Window
    {
        private static RenderTexture s_RenderTexture;

        internal const string TextUseOnYourself = "<color=#AFFEAF>{0}: (use object on yourself)</color>";
        internal const string TextUseOnTarget = "<color=#FEAFAF>{0}:(use object on target)</color>";
        internal const string TextUseWithCrosshairs = "<color=#C37A7A>{0}: (use object with crosshairs)</color>";

        internal static Color NormalColor = new Color(0, 0, 0, 0);
        internal static Color HighlightColor = Core.Colors.ColorFromRGB(0x585858);

        [SerializeField] private OTU_ScrollRect m_HotkeysScrollRect = null;
        [SerializeField] private ToggleGroup m_HotkeyActionsToggleGroup = null;

        [SerializeField] private TMPro.TMP_InputField m_HotkeyTextInputField = null;
        [SerializeField] private Core.Components.CheckboxWrapper m_AutoSendCheckboxWrapper = null;

        [SerializeField] private RawImage m_ObjectImage = null;
        [SerializeField] private Button m_SelectObjectButton = null;
        [SerializeField] private Button m_ClearObjectButton = null;

        [SerializeField] private Core.Components.ToggleWrapper m_UseOnYourselfToggle = null;
        [SerializeField] private Core.Components.ToggleWrapper m_UseOnTargetToggle = null;
        [SerializeField] private Core.Components.ToggleWrapper m_UseWithCrosshairsToggle = null;

        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;

        private IHotkeyAction[] m_PlainKeys;
        private IHotkeyAction[] m_ShiftKeys;
        private IHotkeyAction[] m_ControlKeys;

        private HotkeyActionPanel m_ActiveActionPanel = null;
        private Core.Appearances.ObjectInstance m_ObjectInstance = null;
        private bool m_ChangingSelectedAction = false;

        protected override void Start() {
            base.Start();

            m_PlainKeys = new IHotkeyAction[12];
            m_ShiftKeys = new IHotkeyAction[12];
            m_ControlKeys = new IHotkeyAction[12];

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utility.EventImplPriority.UpperMedium, OnKeyDown);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utility.EventImplPriority.UpperMedium, OnKeyUp);

            // setup events
            m_HotkeyTextInputField.onValueChanged.AddListener(OnHotkeyTextInputValueChanged);
            m_AutoSendCheckboxWrapper.onValueChanged.AddListener(OnAutoSendValueChanged);
            m_SelectObjectButton.onClick.AddListener(OnSelectObjectButtonClick);
            m_ClearObjectButton.onClick.AddListener(OnClearObjectButtonClick);
            m_UseOnYourselfToggle.toggle.onValueChanged.AddListener(OnUseOnYourselfValueChanged);
            m_UseOnTargetToggle.toggle.onValueChanged.AddListener(OnUseOnTargetValueChanged);
            m_UseWithCrosshairsToggle.toggle.onValueChanged.AddListener(OnUseWithCrosshairsValueChanged);
            m_OKButton.onClick.AddListener(OnOKButtonClick);
            m_CancelButton.onClick.AddListener(OnCancelButtonClick);

            // initialize action panels
            for (int i = 0; i < 12 * 3; i++) {
                KeyCode keyCode;
                EventModifiers eventModifiers;

                string baseText = string.Empty;
                if (i < 12) {
                    keyCode = KeyCode.F1 + i;
                    eventModifiers = EventModifiers.None;
                } else if (i < 24) {
                    keyCode = KeyCode.F1 + i - 12;
                    eventModifiers = EventModifiers.Shift;
                    baseText += "Shift+";
                } else {
                    keyCode = KeyCode.F1 + i - 12 * 2;
                    eventModifiers = EventModifiers.Control;
                    baseText += "Control+";
                }

                baseText += keyCode.ToString();

                var hotkeysActionPanel = Instantiate(ModulesManager.Instance.HotkeysActionPanelPrefab, m_HotkeysScrollRect.content);
                hotkeysActionPanel.KeyCode = keyCode;
                hotkeysActionPanel.EventModifiers = eventModifiers;
                hotkeysActionPanel.BaseText = baseText;
                hotkeysActionPanel.textComponent.text = hotkeysActionPanel.BaseText + ":";
                hotkeysActionPanel.toggleComponent.group = m_HotkeyActionsToggleGroup;
                hotkeysActionPanel.normalColor = NormalColor;
                hotkeysActionPanel.highlightColor = HighlightColor;
                hotkeysActionPanel.toggleComponent.onValueChanged.AddListener((value) => {
                    OnHotkeyActionToggleValueChanged(hotkeysActionPanel, value);
                });

                if (i == 0)
                    hotkeysActionPanel.Select();
            }
        }

        protected void OnGUI() {
            var e = Event.current;
            if (e.type != EventType.Repaint)
                return;

            if (!m_ActiveActionPanel) {
                if (m_ObjectImage.enabled)
                    m_ObjectImage.enabled = false;
                return;
            }

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(m_ActiveActionPanel);
            if (objectAction == null || !objectAction.AppearanceType) {
                if (m_ObjectImage.enabled)
                    m_ObjectImage.enabled = false;
                return;
            }
            
            if (s_RenderTexture == null) {
                s_RenderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                s_RenderTexture.filterMode = FilterMode.Point;

                m_ObjectImage.texture = s_RenderTexture;
            } else {
                s_RenderTexture.Release();
            }

            RenderTexture.active = s_RenderTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            var zoom = new Vector2(Screen.width / (float)s_RenderTexture.width, Screen.height / (float)s_RenderTexture.height);
            m_ObjectInstance.DrawTo(new Vector2(0, 0), zoom, 0, 0, 0);

            RenderTexture.active = null;

            if (!m_ObjectImage.enabled)
                m_ObjectImage.enabled = true;
        }

        private void OnKeyDown(Event e, bool repeat) {
            if (repeat)
                OnKeyUp(e, false);
        }

        private void OnKeyUp(Event e, bool _) {
            if (e.keyCode < KeyCode.F1 || e.keyCode > KeyCode.F12)
                return;

            bool shift = (e.modifiers & EventModifiers.Shift) != 0;
            bool control = (e.modifiers & EventModifiers.Control) != 0;
            bool alt = (e.modifiers & EventModifiers.Alt) != 0;

            if (alt || (shift && control))
                return;

            var action = GetHotkeyActionForEvent(e);
            if (action != null)
                action.Apply();
        }
        
        private void OnHotkeyTextInputValueChanged(string text) {
            if (m_ChangingSelectedAction)
                return;

            if (text.Length == 0)
                m_ActiveActionPanel.textComponent.text = m_ActiveActionPanel.BaseText + ":";
            else
                m_ActiveActionPanel.textComponent.text = string.Format("{0}: {1}", m_ActiveActionPanel.BaseText, Core.StringHelper.RichTextSpecialChars(text));

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(m_ActiveActionPanel);
            if (textAction == null) {
                textAction = new HotkeyTextAction(text, m_AutoSendCheckboxWrapper.checkbox.Checked);
                SetHotketActionForPanel(m_ActiveActionPanel, textAction);
            } else {
                textAction.Text = text;
            }
        }

        private void OnAutoSendValueChanged(bool value) {
            if (m_ChangingSelectedAction)
                return;

            if (value)
                m_ActiveActionPanel.textComponent.color = Core.Colors.ColorFromRGB(0xFEFEFE);
            else
                m_ActiveActionPanel.textComponent.color = Core.Colors.ColorFromRGB(0xC0C0C0);

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(m_ActiveActionPanel);
            if (textAction == null) {
                textAction = new HotkeyTextAction(m_HotkeyTextInputField.text, value);
                SetHotketActionForPanel(m_ActiveActionPanel, textAction);
            } else {
                textAction.AutoSend = value;
            }
        }

        private void OnSelectObjectButtonClick() {
            // we don't wanna any information from that //
            Core.Game.ObjectMultiUseHandler.Activate(Vector3Int.zero, null, 0);

            // set the handler to be our handler
            Core.Game.ObjectMultiUseHandler.onUse = OnObjectMultiUseHandlerUse;

            // close the window to allow for game actions
            CloseWindow();
        }

        private void OnClearObjectButtonClick() {
            // should stop drawing //
            SetHotketActionForPanel<IHotkeyAction>(m_ActiveActionPanel, null);

            DisableObjectToggles();
        }

        private void OnUseOnYourselfValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(m_ActiveActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Self;
            m_ActiveActionPanel.textComponent.text = string.Format(TextUseOnYourself, m_ActiveActionPanel.BaseText);
        }

        private void OnUseOnTargetValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(m_ActiveActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Target;
            m_ActiveActionPanel.textComponent.text = string.Format(TextUseOnTarget, m_ActiveActionPanel.BaseText);
        }

        private void OnUseWithCrosshairsValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(m_ActiveActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.CrossHair;
            m_ActiveActionPanel.textComponent.text = string.Format(TextUseWithCrosshairs, m_ActiveActionPanel.BaseText);
        }

        private void OnOKButtonClick() {
            // save //
            CloseWindow();
        }

        private void OnCancelButtonClick() {
            CloseWindow();
        }

        private void OnObjectMultiUseHandlerUse(Vector3Int _, Core.Appearances.ObjectInstance @object, int __) {
            Core.Game.ObjectMultiUseHandler.onUse = null;
            if (!@object) {
                OpenWindow();
                return;
            }

            var objectID = @object.ID;
            if (objectID < 100)
                return;

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(m_ActiveActionPanel);
            if (objectAction == null) {
                objectAction = new HotkeyObjectAction(@object.Type, UseActionTarget.CrossHair);
                SetHotketActionForPanel(m_ActiveActionPanel, objectAction);

                EnableObjectToggles();

                m_ActiveActionPanel.textComponent.text = string.Format(TextUseWithCrosshairs, m_ActiveActionPanel.BaseText);
                m_UseWithCrosshairsToggle.toggle.isOn = true;

                m_HotkeyTextInputField.interactable = false;
                m_AutoSendCheckboxWrapper.DisableComponent();
            } else {
                objectAction.AppearanceType = @object.Type;
            }

            if (!m_ObjectInstance || m_ObjectInstance.ID != objectAction.AppearanceType.ID)
                m_ObjectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType.ID, objectAction.AppearanceType, 0);

            OpenWindow();
        }

        private void OnHotkeyActionToggleValueChanged(HotkeyActionPanel actionPanel, bool value) {
            if (!value)
                return;

            m_ChangingSelectedAction = true;
            
            var action = GetHotkeyActionForPanel<IHotkeyAction>(actionPanel);

            bool isObjectAction = action is HotkeyObjectAction;
            if (isObjectAction)
                m_AutoSendCheckboxWrapper.DisableComponent();
            else
                m_AutoSendCheckboxWrapper.EnableComponent();

            m_HotkeyTextInputField.interactable = !isObjectAction;
            m_ObjectImage.enabled = isObjectAction;

            if (action is HotkeyTextAction textAction) {
                m_HotkeyTextInputField.text = textAction.Text;
                m_AutoSendCheckboxWrapper.checkbox.Checked = textAction.AutoSend;
            } else {
                m_HotkeyTextInputField.text = string.Empty;
                m_AutoSendCheckboxWrapper.checkbox.Checked = false;
            }

            if (action is HotkeyObjectAction objectAction) {
                EnableObjectToggles();
                SelectObjectToggle(objectAction.ActionTarget);

                if (!m_ObjectInstance || m_ObjectInstance.ID != objectAction.AppearanceType.ID)
                    m_ObjectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType.ID, objectAction.AppearanceType, 0);
            } else {
                DisableObjectToggles();
            }

            m_ActiveActionPanel = actionPanel;
            m_ChangingSelectedAction = false;
        }
        
        private T GetHotkeyActionForPanel<T>(HotkeyActionPanel panel) where T : IHotkeyAction {
            if (!panel)
                return default;

            IHotkeyAction[] actionList = null;
            if (panel.EventModifiers == EventModifiers.Shift)
                actionList = m_ShiftKeys;
            else if (panel.EventModifiers == EventModifiers.Control)
                actionList = m_ControlKeys;
            else
                actionList = m_PlainKeys;

            var action = actionList[panel.KeyCode - KeyCode.F1];
            if (action is T t)
                return t;

            return default; // which is null //
        }

        private T SetHotketActionForPanel<T>(HotkeyActionPanel panel, T action) where T : IHotkeyAction {
            IHotkeyAction[] actionList = null;
            if (panel.EventModifiers == EventModifiers.Shift)
                actionList = m_ShiftKeys;
            else if (panel.EventModifiers == EventModifiers.Control)
                actionList = m_ControlKeys;
            else
                actionList = m_PlainKeys;

            actionList[panel.KeyCode - KeyCode.F1] = action;
            return action;
        }

        private IHotkeyAction GetHotkeyActionForEvent(Event e) {
            IHotkeyAction[] actionList = null;
            if ((e.modifiers & EventModifiers.Shift) != 0)
                actionList = m_ShiftKeys;
            else if ((e.modifiers & EventModifiers.Control) != 0)
                actionList = m_ControlKeys;
            else
                actionList = m_PlainKeys;

            return actionList[e.keyCode - KeyCode.F1];
        }

        private void DisableObjectToggles() {
            if (m_UseOnYourselfToggle.toggle.group != null)
                m_UseOnYourselfToggle.toggle.group.allowSwitchOff = true;

            m_UseOnYourselfToggle.toggle.isOn = false;
            m_UseOnTargetToggle.toggle.isOn = false;
            m_UseWithCrosshairsToggle.toggle.isOn = false;

            m_UseOnYourselfToggle.DisableComponent();
            m_UseOnTargetToggle.DisableComponent();
            m_UseWithCrosshairsToggle.DisableComponent();
        }

        private void EnableObjectToggles() {
            if (m_UseOnYourselfToggle.toggle.group != null)
                m_UseOnYourselfToggle.toggle.group.allowSwitchOff = false;

            m_UseOnYourselfToggle.EnableComponent();
            m_UseOnTargetToggle.EnableComponent();
            m_UseWithCrosshairsToggle.EnableComponent();
        }

        private void SelectObjectToggle(UseActionTarget actionTarget) {
            bool isSelf = actionTarget == UseActionTarget.Self;
            bool isTarget = actionTarget == UseActionTarget.Target;

            m_UseOnYourselfToggle.toggle.isOn = isSelf;
            m_UseOnTargetToggle.toggle.isOn = isTarget;
            m_UseWithCrosshairsToggle.toggle.isOn = !isSelf && !isTarget;
        }
    }
}
