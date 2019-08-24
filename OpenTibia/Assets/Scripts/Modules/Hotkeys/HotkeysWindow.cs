using OpenTibiaUnity.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Hotkeys
{
    public class HotkeysWindow : Core.Components.Base.Window
    {
        private static RenderTexture s_RenderTexture;

        public const string TextUseOnYourself = "<color=#AFFEAF>{0}: (use object on yourself)</color>";
        public const string TextUseOnTarget = "<color=#FEAFAF>{0}:(use object on target)</color>";
        public const string TextUseWithCrosshairs = "<color=#C37A7A>{0}: (use object with crosshairs)</color>";

        public static Color NormalColor = new Color(0, 0, 0, 0);
        public static Color HighlightColor = Core.Colors.ColorFromRGB(0x585858);

        [SerializeField] private OTU_ScrollRect _hotkeysScrollRect = null;
        [SerializeField] private ToggleGroup _hotkeyActionsToggleGroup = null;

        [SerializeField] private TMPro.TMP_InputField _hotkeyTextInputField = null;
        [SerializeField] private Core.Components.CheckboxWrapper _autoSendCheckboxWrapper = null;

        [SerializeField] private RawImage _objectImage = null;
        [SerializeField] private Button _selectObjectButton = null;
        [SerializeField] private Button _blearObjectButton = null;

        [SerializeField] private Core.Components.ToggleWrapper _useOnYourselfToggle = null;
        [SerializeField] private Core.Components.ToggleWrapper _useOnTargetToggle = null;
        [SerializeField] private Core.Components.ToggleWrapper _useWithCrosshairsToggle = null;

        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;

        private IHotkeyAction[] _plainKeys;
        private IHotkeyAction[] _shiftKeys;
        private IHotkeyAction[] _bontrolKeys;

        private HotkeyActionPanel _activeActionPanel = null;
        private Core.Appearances.ObjectInstance _objectInstance = null;
        private bool _bhangingSelectedAction = false;

        protected override void Start() {
            base.Start();

            _plainKeys = new IHotkeyAction[12];
            _shiftKeys = new IHotkeyAction[12];
            _bontrolKeys = new IHotkeyAction[12];

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utils.EventImplPriority.UpperMedium, OnKeyDown);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utils.EventImplPriority.UpperMedium, OnKeyUp);

            // setup events
            _hotkeyTextInputField.onValueChanged.AddListener(OnHotkeyTextInputValueChanged);
            _autoSendCheckboxWrapper.onValueChanged.AddListener(OnAutoSendValueChanged);
            _selectObjectButton.onClick.AddListener(OnSelectObjectButtonClick);
            _blearObjectButton.onClick.AddListener(OnClearObjectButtonClick);
            _useOnYourselfToggle.toggle.onValueChanged.AddListener(OnUseOnYourselfValueChanged);
            _useOnTargetToggle.toggle.onValueChanged.AddListener(OnUseOnTargetValueChanged);
            _useWithCrosshairsToggle.toggle.onValueChanged.AddListener(OnUseWithCrosshairsValueChanged);
            _oKButton.onClick.AddListener(OnOKButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);

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

                var hotkeysActionPanel = Instantiate(ModulesManager.Instance.HotkeysActionPanelPrefab, _hotkeysScrollRect.content);
                hotkeysActionPanel.KeyCode = keyCode;
                hotkeysActionPanel.EventModifiers = eventModifiers;
                hotkeysActionPanel.BaseText = baseText;
                hotkeysActionPanel.textComponent.text = hotkeysActionPanel.BaseText + ":";
                hotkeysActionPanel.toggleComponent.group = _hotkeyActionsToggleGroup;
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

            if (!_activeActionPanel) {
                if (_objectImage.enabled)
                    _objectImage.enabled = false;
                return;
            }

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null || !objectAction.AppearanceType) {
                if (_objectImage.enabled)
                    _objectImage.enabled = false;
                return;
            }
            
            if (s_RenderTexture == null) {
                s_RenderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                s_RenderTexture.filterMode = FilterMode.Point;

                _objectImage.texture = s_RenderTexture;
            } else {
                s_RenderTexture.Release();
            }

            RenderTexture.active = s_RenderTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            var zoom = new Vector2(Screen.width / (float)s_RenderTexture.width, Screen.height / (float)s_RenderTexture.height);
            _objectInstance.DrawTo(new Vector2(0, 0), zoom, 0, 0, 0);

            RenderTexture.active = null;

            if (!_objectImage.enabled)
                _objectImage.enabled = true;
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
            if (_bhangingSelectedAction)
                return;

            if (text.Length == 0)
                _activeActionPanel.textComponent.text = _activeActionPanel.BaseText + ":";
            else
                _activeActionPanel.textComponent.text = string.Format("{0}: {1}", _activeActionPanel.BaseText, Core.StringHelper.RichTextSpecialChars(text));

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(_activeActionPanel);
            if (textAction == null) {
                textAction = new HotkeyTextAction(text, _autoSendCheckboxWrapper.checkbox.Checked);
                SetHotketActionForPanel(_activeActionPanel, textAction);
            } else {
                textAction.Text = text;
            }
        }

        private void OnAutoSendValueChanged(bool value) {
            if (_bhangingSelectedAction)
                return;

            if (value)
                _activeActionPanel.textComponent.color = Core.Colors.ColorFromRGB(0xFEFEFE);
            else
                _activeActionPanel.textComponent.color = Core.Colors.ColorFromRGB(0xC0C0C0);

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(_activeActionPanel);
            if (textAction == null) {
                textAction = new HotkeyTextAction(_hotkeyTextInputField.text, value);
                SetHotketActionForPanel(_activeActionPanel, textAction);
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
            Close();
        }

        private void OnClearObjectButtonClick() {
            // should stop drawing //
            SetHotketActionForPanel<IHotkeyAction>(_activeActionPanel, null);

            DisableObjectToggles();
        }

        private void OnUseOnYourselfValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Self;
            _activeActionPanel.textComponent.text = string.Format(TextUseOnYourself, _activeActionPanel.BaseText);
        }

        private void OnUseOnTargetValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Target;
            _activeActionPanel.textComponent.text = string.Format(TextUseOnTarget, _activeActionPanel.BaseText);
        }

        private void OnUseWithCrosshairsValueChanged(bool value) {
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.CrossHair;
            _activeActionPanel.textComponent.text = string.Format(TextUseWithCrosshairs, _activeActionPanel.BaseText);
        }

        private void OnOKButtonClick() {
            // save //
            Close();
        }

        private void OnCancelButtonClick() {
            Close();
        }

        private void OnObjectMultiUseHandlerUse(Vector3Int _, Core.Appearances.ObjectInstance @object, int __) {
            Core.Game.ObjectMultiUseHandler.onUse = null;
            if (!@object) {
                Open();
                return;
            }

            var objectId = @object.Id;
            if (objectId < 100)
                return;

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null) {
                objectAction = new HotkeyObjectAction(@object.Type, UseActionTarget.CrossHair);
                SetHotketActionForPanel(_activeActionPanel, objectAction);

                EnableObjectToggles();

                _activeActionPanel.textComponent.text = string.Format(TextUseWithCrosshairs, _activeActionPanel.BaseText);
                _useWithCrosshairsToggle.toggle.isOn = true;

                _hotkeyTextInputField.interactable = false;
                _autoSendCheckboxWrapper.DisableComponent();
            } else {
                objectAction.AppearanceType = @object.Type;
            }

            if (!_objectInstance || _objectInstance.Id != objectAction.AppearanceType._id)
                _objectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType._id, objectAction.AppearanceType, 0);

            Open();
        }

        private void OnHotkeyActionToggleValueChanged(HotkeyActionPanel actionPanel, bool value) {
            if (!value)
                return;

            _bhangingSelectedAction = true;
            
            var action = GetHotkeyActionForPanel<IHotkeyAction>(actionPanel);

            bool isObjectAction = action is HotkeyObjectAction;
            if (isObjectAction)
                _autoSendCheckboxWrapper.DisableComponent();
            else
                _autoSendCheckboxWrapper.EnableComponent();

            _hotkeyTextInputField.interactable = !isObjectAction;
            _objectImage.enabled = isObjectAction;

            if (action is HotkeyTextAction textAction) {
                _hotkeyTextInputField.text = textAction.Text;
                _autoSendCheckboxWrapper.checkbox.Checked = textAction.AutoSend;
            } else {
                _hotkeyTextInputField.text = string.Empty;
                _autoSendCheckboxWrapper.checkbox.Checked = false;
            }

            if (action is HotkeyObjectAction objectAction) {
                EnableObjectToggles();
                SelectObjectToggle(objectAction.ActionTarget);

                if (!_objectInstance || _objectInstance.Id != objectAction.AppearanceType._id)
                    _objectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType._id, objectAction.AppearanceType, 0);
            } else {
                DisableObjectToggles();
            }

            _activeActionPanel = actionPanel;
            _bhangingSelectedAction = false;
        }
        
        private T GetHotkeyActionForPanel<T>(HotkeyActionPanel panel) where T : IHotkeyAction {
            if (!panel)
                return default;

            IHotkeyAction[] actionList = null;
            if (panel.EventModifiers == EventModifiers.Shift)
                actionList = _shiftKeys;
            else if (panel.EventModifiers == EventModifiers.Control)
                actionList = _bontrolKeys;
            else
                actionList = _plainKeys;

            var action = actionList[panel.KeyCode - KeyCode.F1];
            if (action is T t)
                return t;

            return default; // which is null //
        }

        private T SetHotketActionForPanel<T>(HotkeyActionPanel panel, T action) where T : IHotkeyAction {
            IHotkeyAction[] actionList = null;
            if (panel.EventModifiers == EventModifiers.Shift)
                actionList = _shiftKeys;
            else if (panel.EventModifiers == EventModifiers.Control)
                actionList = _bontrolKeys;
            else
                actionList = _plainKeys;

            actionList[panel.KeyCode - KeyCode.F1] = action;
            return action;
        }

        private IHotkeyAction GetHotkeyActionForEvent(Event e) {
            IHotkeyAction[] actionList = null;
            if ((e.modifiers & EventModifiers.Shift) != 0)
                actionList = _shiftKeys;
            else if ((e.modifiers & EventModifiers.Control) != 0)
                actionList = _bontrolKeys;
            else
                actionList = _plainKeys;

            return actionList[e.keyCode - KeyCode.F1];
        }

        private void DisableObjectToggles() {
            if (_useOnYourselfToggle.toggle.group != null)
                _useOnYourselfToggle.toggle.group.allowSwitchOff = true;

            _useOnYourselfToggle.toggle.isOn = false;
            _useOnTargetToggle.toggle.isOn = false;
            _useWithCrosshairsToggle.toggle.isOn = false;

            _useOnYourselfToggle.DisableComponent();
            _useOnTargetToggle.DisableComponent();
            _useWithCrosshairsToggle.DisableComponent();
        }

        private void EnableObjectToggles() {
            if (_useOnYourselfToggle.toggle.group != null)
                _useOnYourselfToggle.toggle.group.allowSwitchOff = false;

            _useOnYourselfToggle.EnableComponent();
            _useOnTargetToggle.EnableComponent();
            _useWithCrosshairsToggle.EnableComponent();
        }

        private void SelectObjectToggle(UseActionTarget actionTarget) {
            bool isSelf = actionTarget == UseActionTarget.Self;
            bool isTarget = actionTarget == UseActionTarget.Target;

            _useOnYourselfToggle.toggle.isOn = isSelf;
            _useOnTargetToggle.toggle.isOn = isTarget;
            _useWithCrosshairsToggle.toggle.isOn = !isSelf && !isTarget;
        }
    }
}
