using OpenTibiaUnity.Core.Input.GameAction;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    internal class ObjectDragImpl
    {
        protected static List<IWidgetContainerWidget> s_ObjectDragImpls;

        internal static bool AnyDraggingObject = false;
    }

    internal sealed class ObjectDragImpl<T> : ObjectDragImpl where T : IMoveWidget, IWidgetContainerWidget
    {
        private T m_MoveWidget = default;

        private Appearances.ObjectInstance m_DragObject;
        private int m_DragStackPos = -1;
        private Vector3Int m_DragStart = Vector3Int.zero;

        private bool m_DragStarted = false;
        internal bool DragStarted {
            get => m_DragStarted;
        }

        internal ObjectDragImpl(T moveWidget) {
            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null) {
                inputHandler.AddBeginDragListener(Utility.EventImplPriority.High, OnDragBegin);
                inputHandler.AddEndDragListener(Utility.EventImplPriority.High, OnDragEnd);
            }

            m_MoveWidget = moveWidget;

            if (s_ObjectDragImpls == null)
                s_ObjectDragImpls = new List<IWidgetContainerWidget>();

            s_ObjectDragImpls.Add(moveWidget);
        }
        
        private void OnDragBegin(Event e, MouseButton mouseButton, bool repeated) {
            if (AnyDraggingObject || OpenTibiaUnity.InputHandler.GetRawEventModifiers() != EventModifiers.None || mouseButton != MouseButton.Left) {
                return;
            }

            int stackPos = m_MoveWidget.GetMoveObjectUnderPoint(e.mousePosition, out m_DragObject);
            if (stackPos == -1)
                return;
            
            m_DragStart = m_MoveWidget.MousePositionToAbsolutePosition(e.mousePosition).Value;
            m_DragStarted = true;
            m_DragStackPos = stackPos;
            AnyDraggingObject = true;

            e.Use();
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Crosshair, CursorPriority.High);
        }

        private void OnDragEnd(Event e, MouseButton mouseButton, bool repeated) {
            if (!m_DragStarted)
                return;

            e.Use();
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
            m_DragStarted = false;
            AnyDraggingObject = false;

            Vector3Int? destAbsolute = null;
            foreach (var moveWidget in s_ObjectDragImpls) {
                destAbsolute = moveWidget.MousePositionToAbsolutePosition(e.mousePosition);
                if (destAbsolute.HasValue)
                    break;
            }
            
            if (!destAbsolute.HasValue)
                return;

            int moveAmount;
            if (e.shift)
                moveAmount = 1;
            else if (e.control)
                moveAmount = MoveActionImpl.MoveAsk;
            else
                moveAmount = MoveActionImpl.MoveAll;

            new MoveActionImpl(m_DragStart, m_DragObject, m_DragStackPos, destAbsolute.Value, moveAmount).Perform();
        }
    }
}
