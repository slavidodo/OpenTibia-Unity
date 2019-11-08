using OpenTibiaUnity.Core.Container;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Container
{
    public class ContainerWindowHandler : MonoBehaviour
    {
        private ContainerWindow[] _containerWindows;

        protected void Start() {
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            if (containerStorage != null) {
                containerStorage.onContainerAdded.AddListener(OnAddedContainer);
                containerStorage.onContainerClosed.AddListener(OnClosedContainer);
            }

            _containerWindows = new ContainerWindow[Constants.MaxContainerViews];
        }

        protected void OnAddedContainer(ContainerView containerView, int expectedNumberOfObjects) {
            var oldWindow = _containerWindows[containerView.Id];
            if (oldWindow) {
                oldWindow.UpdateProperties(containerView, expectedNumberOfObjects);
            } else {
                var containerWindow = Instantiate(ModulesManager.Instance.ContainerWindowPrefab);
                containerWindow.UpdateProperties(containerView, expectedNumberOfObjects);

                var gameWindowLayout = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
                gameWindowLayout.AddMiniWindow(containerWindow);

                _containerWindows[containerView.Id] = containerWindow;
            }
        }

        protected void OnClosedContainer(ContainerView containerView) {
            var containerWindow = _containerWindows[containerView.Id];
            if (containerWindow) {
                containerWindow.CloseWithoutNotifying();
                _containerWindows[containerView.Id] = null;
            }
        }
    }
}
