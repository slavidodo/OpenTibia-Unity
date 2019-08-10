using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Game;
using OpenTibiaUnity.Core.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Outfit
{
    internal class OutfitWindow : Core.Components.Base.Window
    {
        [SerializeField] private ButtonWrapper m_OKButtonWrapper = null;
        [SerializeField] private Button m_CancelButton = null;
        [SerializeField] private LayoutElement m_PanelContentLayout = null;
        [SerializeField] private RectTransform m_PanelOutfit = null;
        [SerializeField] private RectTransform m_PanelMount = null;
        [SerializeField] private RectTransform m_PanelColors = null;
        [SerializeField] private ToggleGroup m_OutfitColorToggleGroup = null;
        [SerializeField] private ToggleWrapper m_ToggleWrapperHead = null;
        [SerializeField] private ToggleWrapper m_ToggleWrapperBody = null;
        [SerializeField] private ToggleWrapper m_ToggleWrapperLegs = null;
        [SerializeField] private ToggleWrapper m_ToggleWrapperFeet = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelInformation = null;
        [SerializeField] private Button m_ButtonNextOutfitLegacy = null;
        [SerializeField] private Button m_ButtonNextOutfit = null;
        [SerializeField] private Button m_ButtonPrevOutfit = null;
        [SerializeField] private RectTransform m_PanelOutfitName = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelOutfitName = null;
        [SerializeField] private RectTransform m_PanelAddons = null;
        [SerializeField] private CheckboxWrapper m_CheckboxAddon1 = null;
        [SerializeField] private CheckboxWrapper m_CheckboxAddon2 = null;
        [SerializeField] private CheckboxWrapper m_CheckboxAddon3 = null;
        [SerializeField] private Button m_ButtonNextMount = null;
        [SerializeField] private Button m_ButtonPrevMount = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelMountName = null;
        [SerializeField] private RawImage m_RawImageOutfit = null;
        [SerializeField] private RawImage m_RawImageMount = null;

        [SerializeField] private float m_SpacingFactor = 0.8f;
        [SerializeField] private OutfitColorItem ColorItemTemplate = null;

        private List<ProtocolOutfit> m_Outfits = null;
        private List<ProtocolMount> m_Mounts = null;
        private AppearanceInstance m_CurrentOutfit = null;
        private AppearanceInstance m_CurrentMount = null;
        private int m_CurrentOutfitIndex = 0;
        private int m_CurrentMountIndex = 0;
        private Direction m_CurrentDirection = Direction.South;
        private bool m_UpdatingOutfit = false;
        private bool m_Initialized = false;

        private RenderTexture m_RenderTexture = null;
        private OutfitColorItem[] m_ColorItems;
        
        protected override void Start() {
            base.Start();

            // Create outfit colors
            m_ColorItems = new OutfitColorItem[Core.Colors.HSI_H_STEPS * Core.Colors.HSI_SI_VALUES];
            for (int i = 0; i < Core.Colors.HSI_H_STEPS; i++) {
                for (int j = 0; j < Core.Colors.HSI_SI_VALUES; j++) {
                    var colorItem = Instantiate(ColorItemTemplate, m_PanelColors);
                    int hsiColor = j * Core.Colors.HSI_H_STEPS + i;
                    colorItem.toggleComponent.group = m_OutfitColorToggleGroup;
                    colorItem.imageComponent.color = Core.Colors.ColorFromHSI(hsiColor);
                    m_ColorItems[hsiColor] = colorItem;

                    colorItem.toggleComponent.onValueChanged.AddListener((value) => {
                        if (value)
                            UpdateColor(hsiColor);
                    });
                }
            }

            ColorItemTemplate.gameObject.SetActive(false);

            // this was never introduced and was removed from tibia soon
            m_CheckboxAddon3.DisableComponent();

            // setup events
            m_ToggleWrapperHead.toggle.onValueChanged.AddListener(OnHeadToggleValueChanged);
            m_ToggleWrapperBody.toggle.onValueChanged.AddListener(OnBodyToggleValueChanged);
            m_ToggleWrapperLegs.toggle.onValueChanged.AddListener(OnLegsToggleValueChanged);
            m_ToggleWrapperFeet.toggle.onValueChanged.AddListener(OnFeetToggleValueChanged);
            m_OKButtonWrapper.button.onClick.AddListener(OnOkButtonClick);
            m_CancelButton.onClick.AddListener(OnCancelButtonClick);
            m_ButtonNextOutfitLegacy.onClick.AddListener(OnNextOutfitButtonClick);
            m_ButtonNextOutfit.onClick.AddListener(OnNextOutfitButtonClick);
            m_ButtonPrevOutfit.onClick.AddListener(OnPrevOutfitButtonClick);
            m_CheckboxAddon1.onValueChanged.AddListener(OnAddon1CheckboxChange);
            m_CheckboxAddon2.onValueChanged.AddListener(OnAddon2CheckboxChange);
            m_ButtonNextMount.onClick.AddListener(OnNextMountButtonClick);
            m_ButtonPrevMount.onClick.AddListener(OnPrevMountButtonClick);
            
            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);
            
            m_RawImageOutfit.uvRect = new Rect(0, 0, 0.5f, 1f);
            m_RawImageMount.uvRect = new Rect(0.5f, 0, 0.5f, 1f);

            m_Initialized = true;
            if (m_CurrentOutfit)
                OnFirstShow();
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            if (!m_CurrentOutfit && !m_CurrentMount)
                return;

            if (m_RenderTexture == null) {
                var descriptor = new RenderTextureDescriptor(Constants.FieldSize * 2 * 2, Constants.FieldSize * 2, RenderTextureFormat.ARGB32, 0);
                m_RenderTexture = new RenderTexture(descriptor);
                m_RenderTexture.filterMode = FilterMode.Point;

                m_RawImageOutfit.texture = m_RenderTexture;
                m_RawImageMount.texture = m_RenderTexture;
            } else {
                m_RenderTexture.Release();
            }

            RenderTexture.active = m_RenderTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            if (!!m_CurrentOutfit) {
                var screenPosition = new Vector2(Constants.FieldSize, Constants.FieldSize);
                var zoom = new Vector2(Screen.width / (float)m_RenderTexture.width, Screen.height / (float)m_RenderTexture.height);

                if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                    screenPosition *= m_SpacingFactor;

                if (m_CurrentOutfit is OutfitInstance)
                    m_CurrentOutfit.DrawTo(screenPosition, zoom, (int)m_CurrentDirection, 0, 0);
                else
                    m_CurrentOutfit.DrawTo(screenPosition, zoom, 0, 0, 0);
                
                if (!m_RawImageOutfit.enabled)
                    m_RawImageOutfit.enabled = true;
            }

            if (!!m_CurrentMount) {
                var screenPosition = new Vector2(Constants.FieldSize, Constants.FieldSize);
                var zoom = new Vector2(Screen.width / (float)m_RenderTexture.width, Screen.height / (float)m_RenderTexture.height);

                screenPosition *= m_SpacingFactor;
                screenPosition += new Vector2(Constants.FieldSize * 2, 0);

                if (m_CurrentMount is OutfitInstance)
                    m_CurrentMount.DrawTo(screenPosition, zoom, (int)m_CurrentDirection, 0, 0);
                else
                    m_CurrentMount.DrawTo(screenPosition, zoom, 0, 0, 0);
                
                if (!m_RawImageMount.enabled)
                    m_RawImageMount.enabled = true;
            }

            RenderTexture.active = null;
        }

        protected override void OnEnable() {
            if (!m_CurrentOutfit || !m_Initialized)
                return;

            OnFirstShow();
        }

        private void OnFirstShow() {
            m_ToggleWrapperHead.toggle.isOn = true;
            if (m_CurrentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Head);
        }

        private void OnClientVersionChange(int oldVersion, int newVersion) {
            bool hasNewProtocol = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol);
            bool hasAddons = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons);
            bool hasMounts = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts);

            if (hasNewProtocol) {
                m_PanelContentLayout.minWidth = 462;
                m_PanelOutfit.sizeDelta = new Vector2(141, 141);
                m_ToggleWrapperHead.rectTransform.anchoredPosition = new Vector2(153, 0);
                m_ToggleWrapperBody.rectTransform.anchoredPosition = new Vector2(153, -24);
                m_ToggleWrapperLegs.rectTransform.anchoredPosition = new Vector2(153, -48);
                m_ToggleWrapperFeet.rectTransform.anchoredPosition = new Vector2(153, -72);
                m_ToggleWrapperHead.rectTransform.sizeDelta = new Vector2(57, 21);
                m_ToggleWrapperBody.rectTransform.sizeDelta = new Vector2(57, 21);
                m_ToggleWrapperLegs.rectTransform.sizeDelta = new Vector2(57, 21);
                m_ToggleWrapperFeet.rectTransform.sizeDelta = new Vector2(57, 21);
                m_ToggleWrapperBody.label.text = "Primary";
                m_ToggleWrapperLegs.label.text = "Secondary";
                m_ToggleWrapperFeet.label.text = "Detail";

                m_PanelColors.offsetMin = new Vector2(219, m_PanelColors.offsetMin.y);

                if (hasMounts) {
                    m_PanelContentLayout.minHeight = 268;
                    m_LabelInformation.rectTransform.anchoredPosition = new Vector2(0, -180);
                    m_LabelInformation.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL_MOUNT;
                    m_PanelAddons.anchoredPosition = new Vector2(153, -100);
                } else {
                    m_PanelContentLayout.minHeight = 258;
                    m_LabelInformation.rectTransform.anchoredPosition = new Vector2(153, -100);
                    m_LabelInformation.text = TextResources.OUTFIT_LABEL_INFO_NEW_PROTOCOL;
                    m_PanelAddons.anchoredPosition = new Vector2(0, -175);
                }
            } else {
                m_PanelContentLayout.minWidth = 380;
                m_PanelContentLayout.minHeight = 140;
                m_PanelOutfit.sizeDelta = new Vector2(69, 69);
                m_ToggleWrapperHead.rectTransform.anchoredPosition = new Vector2(81, 0);
                m_ToggleWrapperBody.rectTransform.anchoredPosition = new Vector2(81, -24);
                m_ToggleWrapperLegs.rectTransform.anchoredPosition = new Vector2(81, -48);
                m_ToggleWrapperFeet.rectTransform.anchoredPosition = new Vector2(81, -72);
                m_ToggleWrapperHead.rectTransform.sizeDelta = new Vector2(42, 21);
                m_ToggleWrapperBody.rectTransform.sizeDelta = new Vector2(42, 21);
                m_ToggleWrapperLegs.rectTransform.sizeDelta = new Vector2(42, 21);
                m_ToggleWrapperFeet.rectTransform.sizeDelta = new Vector2(42, 21);
                m_ToggleWrapperBody.label.text = "Body";
                m_ToggleWrapperLegs.label.text = "Legs";
                m_ToggleWrapperFeet.label.text = "Feet";

                m_PanelColors.offsetMin = new Vector3(135, m_PanelColors.offsetMin.y);
                m_LabelInformation.rectTransform.anchoredPosition = new Vector2(0, -100);
                m_LabelInformation.text = TextResources.OUTFIT_LABEL_INFO_LEGACY_PROTOCOL;
            }

            m_ButtonNextOutfitLegacy.gameObject.SetActive(!hasNewProtocol);
            m_ButtonNextOutfit.gameObject.SetActive(hasNewProtocol);
            m_ButtonPrevOutfit.gameObject.SetActive(hasNewProtocol);
            m_PanelOutfitName.gameObject.SetActive(hasNewProtocol);
            m_PanelAddons.gameObject.SetActive(hasAddons);
            m_PanelMount.gameObject.SetActive(hasMounts);

            if (m_RenderTexture) {
                m_RenderTexture.Release();
                m_RenderTexture = null;
            }
        }

        private void OnHeadToggleValueChanged(bool value) {
            if (!value)
                return;

            if (m_CurrentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Head);
            else
                UpdateColorItems(0);
        }

        private void OnBodyToggleValueChanged(bool value) {
            if (!value)
                return;

            if (m_CurrentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Torso);
            else
                UpdateColorItems(0);
        }

        private void OnLegsToggleValueChanged(bool value) {
            if (!value)
                return;

            if (m_CurrentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Legs);
            else
                UpdateColorItems(0);
        }

        private void OnFeetToggleValueChanged(bool value) {
            if (!value)
                return;

            if (m_CurrentOutfit is OutfitInstance outfitInstance)
                UpdateColorItems(outfitInstance.Detail);
            else
                UpdateColorItems(0);
        }

        private void OnOkButtonClick() {
            if (!!OpenTibiaUnity.ProtocolGame) {
                if (m_CurrentOutfit is OutfitInstance outfitInstance)
                    OpenTibiaUnity.ProtocolGame.SendSetOutfit(outfitInstance, m_CurrentMount as OutfitInstance);
            }

            m_CurrentOutfit = null;
            m_CurrentMount = null;
            m_Outfits = null;
            m_Mounts = null;
            m_CurrentOutfitIndex = -1;
            m_CurrentMountIndex = -1;
            CloseWindow();
        }

        private void OnCancelButtonClick() {
            CloseWindow();
        }

        private void OnNextOutfitButtonClick() {
            int newIndex = m_CurrentOutfitIndex + 1;
            if (newIndex >= m_Outfits.Count)
                newIndex = 0;
            ChangeOutfitIndex(newIndex);
        }

        private void OnPrevOutfitButtonClick() {
            int newIndex = m_CurrentOutfitIndex - 1;
            if (newIndex < 0)
                newIndex = m_Outfits.Count - 1;
            ChangeOutfitIndex(newIndex);
        }

        private void OnAddon1CheckboxChange(bool value) {
            if (m_UpdatingOutfit)
                return;
            UpdateAddons(value, 1);
        }

        private void OnAddon2CheckboxChange(bool value) {
            if (m_UpdatingOutfit)
                return;
            UpdateAddons(value, 2);
        }

        private void OnNextMountButtonClick() {
            int newIndex = m_CurrentMountIndex + 1;
            if (newIndex >= m_Mounts.Count)
                newIndex = 0;
            ChangeMountIndex(newIndex);
        }

        private void OnPrevMountButtonClick() {
            int newIndex = m_CurrentMountIndex - 1;
            if (newIndex < 0)
                newIndex = m_Mounts.Count - 1;
            ChangeMountIndex(newIndex);
        }

        private void UpdateColorItems(int hsiColor) {
            m_UpdatingOutfit = true;
            m_ColorItems[hsiColor].toggleComponent.isOn = true;
            m_UpdatingOutfit = false;
        }

        private void UpdateColor(int hsiColor) {
            if (m_UpdatingOutfit)
                return;

            if (m_CurrentOutfit is OutfitInstance outfitInstance) {
                int head = outfitInstance.Head;
                int body = outfitInstance.Torso;
                int legs = outfitInstance.Legs;
                int feet = outfitInstance.Detail;
                
                if (m_ToggleWrapperHead.toggle.isOn)
                    head = hsiColor;
                else if (m_ToggleWrapperBody.toggle.isOn)
                    body = hsiColor;
                else if (m_ToggleWrapperLegs.toggle.isOn)
                    legs = hsiColor;
                else if (m_ToggleWrapperFeet.toggle.isOn)
                    feet = hsiColor;

                outfitInstance.UpdateProperties(head, body, legs, feet, outfitInstance.AddOns);
            }
        }

        private void UpdateAddons(bool value, int newAddon) {
            if (m_CurrentOutfit is OutfitInstance outfitInstance) {
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
            if (m_CurrentOutfitIndex == newIndex)
                return;

            m_UpdatingOutfit = true;
            m_CurrentOutfitIndex = newIndex;
            var newProtocolOutfit = m_Outfits[m_CurrentOutfitIndex];

            int head = 0, body = 0, legs = 0, feet = 0, addons = newProtocolOutfit.AddOns;
            if (m_CurrentOutfit is OutfitInstance outfitInstance) {
                head = outfitInstance.Head;
                body = outfitInstance.Torso;
                legs = outfitInstance.Legs;
                feet = outfitInstance.Detail;
            }

            m_CheckboxAddon1.checkbox.Checked = (addons & 1) != 0;
            m_CheckboxAddon2.checkbox.Checked = (addons & 2) != 0;
            m_CheckboxAddon1.SetEnabled(m_CheckboxAddon1.checkbox.Checked);
            m_CheckboxAddon2.SetEnabled(m_CheckboxAddon2.checkbox.Checked);
            
            m_CurrentOutfit = OpenTibiaUnity.AppearanceStorage.CreateOutfitInstance(newProtocolOutfit.ID, head, body, legs, feet, addons);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol))
                UpdateInformation(true, false);

            m_UpdatingOutfit = false;
        }

        private void ChangeMountIndex(int newIndex) {
            if (m_CurrentMountIndex == newIndex)
                return;

            if (m_Mounts.Count == 0) {
                m_CurrentMountIndex = -1;
            } else {
                m_CurrentMountIndex = newIndex;
                var newProtocolMount = m_Mounts[m_CurrentMountIndex];

                m_CurrentMount = OpenTibiaUnity.AppearanceStorage.CreateOutfitInstance(newProtocolMount.ID, 0, 0, 0, 0, 0);
            }

            UpdateInformation(false, true);
        }

        internal bool UpdateProperties(AppearanceInstance outfit, AppearanceInstance mountOutfit, List<ProtocolOutfit> outfits, List<ProtocolMount> mounts) {
            m_Outfits = outfits;
            m_Mounts = mounts;

            int outfitIndex = m_Outfits.FindIndex((x) => x.ID == outfit.ID);
            if (outfitIndex == -1)
                outfitIndex = 0;

            ChangeOutfitIndex(outfitIndex);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts)) {
                int mountIndex = mountOutfit != null ? m_Mounts.FindIndex((x) => x.ID == mountOutfit.ID) : 0;
                if (mountIndex == -1 && m_Mounts.Count > 0)
                    mountIndex = 0;

                ChangeMountIndex(mountIndex);
            }

            m_CurrentOutfit = outfit;
            m_CurrentMount = mountOutfit;
            m_CurrentDirection = Direction.South;
            return true;
        }

        private void UpdateInformation(bool outfit, bool mount) {
            if (outfit) {
                var protocolOutfit = m_Outfits[m_CurrentOutfitIndex];
                m_LabelOutfitName.text = protocolOutfit.Name;
            }

            if (mount) {
                if (m_CurrentMountIndex > -1) {
                    var protocolMount = m_Mounts[m_CurrentMountIndex];
                    m_LabelMountName.text = protocolMount.Name;
                } else {
                    m_LabelMountName.text = "No Mount";
                }
            }
        }
    }
}