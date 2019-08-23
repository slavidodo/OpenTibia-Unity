using OpenTibiaUnity.Core.Container;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Container
{
    public class ContainerWindowHandler : MonoBehaviour
    {
        private ContainerWindow[] _bontainerWindows;

        protected void Start() {
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            if (containerStorage != null) {
                containerStorage.onContainerAdded.AddListener(OnAddedContainer);
                containerStorage.onContainerClosed.AddListener(OnClosedContainer);
            }

            _bontainerWindows = new ContainerWindow[Constants.MaxContainerViews];
        }

        protected void OnAddedContainer(ContainerView containerView) {
            var containerWindow = Instantiate(ModulesManager.Instance.ContainerWindowPrefab);
            containerWindow.UpdateProperties(containerView);

            var gameWindowLayout = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
            gameWindowLayout.AddMiniWindow(containerWindow);

            _bontainerWindows[containerView.Id] = containerWindow;
        }

        protected void OnClosedContainer(ContainerView containerView) {
            var containerWindow = _bontainerWindows[containerView.Id];
            if (containerWindow)
                Destroy(containerWindow.gameObject);
        }
    }
}
