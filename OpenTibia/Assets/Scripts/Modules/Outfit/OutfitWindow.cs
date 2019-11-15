using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Game;
using OpenTibiaUnity.Core.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Outfit
{
    public class OutfitWindow : Core.Components.Base.Window
    {
        [SerializeField] private ButtonWrapper _oKButtonWrapper = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private LayoutElement _panelContentLayout = null;
        [SerializeField] private RectTransform _panelOutfit = null;
        [SerializeField] private RectTransform _panelMount = null;
        [SerializeField] private RectTransform _panelColors = null;
        [SerializeField] private ToggleGroup _outfitColorToggleGroup = null;
        [SerializeField] private ToggleWrapper _toggleWrapperHead = null;
        [SerializeField] private ToggleWrapper _toggleWrapperBody = null;
        [SerializeField] private ToggleWrapper _toggleWrapperLegs = null;
        [SerializeField] private ToggleWrapper _toggleWrapperFeet = null;
        [SerializeField] private TMPro.TextMeshProUGUI _labelInformation = null;
        [SerializeField] private Button _buttonNextOutfitLegacy = null;
        [SerializeField] private Button _buttonNextOutfit = null;
        [SerializeField] private Button _buttonPrevOutfit = null;
        [SerializeField] private RectTransform _panelOutfitName = null;
        [SerializeField] private TMPro.TextMeshProUGUI _labelOutfitName = null;
        [SerializeField] private RectTransform _panelAddons = null;
        [SerializeField] private CheckboxWrapper _checkboxAddon1 = null;
        [SerializeField] private CheckboxWrapper _checkboxAddon2 = null;
        [SerializeField] private CheckboxWrapper _checkboxAddon3 = null;
        [SerializeField] private Button _buttonNextMount = null;
        [SerializeField] private Button _buttonPrevMount = null;
        [SerializeField] private TMPro.TextMeshProUGUI _labelMountName = null;
        [SerializeField] private RawImage _rawImageOutfit = null;
        [SerializeField] private RawImage _rawImageMount = null;

        [SerializeField] private float _spacingFactor = 0.8f;
        [SerializeField] private OutfitColorItem ColorItemTemplate = null;

        private List<ProtocolOutfit> _outfits = null;
        private List<ProtocolMount> _mounts = null;
        private AppearanceInstance _currentOutfit = null;
        private AppearanceInstance _currentMount = null;
        private int _currentOutfitIndex = 0;
        private int _currentMountIndex = 0;
        private Direction _currentDirection = Direction.South;
        private bool _updatingOutfit = false;
        private bool _initialized = false;

        private RenderTexture _renderTexture = null;
        private OutfitColorItem[] _colorItems;
        
        protected override void Start() {
            base.Start();

            // Create outfit colors
            _colorItems = new OutfitColorItem[Core.Colors.HSI_H_STEPS * Core.Colors.HSI_SI_VALUES];
            for (int i = 0; i < Core.Colors.HSI_H_STEPS; i++) {
                for (int j = 0; j < Core.Colors.HSI_SI_VALUES; j++) {
                    var colorItem = Instantiate(ColorItemTemplate, _panelColors);
                    int hsiColor = j * Core.Colors.HSI_H_STEPS + i;
                    colorItem.toggleComponent.group = _outfitColorToggleGroup;
                    colorItem.imageComponent.color = Core.Colors.ColorFromHSI(hsiColor);
                    _colorItems[hsiColor] = colorItem;

                    colorItem.toggleComponent.onValueChanged.AddListener((value) => {
                        if (value)
                            UpdateColor(hsiColor);
                    });
                }
            }

            ColorItemTemplate.gameObject.SetActive(false);

            // this was never introduced and was removed from tibia soon
            _checkboxAddon3.DisableComponent();

            // setup events
            _toggleWrapperHead.toggle.onValueChanged.AddListener(OnHeadToggleValueChanged);
            _toggleWrapperBody.toggle.onValueChanged.AddListener(OnBodyToggleValueChanged);
            _toggleWrapperLegs.toggle.onValueChanged.AddListener(OnLegsToggleValueChanged);
            _toggleWrapperFeet.toggle.onValueChanged.AddListener(OnFeetToggleValueChanged);
            _oKButtonWrapper.button.onClick.AddListener(OnOkButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
            _buttonNextOutfitLegacy.onClick.AddListener(OnNextOutfitButtonClick);
            _buttonNextOutfit.onClick.AddListener(OnNextOutfitButtonClick);
            _buttonPrevOutfit.onClick.AddListener(OnPrevOutfitButtonClick);
            _checkboxAddon1.onValueChanged.AddListener(OnAddon1CheckboxChange);
            _checkboxAddon2.onValueChanged.AddListener(OnAddon2CheckboxChange);
            _buttonNextMount.onClick.AddListener(OnNextMountButtonClick);
            _buttonPrevMount.onClick.AddListener(OnPrevMountButtonClick);
            
            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);
            
            _rawImageOutfit.uvRect = new Rect(0, 0, 0.5f, 1f);
            _rawImageMount.uvRect = new Rect(0.5f, 0, 0.5f, 1f);

            _initialized = true;
            if (_currentOutfit)
                OnFirstShow();


            OpenTibiaUnity.GameManager.GetModule<GameWindow.GameMapContainer>().onInvalidateTRS.AddListener(OnInvalidateTRS);
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

                _rawImageOutfit.texture = _renderTexture;
                _rawImageMount.texture = _renderTexture;
            }

            var commandBuffer = new CommandBuffer();
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
                
                if (!_rawImageOutfit.enabled)
                    _rawImageOutfit.enabled = true;
            }

            if (!!_currentMount) {
                float pos = Constants.FieldSize * _spacingFactor;
                var screenPosition = new Vector2Int((int)pos + 2 * Constants.FieldSize, (int)pos);

                if (_currentMount is OutfitInstance)
                    _currentMount.Draw(commandBuffer, screenPosition, (int)_currentDirection, 0, 0);
                else
                    _currentMount.Draw(commandBuffer, screenPosition, 0, 0, 0);
                
                if (!_rawImageMount.enabled)
                    _rawImageMount.enabled = true;
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();
        }

        protected override void OnEnable() {
            if (!_currentOutfit || !_initialized)
                return;

            OnFirstShow();
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var gameMapContainer = OpenTibiaUnity.GameManager?.GetModule<GameWindow.GameMapContainer>();
            if (gameMapContainer)
                gameMapContainer.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
        }

        protected void OnInvalidateTRS() {
            if (!!_currentOutfit)
                _currentOutfit.InvalidateTRS();

            if (!!_currentMount)
                _currentMount.InvalidateTRS();
        }

        private void OnFirstShow() {
            _toggleWrapperHead.toggle.isOn = true;
            if (_currentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Head);
        }

        private void OnClientVersionChange(int oldVersion, int newVersion) {
            bool hasNewProtocol = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol);
            bool hasAddons = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons);
            bool hasMounts = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts);

            if (hasNewProtocol) {
                _panelContentLayout.minWidth = 462;
                _panelOutfit.sizeDelta = new Vector2(141, 141);
                _toggleWrapperHead.rectTransform.anchoredPosition = new Vector2(153, 0);
                _toggleWrapperBody.rectTransform.anchoredPosition = new Vector2(153, -24);
                _toggleWrapperLegs.rectTransform.anchoredPosition = new Vector2(153, -48);
                _toggleWrapperFeet.rectTransform.anchoredPosition = new Vector2(153, -72);
                _toggleWrapperHead.rectTransform.sizeDelta = new Vector2(57, 21);
                _toggleWrapperBody.rectTransform.sizeDelta = new Vector2(57, 21);
                _toggleWrapperLegs.rectTransform.sizeDelta = new Vector2(57, 21);
                _toggleWrapperFeet.rectTransform.sizeDelta = new Vector2(57, 21);
                _toggleWrapperBody.label.text = "Primary";
                _toggleWrapperLegs.label.text = "Secondary";
                _toggleWrapperFeet.label.text = "Detail";

                _panelColors.offsetMin = new Vector2(219, _panelColors.offsetMin.y);

                if (hasMounts) {
                    _panelContentLayout.minHeight = 268;
                    _labelInformation.rectTransform.anchoredPosition = new Vector2(0, -180);
                    _labelInformation.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL_MOUNT;
                    _panelAddons.anchoredPosition = new Vector2(153, -100);
                } else {
                    _panelContentLayout.minHeight = 258;
                    _labelInformation.rectTransform.anchoredPosition = new Vector2(153, -100);
                    _labelInformation.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL;
                    _panelAddons.anchoredPosition = new Vector2(0, -175);
                }
            } else {
                _panelContentLayout.minWidth = 380;
                _panelContentLayout.minHeight = 140;
                _panelOutfit.sizeDelta = new Vector2(69, 69);
                _toggleWrapperHead.rectTransform.anchoredPosition = new Vector2(81, 0);
                _toggleWrapperBody.rectTransform.anchoredPosition = new Vector2(81, -24);
                _toggleWrapperLegs.rectTransform.anchoredPosition = new Vector2(81, -48);
                _toggleWrapperFeet.rectTransform.anchoredPosition = new Vector2(81, -72);
                _toggleWrapperHead.rectTransform.sizeDelta = new Vector2(42, 21);
                _toggleWrapperBody.rectTransform.sizeDelta = new Vector2(42, 21);
                _toggleWrapperLegs.rectTransform.sizeDelta = new Vector2(42, 21);
                _toggleWrapperFeet.rectTransform.sizeDelta = new Vector2(42, 21);
                _toggleWrapperBody.label.text = "Body";
                _toggleWrapperLegs.label.text = "Legs";
                _toggleWrapperFeet.label.text = "Feet";

                _panelColors.offsetMin = new Vector3(135, _panelColors.offsetMin.y);
                _labelInformation.rectTransform.anchoredPosition = new Vector2(0, -100);
                _labelInformation.text = TextResources.OUTFIT_LABEL_INFO_LEGACY_PROTOCOL;
            }

            _buttonNextOutfitLegacy.gameObject.SetActive(!hasNewProtocol);
            _buttonNextOutfit.gameObject.SetActive(hasNewProtocol);
            _buttonPrevOutfit.gameObject.SetActive(hasNewProtocol);
            _panelOutfitName.gameObject.SetActive(hasNewProtocol);
            _panelAddons.gameObject.SetActive(hasAddons);
            _panelMount.gameObject.SetActive(hasMounts);
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
            Close();
        }

        private void OnCancelButtonClick() {
            Close();
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

        private void UpdateColorItems(int hsiColor) {
            _updatingOutfit = true;
            _colorItems[hsiColor].toggleComponent.isOn = true;
            _updatingOutfit = false;
        }

        private void UpdateColor(int hsiColor) {
            if (_updatingOutfit)
                return;

            if (_currentOutfit is OutfitInstance outfitInstance) {
                int head = outfitInstance.Head;
                int body = outfitInstance.Torso;
                int legs = outfitInstance.Legs;
                int feet = outfitInstance.Detail;
                
                if (_toggleWrapperHead.toggle.isOn)
                    head = hsiColor;
                else if (_toggleWrapperBody.toggle.isOn)
                    body = hsiColor;
                else if (_toggleWrapperLegs.toggle.isOn)
                    legs = hsiColor;
                else if (_toggleWrapperFeet.toggle.isOn)
                    feet = hsiColor;

                outfitInstance.UpdateProperties(head, body, legs, feet, outfitInstance.AddOns);
            }
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

            _checkboxAddon1.checkbox.Checked = (addons & 1) != 0;
            _checkboxAddon2.checkbox.Checked = (addons & 2) != 0;
            _checkboxAddon1.SetEnabled(_checkboxAddon1.checkbox.Checked);
            _checkboxAddon2.SetEnabled(_checkboxAddon2.checkbox.Checked);
            
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
                _labelOutfitName.text = protocolOutfit.Name;
            }

            if (mount) {
                if (_currentMountIndex > -1) {
                    var protocolMount = _mounts[_currentMountIndex];
                    _labelMountName.text = protocolMount.Name;
                } else {
                    _labelMountName.text = "No Mount";
                }
            }
        }
    }
}