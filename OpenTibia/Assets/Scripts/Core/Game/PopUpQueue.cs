using System.Collections.Generic;
using UnityEngine;
using PopUpBase = OpenTibiaUnity.UI.Legacy.PopUpBase;

namespace OpenTibiaUnity.Core.Game
{
    public sealed class PopUpQueue
    {
        private static int BlockerIndexReferenceCounter = 20000;

        private static PopUpQueue s_instance = null;
        public static PopUpQueue Instance {
            get {
                if (s_instance == null)
                    s_instance = new PopUpQueue();
                return s_instance;
            }
        }

        private List<PopUpBase> _queue = new List<PopUpBase>();

        public void Show(PopUpBase popUp) {
            HideByPriority(popUp.Priority);
            _queue.Add(popUp);
            if (_queue.Count == 1)
                ShowInternal(popUp);

            popUp.onClose.AddListener(OnPopUpBaseClosed);
        }

        public void Hide(PopUpBase popUp) {
            int index;
            for (index = _queue.Count - 1; index >= 0; index--) {
                if (_queue[index] == popUp)
                    break;
            }

            if (index == 0)
                HideInternal(_queue[0]);

            if (index == 0 && _queue.Count > 1)
                ShowInternal(_queue[1]);

            if (index > -1) {
                _queue[index].onClose.RemoveListener(OnPopUpBaseClosed);
                _queue.RemoveAt(index);
            }

            if (_queue.Count == 0)
                BlockerIndexReferenceCounter = 20000;
        }

        private void ShowInternal(PopUpBase popUp) {
            var currentContextMenu = UI.Legacy.ContextMenuBase.CurrentContextMenu;
            if (currentContextMenu != null)
                currentContextMenu.Hide();

            OpenTibiaUnity.InputHandler.CaptureKeyboard = false;
            AddPopUp(popUp);
            CenterPopUp(popUp);
        }

        private void HideInternal(PopUpBase popUp) {
            RemovePopUp(popUp);
            OpenTibiaUnity.InputHandler.CaptureKeyboard = true;
        }

        private void HideByPriority(int priority) {
            int i = _queue.Count - 1;
            while (i >= 0 && _queue[i].Priority <= priority) {
                _queue[i].onClose.RemoveListener(OnPopUpBaseClosed);
                if (i == 0)
                    _queue[i].Hide();
                i--;
            }

            for (int j = _queue.Count - 1; j > i; j--)
                _queue.RemoveAt(j);
        }

        private void OnPopUpBaseClosed(PopUpBase popUp) {
            Hide(popUp);
        }

        private static void AddPopUp(PopUpBase popUp) {
            popUp.ChangingVisibility = !popUp.gameObject.activeSelf;
            popUp.gameObject.SetActive(true);
            popUp.ChangingVisibility = false;

            var blocker = OpenTibiaUnity.GameManager.ActiveBlocker;
            if (blocker != null) {
                popUp.queueBlocker = blocker.gameObject;
                blocker.transform.SetAsLastSibling();
                blocker.gameObject.SetActive(true);
            }

            var canvas = OpenTibiaUnity.GameManager.ActiveCanvas;
            if (canvas != null)
                popUp.transform.SetParent(canvas.transform);

            popUp.transform.SetAsLastSibling();
            popUp.canvas.sortingOrder = BlockerIndexReferenceCounter++;
        }

        private static void RemovePopUp(PopUpBase popUp) {
            popUp.ChangingVisibility = popUp.gameObject.activeSelf;
            popUp.gameObject.SetActive(false);
            popUp.ChangingVisibility = false;

            if (popUp.queueBlocker != null)
                popUp.queueBlocker.SetActive(false);
        }

        public static void CenterPopUp(PopUpBase popUp) {
            popUp.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
