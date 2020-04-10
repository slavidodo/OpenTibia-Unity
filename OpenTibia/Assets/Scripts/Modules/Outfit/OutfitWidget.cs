using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Game;
using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;
using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Outfit
{
    public class OutfitWidget : UI.Legacy.PopUpBase
    {
        [SerializeField]
        private OutfitPalette _colorsPalette = null;
        [SerializeField]
        private UI.Legacy.Toggle _headToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _bodyToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _legsToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _feetToggle = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _informationLabel = null;
        [SerializeField]
        private UI.Legacy.Button _legacyNextOutfitButton = null;
        [SerializeField]
        private UI.Legacy.Button _nextOutfitButton = null;
        [SerializeField]
        private UI.Legacy.Button _prevOutfitButton = null;
        [SerializeField]
        private RectTransform _outfitNamePanel = null;
        [SerializeField]
        private UnityUI.RawImage _outfitImage = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _outfitNameLabel = null;
        [SerializeField]
        private RectTransform _addonsPanel = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _addon1Checkbox = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _addon2Checkbox = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _addon3Checkbox = null;
        [SerializeField]
        private RectTransform _mountPanel = null;
        [SerializeField]
        private UI.Legacy.Button _nextMountButton = null;
        [SerializeField]
        private UI.Legacy.Button _prevMountButton = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _mountNameLabel = null;
        [SerializeField]
        private UnityUI.RawImage _mountImage = null;

        [SerializeField] private float _spacingFactor = 0.8f;

        private List<ProtocolOutfit> _outfits = null;
        private List<ProtocolMount> _mounts = null;
        private AppearanceInstance _currentOutfit = null;
        private AppearanceInstance _currentMount = null;
        private int _currentOutfitIndex = 0;
        private int _currentMountIndex = 0;
        private Direction _currentDirection = Direction.South;
        private bool _updatingOutfit = false;
        private bool _initialized = false;

        // fields
        private RenderTexture _renderTexture = null;

        protected override void Awake() {
            base.Awake();

            // buttons
            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkButtonClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelButtonClick);

            // events
            _colorsPalette.onColorChanged.AddListener(OnColorPaletteColorChanged);
            _headToggle.onValueChanged.AddListener(OnHeadToggleValueChanged);
            _bodyToggle.onValueChanged.AddListener(OnBodyToggleValueChanged);
            _legsToggle.onValueChanged.AddListener(OnLegsToggleValueChanged);
            _feetToggle.onValueChanged.AddListener(OnFeetToggleValueChanged);
            _legacyNextOutfitButton.onClick.AddListener(OnNextOutfitButtonClick);
            _nextOutfitButton.onClick.AddListener(OnNextOutfitButtonClick);
            _prevOutfitButton.onClick.AddListener(OnPrevOutfitButtonClick);
            _addon1Checkbox.onValueChanged.AddListener(OnAddon1CheckboxChange);
            _addon2Checkbox.onValueChanged.AddListener(OnAddon2CheckboxChange);
            _nextMountButton.onClick.AddListener(OnNextMountButtonClick);
            _prevMountButton.onClick.AddListener(OnPrevMountButtonClick);
        }

        protected override void Start() {
            base.Start();

            // this was never introduced and was removed from tibia soon
            _addon3Checkbox.DisableComponent();

            _outfitImage.uvRect = new Rect(0, 0, 0.5f, 1f);
            _mountImage.uvRect = new Rect(0.5f, 0, 0.5f, 1f);

            _initialized = true;
            if (_currentOutfit)
                OnFirstShow();

            OpenTibiaUnity.GameManager.GetModule<UI.Legacy.WorldMapWidget>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint || !Visible)
                return;

            if (!_currentOutfit && !_currentMount)
                return;

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(Constants.FieldSize * 2 * 2, Constants.FieldSize * 2, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Bilinear;
                _renderTexture.Create();

                _outfitImage.texture = _renderTexture;
                _mountImage.texture = _renderTexture;
            }

            using (var commandBuffer = new CommandBuffer()) {
                commandBuffer.SetRenderTarget(_renderTexture);
                commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

                var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
                commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                    OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

                if (!!_currentOutfit) {
                    var screenPosition = new Vector2Int(Constants.FieldSize, Constants.FieldSize);
                    if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts)) {
                        screenPosition.x = (int)(screenPosition.x * _spacingFactor);
                        screenPosition.y = (int)(screenPosition.x * _spacingFactor);
                    }

                    if (_currentOutfit is OutfitInstance)
                        _currentOutfit.Draw(commandBuffer, screenPosition, (int)_currentDirection, 0, 0);
                    else
                        _currentOutfit.Draw(commandBuffer, screenPosition, 0, 0, 0);

                    if (!_outfitImage.enabled)
                        _outfitImage.enabled = true;
                }

                if (!!_currentMount) {
                    float pos = Constants.FieldSize * _spacingFactor;
                    var screenPosition = new Vector2Int((int)pos + 2 * Constants.FieldSize, (int)pos);

                    if (_currentMount is OutfitInstance)
                        _currentMount.Draw(commandBuffer, screenPosition, (int)_currentDirection, 0, 0);
                    else
                        _currentMount.Draw(commandBuffer, screenPosition, 0, 0, 0);

                    if (!_mountImage.enabled)
                        _mountImage.enabled = true;
                }

                Graphics.ExecuteCommandBuffer(commandBuffer);
            }
        }

        protected override void OnEnable() {
            if (!_currentOutfit || !_initialized)
                return;

            OnFirstShow();
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var worldmapWidget = OpenTibiaUnity.GameManager?.GetModule<UI.Legacy.WorldMapWidget>();
            if (worldmapWidget)
                worldmapWidget.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
        }

        protected void OnInvalidateTRS() {
            if (!!_currentOutfit)
                _currentOutfit.InvalidateTRS();

            if (!!_currentMount)
                _currentMount.InvalidateTRS();
        }

        private void OnFirstShow() {
            _headToggle.isOn = true;
            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Head);
        }

        protected override void OnClientVersionChange(int oldVersion, int newVersion) {
            bool hasNewProtocol = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol);
            bool hasAddons = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons);
            bool hasMounts = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts);

            Vector2 minContentSize = Vector2.zero;
            Vector2 outfitImageSize = Vector2.zero;
            float togglesX, togglesWidth;

            if (hasNewProtocol) {
                minContentSize.Set(462, hasMounts ? 268 : 258);
                outfitImageSize.Set(141, 141);
                minContentSize.x = 462;
                togglesX = 153;
                togglesWidth = 57;

                _bodyToggle.text = "Primary";
                _legsToggle.text = "Secondary";
                _feetToggle.text = "Detail";

                _colorsPalette.rectTransform.offsetMin = new Vector2(219, _colorsPalette.rectTransform.offsetMin.y);
                if (hasMounts) {
                    _informationLabel.rectTransform.anchoredPosition = new Vector2(0, -184);
                    _informationLabel.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL_MOUNT;
                    _addonsPanel.anchoredPosition = new Vector2(153, -104);
                } else {
                    _informationLabel.rectTransform.anchoredPosition = new Vector2(153, -104);
                    _informationLabel.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL;
                    _addonsPanel.anchoredPosition = new Vector2(0, -179);
                }
            } else {
                minContentSize.Set(380, 144);
                outfitImageSize.Set(69, 69);
                togglesX = 81;
                togglesWidth = 42;

                _bodyToggle.text = "Body";
                _legsToggle.text = "Legs";
                _feetToggle.text = "Feet";

                _colorsPalette.rectTransform.offsetMin = new Vector3(135, _colorsPalette.rectTransform.offsetMin.y);
                _informationLabel.rectTransform.anchoredPosition = new Vector2(0, -104);
                _informationLabel.text = TextResources.OUTFIT_LABEL_INFO_LEGACY_PROTOCOL;
            }


            var contentLayoutElement = _content.GetComponent<UnityUI.LayoutElement>();
            contentLayoutElement.minWidth = minContentSize.x;
            contentLayoutElement.minHeight = minContentSize.y;

            var imageWrapperTransform = _outfitImage.transform.parent as RectTransform;
            imageWrapperTransform.sizeDelta = outfitImageSize;

            _headToggle.rectTransform.anchoredPosition = new Vector2(togglesX, 0);
            _bodyToggle.rectTransform.anchoredPosition = new Vector2(togglesX, -25);
            _legsToggle.rectTransform.anchoredPosition = new Vector2(togglesX, -50);
            _feetToggle.rectTransform.anchoredPosition = new Vector2(togglesX, -75);
            _headToggle.rectTransform.sizeDelta = new Vector2(togglesWidth, 21);
            _bodyToggle.rectTransform.sizeDelta = new Vector2(togglesWidth, 21);
            _legsToggle.rectTransform.sizeDelta = new Vector2(togglesWidth, 21);
            _feetToggle.rectTransform.sizeDelta = new Vector2(togglesWidth, 21);

            _legacyNextOutfitButton.gameObject.SetActive(!hasNewProtocol);
            _nextOutfitButton.gameObject.SetActive(hasNewProtocol);
            _prevOutfitButton.gameObject.SetActive(hasNewProtocol);
            _outfitNamePanel.gameObject.SetActive(hasNewProtocol);
            _addonsPanel.gameObject.SetActive(hasAddons);
            _mountPanel.gameObject.SetActive(hasMounts);
        }

        private void OnHeadToggleValueChanged(bool value) {
            if (!value)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Head);
            else
                UpdateColorItems(0);
        }

        private void OnBodyToggleValueChanged(bool value) {
            if (!value)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Torso);
            else
                UpdateColorItems(0);
        }

        private void OnLegsToggleValueChanged(bool value) {
            if (!value)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Legs);
            else
                UpdateColorItems(0);
        }

        private void OnFeetToggleValueChanged(bool value) {
            if (!value)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Detail);
            else
                UpdateColorItems(0);
        }

        private void OnOkButtonClick() {
            if (!!OpenTibiaUnity.ProtocolGame) {
                if (_currentOutfit is OutfitInstance outfitInstance)
                    OpenTibiaUnity.ProtocolGame.SendSetOutfit(outfitInstance, _currentMount as OutfitInstance);
            }

            _currentOutfit = null;
            _currentMount = null;
            _outfits = null;
            _mounts = null;
            _currentOutfitIndex = -1;
            _currentMountIndex = -1;
        }

        private void OnCancelButtonClick() {
            
        }

        private void OnNextOutfitButtonClick() {
            int newIndex = _currentOutfitIndex + 1;
            if (newIndex >= _outfits.Count)
                newIndex = 0;
            ChangeOutfitIndex(newIndex);
        }

        private void OnPrevOutfitButtonClick() {
            int newIndex = _currentOutfitIndex - 1;
            if (newIndex < 0)
                newIndex = _outfits.Count - 1;
            ChangeOutfitIndex(newIndex);
        }

        private void OnAddon1CheckboxChange(bool value) {
            if (_updatingOutfit)
                return;
            UpdateAddons(value, 1);
        }

        private void OnAddon2CheckboxChange(bool value) {
            if (_updatingOutfit)
                return;
            UpdateAddons(value, 2);
        }

        private void OnNextMountButtonClick() {
            int newIndex = _currentMountIndex + 1;
            if (newIndex >= _mounts.Count)
                newIndex = 0;
            ChangeMountIndex(newIndex);
        }

        private void OnPrevMountButtonClick() {
            int newIndex = _currentMountIndex - 1;
            if (newIndex < 0)
                newIndex = _mounts.Count - 1;
            ChangeMountIndex(newIndex);
        }

        private void OnColorPaletteColorChanged(int _, int hsiColor) {
            if (_updatingOutfit)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance) {
                int head = outfitInstance.Head;
                int body = outfitInstance.Torso;
                int legs = outfitInstance.Legs;
                int feet = outfitInstance.Detail;

                if (_headToggle.isOn)
                    head = hsiColor;
                else if (_bodyToggle.isOn)
                    body = hsiColor;
                else if (_legsToggle.isOn)
                    legs = hsiColor;
                else if (_feetToggle.isOn)
                    feet = hsiColor;

                outfitInstance.UpdateProperties(head, body, legs, feet, outfitInstance.AddOns);
            }
        }

        private void UpdateColorItems(int hsiColor) {
            _updatingOutfit = true;
            _colorsPalette.SetActiveColor(hsiColor, true);
            _updatingOutfit = false;
        }

        private void UpdateAddons(bool value, int newAddon) {
            if (_currentOutfit is OutfitInstance outfitInstance) {
                int addons = outfitInstance.AddOns;
                if (value)
                    addons |= newAddon;
                else
                    addons &= ~newAddon;

                outfitInstance.UpdateProperties(outfitInstance.Head,
                    outfitInstance.Torso,
                    outfitInstance.Legs,
                    outfitInstance.Detail,
                    addons);
            }
        }

        private void ChangeOutfitIndex(int newIndex) {
            if (_currentOutfitIndex == newIndex)
                return;

            _updatingOutfit = true;
            _currentOutfitIndex = newIndex;
            var newProtocolOutfit = _outfits[_currentOutfitIndex];

            int head = 0, body = 0, legs = 0, feet = 0, addons = newProtocolOutfit.AddOns;
            if (_currentOutfit is OutfitInstance outfitInstance) {
                head = outfitInstance.Head;
                body = outfitInstance.Torso;
                legs = outfitInstance.Legs;
                feet = outfitInstance.Detail;
            }

            _addon1Checkbox.checkbox.Checked = (addons & 1) != 0;
            _addon2Checkbox.checkbox.Checked = (addons & 2) != 0;
            _addon1Checkbox.SetEnabled(_addon1Checkbox.checkbox.Checked);
            _addon2Checkbox.SetEnabled(_addon2Checkbox.checkbox.Checked);
            
            _currentOutfit = OpenTibiaUnity.AppearanceStorage.CreateOutfitInstance(newProtocolOutfit._id, head, body, legs, feet, addons);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol))
                UpdateInformation(true, false);

            _updatingOutfit = false;
        }

        private void ChangeMountIndex(int newIndex) {
            if (_currentMountIndex == newIndex)
                return;

            if (_mounts.Count == 0) {
                _currentMountIndex = -1;
            } else {
                _currentMountIndex = newIndex;
                var newProtocolMount = _mounts[_currentMountIndex];

                _currentMount = OpenTibiaUnity.AppearanceStorage.CreateOutfitInstance(newProtocolMount._id, 0, 0, 0, 0, 0);
            }

            UpdateInformation(false, true);
        }

        public bool UpdateProperties(AppearanceInstance outfit, AppearanceInstance mountOutfit, List<ProtocolOutfit> outfits, List<ProtocolMount> mounts) {
            _outfits = outfits;
            _mounts = mounts;

            int outfitIndex = _outfits.FindIndex((x) => x._id == outfit.Id);
            if (outfitIndex == -1)
                outfitIndex = 0;

            ChangeOutfitIndex(outfitIndex);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts)) {
                int mountIndex = mountOutfit != null ? _mounts.FindIndex((x) => x._id == mountOutfit.Id) : 0;
                if (mountIndex == -1 && _mounts.Count > 0)
                    mountIndex = 0;

                ChangeMountIndex(mountIndex);
            }

            _currentOutfit = outfit;
            _currentMount = mountOutfit;
            _currentDirection = Direction.South;
            return true;
        }

        private void UpdateInformation(bool outfit, bool mount) {
            if (outfit) {
                var protocolOutfit = _outfits[_currentOutfitIndex];
                _outfitNameLabel.text = protocolOutfit.Name;
            }

            if (mount) {
                if (_currentMountIndex > -1) {
                    var protocolMount = _mounts[_currentMountIndex];
                    _mountNameLabel.text = protocolMount.Name;
                } else {
                    _mountNameLabel.text = "No Mount";
                }
            }
        }
    }
}