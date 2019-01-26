using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Utility
{
    public enum SelectableFormat
    {
        AnySelectable = 1 << 0,
        InputField = 1 << 0,
        Toggle = 1 << 1,
        Button = 1 << 2,
        TMP_InputField = 1 << 3,
    }

    public class SelectableMehcanism
    {
        public static bool AppIsQuiting {
            get {
                return OpenTibiaUnity.Quiting;
            }
        }

        public static bool CanDeselect(SelectableFormat format = SelectableFormat.AnySelectable) {
            if (AppIsQuiting)
                return true;

            var eventSystem = EventSystem.current;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (!selectedGameObject) {
                return false;
            }

            var selectable = selectedGameObject.GetComponent<Selectable>();
            if (!selectable) {
                return false;
            }

            if ((format & SelectableFormat.InputField) != 0 && selectable as InputField) {
                return true;
            }

            if ((format & SelectableFormat.Toggle) != 0 && selectable as Toggle) {
                return true;
            }

            if ((format & SelectableFormat.Button) != 0 && selectable as Button) {
                return true;
            }

            if ((format & SelectableFormat.TMP_InputField) != 0 && selectable as TMPro.TMP_InputField) {
                return true;
            }

            return false;
        }

        public static bool CanDeselect<T0>()
                    where T0 : UnityEngine.Component {
            if (AppIsQuiting)
                return true;

            var eventSystem = EventSystem.current;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (!selectedGameObject) {
                return false;
            }
            
            return selectedGameObject.GetComponent<T0>() != null;
        }

        public static bool CanDeselect<T0, T1>()
                    where T0 : UnityEngine.Component
                    where T1 : UnityEngine.Component {
            if (AppIsQuiting)
                return true;

            var eventSystem = EventSystem.current;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (!selectedGameObject) {
                return false;
            }

            return selectedGameObject.GetComponent<T0>() != null
                && selectedGameObject.GetComponent<T1>() != null;
        }

        public static bool CanDeselect<T0, T1, T2>()
                    where T0 : UnityEngine.Component
                    where T1 : UnityEngine.Component
                    where T2 : UnityEngine.Component {
            if (AppIsQuiting)
                return true;

            var eventSystem = EventSystem.current;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (!selectedGameObject) {
                return false;
            }

            return selectedGameObject.GetComponent<T0>() != null
                && selectedGameObject.GetComponent<T1>() != null
                && selectedGameObject.GetComponent<T2>() != null;
        }

        public static bool CanDeselect<T0, T1, T2, T3>()
                    where T0 : UnityEngine.Component
                    where T1 : UnityEngine.Component
                    where T2 : UnityEngine.Component
                    where T3 : UnityEngine.Component {
            if (AppIsQuiting)
                return true;

            var eventSystem = EventSystem.current;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (!selectedGameObject) {
                return false;
            }

            return selectedGameObject.GetComponent<T0>() != null
                && selectedGameObject.GetComponent<T1>() != null
                && selectedGameObject.GetComponent<T2>() != null
                && selectedGameObject.GetComponent<T3>() != null;
        }
    }
}
