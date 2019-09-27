using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class DropdownExtention : Base.AbstractComponent
    {
        [Header("Panel Wrapper")]
        [SerializeField] private Sprite _panelWrapperNormalSprite = null;
        [SerializeField] private Sprite _panelWrapperHighlightedSprite = null;

        [Header("Panel Arrow")]
        [SerializeField] private Sprite _panelArrowNormalSprite = null;
        [SerializeField] private Sprite _panelArrowHighlightedSprite = null;

        [Header("Panels")]
        [SerializeField] private Image _panelWrapperImage = null;
        [SerializeField] private Image _panelArrowImage = null;

        bool _templateVisible = false;

        private TMPro.TMP_Dropdown _dropdown = null;
        public TMPro.TMP_Dropdown dropdown {
            get {
                if (_dropdown == null)
                    _dropdown = GetComponent<TMPro.TMP_Dropdown>();
                return _dropdown;
            }
        }

        private OTU_ScrollRect _scrollRect = null;
        private OTU_ScrollRect scrollRect {
            get {
                if (_scrollRect = null)
                    _scrollRect = GetComponentInChildren<OTU_ScrollRect>();
                return _scrollRect;
            }
        }

        protected void LateUpdate() {
            if (dropdown.IsExpanded != _templateVisible) {
                // highlight the dropdown
                if (!dropdown.IsExpanded) {
                    _panelWrapperImage.sprite = _panelWrapperNormalSprite;
                    _panelArrowImage.sprite = _panelArrowNormalSprite;
                } else {
                    _panelWrapperImage.sprite = _panelWrapperHighlightedSprite;
                    _panelArrowImage.sprite = _panelArrowHighlightedSprite;
                }

                if (dropdown.IsExpanded) {
                    var scrollRect = transform.GetComponentInChildren<OTU_ScrollRect>();
                    if (scrollRect) {
                        float value = 1f - dropdown.value / (float)(scrollRect.content.childCount - 2);

                        if (scrollRect.horizontalScrollbar)
                            scrollRect.horizontalScrollbar.value = value;
                        else if (scrollRect.verticalScrollbar)
                            scrollRect.verticalScrollbar.value = value;
                    }
                }

                _templateVisible = dropdown.IsExpanded;
            }
        }
    }
}
