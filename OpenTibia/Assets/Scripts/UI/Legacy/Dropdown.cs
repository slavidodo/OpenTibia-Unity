using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class Dropdown : BasicElement
    {
        [Header("Panel Wrapper")]
        [SerializeField] private Sprite _panelWrapperNormalSprite = null;
        [SerializeField] private Sprite _panelWrapperHighlightedSprite = null;

        [Header("Panel Arrow")]
        [SerializeField] private Sprite _panelArrowNormalSprite = null;
        [SerializeField] private Sprite _panelArrowHighlightedSprite = null;

        [Header("Panels")]
        [SerializeField] private UnityUI.Image _panelWrapperImage = null;
        [SerializeField] private UnityUI.Image _panelArrowImage = null;

        bool _templateVisible = false;

        private TMPro.TMP_Dropdown _dropdown = null;
        public TMPro.TMP_Dropdown dropdown {
            get {
                if (_dropdown == null)
                    _dropdown = GetComponent<TMPro.TMP_Dropdown>();
                return _dropdown;
            }
        }

        private UnityUI.ScrollRect _scrollRect = null;
        private UnityUI.ScrollRect scrollRect {
            get {
                if (_scrollRect = null)
                    _scrollRect = GetComponentInChildren<UnityUI.ScrollRect>();
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
                    var scrollRect = transform.GetComponentInChildren<UnityUI.ScrollRect>();
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
