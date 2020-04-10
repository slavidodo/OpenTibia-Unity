using OpenTibiaUnity.Core.Communication.Login;
using System.Threading.Tasks;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    public class CharacterSelectionWidget : UI.Legacy.PopUpBase
    {
        /**
         * Notes:
         *  - Get Premium button appeared in 10.97 up to 12.00 (new position indicated)
         *  - Premium benefits appeared in 11.48
         *  - outfits appeared in tibia 12
         *  - Get Premium button was positioned horizontally aligned to account status
         *     on the opposite anchor
         */ 

        // Serializable Fields
        [SerializeField]
        private UI.Legacy.ScrollRect _characterScrollRect = null;
        [SerializeField]
        private UnityUI.ToggleGroup _characterToggleGroup = null;
        [SerializeField]
        private UI.Legacy.Button _getPremiumLegacyButton = null;
        [SerializeField]
        private UI.Legacy.Button _getPremiumV12Button = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _accountStatusLabel = null;
        [SerializeField]
        private GameObject _premiumPanel = null;

        // Fields
        private UI.Legacy.MessageWidget _messageWidget = null;
        private PlayData _playdata = null;
        private bool _popupIsAdvice = false;
        private int _loggedInCharacterIndex = -1;
        private int _pendingCharacterIndex = -1;
        private int _pendingValidUntil = -1;
        private int _selectedCharacterIndex = -1;

        protected override void Awake() {
            base.Awake();

            // todo: remove this and use default binder
            KeyMask = UI.Legacy.PopUpKeyMask.None;

            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkButtonClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelButtonClick);
        }

        protected override void Start() {
            base.Start();

            // setup events
            _getPremiumLegacyButton.onClick.AddListener(OnGetPremiumButtonClicked);
            _getPremiumV12Button.onClick.AddListener(OnGetPremiumButtonClicked);

            // setup game events
            OpenTibiaUnity.GameManager.onGameStart.AddListener(OnGameStart);
            OpenTibiaUnity.GameManager.onProcessLogoutCharacter.AddListener(OnProcessLogoutCharacter);
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
            if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _characterScrollRect.content.childCount)
                _selectedCharacterIndex = 0;

            if (_characterScrollRect.content.childCount > 0)
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(_selectedCharacterIndex));
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);
            if ((e.shift || e.control || e.alt) && (e.keyCode < KeyCode.UpArrow || e.keyCode >= KeyCode.LeftArrow))
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
                    Hide();
                    OnOkButtonClick();
                    break;
                case KeyCode.Escape:
                    e.Use();
                    Hide();
                    OnCancelButtonClick();
                    break;
            }
        }

        protected void OnOkButtonClick() {
            if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _characterScrollRect.content.childCount)
                return;

            _pendingCharacterIndex = _selectedCharacterIndex;
            if (OpenTibiaUnity.GameManager.IsGameRunning) {
                // if the pending character failed to log-in, then revert the change back
                // this will occur if the server denied the logout message
                _selectedCharacterIndex = _loggedInCharacterIndex;
                _pendingValidUntil = OpenTibiaUnity.TicksMillis + Constants.CharacterSwitchTimeout;

                OpenTibiaUnity.ProtocolGame.Disconnect(false);
            } else {
                TryLogin();
            }
        }

        protected void OnCancelButtonClick() {
            _pendingCharacterIndex = -1;
            if (!OpenTibiaUnity.GameManager.IsGameRunning)
                ModulesManager.Instance.LoginWidget.Show();
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

            // TODO show a waiting time widget & then reconnect
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }

        protected void OnGameStart() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            RemoveProtocolGameListeners(protocolGame);

            _loggedInCharacterIndex = _selectedCharacterIndex;

            if (_messageWidget != null) {
                if (_popupIsAdvice) {
                    return;
                } else {
                    _messageWidget.Hide();
                    _messageWidget = null;
                }
            }

            SwitchToGameplayCanvas();
        }

        protected void OnProcessLogoutCharacter() {
            _pendingCharacterIndex = -1;
        }

        protected void OnProcessChangeCharacter() {
            var selectedCharacterIndex = _selectedCharacterIndex;
            Show();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(selectedCharacterIndex));
        }

        protected void OnGameEnd() {
            // run a final check whether it's valid to log-in this character
            if (_pendingCharacterIndex != -1 && _pendingValidUntil <= OpenTibiaUnity.TicksMillis)
                _pendingCharacterIndex = -1;

            if (_pendingCharacterIndex != -1) {
                // it's safe now to actually mark this character as the selected one
                _selectedCharacterIndex = _pendingCharacterIndex;
                TryLogin();
            } else {
                var selectedCharacterIndex = _selectedCharacterIndex;
                Show();
                SwitchToBackgroundCanvas();
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SelectCharacterByIndex(selectedCharacterIndex));
            }
        }

        private void OnCharacterValueToggleValueChanged(AccountCharacter panel, bool value) {
            if (ChangingVisibility || !value)
                return;

            _selectedCharacterIndex = panel.transform.GetSiblingIndex();
        }

        public void Setup(PlayData playData) {
            _playdata = playData;

            foreach (Transform child in _characterScrollRect.content)
                Destroy(child.gameObject);

            int clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
            int characterCount = playData.Characters.Count;
            for (int i = 0; i < characterCount; i++) {
                var character = playData.Characters[i];
                var world = playData.FindWorld(character.WorldId);
                string worldName = world.Name;
                if (world.PreviewState == 1) {
                    if (clientVersion >= 1100)
                        worldName += " (Experimental)";
                    else
                        worldName += " (Preview)";
                }

                if (clientVersion >= 1200)
                    worldName += $"\n({world.GetPvPTypeDescription()})";

                var characterPanel = Instantiate(ModulesManager.Instance.AccountCharacterPrefab, _characterScrollRect.content);
                characterPanel.UseAlternateColor = clientVersion >= 1100;
                characterPanel.CharacterName = character.Name;
                characterPanel.WorldName = worldName;
                characterPanel.toggle.onValueChanged.AddListener((value) => OnCharacterValueToggleValueChanged(characterPanel, value));
                characterPanel.toggle.group = _characterToggleGroup;

                if (clientVersion >= 1200)
                    characterPanel.GetComponent<UnityUI.LayoutElement>().minHeight = 34;
                characterPanel.onDoubleClick.AddListener(OnOkButtonClick);
            }
        }

        private bool SelectCharacterByIndex(int index) {
            if (index >= _characterScrollRect.content.childCount)
                return false;

            var child = _characterScrollRect.content.GetChild(index);
            var characterPanel = child.GetComponent<AccountCharacter>();
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
            if (_characterScrollRect.content.childCount > 0) {
                var child = _characterScrollRect.content.GetChild(0);
                var characterPanel = child.GetComponent<AccountCharacter>();
                characterPanel.Select();
            }
        }

        protected void SelectNextCharacter() {
            if (_characterScrollRect.content.childCount > 1) {
                _selectedCharacterIndex = (++_selectedCharacterIndex) % _characterScrollRect.content.childCount;
                var child = _characterScrollRect.content.GetChild(_selectedCharacterIndex);
                var characterPanel = child.GetComponent<AccountCharacter>();
                characterPanel.Select();
                Core.Utils.UIHelper.EnsureChildVisible(_characterScrollRect, child as RectTransform);
            }
        }

        protected void SelectPreviousCharacter() {
            if (_characterScrollRect.content.childCount > 1) {
                if (_selectedCharacterIndex <= 0)
                    _selectedCharacterIndex = _characterScrollRect.content.childCount - 1;
                else
                    _selectedCharacterIndex = (_selectedCharacterIndex - 1) % _characterScrollRect.content.childCount;
                
                var child = _characterScrollRect.content.GetChild(_selectedCharacterIndex);
                var characterPanel = child.GetComponent<AccountCharacter>();
                characterPanel.Select();
                Core.Utils.UIHelper.EnsureChildVisible(_characterScrollRect, child as RectTransform);
            } else if (_characterScrollRect.content.childCount > 0 && _selectedCharacterIndex != 0) {
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

            bool premium = _playdata.Session.IsPremium;
            bool showV12Button = !premium && clientVersion >= 1200;
            _getPremiumLegacyButton.gameObject.SetActive(!premium && clientVersion < 1200);
            _getPremiumV12Button.gameObject.SetActive(showV12Button);
            _premiumPanel.gameObject.SetActive(!premium && clientVersion >= 1148);

            _accountStatusLabel.text = string.Format("  <sprite=\"PremiumStatus\" index={0}> {1} Account", premium ? 1 : 0, premium ? "Premium" : "Free");

            int preferredHeight;
            if (clientVersion >= 1200)
                preferredHeight = premium ? 300 : 200;
            else if (clientVersion >= 1148)
                preferredHeight = premium ? 250 : 150;
            else
                preferredHeight = premium ? 250 : 250;

            var scrollRectLayoutElement = _characterScrollRect.GetComponent<UnityUI.LayoutElement>();
            scrollRectLayoutElement.preferredHeight = preferredHeight;
        }

        protected UI.Legacy.MessageWidget PopupOk(string title, string message) {
            if (_messageWidget != null)
                _messageWidget.Hide();

            _messageWidget = UI.Legacy.MessageWidget.CreateOkPopUp(transform.parent, title, message, OnPopupOkClick);
            _messageWidget.maxWidth = 500;
            _messageWidget.maxHeight = 250;
            return _messageWidget;
        }

        protected UI.Legacy.MessageWidget PopupCancel(string title, string message) {
            if (_messageWidget != null)
                _messageWidget.Hide();

            _messageWidget = UI.Legacy.MessageWidget.CreateCancelPopUp(transform.parent, title, message, OnPopupCancelClick);
            _messageWidget.maxWidth = 500;
            _messageWidget.maxHeight = 250;
            return _messageWidget;
        }

        private void TryLogin() {
            var character = _playdata.Characters[_pendingCharacterIndex];
            _pendingCharacterIndex = -1;
            string characterName = character.Name;

            var world = _playdata.FindWorld(character.WorldId);
            string worldName = world.Name;
            string worldAddress = world.GetAddress(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);
            int worldPort = world.GetPort(OpenTibiaUnity.GameManager.ClientVersion, OpenTibiaUnity.GameManager.BuildVersion);

            InternalEnterGame(characterName, worldAddress, worldName, worldPort);
        }

        private async Task<bool> CheckGameAssets() {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!gameManager.HasLoadedClientAssets) {
                gameObject.SetActive(false); // hide without unlocking
                gameManager.LobbyPanel.gameObject.SetActive(false);
                gameManager.LoadingAppearancesWindow.gameObject.SetActive(true);

                if (!gameManager.IsLoadingClientAssets)
                    await gameManager.LoadThingsAsyncAwaitable(gameManager.ClientVersion, gameManager.BuildVersion, gameManager.ClientSpecification);

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
                return false;
            }

            return true;
        }

        protected async void InternalEnterGame(string characterName, string worldAddress, string worldName, int worldPort) {
            bool loaded = await CheckGameAssets();
            if (!loaded)
                return;

            PopupCancel("Connecting", "Connecting to the game world. Please wait.");

            OpenTibiaUnity.ChatStorage.Reset();
            OpenTibiaUnity.ContainerStorage.Reset();
            OpenTibiaUnity.CreatureStorage.Reset();
            OpenTibiaUnity.SpellStorage.Reset();
            OpenTibiaUnity.WorldMapStorage.Reset();

            var protocolGame = new Core.Communication.Game.ProtocolGame();
            protocolGame.CharacterName = characterName;
            protocolGame.WorldName = worldName;
            protocolGame.SessionKey = _playdata.Session.Key;
            protocolGame.AccountName = _playdata.Session.AccountName;
            protocolGame.Password = _playdata.Session.Password;
            protocolGame.Token = _playdata.Session.Token;
            AddProtocolGameListeners(protocolGame);

            OpenTibiaUnity.ProtocolGame = protocolGame;
            new Task(() => protocolGame.Connect(worldAddress, worldPort)).Start();
        }
    }
}
