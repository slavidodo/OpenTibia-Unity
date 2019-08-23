using OpenTibiaUnity.Core.Input.GameAction;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    public class ObjectDragImpl
    {
        protected static List<IWidgetContainerWidget> s_ObjectDragImpls;

        public static bool AnyDraggingObject = false;
    }

    public sealed class ObjectDragImpl<T> : ObjectDragImpl where T : IMoveWidget, IWidgetContainerWidget
    {
        private T _moveWidget = default;

        private Appearances.ObjectInstance _dragObject;
        private int _dragStackPos = -1;
        private Vector3Int _dragStart = Vector3Int.zero;

        private bool _dragStarted = false;
        public bool DragStarted {
            get => _dragStarted;
        }

        public ObjectDragImpl(T moveWidget) {
            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null) {
                inputHandler.AddBeginDragListener(Utils.EventImplPriority.High, OnDragBegin);
                inputHandler.AddEndDragListener(Utils.EventImplPriority.High, OnDragEnd);
            }

            _moveWidget = moveWidget;

            if (s_ObjectDragImpls == null)
                s_ObjectDragImpls = new List<IWidgetContainerWidget>();

            s_ObjectDragImpls.Add(moveWidget);
        }
        
        private void OnDragBegin(Event e, MouseButton mouseButton, bool repeated) {
            if (AnyDraggingObject || OpenTibiaUnity.InputHandler.GetRawEventModifiers() != EventModifiers.None || mouseButton != MouseButton.Left) {
                return;
            }

            int stackPos = _moveWidget.GetMoveObjectUnderPoint(e.mousePosition, out _dragObject);
            if (stackPos == -1)
                return;
            
            _dragStart = _moveWidget.MousePositionToAbsolutePosition(e.mousePosition).Value;
            _dragStarted = true;
            _dragStackPos = stackPos;
            AnyDraggingObject = true;

            e.Use();
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Crosshair, CursorPriority.High);
        }

        private void OnDragEnd(Event e, MouseButton mouseButton, bool repeated) {
            if (!_dragStarted)
                return;

            e.Use();
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
            _dragStarted = false;
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

            new MoveActionImpl(_dragStart, _dragObject, _dragStackPos, destAbsolute.Value, moveAmount).Perform();
        }
    }
}
