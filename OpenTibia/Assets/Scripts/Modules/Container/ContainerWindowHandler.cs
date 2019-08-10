using OpenTibiaUnity.Core.Container;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Container
{
    internal class ContainerWindowHandler : MonoBehaviour
    {
        private ContainerWindow[] m_ContainerWindows;

        protected void Start() {
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            if (containerStorage != null) {
                containerStorage.onContainerAdded.AddListener(OnAddedContainer);
                containerStorage.onContainerClosed.AddListener(OnClosedContainer);
            }

            m_ContainerWindows = new ContainerWindow[Constants.MaxContainerViews];
        }

        protected void OnAddedContainer(ContainerView containerView) {
            var containerWindow = Instantiate(ModulesManager.Instance.ContainerWindowPrefab);
            containerWindow.UpdateProperties(containerView);

            var gameWindowLayout = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
            gameWindowLayout.AddMiniWindow(containerWindow);

            m_ContainerWindows[containerView.ID] = containerWindow;
        }

        protected void OnClosedContainer(ContainerView containerView) {
            var containerWindow = m_ContainerWindows[containerView.ID];
            if (containerWindow)
                Destroy(containerWindow.gameObject);
        }
    }
}
