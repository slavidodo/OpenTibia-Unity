using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Hotkeys
{
    public class HotkeysWindow : Core.Components.Base.Window {
        private static EventModifiers[] AllowedEventModifiers = {
            EventModifiers.None,
            EventModifiers.Shift,
            EventModifiers.Control,
        };

        private const KeyCode StartKeyCode = KeyCode.F1;
        private const KeyCode EndKeyCode = KeyCode.F12;

        private static RenderTexture s_renderTexture;

        public const string TextUseOnYourself = "<color=#AFFEAF>{0}: (use object on yourself)</color>";
        public const string TextUseOnTarget = "<color=#FEAFAF>{0}: (use object on target)</color>";
        public const string TextUseWithCrosshairs = "<color=#C37A7A>{0}: (use object with crosshairs)</color>";
        public const string TextUseAuto= "<color=#AFAFFE>{0}: (use object)</color>";

        public static Color NormalColor = new Color(0, 0, 0, 0);
        public static Color HighlightColor = Core.Colors.ColorFromRGB(0x585858);

        [SerializeField] private OTU_ScrollRect _hotkeysScrollRect = null;
        [SerializeField] private ToggleGroup _hotkeyActionsToggleGroup = null;

        [SerializeField] private TMPro.TMP_InputField _hotkeyTextInputField = null;
        [SerializeField] private CheckboxWrapper _autoSendCheckboxWrapper = null;

        [SerializeField] private RawImage _objectImage = null;
        [SerializeField] private Button _selectObjectButton = null;
        [SerializeField] private Button _blearObjectButton = null;

        [SerializeField] private ToggleWrapper _useOnYourselfToggle = null;
        [SerializeField] private ToggleWrapper _useOnTargetToggle = null;
        [SerializeField] private ToggleWrapper _useWithCrosshairsToggle = null;

        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;

        private Dictionary<EventModifiers, HotkeyAction[]> _lists;

        private HotkeyActionPanel _activeActionPanel = null;
        private Core.Appearances.ObjectInstance _objectInstance = null;
        private bool _changingSelectedAction = false;

        protected override void Awake() {
            base.Awake();

            _lists = new Dictionary<EventModifiers, HotkeyAction[]>();
            foreach (var eventModifiers in AllowedEventModifiers)
                _lists.Add(eventModifiers, new HotkeyAction[EndKeyCode - StartKeyCode + 1]);

            OpenTibiaUnity.GameManager.onLoadedGameAssets.AddListener(OnLoadedGameAssets);

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
        }

        protected override void Start() {
            base.Start();

            // initialize action panels
            foreach (var eventModifiers in AllowedEventModifiers) {
                for (int i = 0; i < (EndKeyCode - StartKeyCode + 1); i++) {
                    var keyCode = StartKeyCode + i;
                    string baseText = string.Empty;
                    if (eventModifiers != EventModifiers.None)
                        baseText += eventModifiers.ToString() + "+";

                    baseText += keyCode.ToString();

                    var hotkeysActionPanel = Instantiate(ModulesManager.Instance.HotkeysActionPanelPrefab, _hotkeysScrollRect.content);
                    hotkeysActionPanel.gameObject.name = ModulesManager.Instance.HotkeysActionPanelPrefab.name + "(" + baseText + ")";
                    hotkeysActionPanel.KeyCode = keyCode;
                    hotkeysActionPanel.EventModifiers = eventModifiers;
                    hotkeysActionPanel.BaseText = baseText;
                    hotkeysActionPanel.normalColor = NormalColor;
                    hotkeysActionPanel.highlightColor = HighlightColor;

                    hotkeysActionPanel.textComponent.text = hotkeysActionPanel.BaseText + ":";
                    hotkeysActionPanel.toggleComponent.onValueChanged.AddListener((value) =>
                        OnHotkeyActionToggleValueChanged(hotkeysActionPanel, value));

                    hotkeysActionPanel.toggleComponent.group = _hotkeyActionsToggleGroup;

                    UpdateHotkeyPanelWithAction(hotkeysActionPanel, _lists[eventModifiers][i]);
                }
            }

            OpenTibiaUnity.GameManager.GetModule<GameWindow.GameMapContainer>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        protected void OnGUI() {
            var e = Event.current;
            if (e.type != EventType.Repaint || !Visible)
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

            if (s_renderTexture == null) {
                s_renderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                s_renderTexture.filterMode = FilterMode.Bilinear;
                s_renderTexture.Create();
                _objectImage.texture = s_renderTexture;
            }

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(s_renderTexture);
            commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

            var zoom = new Vector2(Screen.width / (float)s_renderTexture.width, Screen.height / (float)s_renderTexture.height);
            commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

            _objectInstance.Draw(commandBuffer, new Vector2Int(0, 0), 0, 0, 0);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();

            if (!_objectImage.enabled)
                _objectImage.enabled = true;
        }

        protected override void OnEnable() {
            base.OnEnable();

            _hotkeyTextInputField.ActivateInputField();
            _hotkeyTextInputField.MoveTextEnd(false);

            // select first element
            if (_hotkeysScrollRect.content.childCount > 0) {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectPanelByIndex(0));
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var gameMapContainer = OpenTibiaUnity.GameManager?.GetModule<GameWindow.GameMapContainer>();
            if (gameMapContainer)
                gameMapContainer.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
        }

        private void OnInvalidateTRS() {
            if (!!_objectInstance)
                _objectInstance.InvalidateTRS();
        }

        private void OnLoadedGameAssets() {
            var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
            string jsonData = OpenTibiaUnity.OptionStorage.LoadCustomOptions(clientVersion + "_" + Core.Options.OptionStorage.LegacyHotkeysFileName);
            if (jsonData != null && jsonData.Length > 0) {
                try {
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(jsonData);
                    if (jObject != null)
                        Unserialize(jObject);
                } catch (System.Exception) { }
            }
        }

        private void OnKeyDown(Event e, bool repeat) {
            if (repeat)
                OnKeyUp(e, false);
        }

        private void OnKeyUp(Event e, bool _) {
            var action = GetHotkeyActionForEvent<HotkeyAction>(e);
            if (action != null) {
                e.Use();
                action.Apply();
                return;
            }

            if (e.alt || e.shift || e.control || !InputHandler.IsHighlighted(this))
                return;

            switch (e.keyCode) {
                case KeyCode.DownArrow:
                    e.Use();
                    SelectNextHotkeyPanel();
                    break;
                case KeyCode.UpArrow:
                    e.Use();
                    SelectPrevHotkeyPanel();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    e.Use();
                    OnOKButtonClick();
                    break;
                case KeyCode.Escape:
                    e.Use();
                    OnCancelButtonClick();
                    break;
            }
        }

        private void OnHotkeyTextInputValueChanged(string text) {
            if (_changingVisibility || _changingSelectedAction)
                return;

            UpdateHotkeyPanelWithText(_activeActionPanel, text);

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(_activeActionPanel);
            if (text.Length == 0) {
                if (textAction != null)
                    SetHotketActionForPanel<HotkeyAction>(_activeActionPanel, null);
            } else if (textAction == null) {
                textAction = new HotkeyTextAction(text, _autoSendCheckboxWrapper.checkbox.Checked);
                SetHotketActionForPanel(_activeActionPanel, textAction);
            } else {
                textAction.Text = text;
            }
        }

        private void OnAutoSendValueChanged(bool value) {
            if (_changingSelectedAction)
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
            SetHotketActionForPanel<HotkeyAction>(_activeActionPanel, null);

            DisableObjectToggles();
            UpdateHotkeyPanelWithAction(_activeActionPanel, null);
        }

        private void OnUseOnYourselfValueChanged(bool value) {
            if (_changingVisibility || _changingSelectedAction)
                return;
            
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Self;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnUseOnTargetValueChanged(bool value) {
            if (_changingVisibility || _changingSelectedAction)
                return;

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Target;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnUseWithCrosshairsValueChanged(bool value) {
            if (_changingVisibility || _changingSelectedAction)
                return;
            
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.CrossHair;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnOKButtonClick() {
            Close();

            // todo, only if anything has changed
            SaveHotkeys();
        }

        private void OnCancelButtonClick() {
            Close();

            // reset hotkeys
            foreach (Transform child in _hotkeysScrollRect.content) {
                var panel = child.GetComponent<HotkeyActionPanel>();
                var action = GetHotkeyActionForPanel<HotkeyAction>(panel);
                UpdateHotkeyPanelWithAction(panel, action);
            }
        }

        private void OnObjectMultiUseHandlerUse(Vector3Int _, Core.Appearances.ObjectInstance @object, int __) {
            _activeActionPanel.toggleComponent.isOn = true;

            Core.Game.ObjectMultiUseHandler.onUse = null;
            if (!@object) {
                Open();
                return;
            }

            var objectId = @object.Id;
            if (objectId < 100)
                return;

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            bool firstAdd = objectAction == null;
            if (firstAdd) {
                objectAction = new HotkeyObjectAction(@object.Type, UseActionTarget.CrossHair);
                SetHotketActionForPanel(_activeActionPanel, objectAction);

                _useWithCrosshairsToggle.toggle.isOn = true;
                _hotkeyTextInputField.interactable = false;
                _autoSendCheckboxWrapper.DisableComponent();
            } else {
                objectAction.AppearanceType = @object.Type;
                if (@object.Type.IsMultiUse && objectAction.ActionTarget == UseActionTarget.Auto)
                    objectAction.ActionTarget = UseActionTarget.CrossHair;
                else if (!@object.Type.IsMultiUse && objectAction.ActionTarget != UseActionTarget.Auto)
                    objectAction.ActionTarget = UseActionTarget.Auto;
            }

            if (objectAction.ActionTarget != UseActionTarget.Auto)
                EnableObjectToggles();
            else
                DisableObjectToggles();

            UpdateHotkeyPanelWithAction(_activeActionPanel, objectAction);
            if (!_objectInstance || _objectInstance.Id != objectAction.AppearanceType.Id)
                _objectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType.Id, objectAction.AppearanceType, 0);

            Open();
        }

        private void OnHotkeyActionToggleValueChanged(HotkeyActionPanel actionPanel, bool value) {
            if (_changingVisibility || !value)
                return;

            _changingSelectedAction = true;

            var action = GetHotkeyActionForPanel<HotkeyAction>(actionPanel);

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
                if (objectAction.ActionTarget != UseActionTarget.Auto) {
                    EnableObjectToggles();
                    SelectObjectToggle(objectAction.ActionTarget);
                } else {
                    DisableObjectToggles();
                }

                if (!_objectInstance || _objectInstance.Id != objectAction.AppearanceType.Id)
                    _objectInstance = new Core.Appearances.ObjectInstance(objectAction.AppearanceType.Id, objectAction.AppearanceType, 0);
            } else {

                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                    if (action == null || action is HotkeyTextAction) {
                        _hotkeyTextInputField.ActivateInputField();
                        _hotkeyTextInputField.MoveTextEnd(false);
                    }
                });

                DisableObjectToggles();

                if (action == null || action is HotkeyTextAction) {
                    OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                        _hotkeyTextInputField.ActivateInputField();
                        _hotkeyTextInputField.MoveTextEnd(false);
                    });
                }
            }

            _activeActionPanel = actionPanel;
            _changingSelectedAction = false;
        }

        protected void SelectNextHotkeyPanel() {
            int index = -1;
            if (!_activeActionPanel)
                index = 0;
            else if (_hotkeysScrollRect.content.childCount > 1)
                index = Mathf.Min(_activeActionPanel.transform.GetSiblingIndex() + 1, _hotkeysScrollRect.content.childCount - 1);

            if (index != -1)
                SelectPanelByIndex(index);
        }

        protected void SelectPrevHotkeyPanel() {
            int index = -1;
            if (!_activeActionPanel)
                index = 0;
            else if (_hotkeysScrollRect.content.childCount > 1)
                index = Mathf.Max(_activeActionPanel.transform.GetSiblingIndex() - 1, 0);

            if (index != -1)
                SelectPanelByIndex(index);
        }

        private bool SelectPanelByIndex(int index) {
            if (index >= _hotkeysScrollRect.content.childCount)
                return false;

            var child = _hotkeysScrollRect.content.GetChild(index);
            var hotkeyPanel = child.GetComponent<HotkeyActionPanel>();
            hotkeyPanel.Select();
            return true;
        }

        private T GetHotkeyActionForPanel<T>(HotkeyActionPanel panel) where T : HotkeyAction {
            HotkeyAction[] actionList;
            if (!!panel && _lists.TryGetValue(panel.EventModifiers, out actionList)) {
                if (actionList[panel.KeyCode - StartKeyCode] is T action)
                    return action;
            }

            return default;
        }

        private T SetHotketActionForPanel<T>(HotkeyActionPanel panel, T action) where T : HotkeyAction {
            HotkeyAction[] actionList;
            if (!!panel && _lists.TryGetValue(panel.EventModifiers, out actionList))
                actionList[panel.KeyCode - StartKeyCode] = action;

            return action;
        }
        
        private T GetHotkeyActionForEvent<T>(Event e) where T : HotkeyAction {
            if (e.keyCode < StartKeyCode || e.keyCode > EndKeyCode)
                return default;

            var modifiers = e.modifiers;
            if ((modifiers & EventModifiers.FunctionKey) != 0)
                modifiers &= ~EventModifiers.FunctionKey;

            HotkeyAction[] actionList;
            if (_lists.TryGetValue(modifiers, out actionList)) {
                if (actionList[e.keyCode - StartKeyCode] is T action)
                    return action;
            }

            return default;
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

        private void UpdateHotkeyPanelWithAction(HotkeyActionPanel panel, HotkeyAction action) {
            if (action is HotkeyTextAction textAction) {
                UpdateHotkeyPanelWithText(panel, textAction.Text);
                if (textAction.AutoSend)
                    panel.textComponent.color = Core.Colors.ColorFromRGB(0xFEFEFE);
                else
                    panel.textComponent.color = Core.Colors.ColorFromRGB(0xC0C0C0);
            } else if (action is HotkeyObjectAction objectAction) {
                UpdateHotkeyPanelWithUseTarget(panel, objectAction.ActionTarget);
                panel.textComponent.color = Core.Colors.ColorFromRGB(0xC0C0C0);
            } else {
                UpdateHotkeyPanelWithText(panel, null);
                panel.textComponent.color = Core.Colors.ColorFromRGB(0xC0C0C0);
            }
        }

        private void UpdateHotkeyPanelWithText(HotkeyActionPanel panel, string text) {
            if (string.IsNullOrEmpty(text))
                panel.textComponent.text = panel.BaseText + ":";
            else
                panel.textComponent.text = string.Format("{0}: {1}", panel.BaseText, Core.StringHelper.RichTextSpecialChars(text));
        }

        private void UpdateHotkeyPanelWithUseTarget(HotkeyActionPanel panel, UseActionTarget useTarget) {
            switch (useTarget) {
                case UseActionTarget.Self:
                    panel.textComponent.text = string.Format(TextUseOnYourself, panel.BaseText);
                    break;
                case UseActionTarget.Target:
                    panel.textComponent.text = string.Format(TextUseOnTarget, panel.BaseText);
                    break;
                case UseActionTarget.CrossHair:
                    panel.textComponent.text = string.Format(TextUseWithCrosshairs, panel.BaseText);
                    break;
                case UseActionTarget.Auto:
                    panel.textComponent.text = string.Format(TextUseAuto, panel.BaseText);
                    break;
            }
        }

        private void SaveHotkeys() {
            var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
            OpenTibiaUnity.OptionStorage.SaveCustomOptions(clientVersion + "_" + Core.Options.OptionStorage.LegacyHotkeysFileName, Serialize().ToString());
        }

        public JObject Serialize() {
            JObject jobject = new JObject();
            foreach (var list in _lists) {
                JArray listArray = null;
                for (int i = 0; i < list.Value.Length; i++) {
                    var action = list.Value[i];
                    if (action != null) {
                        if (listArray == null)
                            listArray = new JArray();

                        var actionObject = action.Serialize();
                        actionObject.Add("keyCode", (int)StartKeyCode + i);
                        listArray.Add(actionObject);
                    }
                }

                if (listArray != null) {
                    jobject.Add(list.Key.ToString().ToLower(), listArray);
                }
            }

            return jobject;
        }

        public void Unserialize(JObject data) {
            foreach (var listProp in data.Properties()) {
                if (listProp.Value.Type != JTokenType.Array)
                    continue;

                string name = listProp.Name;

                HotkeyAction[] actionList = null;
                foreach (var list in _lists) {
                    if (list.Key.ToString().ToLower() == name) {
                        actionList = list.Value;
                        break;
                    }
                }

                if (actionList == null)
                    continue;

                JArray listArray = listProp.Value as JArray;
                foreach (var actionObject in listArray) {
                    if (actionObject.Type != JTokenType.Object)
                        continue;

                    int rawKeyCode;
                    if (actionObject["keyCode"] == null || !int.TryParse(actionObject["keyCode"].ToString(), out rawKeyCode))
                        continue;
                    
                    if (rawKeyCode < (int)StartKeyCode || rawKeyCode > (int)EndKeyCode)
                        continue;
                    
                    try {
                        var action = HotkeyAction.Unserialize(actionObject as JObject);
                        if (action == null)
                            continue;
                        
                        actionList[rawKeyCode - (int)StartKeyCode] = action;
                    } catch (System.Exception) {}
                }
            }
        }
    }
}
