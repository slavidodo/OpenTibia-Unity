using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    public static class ObjectMultiUseHandler
    {
        public delegate void ObjectMultiUseDelegate(Vector3Int targetAbsolute, ObjectInstance @object, int targetStackPos);

        private static bool s_Activated = false;
        private static Vector3Int s_AbsolutePosition = Vector3Int.zero;
        private static ObjectInstance s_Object = null;
        private static int s_PositionOrData = 0;
        private static List<IUseWidget> s_ObjectUseImpls;

        public static ObjectMultiUseDelegate onUse = null;
        public static bool AnyDraggingObject = false;

        static ObjectMultiUseHandler() {
            if (s_ObjectUseImpls == null)
                s_ObjectUseImpls = new List<IUseWidget>();
        }
        
        private static void OnMouseDown(Event e, MouseButton mouseButton, bool repeat) {
            if (!s_Activated || mouseButton == MouseButton.Left)
                return;

            e.Use();
            s_Activated = false;
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
        }

        private static void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (!s_Activated)
                return;

            e.Use();
            s_Activated = false;
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
            
            if (mouseButton != MouseButton.Left)
                return;
            
            foreach (var widget in s_ObjectUseImpls) {
                int targetStackPos = widget.GetTopObjectUnderPoint(e.mousePosition, out ObjectInstance targetObject);
                if (targetStackPos == -1)
                    continue;
                
                var targetAbsolute = ((IWidgetContainerWidget)widget).MousePositionToAbsolutePosition(e.mousePosition);
                if (targetAbsolute.HasValue) {
                    if (onUse != null)
                        onUse.Invoke(targetAbsolute.Value, targetObject, targetStackPos);
                    else
                        new Input.GameAction.UseActionImpl(s_AbsolutePosition, s_Object, s_PositionOrData, targetAbsolute.Value, targetObject, targetStackPos, UseActionTarget.Auto).Perform();

                    return;
                }
            }
            
            if (onUse != null)
                onUse.Invoke(Vector3Int.zero, null, -1);
        }

        public static void Initialize() {
            var inputHandler = OpenTibiaUnity.InputHandler;
            inputHandler.AddMouseDownListener(Utils.EventImplPriority.UpperMedium, OnMouseDown);
            inputHandler.AddMouseUpListener(Utils.EventImplPriority.UpperMedium, OnMouseUp);
        }

        public static void RegisterContainer<T>(T widget) where T : IUseWidget, IWidgetContainerWidget {
            s_ObjectUseImpls.Add(widget);
        }

        public static void Activate(Vector3Int absolutePosition, ObjectInstance @object, int positionOrData) {
            s_Activated = true;
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Crosshair, CursorPriority.High);

            s_AbsolutePosition = absolutePosition;
            s_Object = @object;
            s_PositionOrData = positionOrData;
        }
    }
}
