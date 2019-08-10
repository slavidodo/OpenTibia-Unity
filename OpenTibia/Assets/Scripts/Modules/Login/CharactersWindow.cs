using OpenTibiaUnity.Core.Communication.Login;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Input;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    internal class CharactersWindow : Core.Components.Base.Window
    {
        internal class ClosedEvent : UnityEvent { }

        /**
         * Notes:
         *  - Get Premium button appeared in 10.97 up to 12.00 (new position indicated)
         *  - Premium benefits appeared in 11.48
         *  - outfits appeared in tibia 12
         *  - Get Premium button was positioned horizontally aligned to account status
         *     on the opposite anchor
         */ 

        // Serializable Fields
        [SerializeField] private OTU_ScrollRect m_CharactersScrollRect = null;
        [SerializeField] private ToggleGroup m_CharactersToggleGroup = null;
        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;
        [SerializeField] private Button m_GetPremiumLegacyButton = null;
        [SerializeField] private Button m_GetPremiumV12Button = null;

        [SerializeField] private TMPro.TextMeshProUGUI m_AccountStatusLabel = null;

        [SerializeField] private GameObject m_HorizontalSeparator1 = null;
        [SerializeField] private GameObject m_PremiumBenefitsLabel = null;
        [SerializeField] private GameObject m_PremiumBenefitsPanel = null;
        [SerializeField] private GameObject m_AndManyMoreLabel = null;

        // Fields
        protected string m_SessionKey = null;
        protected string m_AccountName = null;
        protected string m_Password = null;
        protected string m_Token = null;
        protected CharacterList m_CharactersList = null;
        protected bool m_PopupIsAdvice = false;

        protected Playdata m_Playdata = null;
        protected Session m_Session = null;

        protected int m_SelectedCharacterIndex = -1;
        protected Core.Components.PopupWindow m_PopupWindow = null;
        
        protected override void Start() {
            base.Start();

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utility.EventImplPriority.High, OnKeyDown);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utility.EventImplPriority.High, OnKeyUp);

            // setup events
            m_OKButton.onClick.AddListener(OnOkButtonClick);
            m_CancelButton.onClick.AddListener(OnCancelButtonClick);
            m_GetPremiumLegacyButton.onClick.AddListener(OnGetPremiumButtonClicked);
            m_GetPremiumV12Button.onClick.AddListener(OnGetPremiumButtonClicked);
            
            // setup popup message
            m_PopupWindow = Instantiate(OpenTibiaUnity.GameManager.PopupWindowPrefab, transform.parent);
            m_PopupWindow.name = "PopupWindow_CharactersWindow";
            m_PopupWindow.HideWindow();

            m_PopupWindow.onOKClick.AddListener(OnPopupOkClick);
            m_PopupWindow.onCancelClick.AddListener(OnPopupCancelClick);

            // setup game events
            OpenTibiaUnity.GameManager.onGameStart.AddListener(OnGameStart);
            OpenTibiaUnity.GameManager.onProcessChangeCharacter.AddListener(OnProcessChangeCharacter);
            OpenTibiaUnity.GameManager.onGameEnd.AddListener(OnGameEnd);

            // select first character
            if (OpenTibiaUnity.GameManager.ClientVersion < 1200)
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectFirstCharacter());
            else
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(gameObject);

            // layout update based on client version
            ForceClientVersionUpdate();
        }

        protected void OnKeyDown(Event e, bool repeat) {
            if (repeat)
                OnKeyUp(e, false);
        }

        protected void OnKeyUp(Event e, bool _) {
            if (e.alt || e.shift || e.control || !InputHandler.IsHighlighted(this))
                return;
            
            switch (e.keyCode) {
                case KeyCode.DownArrow:
                case KeyCode.RightArrow:
                    e.Use();
                    SelectNextCharacter();
                    break;
                case KeyCode.UpArrow:
                case KeyCode.LeftArrow:
                    e.Use();
                    SelectPreviousCharacter();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (e.type == EventType.KeyDown)
                        return;

                    e.Use();
                    OnOkButtonClick();
                    break;
            }
        }

        protected void OnOkButtonClick() {
            if (m_SelectedCharacterIndex < 0 || m_SelectedCharacterIndex >= m_CharactersScrollRect.content.childCount)
                return;

            var child = m_CharactersScrollRect.content.GetChild(m_SelectedCharacterIndex);
            var characterPanel = child.GetComponent<CharacterPanel>();

            string characterName;
            string worldName;
            string worldAddress;
            int worldPort;

            if (m_Playdata != null) {
                var character = m_Playdata.Characters[m_SelectedCharacterIndex];
                characterName = character.Name;

                var world = m_Playdata.FindWorld(character.WorldID);
                worldName = world.Name;
                worldAddress = world.GetAddress(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);
                worldPort = world.GetPort(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);
            } else {
                var character = m_CharactersList.Characters[m_SelectedCharacterIndex];
                characterName = character.Name;

                var world = m_CharactersList.FindWorld(character.WorldID);
                worldName = world.Name;
                worldAddress = world.HostName;
                worldPort = world.Port;
            }

            DoEnterGame(characterName, worldAddress, worldName, worldPort);
        }

        protected void OnCancelButtonClick() {
            CloseWindow();
        }

        protected void OnGetPremiumButtonClicked() {
            // do nothing
            // this is left up to the desire of the client user
        }

        protected void OnPopupOkClick() {
            ShowWindow();

            if (m_PopupIsAdvice) {
                m_PopupIsAdvice = false;
                
                if (OpenTibiaUnity.ProtocolGame != null && OpenTibiaUnity.ProtocolGame.IsGameRunning)
                    SwitchToGameplayCanvas();
            }
        }

        protected void OnPopupCancelClick() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame)
                return;

            RemoveProtocolGameListeners(protocolGame);
            protocolGame.Disconnect();
            OpenTibiaUnity.ProtocolGame = null;

            ShowWindow();
        }

        protected void OnProtocolGameConnectionError(string message, bool disconnected) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            if (!disconnected)
                protocolGame.Disconnect();

            OpenTibiaUnity.ProtocolGame = null;
            PopupMessage("Sorry", message);
        }

        protected void OnProtocolGameLoginError(string message) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            protocolGame.Disconnect();
            OpenTibiaUnity.ProtocolGame = null;

            PopupMessage("Sorry", message);
        }

        protected void OnProtocolGameLoginAdvice(string advice) {
            PopupMessage("For your information", advice);
            m_PopupIsAdvice = true;
        }

        protected void OnProtocolGameLoginWait(string message, int time) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            protocolGame.Disconnect();
            OpenTibiaUnity.ProtocolGame = null;

            // TODO show a waiting time widget & then reconnect //
            PopupMessage("Sorry", message);
        }

        protected void OnGameStart() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);

            if (m_PopupWindow.Visible) {
                if (m_PopupIsAdvice)
                    return;
                else
                    m_PopupWindow.CloseWindow();
            }

            SwitchToGameplayCanvas();
        }

        protected void OnProcessChangeCharacter() {
            OpenWindow();
        }

        protected void OnGameEnd() {
            gameObject.SetActive(true);
            ResetLocalPosition();
            LockToOverlay();

            SwitchToBackgroundCanvas();
        }


        internal void Setup(string sessionKey, string accountName, string password, string token, CharacterList characterList) {
            m_SessionKey = sessionKey;
            m_AccountName = accountName;
            m_Password = password;
            m_Token = token;
            m_CharactersList = characterList;
            m_Session = null;
            m_Playdata = null;

            var content = m_CharactersScrollRect.content;
            foreach (Transform child in content)
                Destroy(child.gameObject);
            
            for (int i = 0; i < characterList.Characters.Count; i++) {
                int characterIndex = i;

                var character = characterList.Characters[i];
                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab);
                characterPanel.transform.SetParent(content);
                characterPanel.characterName.text = character.Name;
                characterPanel.worldName.text = characterList.FindWorld(character.WorldID).Name;
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => { if (value) m_SelectedCharacterIndex = characterIndex; });
                characterPanel.toggleComponent.group = m_CharactersToggleGroup;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            m_SelectedCharacterIndex = -1;
        }

        internal void Setup(Session session, Playdata playData) {
            m_Session = session;
            m_Playdata = playData;
            m_SessionKey = null;
            m_AccountName = null;
            m_Password = null;
            m_Token = null;

            var content = m_CharactersScrollRect.content;
            foreach (Transform child in content)
                Destroy(child.gameObject);
            
            for (int i = 0; i < playData.Characters.Count; i++) {
                int characterIndex = i;

                var character = playData.Characters[i];
                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab);
                characterPanel.GetComponent<LayoutElement>().minHeight = 34;

                characterPanel.transform.SetParent(content);
                characterPanel.characterName.text = character.Name;

                var world = playData.FindWorld(character.WorldID);

                characterPanel.worldName.text = string.Format("{0}\n({1})", world.Name, world.GetPvPTypeDescription());
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => { if (value) m_SelectedCharacterIndex = characterIndex; });
                characterPanel.toggleComponent.group = m_CharactersToggleGroup;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            m_SelectedCharacterIndex = -1;
        }

        protected void AddProtocolGameListeners(Core.Communication.Game.ProtocolGame protocolGame) {
            protocolGame.onConnectionError.AddListener(OnProtocolGameConnectionError);
            protocolGame.onLoginError.AddListener(OnProtocolGameLoginError);
            protocolGame.onLoginAdvice.AddListener(OnProtocolGameLoginAdvice);
            protocolGame.onLoginWait.AddListener(OnProtocolGameLoginWait);
        }

        protected void RemoveProtocolGameListeners(Core.Communication.Game.ProtocolGame protocolGame) {
            protocolGame.onConnectionError.RemoveListener(OnProtocolGameConnectionError);
            protocolGame.onLoginError.RemoveListener(OnProtocolGameLoginError);
            protocolGame.onLoginAdvice.RemoveListener(OnProtocolGameLoginAdvice);
            protocolGame.onLoginWait.RemoveListener(OnProtocolGameLoginWait);
        }

        protected void ForceClientVersionUpdate() {
            int clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
            rectTransform.sizeDelta = new Vector2(clientVersion >= 1200 ? 600 : 340, rectTransform.sizeDelta.y);

            UpdatePremium();
        }

        protected void SelectFirstCharacter() {
            if (m_CharactersScrollRect.content.childCount > 0) {
                var child = m_CharactersScrollRect.content.GetChild(0);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            }
        }

        protected void SelectNextCharacter() {
            if (m_CharactersScrollRect.content.childCount > 1) {
                m_SelectedCharacterIndex = (++m_SelectedCharacterIndex) % m_CharactersScrollRect.content.childCount;
                var child = m_CharactersScrollRect.content.GetChild(m_SelectedCharacterIndex);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            }
        }

        protected void SelectPreviousCharacter() {
            if (m_CharactersScrollRect.content.childCount > 1) {
                if (m_SelectedCharacterIndex <= 0)
                    m_SelectedCharacterIndex = m_CharactersScrollRect.content.childCount - 1;
                else
                    m_SelectedCharacterIndex = (m_SelectedCharacterIndex - 1) % m_CharactersScrollRect.content.childCount;
                
                var child = m_CharactersScrollRect.content.GetChild(m_SelectedCharacterIndex);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            } else if (m_CharactersScrollRect.content.childCount > 0 && m_SelectedCharacterIndex != 0) {
                SelectFirstCharacter();
            }
        }

        protected void SwitchToGameplayCanvas() {
            OpenTibiaUnity.GameManager.BackgroundCanvas.gameObject.SetActive(false);
            OpenTibiaUnity.GameManager.EventSystem.SetSelectedGameObject(OpenTibiaUnity.GameManager.GameCanvas.gameObject);
        }

        protected void SwitchToBackgroundCanvas() {
            OpenTibiaUnity.GameManager.BackgroundCanvas.gameObject.SetActive(true);
            OpenTibiaUnity.GameManager.EventSystem.SetSelectedGameObject(gameObject);
        }

        protected void UpdatePremium() {
            int clientVersion = OpenTibiaUnity.GameManager.ClientVersion;

            bool premium;
            if (m_Playdata != null) {
                premium = m_Session.IsPremium;
                bool showV12Button = !premium && clientVersion >= 1200;
                m_GetPremiumLegacyButton.gameObject.SetActive(!premium && clientVersion < 1200);
                m_GetPremiumV12Button.gameObject.SetActive(showV12Button);
                m_HorizontalSeparator1.gameObject.SetActive(!premium && clientVersion >= 1148);
                m_PremiumBenefitsLabel.gameObject.SetActive(!premium && clientVersion >= 1148);
                m_PremiumBenefitsPanel.gameObject.SetActive(!premium && clientVersion >= 1148);
                m_AndManyMoreLabel.gameObject.SetActive(!premium && clientVersion >= 1148);

                m_AccountStatusLabel.text = string.Format("  <sprite=\"PremiumStatus\" index={0}> {1} Account", premium ? 1 : 0, premium ? "Premium" : "Free");
            } else {
                premium = m_CharactersList.IsPremium;
                m_GetPremiumLegacyButton.gameObject.SetActive(!m_CharactersList.IsPremium && clientVersion >= 1098);
                m_GetPremiumV12Button.gameObject.SetActive(false);
                m_HorizontalSeparator1.gameObject.SetActive(false);
                m_PremiumBenefitsLabel.gameObject.SetActive(false);
                m_PremiumBenefitsPanel.gameObject.SetActive(false);
                m_AndManyMoreLabel.gameObject.SetActive(false);

                m_AccountStatusLabel.text = string.Format("  {0} Account", premium ? "Premium" : "Free");
                m_CharactersScrollRect.GetComponent<LayoutElement>().preferredHeight = 250;
            }

            var scrollRectLayoutElement = m_CharactersScrollRect.GetComponent<LayoutElement>();
            if (premium) {
                scrollRectLayoutElement.preferredHeight = (clientVersion < 1200) ? 250 : 300;
            } else {
                scrollRectLayoutElement.preferredHeight = (clientVersion < 1200)
                    ? (clientVersion < 1148) ? 250 : 150
                    : 200;
            }
        }
        
        protected void PopupMessage(string title, string message, PopupMenuType popupType = PopupMenuType.OK, TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.MidlineGeoAligned) {
            m_PopupWindow.ShowWindow();

            m_PopupWindow.PopupType = popupType;

            m_PopupWindow.SetTitle(title);
            m_PopupWindow.SetMessage(message, 500, 250);
            m_PopupWindow.SetMessageAlignment(alignment);
        }

        protected async void DoEnterGame(string characterName, string worldAddress, string worldName, int worldPort) {
            CloseWindow();

            var gameManager = OpenTibiaUnity.GameManager;
            if (gameManager.IsLoadingClientAssets) {
                gameManager.LobbyPanel.gameObject.SetActive(false);
                gameManager.LoadingAppearancesWindow.gameObject.SetActive(true);

                while (gameManager.IsLoadingClientAssets)
                    await System.Threading.Tasks.Task.Yield();

                gameManager.LobbyPanel.gameObject.SetActive(OpenTibiaUnity.GameManager.ClientVersion >= 1200);
                gameManager.LoadingAppearancesWindow.gameObject.SetActive(false);

                if (!gameManager.HasLoadedClientAssets) {
                    var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
                    var buildVersion = OpenTibiaUnity.GameManager.BuildVersion;

                    PopupMessage("Sorry", string.Format("Couldn't load appearances for version {0}.{1}.", clientVersion / 100f, buildVersion));
                    return;
                }
            }
            
            PopupMessage("Connecting", "Connecting to the game world. Please wait.", PopupMenuType.Cancel);

            OpenTibiaUnity.ChatStorage.Reset();
            OpenTibiaUnity.ContainerStorage.Reset();
            OpenTibiaUnity.CreatureStorage.Reset();
            OpenTibiaUnity.SpellStorage.Reset();
            OpenTibiaUnity.WorldMapStorage.Reset();

            var protocolGame = new Core.Communication.Game.ProtocolGame();

            if (m_Session != null) {
                protocolGame.SessionKey = m_Session.SessionKey;
            } else {
                protocolGame.SessionKey = m_SessionKey;
                protocolGame.AccountName = m_AccountName;
                protocolGame.Password = m_Password;
                protocolGame.Token = m_Token;
            }

            protocolGame.CharacterName = characterName;
            protocolGame.WorldName = worldName;

            AddProtocolGameListeners(protocolGame);

            OpenTibiaUnity.ProtocolGame = protocolGame;
            new Task(() => protocolGame.Connect(worldAddress, worldPort)).Start();
        }
    }
}
