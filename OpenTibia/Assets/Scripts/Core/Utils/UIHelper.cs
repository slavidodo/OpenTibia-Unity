using OpenTibiaUnity.UI.Legacy;
using UnityEngine;

namespace OpenTibiaUnity.Core.Utils
{
    public static class UIHelper
    {
        public static void EnsureChildVisible(ScrollRect scrollRect, RectTransform child) {
            var content = scrollRect.content;
            var viewport = scrollRect.viewport;
            if (child.parent != content)
                return;

            var contentSize = Vector2.Scale(content.rect.size, content.lossyScale);

            var viewportPosition = viewport.position;
            var viewportSize = Vector2.Scale(viewport.rect.size, viewport.lossyScale);
            viewportPosition = new Vector2 {
                x = viewportPosition.x - viewport.pivot.x * viewportSize.x,
                y = Screen.height - viewportPosition.y - (1.0f - viewport.pivot.y) * viewportSize.y,
            };

            var childPosition = child.position;
            var childSize = Vector2.Scale(child.rect.size, child.lossyScale);
            childPosition = new Vector2 {
                x = childPosition.x - child.pivot.x * childSize.x,
                y = Screen.height - childPosition.y - (1.0f - child.pivot.y) * childSize.y,
            };

            if (scrollRect.verticalScrollbar) {
                var deltaY = viewportPosition.y - childPosition.y;
                if (deltaY > 0) {
                    scrollRect.verticalScrollbar.value += 2 * deltaY / viewportSize.y;
                    childPosition.y += deltaY;
                }

                deltaY = (childPosition.y + childSize.y) - (viewportPosition.y + viewportSize.y);
                if (deltaY > 0)
                    scrollRect.verticalScrollbar.value -= 2 * deltaY / viewportSize.y;
            } else if (scrollRect.horizontalScrollbar) {
                var deltaX = viewportPosition.x - childPosition.x;
                if (deltaX > 0) {
                    scrollRect.horizontalScrollbar.value += 2 * deltaX / viewportSize.x;
                    childPosition.y += deltaX;
                }

                deltaX = (childPosition.x + childSize.x) - (viewportPosition.x + viewportSize.x);
                if (deltaX > 0)
                    scrollRect.horizontalScrollbar.value -= 2 * deltaX / viewportSize.x;
            }
        }
    }
}
