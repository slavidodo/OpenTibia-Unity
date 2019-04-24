using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class ContextMenuBase : Base.AbstractComponent {
        public static ContextMenuBase CurrentContextMenu = null;

        bool m_ShouldAddSeparator = false;
        float m_MaxWidth = 0;
        Dictionary<int, System.Action> m_MappedFunctions = new Dictionary<int, System.Action>();

        public void CreateSeparatorItem() {
            m_ShouldAddSeparator = true;
        }

        private void InternalCreateSeparatorItem() {
            var separatorGO = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, transform);
            var layoutElement = separatorGO.AddComponent<LayoutElement>();

            layoutElement.minWidth = 120;
        }

        public void CreateTextItem(string text, System.Action func) {
            CreateTextItem(text, null, func);
        }

        public void CreateTextItem(string text, string shortcut, System.Action func) {
            if (m_ShouldAddSeparator)
                InternalCreateSeparatorItem();

            var layoutElement = Instantiate(OpenTibiaUnity.GameManager.ContextMenuItemPrefab, transform);
            var toggle = layoutElement.GetComponent<Toggle>();
            var labelText = layoutElement.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            var labelShortcut = layoutElement.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
            var siblingIndex = layoutElement.transform.GetSiblingIndex();

            toggle.onValueChanged.AddListener((x) => OnSelectedToggle(siblingIndex));
            if (shortcut != null && shortcut.Length > 0)
                shortcut = string.Format("({0})", shortcut);

            labelText.text = text;
            labelShortcut.text = shortcut;

            var sizeDelta = rectTransform.sizeDelta;
            m_MaxWidth = Mathf.Max(100f, m_MaxWidth, labelText.preferredWidth + 60);
            sizeDelta.x = m_MaxWidth;
            rectTransform.sizeDelta = sizeDelta;

            m_MappedFunctions.Add(siblingIndex, func);
        }

        public virtual void InitialiseOptions() { }

        protected virtual void OnSelectedToggle(int index) {
            System.Action action;
            if (m_MappedFunctions.TryGetValue(index, out action))
                action.Invoke();

            Hide();
        }

        public void Display(Vector3 position) {
            if (CurrentContextMenu != null)
                CurrentContextMenu.Hide();

            InitialiseOptions();
            LockToOverlay();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector3(position.x, -position.y, 0);

            var activeBlocker = OpenTibiaUnity.GameManager.ActiveBlocker;
            var buttonComponent = activeBlocker.GetComponent<Button>();
            if (buttonComponent)
                Destroy(buttonComponent);

            buttonComponent = activeBlocker.gameObject.AddComponent<Button>();
            buttonComponent.onClick.AddListener(Hide);

            CurrentContextMenu = this;
        }
        
        public void Hide() {
            UnlockFromOverlay();
            Destroy(gameObject);

            var activeBlocker = OpenTibiaUnity.GameManager.ActiveBlocker;
            var buttonComponent = activeBlocker.GetComponent<Button>();
            if (buttonComponent)
                Destroy(buttonComponent);

            CurrentContextMenu = null;
        }
    }
}
