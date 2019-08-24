using OpenTibiaUnity.Core.Communication.Login;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Input;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    public class CharactersWindow : Core.Components.Base.Window
    {
        public class ClosedEvent : UnityEvent { }

        /**
         * Notes:
         *  - Get Premium button appeared in 10.97 up to 12.00 (new position indicated)
         *  - Premium benefits appeared in 11.48
         *  - outfits appeared in tibia 12
         *  - Get Premium button was positioned horizontally aligned to account status
         *     on the opposite anchor
         */ 

        // Serializable Fields
        [SerializeField] private OTU_ScrollRect _charactersScrollRect = null;
        [SerializeField] private ToggleGroup _charactersToggleGroup = null;
        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private Button _getPremiumLegacyButton = null;
        [SerializeField] private Button _getPremiumV12Button = null;

        [SerializeField] private TMPro.TextMeshProUGUI _accountStatusLabel = null;

        [SerializeField] private GameObject _horizontalSeparator1 = null;
        [SerializeField] private GameObject _premiumBenefitsLabel = null;
        [SerializeField] private GameObject _premiumBenefitsPanel = null;
        [SerializeField] private GameObject _andManyMoreLabel = null;

        // Fields
        protected string _sessionKey = null;
        protected string _accountName = null;
        protected string _password = null;
        protected string _token = null;
        protected CharacterList _charactersList = null;
        protected bool _popupIsAdvice = false;

        protected Playdata _playdata = null;
        protected Session _session = null;

        protected int _selectedCharacterIndex = -1;
        protected Core.Components.PopupWindow _popupWindow = null;
        
        protected override void Start() {
            base.Start();

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utils.EventImplPriority.High, OnKeyDown);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utils.EventImplPriority.High, OnKeyUp);

            // setup events
            _oKButton.onClick.AddListener(OnOkButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
            _getPremiumLegacyButton.onClick.AddListener(OnGetPremiumButtonClicked);
            _getPremiumV12Button.onClick.AddListener(OnGetPremiumButtonClicked);
            
            // setup popup message
            _popupWindow = Instantiate(OpenTibiaUnity.GameManager.PopupWindowPrefab, transform.parent);
            _popupWindow.name = "PopupWindow_CharactersWindow";
            _popupWindow.Hide();

            _popupWindow.onOKClick.AddListener(OnPopupOkClick);
            _popupWindow.onCancelClick.AddListener(OnPopupCancelClick);

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
            if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _charactersScrollRect.content.childCount)
                return;

            var child = _charactersScrollRect.content.GetChild(_selectedCharacterIndex);
            var characterPanel = child.GetComponent<CharacterPanel>();

            string characterName;
            string worldName;
            string worldAddress;
            int worldPort;

            if (_playdata != null) {
                var character = _playdata.Characters[_selectedCharacterIndex];
                characterName = character.Name;

                var world = _playdata.FindWorld(character.WorldId);
                worldName = world.Name;
                worldAddress = world.GetAddress(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);
                worldPort = world.GetPort(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);
            } else {
                var character = _charactersList.Characters[_selectedCharacterIndex];
                characterName = character.Name;

                var world = _charactersList.FindWorld(character.WorldId);
                worldName = world.Name;
                worldAddress = world.HostName;
                worldPort = world.Port;
            }

            DoEnterGame(characterName, worldAddress, worldName, worldPort);
        }

        protected void OnCancelButtonClick() {
            Close();
        }

        protected void OnGetPremiumButtonClicked() {
            // do nothing
            // this is left up to the desire of the client user
        }

        protected void OnPopupOkClick() {
            Show();

            if (_popupIsAdvice) {
                _popupIsAdvice = false;
                
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

            Show();
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
            _popupIsAdvice = true;
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

            if (_popupWindow.Visible) {
                if (_popupIsAdvice)
                    return;
                else
                    _popupWindow.Close();
            }

            SwitchToGameplayCanvas();
        }

        protected void OnProcessChangeCharacter() {
            Open();
        }

        protected void OnGameEnd() {
            gameObject.SetActive(true);
            ResetLocalPosition();
            LockToOverlay();

            SwitchToBackgroundCanvas();
        }


        public void Setup(string sessionKey, string accountName, string password, string token, CharacterList characterList) {
            _sessionKey = sessionKey;
            _accountName = accountName;
            _password = password;
            _token = token;
            _charactersList = characterList;
            _session = null;
            _playdata = null;

            var content = _charactersScrollRect.content;
            foreach (Transform child in content)
                Destroy(child.gameObject);
            
            for (int i = 0; i < characterList.Characters.Count; i++) {
                int characterIndex = i;

                var character = characterList.Characters[i];
                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab);
                characterPanel.transform.SetParent(content);
                characterPanel.characterName.text = character.Name;
                characterPanel.worldName.text = characterList.FindWorld(character.WorldId).Name;
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => { if (value) _selectedCharacterIndex = characterIndex; });
                characterPanel.toggleComponent.group = _charactersToggleGroup;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            _selectedCharacterIndex = -1;
        }

        public void Setup(Session session, Playdata playData) {
            _session = session;
            _playdata = playData;
            _sessionKey = null;
            _accountName = null;
            _password = null;
            _token = null;

            var content = _charactersScrollRect.content;
            foreach (Transform child in content)
                Destroy(child.gameObject);
            
            for (int i = 0; i < playData.Characters.Count; i++) {
                int characterIndex = i;

                var character = playData.Characters[i];
                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab);
                characterPanel.GetComponent<LayoutElement>().minHeight = 34;

                characterPanel.transform.SetParent(content);
                characterPanel.characterName.text = character.Name;

                var world = playData.FindWorld(character.WorldId);

                characterPanel.worldName.text = string.Format("{0}\n({1})", world.Name, world.GetPvPTypeDescription());
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => { if (value) _selectedCharacterIndex = characterIndex; });
                characterPanel.toggleComponent.group = _charactersToggleGroup;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            _selectedCharacterIndex = -1;
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
            if (_charactersScrollRect.content.childCount > 0) {
                var child = _charactersScrollRect.content.GetChild(0);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            }
        }

        protected void SelectNextCharacter() {
            if (_charactersScrollRect.content.childCount > 1) {
                _selectedCharacterIndex = (++_selectedCharacterIndex) % _charactersScrollRect.content.childCount;
                var child = _charactersScrollRect.content.GetChild(_selectedCharacterIndex);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            }
        }

        protected void SelectPreviousCharacter() {
            if (_charactersScrollRect.content.childCount > 1) {
                if (_selectedCharacterIndex <= 0)
                    _selectedCharacterIndex = _charactersScrollRect.content.childCount - 1;
                else
                    _selectedCharacterIndex = (_selectedCharacterIndex - 1) % _charactersScrollRect.content.childCount;
                
                var child = _charactersScrollRect.content.GetChild(_selectedCharacterIndex);
                var characterPanel = child.GetComponent<CharacterPanel>();
                characterPanel.Select();
            } else if (_charactersScrollRect.content.childCount > 0 && _selectedCharacterIndex != 0) {
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
            if (_playdata != null) {
                premium = _session.IsPremium;
                bool showV12Button = !premium && clientVersion >= 1200;
                _getPremiumLegacyButton.gameObject.SetActive(!premium && clientVersion < 1200);
                _getPremiumV12Button.gameObject.SetActive(showV12Button);
                _horizontalSeparator1.gameObject.SetActive(!premium && clientVersion >= 1148);
                _premiumBenefitsLabel.gameObject.SetActive(!premium && clientVersion >= 1148);
                _premiumBenefitsPanel.gameObject.SetActive(!premium && clientVersion >= 1148);
                _andManyMoreLabel.gameObject.SetActive(!premium && clientVersion >= 1148);

                _accountStatusLabel.text = string.Format("  <sprite=\"PremiumStatus\" index={0}> {1} Account", premium ? 1 : 0, premium ? "Premium" : "Free");
            } else {
                premium = _charactersList.IsPremium;
                _getPremiumLegacyButton.gameObject.SetActive(!_charactersList.IsPremium && clientVersion >= 1098);
                _getPremiumV12Button.gameObject.SetActive(false);
                _horizontalSeparator1.gameObject.SetActive(false);
                _premiumBenefitsLabel.gameObject.SetActive(false);
                _premiumBenefitsPanel.gameObject.SetActive(false);
                _andManyMoreLabel.gameObject.SetActive(false);

                _accountStatusLabel.text = string.Format("  {0} Account", premium ? "Premium" : "Free");
                _charactersScrollRect.GetComponent<LayoutElement>().preferredHeight = 250;
            }

            var scrollRectLayoutElement = _charactersScrollRect.GetComponent<LayoutElement>();
            if (premium) {
                scrollRectLayoutElement.preferredHeight = (clientVersion < 1200) ? 250 : 300;
            } else {
                scrollRectLayoutElement.preferredHeight = (clientVersion < 1200)
                    ? (clientVersion < 1148) ? 250 : 150
                    : 200;
            }
        }
        
        protected void PopupMessage(string title, string message, PopupMenuType popupType = PopupMenuType.OK, TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.MidlineGeoAligned) {
            _popupWindow.Show();

            _popupWindow.PopupType = popupType;

            _popupWindow.SetTitle(title);
            _popupWindow.SetMessage(message, 500, 250);
            _popupWindow.SetMessageAlignment(alignment);
        }

        protected async void DoEnterGame(string characterName, string worldAddress, string worldName, int worldPort) {
            Close();

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

            if (_session != null) {
                protocolGame.SessionKey = _session.SessionKey;
            } else {
                protocolGame.SessionKey = _sessionKey;
                protocolGame.AccountName = _accountName;
                protocolGame.Password = _password;
                protocolGame.Token = _token;
            }

            protocolGame.CharacterName = characterName;
            protocolGame.WorldName = worldName;

            AddProtocolGameListeners(protocolGame);

            OpenTibiaUnity.ProtocolGame = protocolGame;
            new Task(() => protocolGame.Connect(worldAddress, worldPort)).Start();
        }
    }
}
