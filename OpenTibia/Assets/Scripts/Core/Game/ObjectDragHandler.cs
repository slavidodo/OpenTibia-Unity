using OpenTibiaUnity.Core.Input.GameAction;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    public class ObjectDragHandler
    {
        // public static fields
        public static bool AnyDraggingObject { get; set; } = false;

        // private static fields
        private static List<IWidgetContainerWidget> s_ObjectDragImpls;
        private static Appearances.ObjectInstance s_dragObject;
        private static int s_dragStackPos = -1;
        private static Vector3Int s_dragStart = Vector3Int.zero;


        public static void Initialize() {
            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null) {
                inputHandler.AddBeginDragListener(Utils.EventImplPriority.High, OnDragBegin);
                inputHandler.AddEndDragListener(Utils.EventImplPriority.High, OnDragEnd);
            }
        }

        public static void RegisterHandler<T>(T moveWidget) where T : IMoveWidget, IWidgetContainerWidget {
            if (s_ObjectDragImpls == null)
                s_ObjectDragImpls = new List<IWidgetContainerWidget>();

            s_ObjectDragImpls.Add(moveWidget);
        }
        
        private static void OnDragBegin(Event e, MouseButton mouseButton, bool repeated) {
            if (AnyDraggingObject || mouseButton != MouseButton.Left) {
                return;
            }

            foreach (var moveWidget in s_ObjectDragImpls) {
                var dragAbsolute = moveWidget.MousePositionToAbsolutePosition(e.mousePosition);
                if (!dragAbsolute.HasValue)
                    continue;

                s_dragStart = dragAbsolute.Value;
                s_dragStackPos = ((IMoveWidget)moveWidget).GetMoveObjectUnderPoint(e.mousePosition, out s_dragObject);
                if (s_dragStackPos == -1)
                    break;

                OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Crosshair, CursorPriority.High);
                AnyDraggingObject = true;

                e.Use();
                break;
            }
        }

        private static void OnDragEnd(Event e, MouseButton mouseButton, bool repeated) {
            if (!AnyDraggingObject)
                return;

            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
            AnyDraggingObject = false;

            e.Use();

            Vector3Int? destAbsolute = null;
            foreach (var moveWidget in s_ObjectDragImpls) {
                if ((destAbsolute = moveWidget.MousePositionToAbsolutePosition(e.mousePosition)).HasValue)
                    break;
            }

            if (!destAbsolute.HasValue)
                return;

            int moveAmount;
            if (e.shift)
                moveAmount = 1;
            else if (e.control == OpenTibiaUnity.OptionStorage.PressCtrlToDragCompleteStacks)
                moveAmount = MoveActionImpl.MoveAsk;
            else
                moveAmount = MoveActionImpl.MoveAll;

            new MoveActionImpl(s_dragStart, s_dragObject, s_dragStackPos, destAbsolute.Value, moveAmount).Perform();
        }
    }
}
