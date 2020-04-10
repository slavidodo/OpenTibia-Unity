using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTibiaUnity.Core.Input;
using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;
using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Options
{
    public class LegacyHotkeyOptionsWidget : UI.Legacy.PopUpBase
    {
        // constants
        private static EventModifiers[] AllowedEventModifiers = {
            EventModifiers.None,
            EventModifiers.Shift,
            EventModifiers.Control,
        };

        private const KeyCode StartKeyCode = KeyCode.F1;
        private const KeyCode EndKeyCode = KeyCode.F12;

        // serialized fields
        [SerializeField]
        private UI.Legacy.ScrollRect _hotkeyScrollRect = null;
        [SerializeField]
        private UnityUI.ToggleGroup _toggleGroup = null;
        [SerializeField]
        private TMPro.TMP_InputField _inputField = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _autoSendCheckbox = null;
        [SerializeField]
        private UnityUI.RawImage _objectImage = null;
        [SerializeField]
        private UI.Legacy.Button _selectObjectButton = null;
        [SerializeField]
        private UI.Legacy.Button _clearObjectButton = null;
        [SerializeField]
        private UI.Legacy.Toggle _useOnYourselfToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _useOnTargetToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _useWithCrosshairsToggle = null;

        // fields
        private RenderTexture _renderTexture = null;
        private HotkeyActionPanel _activeActionPanel = null;
        private Dictionary<EventModifiers, HotkeyAction[]> _lists;
        private Core.Appearances.ObjectInstance _objectInstance = null;
        private bool _changingSelectedAction = false;

        protected override void Awake() {
            base.Awake();

            _lists = new Dictionary<EventModifiers, HotkeyAction[]>();
            foreach (var eventModifiers in AllowedEventModifiers)
                _lists.Add(eventModifiers, new HotkeyAction[EndKeyCode - StartKeyCode + 1]);

            OpenTibiaUnity.GameManager.onLoadedGameAssets.AddListener(OnLoadedGameAssets);

            // setup ui
            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOKButtonClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelButtonClick);

            // setup events
            _inputField.onValueChanged.AddListener(OnHotkeyTextInputValueChanged);
            _autoSendCheckbox.onValueChanged.AddListener(OnAutoSendValueChanged);
            _selectObjectButton.onClick.AddListener(OnSelectObjectButtonClick);
            _clearObjectButton.onClick.AddListener(OnClearObjectButtonClick);
            _useOnYourselfToggle.onValueChanged.AddListener(OnUseOnYourselfValueChanged);
            _useOnTargetToggle.onValueChanged.AddListener(OnUseOnTargetValueChanged);
            _useWithCrosshairsToggle.onValueChanged.AddListener(OnUseWithCrosshairsValueChanged);
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

                    var hotkeysActionPanel = Instantiate(ModulesManager.Instance.HotkeysActionPanelPrefab, _hotkeyScrollRect.content);
                    hotkeysActionPanel.gameObject.name = ModulesManager.Instance.HotkeysActionPanelPrefab.name + "(" + baseText + ")";
                    hotkeysActionPanel.KeyCode = keyCode;
                    hotkeysActionPanel.EventModifiers = eventModifiers;
                    hotkeysActionPanel.BaseText = baseText;
                    hotkeysActionPanel.text = $"{baseText}:";
                    hotkeysActionPanel.toggle.onValueChanged.AddListener((value) =>
                        OnHotkeyActionToggleValueChanged(hotkeysActionPanel, value));

                    hotkeysActionPanel.toggle.group = _toggleGroup;

                    UpdateHotkeyPanelWithAction(hotkeysActionPanel, _lists[eventModifiers][i]);
                }
            }

            OpenTibiaUnity.GameManager.GetModule<UI.Legacy.WorldMapWidget>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        protected override void OnEnable() {
            base.OnEnable();

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null)
                inputHandler.AddKeyUpListener(Core.Utils.EventImplPriority.UpperMedium, OnKeyUp);

            _inputField.ActivateInputField();
            _inputField.MoveTextEnd(false);

            // select first element
            if (_hotkeyScrollRect.content.childCount > 0)
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectPanelByIndex(0));
        }

        protected override void OnDisable() {
            base.OnDisable();

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null)
                inputHandler.RemoveKeyUpListener(OnKeyUp);
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var worldMapWidget = OpenTibiaUnity.GameManager?.GetModule<UI.Legacy.WorldMapWidget>();
            if (worldMapWidget)
                worldMapWidget.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
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

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Bilinear;
                _renderTexture.Create();
                _objectImage.texture = _renderTexture;
            }

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(_renderTexture);
            commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

            var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
            commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

            _objectInstance.Draw(commandBuffer, new Vector2Int(0, 0), 0, 0, 0);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();

            if (!_objectImage.enabled)
                _objectImage.enabled = true;
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);
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

        private void OnHotkeyTextInputValueChanged(string text) {
            if (ChangingVisibility || _changingSelectedAction)
                return;

            UpdateHotkeyPanelWithText(_activeActionPanel, text);

            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(_activeActionPanel);
            if (text.Length == 0) {
                if (textAction != null)
                    SetHotketActionForPanel<HotkeyAction>(_activeActionPanel, null);
            } else if (textAction == null) {
                textAction = new HotkeyTextAction(text, _autoSendCheckbox.checkbox.Checked);
                SetHotketActionForPanel(_activeActionPanel, textAction);
            } else {
                textAction.Text = text;
            }
        }

        private void OnAutoSendValueChanged(bool value) {
            if (_changingSelectedAction)
                return;

            _activeActionPanel.textColor = value ? Core.Colors.White : Core.Colors.Default;
            var textAction = GetHotkeyActionForPanel<HotkeyTextAction>(_activeActionPanel);
            if (textAction == null) {
                textAction = new HotkeyTextAction(_inputField.text, value);
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
        }

        private void OnClearObjectButtonClick() {
            SetHotketActionForPanel<HotkeyAction>(_activeActionPanel, null);

            DisableObjectToggles();
            UpdateHotkeyPanelWithAction(_activeActionPanel, null);
        }

        private void OnUseOnYourselfValueChanged(bool value) {
            if (ChangingVisibility || _changingSelectedAction)
                return;
            
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Self;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnUseOnTargetValueChanged(bool value) {
            if (ChangingVisibility || _changingSelectedAction)
                return;

            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.Target;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnUseWithCrosshairsValueChanged(bool value) {
            if (ChangingVisibility || _changingSelectedAction)
                return;
            
            var objectAction = GetHotkeyActionForPanel<HotkeyObjectAction>(_activeActionPanel);
            if (objectAction == null)
                return;

            objectAction.ActionTarget = UseActionTarget.CrossHair;
            UpdateHotkeyPanelWithUseTarget(_activeActionPanel, objectAction.ActionTarget);
        }

        private void OnOKButtonClick() {
            // todo, only if anything has changed
            SaveHotkeys();
        }

        private void OnCancelButtonClick() {
            // reset hotkeys
            foreach (Transform child in _hotkeyScrollRect.content) {
                var panel = child.GetComponent<HotkeyActionPanel>();
                var action = GetHotkeyActionForPanel<HotkeyAction>(panel);
                UpdateHotkeyPanelWithAction(panel, action);
            }
        }

        private void OnObjectMultiUseHandlerUse(Vector3Int _, Core.Appearances.ObjectInstance @object, int __) {
            _activeActionPanel.toggle.isOn = true;

            Core.Game.ObjectMultiUseHandler.onUse = null;
            if (!@object) {
                Show();
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

                _useWithCrosshairsToggle.isOn = true;
                _inputField.interactable = false;
                _autoSendCheckbox.DisableComponent();
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

            Show();
        }

        private void OnHotkeyActionToggleValueChanged(HotkeyActionPanel actionPanel, bool value) {
            if (ChangingVisibility || !value)
                return;

            _changingSelectedAction = true;

            var action = GetHotkeyActionForPanel<HotkeyAction>(actionPanel);

            bool isObjectAction = action is HotkeyObjectAction;
            if (isObjectAction)
                _autoSendCheckbox.DisableComponent();
            else
                _autoSendCheckbox.EnableComponent();

            _inputField.interactable = !isObjectAction;
            _objectImage.enabled = isObjectAction;

            if (action is HotkeyTextAction textAction) {
                _inputField.text = textAction.Text;
                _autoSendCheckbox.checkbox.Checked = textAction.AutoSend;
            } else {
                _inputField.text = string.Empty;
                _autoSendCheckbox.checkbox.Checked = false;
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
                        _inputField.ActivateInputField();
                        _inputField.MoveTextEnd(false);
                    }
                });

                DisableObjectToggles();

                if (action == null || action is HotkeyTextAction) {
                    OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                        _inputField.ActivateInputField();
                        _inputField.MoveTextEnd(false);
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
            else if (_hotkeyScrollRect.content.childCount > 1)
                index = Mathf.Min(_activeActionPanel.transform.GetSiblingIndex() + 1, _hotkeyScrollRect.content.childCount - 1);

            if (index != -1)
                SelectPanelByIndex(index);
        }

        protected void SelectPrevHotkeyPanel() {
            int index = -1;
            if (!_activeActionPanel)
                index = 0;
            else if (_hotkeyScrollRect.content.childCount > 1)
                index = Mathf.Max(_activeActionPanel.transform.GetSiblingIndex() - 1, 0);

            if (index != -1)
                SelectPanelByIndex(index);
        }

        private bool SelectPanelByIndex(int index) {
            if (index >= _hotkeyScrollRect.content.childCount)
                return false;

            var child = _hotkeyScrollRect.content.GetChild(index);
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
            if (_useOnYourselfToggle.group != null)
                _useOnYourselfToggle.group.allowSwitchOff = true;

            _useOnYourselfToggle.isOn = false;
            _useOnTargetToggle.isOn = false;
            _useWithCrosshairsToggle.isOn = false;

            _useOnYourselfToggle.Disable();
            _useOnTargetToggle.Disable();
            _useWithCrosshairsToggle.Disable();
        }

        private void EnableObjectToggles() {
            if (_useOnYourselfToggle.group != null)
                _useOnYourselfToggle.group.allowSwitchOff = false;

            _useOnYourselfToggle.Enable();
            _useOnTargetToggle.Enable();
            _useWithCrosshairsToggle.Enable();
        }

        private void SelectObjectToggle(UseActionTarget actionTarget) {
            bool isSelf = actionTarget == UseActionTarget.Self;
            bool isTarget = actionTarget == UseActionTarget.Target;

            _useOnYourselfToggle.isOn = isSelf;
            _useOnTargetToggle.isOn = isTarget;
            _useWithCrosshairsToggle.isOn = !isSelf && !isTarget;
        }

        private void UpdateHotkeyPanelWithAction(HotkeyActionPanel panel, HotkeyAction action) {
            if (action is HotkeyTextAction textAction) {
                UpdateHotkeyPanelWithText(panel, textAction.Text);
                panel.textColor = textAction.AutoSend ? Core.Colors.White : Core.Colors.Default;
            } else if (action is HotkeyObjectAction objectAction) {
                UpdateHotkeyPanelWithUseTarget(panel, objectAction.ActionTarget);
                panel.textColor = Core.Colors.Default;
            } else {
                UpdateHotkeyPanelWithText(panel, null);
                panel.textColor = Core.Colors.Default;
            }
        }

        private void UpdateHotkeyPanelWithText(HotkeyActionPanel panel, string text) {
            if (string.IsNullOrEmpty(text))
                panel.text = $"{panel.BaseText}:";
            else
                panel.text = $"{panel.BaseText}: {Core.StringHelper.RichTextSpecialChars(text)}";
        }

        private void UpdateHotkeyPanelWithUseTarget(HotkeyActionPanel panel, UseActionTarget useTarget) {
            switch (useTarget) {
                case UseActionTarget.Self:
                    panel.text = string.Format(TextResources.HOTKEYS_DLG_USE_OBJECT_YOURSELF, panel.BaseText);
                    break;
                case UseActionTarget.Target:
                    panel.text = string.Format(TextResources.HOTKEYS_DLG_USE_OBJECT_TARGET, panel.BaseText);
                    break;
                case UseActionTarget.CrossHair:
                    panel.text = string.Format(TextResources.HOTKEYS_DLG_USE_OBJECT_CROSSHAIRS, panel.BaseText);
                    break;
                case UseActionTarget.Auto:
                    panel.text = string.Format(TextResources.HOTKEYS_DLG_USE_OBJECT_AUTO, panel.BaseText);
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
