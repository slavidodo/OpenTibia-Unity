using System.Collections.Generic;
using System.Linq;

namespace OpenTibiaUnity.Core.Utils
{
    public enum EventImplPriority
    {
        Low = 1,
        Medium = 2,
        UpperMedium = 3,
        High = 4,

        Default = Low
    }

    public delegate bool WhileCondition();

    public abstract class EventImplBase
    {
        protected static void InternalAddListener<T>(ref List<KeyValuePair<EventImplPriority, T>> list, EventImplPriority priority, T action) {
            int l = 0, r = list.Count - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var listener = list[i];
                if (listener.Key < priority)
                    l = i + 1;
                else if (listener.Key > priority)
                    r = i - 1;
                else
                    break;
            }

            list.Insert(l, new KeyValuePair<EventImplPriority, T>(priority, action));
        }

        protected static bool InternalRemoveListener<T>(ref List<KeyValuePair<EventImplPriority, T>> list, T action) {
            return list.RemoveAll((x) => x.Value.Equals(action)) != 0;
        }
    }

    public class EventImpl : EventImplBase
    {
        private List<KeyValuePair<EventImplPriority, System.Action>> Listeners;

        public void AddListener(EventImplPriority priority, System.Action action) {
            if (Listeners == null)
                Listeners = new List<KeyValuePair<EventImplPriority, System.Action>>();

            InternalAddListener(ref Listeners, priority, action);
        }

        public void RemoveListener(System.Action action) {
            if (Listeners != null)
                InternalRemoveListener(ref Listeners, action);
        }

        public void Invoke() {
            int index = Listeners.Count - 1;
            while (index >= 0)
                Listeners[index--].Value.Invoke();
        }

        public void InvokeWhile(WhileCondition condition) {
            int index = Listeners.Count - 1;
            while (index >= 0) {
                if (!condition.Invoke())
                    break;

                Listeners[index].Value.Invoke();
                index--;
            }
        }
    }

    public class EventImpl<T0> : EventImplBase
    {
        private List<KeyValuePair<EventImplPriority, System.Action<T0>>> Listeners;

        public void AddListener(EventImplPriority priority, System.Action<T0> action) {
            if (Listeners == null)
                Listeners = new List<KeyValuePair<EventImplPriority, System.Action<T0>>>();

            InternalAddListener(ref Listeners, priority, action);
        }

        public void RemoveListener(System.Action<T0> action) {
            if (Listeners != null)
                InternalRemoveListener(ref Listeners, action);
        }

        public void Invoke(T0 t0) {
            int index = Listeners.Count - 1;
            while (index >= 0)
                Listeners[index--].Value.Invoke(t0);
        }

        public void InvokeWhile(T0 t0, WhileCondition condition) {
            int index = Listeners.Count - 1;
            while (index >= 0) {
                if (!condition.Invoke())
                    break;

                Listeners[index].Value.Invoke(t0);
                index--;
            }
        }
    }

    public class EventImpl<T0, T1> : EventImplBase
    {
        private List<KeyValuePair<EventImplPriority, System.Action<T0, T1>>> Listeners;

        public void AddListener(EventImplPriority priority, System.Action<T0, T1> action) {
            if (Listeners == null)
                Listeners = new List<KeyValuePair<EventImplPriority, System.Action<T0, T1>>>();

            InternalAddListener(ref Listeners, priority, action);
        }

        public void RemoveListener(System.Action<T0, T1> action) {
            if (Listeners != null)
                InternalRemoveListener(ref Listeners, action);
        }

        public void Invoke(T0 t0, T1 t1) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0)
                Listeners[index--].Value.Invoke(t0, t1);
        }

        public void InvokeWhile(T0 t0, T1 t1, WhileCondition condition) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0) {
                if (!condition.Invoke())
                    break;

                Listeners[index].Value.Invoke(t0, t1);
                index--;
            }
        }
    }

    public class EventImpl<T0, T1, T2> : EventImplBase
    {
        private List<KeyValuePair<EventImplPriority, System.Action<T0, T1, T2>>> Listeners;

        public void AddListener(EventImplPriority priority, System.Action<T0, T1, T2> action) {
            if (Listeners == null)
                Listeners = new List<KeyValuePair<EventImplPriority, System.Action<T0, T1, T2>>>();

            InternalAddListener(ref Listeners, priority, action);
        }

        public void RemoveListener(System.Action<T0, T1, T2> action) {
            if (Listeners != null)
                InternalRemoveListener(ref Listeners, action);
        }

        public void Invoke(T0 t0, T1 t1, T2 t2) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0)
                Listeners[index--].Value.Invoke(t0, t1, t2);
        }

        public void InvokeWhile(T0 t0, T1 t1, T2 t2, WhileCondition condition) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0) {
                if (!condition.Invoke())
                    break;

                Listeners[index].Value.Invoke(t0, t1, t2);
                index--;
            }
        }
    }

    public class EventImpl<T0, T1, T2, T3> : EventImplBase
    {
        private List<KeyValuePair<EventImplPriority, System.Action<T0, T1, T2, T3>>> Listeners;

        public void AddListener(EventImplPriority priority, System.Action<T0, T1, T2, T3> action) {
            if (Listeners == null)
                Listeners = new List<KeyValuePair<EventImplPriority, System.Action<T0, T1, T2, T3>>>();

            InternalAddListener(ref Listeners, priority, action);
        }

        public void RemoveListener(System.Action<T0, T1, T2, T3> action) {
            if (Listeners != null)
                InternalRemoveListener(ref Listeners, action);
        }

        public void Invoke(T0 t0, T1 t1, T2 t2, T3 t3) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0)
                Listeners[index--].Value.Invoke(t0, t1, t2, t3);
        }

        public void InvokeWhile(T0 t0, T1 t1, T2 t2, T3 t3, WhileCondition condition) {
            if (Listeners == null)
                return;

            int index = Listeners.Count - 1;
            while (index >= 0) {
                if (!condition.Invoke())
                    break;

                Listeners[index].Value.Invoke(t0, t1, t2, t3);
                index--;
            }
        }
    }
}
