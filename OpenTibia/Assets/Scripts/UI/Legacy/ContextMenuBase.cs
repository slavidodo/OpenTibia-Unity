using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class ContextMenuBase : Core.Components.Base.AbstractComponent
    {
        public static ContextMenuBase CurrentContextMenu = null;

        // fields
        private bool _shouldAddSeparator = false;
        private float _maxWidth = 0;
        private Dictionary<int, System.Action> _mappedFunctions = new Dictionary<int, System.Action>();

        private void OnMouseHide() {
            Hide();
        }

        public void CreateSeparatorItem() {
            _shouldAddSeparator = true;
        }

        public void CreateTextItem(string text, System.Action func) {
            CreateTextItem(text, null, func);
        }

        public void CreateTextItem(string text, string shortcut, System.Action func) {
            if (_shouldAddSeparator) {
                InternalCreateSeparatorItem();
                _shouldAddSeparator = false;
            }

            var gameObject = Instantiate(OpenTibiaUnity.GameManager.ContextMenuItemPrefab, transform);
            var toggle = gameObject.GetComponent<UnityUI.Toggle>();
            var labelText = gameObject.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            var labelShortcut = gameObject.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
            var siblingIndex = gameObject.transform.GetSiblingIndex();

            toggle.onValueChanged.AddListener((x) => OnSelectedToggle(siblingIndex));
            if (shortcut != null && shortcut.Length > 0)
                shortcut = string.Format("({0})", shortcut);

            labelText.text = text;
            labelShortcut.text = shortcut;

            float prefferedSize = labelText.preferredWidth + 30;
            if (!string.IsNullOrEmpty(shortcut))
                prefferedSize += labelShortcut.preferredWidth;

            var sizeDelta = rectTransform.sizeDelta;
            _maxWidth = Mathf.Max(100f, _maxWidth, prefferedSize);
            sizeDelta.x = _maxWidth;
            rectTransform.sizeDelta = sizeDelta;

            _mappedFunctions.Add(siblingIndex, func);
        }

        public virtual void InitialiseOptions() { }

        private void InternalCreateSeparatorItem() {
            var separatorGO = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, transform);
            var layoutElement = separatorGO.AddComponent<UnityUI.LayoutElement>();
            layoutElement.minWidth = 120;
        }

        protected virtual void OnSelectedToggle(int index) {
            System.Action action;
            if (_mappedFunctions.TryGetValue(index, out action))
                action.Invoke();

            Hide();
        }

        public void Display(Vector3 position) {
            if (CurrentContextMenu != null) {
                CurrentContextMenu.Hide();
                CurrentContextMenu = null;
            }

            // add options
            try {
                InitialiseOptions();
            } catch (System.Exception) {
                return;
            }

            var blocker = OpenTibiaUnity.GameManager.ActiveBlocker;
            blocker.transform.SetAsLastSibling();
            blocker.gameObject.SetActive(true);

            var buttonComponent = blocker.GetComponent<UnityUI.Button>();
            if (buttonComponent)
                Destroy(buttonComponent);

            buttonComponent = blocker.gameObject.AddComponent<UnityUI.Button>();
            buttonComponent.onClick.AddListener(OnMouseHide);

            // todo replace this with expected values when adding options
            // instead of rebuilding twice
            UnityUI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            position.x = Mathf.Min(position.x, Screen.width - rectTransform.rect.width);
            position.y = Mathf.Min(position.y, Screen.height - rectTransform.rect.height);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector3(position.x, -position.y, 0);

            CurrentContextMenu = this;
        }

        public void Hide() {
            Destroy(gameObject);

            var blocker = OpenTibiaUnity.GameManager.ActiveBlocker;
            blocker.gameObject.SetActive(false);

            var buttonComponent = blocker.GetComponent<UnityUI.Button>();
            if (buttonComponent)
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => Destroy(buttonComponent));

            CurrentContextMenu = null;
        }
    }
}
