using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class DropdownExtention : Base.AbstractComponent
    {
#pragma warning disable CS0649 // never assigned to
        [Header("Panel Wrapper")]
        [SerializeField] private Sprite m_PanelWrapperNormalSprite;
        [SerializeField] private Sprite m_PanelWrapperHighlightedSprite;

        [Header("Panel Arrow")]
        [SerializeField] private Sprite m_PanelArrowNormalSprite;
        [SerializeField] private Sprite m_PanelArrowHighlightedSprite;

        [Header("Panels")]
        [SerializeField] private Image m_PanelWrapperImage;
        [SerializeField] private Image m_PanelArrowImage;
#pragma warning restore CS0649 // never assigned to

        bool m_TemplateVisible = false;

        private TMPro.TMP_Dropdown m_Dropdown;
        public TMPro.TMP_Dropdown dropdown {
            get {
                if (m_Dropdown == null)
                    m_Dropdown = GetComponent<TMPro.TMP_Dropdown>();

                return m_Dropdown;
            }
        }
        
        protected void LateUpdate() {
            if (dropdown.IsExpanded != m_TemplateVisible) {
                if (!dropdown.IsExpanded) {
                    m_PanelWrapperImage.sprite = m_PanelWrapperNormalSprite;
                    m_PanelArrowImage.sprite = m_PanelArrowNormalSprite;
                } else {
                    m_PanelWrapperImage.sprite = m_PanelWrapperHighlightedSprite;
                    m_PanelArrowImage.sprite = m_PanelArrowHighlightedSprite;
                }

                m_TemplateVisible = dropdown.IsExpanded;
            }
        }
    }
}
