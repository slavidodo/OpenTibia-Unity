using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class DropdownExtention : Base.AbstractComponent
    {
        [Header("Panel Wrapper")]
        [SerializeField] private Sprite m_PanelWrapperNormalSprite = null;
        [SerializeField] private Sprite m_PanelWrapperHighlightedSprite = null;

        [Header("Panel Arrow")]
        [SerializeField] private Sprite m_PanelArrowNormalSprite = null;
        [SerializeField] private Sprite m_PanelArrowHighlightedSprite = null;

        [Header("Panels")]
        [SerializeField] private Image m_PanelWrapperImage = null;
        [SerializeField] private Image m_PanelArrowImage = null;

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
                // highlight the dropdown
                if (!dropdown.IsExpanded) {
                    m_PanelWrapperImage.sprite = m_PanelWrapperNormalSprite;
                    m_PanelArrowImage.sprite = m_PanelArrowNormalSprite;
                } else {
                    m_PanelWrapperImage.sprite = m_PanelWrapperHighlightedSprite;
                    m_PanelArrowImage.sprite = m_PanelArrowHighlightedSprite;
                }

                if (dropdown.IsExpanded) {
                    var scrollRect = transform.GetComponentInChildren<ScrollRect>();
                    if (scrollRect) {
                        float value = 1f - dropdown.value / (float)(scrollRect.content.childCount - 2);

                        if (scrollRect.horizontalScrollbar)
                            scrollRect.horizontalScrollbar.value = value;
                        else if (scrollRect.verticalScrollbar)
                            scrollRect.verticalScrollbar.value = value;
                    }
                }

                // scroll to selection
                //EnsureChildVisible();

                m_TemplateVisible = dropdown.IsExpanded;
            }
        }
    }
}
