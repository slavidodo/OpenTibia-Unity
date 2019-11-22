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
        protected PopupWindow _popupWindow = null;

        protected override void Awake() {
            base.Awake();

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utils.EventImplPriority.High, OnKeyDown);
        }

        protected override void Start() {
            base.Start();

            // setup events
            _oKButton.onClick.AddListener(OnOkButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
            _getPremiumLegacyButton.onClick.AddListener(OnGetPremiumButtonClicked);
            _getPremiumV12Button.onClick.AddListener(OnGetPremiumButtonClicked);

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

        protected override void OnEnable() {
            base.OnEnable();

            // select first element
            if (_charactersScrollRect.content.childCount > 0)
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(_selectedCharacterIndex));
        }

        protected void OnKeyDown(Event e, bool _) {
            if ((e.shift || e.control || e.alt) && (e.keyCode < KeyCode.UpArrow || e.keyCode >= KeyCode.LeftArrow))
                return;

            if (!InputHandler.IsHighlighted(this))
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
                    e.Use();
                    OnOkButtonClick();
                    break;
                case KeyCode.Escape:
                    e.Use();
                    OnCancelButtonClick();
                    break;
            }
        }

        protected void OnOkButtonClick() {
            if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _charactersScrollRect.content.childCount)
                return;

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
                
                if (OpenTibiaUnity.GameManager.IsGameRunning)
                    SwitchToGameplayCanvas();
            }
        }

        protected void OnPopupCancelClick() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame) {
                RemoveProtocolGameListeners(protocolGame);
                protocolGame.Disconnect();
                OpenTibiaUnity.ProtocolGame = null;
            }

            Show();
        }

        protected void OnProtocolGameConnectionError(string message, bool disconnected) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            if (!disconnected)
                protocolGame.Disconnect();

            OpenTibiaUnity.ProtocolGame = null;
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }

        protected void OnProtocolGameLoginError(string message) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            protocolGame.Disconnect();
            OpenTibiaUnity.ProtocolGame = null;

            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }

        protected void OnProtocolGameLoginAdvice(string advice) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("For your information", advice));
            _popupIsAdvice = true;
        }

        protected void OnProtocolGameLoginWait(string message, int time) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);
            protocolGame.Disconnect();
            OpenTibiaUnity.ProtocolGame = null;

            // TODO show a waiting time widget & then reconnect //
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }

        protected void OnGameStart() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);

            if (_popupWindow != null) {
                if (_popupIsAdvice) {
                    return;
                } else {
                    _popupWindow.Destroy();
                    _popupWindow = null;
                }
            }

            SwitchToGameplayCanvas();
        }

        protected void OnProcessChangeCharacter() {
            var selectionCharacterIndex = _selectedCharacterIndex;
            Open();
            SwitchToGameplayCanvas();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(selectionCharacterIndex));
        }

        protected void OnGameEnd() {
            var selectionCharacterIndex = _selectedCharacterIndex;
            Open();
            SwitchToBackgroundCanvas();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(selectionCharacterIndex));
        }

        private void OnCharacterValueToggleValueChanged(CharacterPanel panel, bool value) {
            if (_changingVisibility || !value)
                return;

            _selectedCharacterIndex = panel.transform.GetSiblingIndex();
        }

        public void Setup(string sessionKey, string accountName, string password, string token, CharacterList characterList) {
            _sessionKey = sessionKey;
            _accountName = accountName;
            _password = password;
            _token = token;
            _charactersList = characterList;
            _session = null;
            _playdata = null;

            foreach (Transform child in _charactersScrollRect.content)
                Destroy(child.gameObject);

            int characterCount = characterList.Characters.Count;
            for (int i = 0; i < characterCount; i++) {
                var character = characterList.Characters[i];
                var world = characterList.FindWorld(character.WorldId);
                string worldName = world.Name;
                if (world.Preview)
                    worldName += " (Preview)";

                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab, _charactersScrollRect.content);
                characterPanel.ColorReversed = characterCount % 3 == 0;
                characterPanel.characterName.text = character.Name;
                characterPanel.worldName.text = worldName;
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => OnCharacterValueToggleValueChanged(characterPanel, value));
                characterPanel.toggleComponent.group = _charactersToggleGroup;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(0));
        }

        public void Setup(Session session, Playdata playData) {
            _session = session;
            _playdata = playData;
            _sessionKey = null;
            _accountName = null;
            _password = null;
            _token = null;

            foreach (Transform child in _charactersScrollRect.content)
                Destroy(child.gameObject);

            int characterCount = playData.Characters.Count;
            for (int i = 0; i < characterCount; i++) {
                var character = playData.Characters[i];
                var world = playData.FindWorld(character.WorldId);
                string worldName = world.Name;
                if (world.PreviewState == 1)
                    worldName += " (Experimental)";
                if (OpenTibiaUnity.GameManager.ClientVersion >= 1200)
                    worldName += $"\n({world.GetPvPTypeDescription()})";

                var characterPanel = Instantiate(ModulesManager.Instance.CharacterPanelPrefab, _charactersScrollRect.content);
                characterPanel.ColorReversed = characterCount % 3 == 0;
                characterPanel.characterName.text = character.Name;
                characterPanel.worldName.text = worldName;
                characterPanel.toggleComponent.onValueChanged.AddListener((value) => OnCharacterValueToggleValueChanged(characterPanel, value));
                characterPanel.toggleComponent.group = _charactersToggleGroup;
                characterPanel.GetComponent<LayoutElement>().minHeight = 34;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }

            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(0));
        }

        private bool SelectCharacterByIndex(int index) {
            if (index >= _charactersScrollRect.content.childCount)
                return false;

            var child = _charactersScrollRect.content.GetChild(index);
            var characterPanel = child.GetComponent<CharacterPanel>();
            characterPanel.Select();

            _selectedCharacterIndex = index;
            return true;
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
                Core.Utils.UIHelper.EnsureChildVisible(_charactersScrollRect, child as RectTransform);
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
                Core.Utils.UIHelper.EnsureChildVisible(_charactersScrollRect, child as RectTransform);
            } else if (_charactersScrollRect.content.childCount > 0 && _selectedCharacterIndex != 0) {
                SelectFirstCharacter();
            }
        }

        protected void SwitchToGameplayCanvas() {
            OpenTibiaUnity.GameManager.BackgroundCanvas.gameObject.SetActive(false);
            OpenTibiaUnity.GameManager.EventSystem.SetSelectedGameObject(OpenTibiaUnity.GameManager.GameCanvas.gameObject);
        }

        protected void SwitchToBackgroundCanvas() {
            if (OpenTibiaUnity.GameManager.BackgroundCanvas) {
                OpenTibiaUnity.GameManager.BackgroundCanvas.gameObject.SetActive(true);
            }

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

        protected PopupWindow PopupOk(string title, string message) {
            if (_popupWindow != null)
                _popupWindow.Destroy();

            _popupWindow = PopupWindow.CreateOkPopup(transform.parent, title, message, OnPopupOkClick);
            _popupWindow.SetMessage(message, 500, 250);
            return _popupWindow;
        }

        protected PopupWindow PopupCancel(string title, string message) {
            if (_popupWindow != null)
                _popupWindow.Destroy();

            _popupWindow = PopupWindow.CreateCancelPopup(transform.parent, title, message, OnPopupOkClick);
            _popupWindow.SetMessage(message, 500, 250);
            return _popupWindow;
        }

        protected async void DoEnterGame(string characterName, string worldAddress, string worldName, int worldPort) {
            CloseWithoutNotify();

            var gameManager = OpenTibiaUnity.GameManager;
            if (!gameManager.IsLoadingClientAssets && !gameManager.HasLoadedClientAssets) {
                await gameManager.LoadThingsAsyncAwaitable(gameManager.ClientVersion, gameManager.BuildVersion, gameManager.ClientSpecification);
            }

            if (gameManager.IsLoadingClientAssets) {
                gameManager.LobbyPanel.gameObject.SetActive(false);
                gameManager.LoadingAppearancesWindow.gameObject.SetActive(true);

                while (gameManager.IsLoadingClientAssets)
                    await Task.Yield();

                gameManager.LobbyPanel.gameObject.SetActive(OpenTibiaUnity.GameManager.ClientVersion >= 1200);
                gameManager.LoadingAppearancesWindow.gameObject.SetActive(false);
            }

            if (!gameManager.HasLoadedClientAssets) {
                var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
                var buildVersion = OpenTibiaUnity.GameManager.BuildVersion;
                string versionLiteral;
                if (clientVersion >= 1100)
                    versionLiteral = $"{clientVersion}.{buildVersion}";
                else
                    versionLiteral = clientVersion.ToString();

                PopupOk("Sorry", string.Format($"Couldn't load appearances for version {versionLiteral}."));
                return;
            }

            PopupCancel("Connecting", "Connecting to the game world. Please wait.");

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
