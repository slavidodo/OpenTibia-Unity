using OpenTibiaUnity.Core.Input;
using System;
using UnityEngine;
using UnityEngine.UI;
using OpenTibiaUnity.Core.Communication.Login;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Modules.Login
{
    public class LoginWindow : Core.Components.Base.Window
    {
        // Serializable Fields
        [SerializeField] protected RawImage _backgroundImageComponent = null;
        [SerializeField] protected AspectRatioFitter _backgroundAspectRatioFitter = null;

        [SerializeField] protected TMPro.TextMeshProUGUI _accountIdentifierLabel = null;
        [SerializeField] protected TMPro.TMP_InputField _accountIdentifierInput = null;
        [SerializeField] protected TMPro.TMP_InputField _passwordInput = null;
        [SerializeField] protected TMPro.TMP_InputField _tokenInput = null;
        [SerializeField] protected TMPro.TMP_InputField _addressInput = null; // ip-address or login web address

        [SerializeField] protected TMPro.TextMeshProUGUI _addressLabel = null;

        [SerializeField] protected TMPro.TMP_Dropdown _clientVersionDropdown = null;
        [SerializeField] protected TMPro.TMP_Dropdown _buildVersionDropdown = null;

        [SerializeField] private Button _authButton = null;
        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;

        [SerializeField] private RectTransform _panelAccountManagementV11 = null;
        [SerializeField] private Core.Components.Draggable _draggableComponent = null;

        [SerializeField] private Texture2D _clientArtwork700 = null;
        [SerializeField] private Texture2D _clientArtwork800 = null;
        [SerializeField] private Texture2D _clientArtwork910 = null;
        [SerializeField] private Texture2D _clientArtwork1020 = null;
        [SerializeField] private Texture2D _clientArtwork1100 = null;
        [SerializeField] private Texture2D _clientArtwork1103 = null;
        [SerializeField] private Texture2D _clientArtwork1140 = null;
        [SerializeField] private Texture2D _clientArtwork1150 = null;
        [SerializeField] private Texture2D _clientArtwork1180 = null;
        [SerializeField] private Texture2D _clientArtwork1200 = null;

        // Fields
        protected string _sessionKey = string.Empty;
        protected string _protocolAccountIdentifier = string.Empty;
        protected string _protocolPassword = string.Empty;
        protected string _protocolToken = string.Empty;

        protected ProtocolLogin _protocolLogin = null;
        protected LoginWebClient _loginWebClient = null;

        protected Core.Components.PopupWindow _popupWindow;
        protected bool _popupIsMOTD = false;

        // Properties
        protected string AcccountIdentifier {
            get => _accountIdentifierInput.text;
            set => _accountIdentifierInput.text = value;
        }
        protected string Password {
            get => _passwordInput.text;
            set => _passwordInput.text = value;
        }
        protected string Token {
            get => _tokenInput.text;
            set => _tokenInput.text = value;
        }
        protected string Address {
            get => _addressInput.text;
            set => _addressInput.text = value;
        }

        protected override void Awake() {
            base.Awake();

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utils.EventImplPriority.High, OnKeyDown);
        }

        protected override void Start() {
            base.Start();

            // setup events
            _authButton.onClick.AddListener(OnAuthButtonClick);
            _oKButton.onClick.AddListener(OnOkButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
            _clientVersionDropdown.onValueChanged.AddListener(OnClientVersionDropdownValueChanged);
            _buildVersionDropdown.onValueChanged.AddListener(OnBuildVersionDropdownValueChanged);

            // setup client versions
            var options = new System.Collections.Generic.List<TMPro.TMP_Dropdown.OptionData>();
            foreach (int version in OpenTibiaUnity.GetClientVersions())
                options.Add(new TMPro.TMP_Dropdown.OptionData(version.ToString()));
            _clientVersionDropdown.options = options;

            // setup our client with loaded options
            SetupWithOptions();
        }

        protected override void OnEnable() {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => _accountIdentifierInput.Select());
        }

        protected void OnKeyDown(Event e, bool _) {
            if (e.alt || (e.shift && e.keyCode != KeyCode.Tab) || e.control || !InputHandler.IsHighlighted(this))
                return;
            
            switch (e.keyCode) {
                case KeyCode.Tab:
                    e.Use();

                    var inputFields = new TMPro.TMP_InputField[] {
                        _accountIdentifierInput,
                        _passwordInput,
                        _tokenInput,
                        _addressInput,
                    };

                    int direction = e.shift ? -1 : 1;
                    bool found = false;
                    for (int i = 0; i < inputFields.Length; i++) {
                        if (InputHandler.IsHighlighted(inputFields[i])) {
                            for (int j = i + direction; Mathf.Abs(j - i) <= inputFields.Length; j += direction) {
                                int newIndex = j % inputFields.Length;
                                if (newIndex < 0)
                                    newIndex += inputFields.Length;

                                var newField = inputFields[newIndex];
                                if (newField.gameObject.activeInHierarchy) {
                                    found = true;
                                    newField.Select();
                                    newField.MoveTextEnd(false);
                                    break;
                                }
                            };
                            break;
                        }
                    }

                    if (!found) {
                        inputFields[0].Select();
                        inputFields[0].MoveTextEnd(false);
                    }

                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    e.Use();
                    OnOkButtonClick();
                    break;
            }
        }

        protected void OnAuthButtonClick() {

        }

        protected void OnOkButtonClick() {
            if (!InputHandler.IsHighlighted(this))
                return;

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1100 && AcccountIdentifier.Length == 0)
                return;

            DoLogin(AcccountIdentifier, Password, Token, Address);
        }

        protected void OnCancelButtonClick() {
        }

        protected void OnClientVersionDropdownValueChanged(int value) {
            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            int oldVersion = gameManager.ClientVersion;
            int newVersion = OpenTibiaUnity.GetClientVersions()[value];

            gameManager.SetClientVersion(newVersion);

            // check character validation
            // on older protocols, account name consisted only of numbers
            var oldValidation = _accountIdentifierInput.characterValidation;
            if (gameManager.GetFeature(GameFeature.GameAccountNames))
                _accountIdentifierInput.characterValidation = TMPro.TMP_InputField.CharacterValidation.None;
            else
                _accountIdentifierInput.characterValidation = TMPro.TMP_InputField.CharacterValidation.Integer;

            if (oldValidation != _accountIdentifierInput.characterValidation)
                _accountIdentifierInput.text = string.Empty;
            
            // check authenticator button
            // on tibia 11, authenticator button was hidden until it's required
            // on tibia 10, the player has the ability to switch it on/off
            bool authButtonSupported = gameManager.GetFeature(GameFeature.GameAuthenticator) && newVersion < 1100;
            _authButton.gameObject.SetActive(authButtonSupported);
            if (authButtonSupported && optionStorage.AuthenticatorTokenOn) {
                _tokenInput.transform.parent.gameObject.SetActive(true);
                _tokenInput.text = "";
                _authButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_AUTHBUTTON_ON);
            } else {
                _tokenInput.transform.parent.gameObject.SetActive(false);
                _authButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_AUTHBUTTON_OFF);
                optionStorage.AuthenticatorTokenOn = false;
            }
            
            // check address label, okButton & cancelButton
            // on tibia 11 & later versions, we use a login web client
            // on earlier versions (10, 9, 8, 7.x) we use protocols, thus requires address & port
            if ((oldVersion < 1100 && newVersion >= 1100) || (oldVersion >= 1100 && newVersion < 1100)) {
                _cancelButton.gameObject.SetActive(newVersion < 1100);
                _oKButton.GetComponent<LayoutElement>().preferredWidth = newVersion < 1100 ? 50 : 100;
                if (newVersion < 1100) {
                    _oKButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_OKBUTTON_LEGACY);
                    _addressLabel.SetText(TextResources.IP_ADDRESS_LABEL);
                    _addressInput.placeholder.GetComponent<TMPro.TextMeshProUGUI>().SetText(Constants.OpenTibiaDefaultFullIPAddress);
                } else {
                    _oKButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_OKBUTTON_NEW);
                    _addressLabel.SetText(TextResources.LOGIN_WEB_ADDRESS_LABEL);
                    _addressInput.placeholder.GetComponent<TMPro.TextMeshProUGUI>().SetText(Constants.OpenTibiaDefaultClientServicesAddress);
                }
            }

            // check abilities of the login window
            // reset the window the the center of the screen
            // the player can drag the window if it's not tibia 11
            ResetLocalPosition();
            _draggableComponent.enabled = newVersion < 1100;

            // check build versions
            // build version is supported from tibia 11 and later versions
            var buildVersions = OpenTibiaUnity.GetBuildVersions(newVersion); // == null on tibia 10
            _buildVersionDropdown.gameObject.SetActive(buildVersions != null);

            int buildVersion = 0;
            if (buildVersions != null) {
                var options = new System.Collections.Generic.List<string>();
                foreach (var version in buildVersions) {
                    string text;
                    if (OpenTibiaUnity.IsTestBuildVersion(version))
                        text = string.Format("{0} <pos=70%><color=#{1:x}>Test</color></pos>", version, Core.Chat.MessageColors.Red);
                    else
                        text = version.ToString();
                    options.Add(text);
                }
                
                _buildVersionDropdown.ClearOptions();
                _buildVersionDropdown.AddOptions(options);
                OpenTibiaUnity.GameManager.SetBuildVersion(buildVersions[0]);
                buildVersion = buildVersions[0];
            }

            // now update protocol version
            // this is required for login purposes later
            gameManager.SetProtocolVersion(OpenTibiaUnity.GetProtocolVersion(newVersion, buildVersion));

            // update the layout of clientVersions dropdown
            var clientVersionsRectTransform = _clientVersionDropdown.GetComponent<RectTransform>();
            var anchorMin = clientVersionsRectTransform.anchorMin;
            var anchorMax = clientVersionsRectTransform.anchorMax;
            if (newVersion >= 1100) {
                clientVersionsRectTransform.anchorMin = new Vector2(0.0f, anchorMin.y);
                clientVersionsRectTransform.anchorMax = new Vector2(0.5f, anchorMax.y);
            } else {
                clientVersionsRectTransform.anchorMin = new Vector2(0.25f, anchorMin.y);
                clientVersionsRectTransform.anchorMax = new Vector2(0.8f, anchorMax.y);
            }

            // the manage account panel should only be visible on tibia 11
            _panelAccountManagementV11.gameObject.SetActive(newVersion >= 1100);

            // update background image
            Texture2D newTexture;
            if (newVersion < 800)
                newTexture = _clientArtwork700;
            else if (newVersion < 910)
                newTexture = _clientArtwork800;
            else if (newVersion < 1020)
                newTexture = _clientArtwork910;
            else if (newVersion < 1100)
                newTexture = _clientArtwork1020;
            else if (newVersion < 1103)
                newTexture = _clientArtwork1100;
            else if (newVersion < 1140)
                newTexture = _clientArtwork1103;
            else if (newVersion < 1150)
                newTexture = _clientArtwork1140;
            else if (newVersion < 1180)
                newTexture = _clientArtwork1150;
            else if (newVersion < 1200)
                newTexture = _clientArtwork1180;
            else
                newTexture = _clientArtwork1200;

            _backgroundImageComponent.texture = newTexture;
            _backgroundImageComponent.enabled = true;

            float aspectRatio = 1.6f;
            if (newVersion < 1020)
                aspectRatio = Screen.width / Screen.height;

            _backgroundAspectRatioFitter.aspectRatio = aspectRatio;

            // update based on both client version & build version
            OnStaticVersionChange();

            // update options
            OpenTibiaUnity.OptionStorage.SelectedClientVersion = newVersion;
        }

        protected void OnBuildVersionDropdownValueChanged(int value) {
            var gameManager = OpenTibiaUnity.GameManager;
            int clientVersion = gameManager.ClientVersion;
            //int oldVersion = gameManager.BuildVersion;

            var buildVersions = OpenTibiaUnity.GetBuildVersions(clientVersion);
            int newVersion = buildVersions != null ? buildVersions[value] : 0;
            
            gameManager.SetBuildVersion(newVersion);

            // update based on both client version & build version
            OnStaticVersionChange();

            // update options
            OpenTibiaUnity.OptionStorage.SelectedBuildVersion = newVersion;
        }

        protected void OnStaticVersionChange() {
            var gameManager = OpenTibiaUnity.GameManager;

            string newAccountIdentifierLabel;
            if (gameManager.GetFeature(GameFeature.GameAccountEmailAddress))
                newAccountIdentifierLabel = TextResources.ACCOUNT_IDENTIFIER_EMAIL;
            else if (gameManager.GetFeature(GameFeature.GameAccountNames))
                newAccountIdentifierLabel = TextResources.ACCOUNT_IDENTIFIER_ACCOUNTNAME;
            else
                newAccountIdentifierLabel = TextResources.ACCOUNT_IDENTIFIER_ACCOUNTNUMBER;

            if (_accountIdentifierLabel.text != newAccountIdentifierLabel)
                _accountIdentifierLabel.text = newAccountIdentifierLabel;
        }

        protected void OnPopupOkClick() {
            _popupWindow = null;

            if (_loginWebClient) {
                RemoveLoginWebClientListeners();
                _loginWebClient = null;
            }

            if (_protocolLogin) {
                RemoveProtocolLoginListeners();
                _protocolLogin.Disconnect();
                _protocolLogin = null;
            }

            if (_popupIsMOTD) { // up to 10.99, motd can be displayed!
                _popupIsMOTD = false;
                Hide();

                ModulesManager.Instance.CharactersWindow.Open();
            } else {
                Show();

                _accountIdentifierInput.Select();
            }
        }

        protected void OnPopupCancelClick() {
            _popupWindow = null;

            if (_loginWebClient) {
                RemoveLoginWebClientListeners();
                _loginWebClient = null;
            }

            if (_protocolLogin) {
                RemoveProtocolLoginListeners();
                _protocolLogin.Disconnect();
                _protocolLogin = null;
            }

            gameObject.SetActive(true);
            _accountIdentifierInput.Select();
            ResetLocalPosition();
        }

        protected void SetupWithOptions() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            var gameManager = OpenTibiaUnity.GameManager;
            var supportedVersions = OpenTibiaUnity.GetClientVersions();

            // update client version
            if (optionStorage.SelectedClientVersion > 0) {
                int foundIndex = -1;
                for (int i = 0; i < supportedVersions.Length; i++) {
                    if (supportedVersions[i] == optionStorage.SelectedClientVersion)
                        foundIndex = i;
                }

                if (foundIndex != -1) {
                    _clientVersionDropdown.value = foundIndex;
                } else {
                    OnClientVersionDropdownValueChanged(0);
                    optionStorage.SelectedClientVersion = gameManager.ClientVersion;
                }
            } else {
                OnClientVersionDropdownValueChanged(0);
            }

            // update build version
            if (optionStorage.SelectedBuildVersion > 0) {
                var buildVersions = OpenTibiaUnity.GetBuildVersions(gameManager.ClientVersion);

                int foundIndex = -1;
                if (buildVersions != null) {
                    for (int i = 0; i < buildVersions.Length; i++) {
                        if (buildVersions[i] == optionStorage.SelectedBuildVersion)
                            foundIndex = i;
                    }
                }

                if (foundIndex != -1) {
                    _buildVersionDropdown.value = foundIndex;
                } else {
                    OnBuildVersionDropdownValueChanged(0);
                    optionStorage.SelectedBuildVersion = gameManager.BuildVersion;
                }
            } else {
                OnBuildVersionDropdownValueChanged(0);
            }

            // update login address
            _addressInput.text = optionStorage.LoginAddress;

            // todo: save account name & password encrypted //
        }

        protected void PopupOk(string title, string message) {
            if (_popupWindow != null)
                _popupWindow.Destroy();

            _popupWindow = Core.Components.PopupWindow.CreateOkPopup(transform.parent, title, message, OnPopupOkClick);
            _popupWindow.SetMessage(message, 500, 250);
        }

        protected void PopupCancel(string title, string message) {
            if (_popupWindow != null)
                _popupWindow.Destroy();

            _popupWindow = Core.Components.PopupWindow.CreateCancelPopup(transform.parent, title, message, OnPopupOkClick);
            _popupWindow.SetMessage(message, 500, 250);
        }

        protected async void DoLogin(string accountIdentifier, string password, string token, string address) {
            _protocolAccountIdentifier = accountIdentifier;
            _protocolPassword = password;
            _protocolToken = token;

            AcccountIdentifier = string.Empty;
            Password = string.Empty;
            Token = string.Empty;

            OpenTibiaUnity.OptionStorage.LoginAddress = address;

            var gameManager = OpenTibiaUnity.GameManager;
            int clientVersion = gameManager.ClientVersion;
            int buildVersion = gameManager.BuildVersion;

            var specification = ClientSpecification.OpenTibia;

            string versionLiteral;
            if (clientVersion >= 1100)
                versionLiteral = $"{clientVersion}.{buildVersion}";
            else
                versionLiteral = clientVersion.ToString();

            if (clientVersion >= 1200) {
                if (address.ToLower() == "cipsoft")
                    specification = ClientSpecification.Cipsoft;

                gameObject.SetActive(false);
                gameManager.LoadingAppearancesWindow.Open();
                bool loaded = await gameManager.LoadThingsAsyncAwaitable(clientVersion, buildVersion, specification);
                gameManager.LoadingAppearancesWindow.Close();
                gameObject.SetActive(true);

                if (!loaded) {
                    PopupOk("Sorry", $"Couldn't load appearances for version {versionLiteral}.");
                    return;
                }
            } else {
                if (!gameManager.CanLoadThings(clientVersion, buildVersion, specification)) {
                    PopupOk("Sorry", $"Couldn't load appearances for version {versionLiteral}.");
                    return;
                }

                gameManager.LoadThingsAsync(clientVersion, buildVersion, specification);
            }

            gameManager.SetClientSpecification(specification);
            if (clientVersion >= 1100) {
                _loginWebClient = new LoginWebClient(clientVersion, buildVersion) {
                    AccountIdentifier = _protocolAccountIdentifier,
                    Password = _protocolPassword,
                    Token = _protocolToken,
                };

                if (specification == ClientSpecification.Cipsoft)
                    address = Constants.RealTibiaClientServicesAddress;
                else if (address.Length == 0)
                    address = Constants.OpenTibiaDefaultClientServicesAddress;
                
                AddLoginWebClientListeners();
                new Task(() => _loginWebClient.LoadDataAsync(address)).Start();
            } else {
                string ip;
                int port = 0;
                
                if (address.Length != 0) {
                    var split = address.Split(':');
                    ip = split[0];
                    if (split.Length < 2 || !int.TryParse(split[1], out port))
                        port = Constants.OpenTibiaDefaultPort;
                } else {
                    ip = Constants.OpenTibiaDefaultIPAddress;
                    port = Constants.OpenTibiaDefaultPort;
                }
                
                _protocolLogin = new ProtocolLogin() {
                    AccountName = _protocolAccountIdentifier,
                    Password = _protocolPassword,
                    Token = _protocolToken
                };

                AddProtocolLoginListners();
                new Task(() => _protocolLogin.Connect(ip, port)).Start();
            }

            gameObject.SetActive(clientVersion >= 1100);
            PopupCancel("Connecting", "Your character list is being loaded. Please wait.");
        }

        public void DoLoginWithNewToken(string token) {
            DoLogin(_protocolAccountIdentifier, _protocolPassword, token, OpenTibiaUnity.OptionStorage.LoginAddress);
        }

        private void AddLoginWebClientListeners() {
            if (_loginWebClient) {
                _loginWebClient.onTechnicalError.AddListener(OnLoginWebClientTechnicalError);
                _loginWebClient.onLoginError.AddListener(OnLoginWebClientLoginError);
                _loginWebClient.onTokenError.AddListener(OnLoginWebClientTokenError);
                _loginWebClient.onLoginSuccess.AddListener(OnLoginWebClientSuccess);
            }
        }

        private void RemoveLoginWebClientListeners() {
            if (_loginWebClient) {
                _loginWebClient.onTechnicalError.RemoveListener(OnLoginWebClientTechnicalError);
                _loginWebClient.onLoginError.RemoveListener(OnLoginWebClientLoginError);
                _loginWebClient.onTokenError.RemoveListener(OnLoginWebClientTokenError);
                _loginWebClient.onLoginSuccess.RemoveListener(OnLoginWebClientSuccess);
            }
        }

        private void AddProtocolLoginListners() {
            if (_protocolLogin) {
                _protocolLogin.onInternalError.AddListener(OnProtocolLoginInternalError);
                _protocolLogin.onLoginError.AddListener(OnProtocolLoginError);
                _protocolLogin.onLoginTokenError.AddListener(OnProtocolLoginTokenError);
                _protocolLogin.onMessageOfTheDay.AddListener(OnProtocolLoginMOTD);
                _protocolLogin.onSessionKey.AddListener(OnProtocolLoginSessionKey);
                _protocolLogin.onCharacterList.AddListener(OnProtocolLoginCharacterList);
                _protocolLogin.onUpdateRequired.AddListener(OnProtocolUpdateRequired);
            }
        }

        private void RemoveProtocolLoginListeners() {
            if (_protocolLogin) {
                _protocolLogin.onInternalError.RemoveListener(OnProtocolLoginInternalError);
                _protocolLogin.onLoginError.RemoveListener(OnProtocolLoginError);
                _protocolLogin.onLoginTokenError.RemoveListener(OnProtocolLoginTokenError);
                _protocolLogin.onMessageOfTheDay.RemoveListener(OnProtocolLoginMOTD);
                _protocolLogin.onSessionKey.RemoveListener(OnProtocolLoginSessionKey);
                _protocolLogin.onCharacterList.RemoveListener(OnProtocolLoginCharacterList);
                _protocolLogin.onUpdateRequired.RemoveListener(OnProtocolUpdateRequired);
            }
        }

        #region ProtocolLoginListeners
        private void OnProtocolLoginInternalError(string message) {
            PopupOk("Sorry", message);
            _popupWindow.SetMessageAlignment(TMPro.TextAlignmentOptions.MidlineLeft);
        }

        private void OnProtocolLoginError(string message) {
            PopupOk("Sorry", message);
        }
        
        private void OnProtocolLoginTokenError(int tries) {
            PopupOk("Authentication Error", TextResources.ERRORMSG_AUTHENTICATION_ERROR);
        }

        private void OnProtocolLoginMOTD(int number, string message) {
            PopupOk("Message of the day", message);
            _popupIsMOTD = true;
        }

        private void OnProtocolLoginSessionKey(string sessionKey) {
            _sessionKey = sessionKey;
        }

        private void OnProtocolLoginCharacterList(CharacterList characterList) {
            ModulesManager.Instance.CharactersWindow.Setup(_sessionKey, _protocolAccountIdentifier, _protocolPassword, _protocolToken, characterList);

            if (_popupWindow != null) {
                if (_popupIsMOTD) {
                    return;
                } else {
                    _popupWindow.Destroy();
                    _popupWindow = null;
                }
            }

            ShowCharactersWindow();
        }

        private void OnProtocolUpdateRequired() {
            throw new NotImplementedException();
        }
        #endregion

        #region LoginWebClientListeners
        private void OnLoginWebClientTechnicalError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }

        private void OnLoginWebClientLoginError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupOk("Sorry", message));
        }
        
        private void OnLoginWebClientTokenError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => ModulesManager.Instance.AuthenticatorWindow.Open());
        }

        private void OnLoginWebClientSuccess(Session session, Playdata playdata) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                ModulesManager.Instance.CharactersWindow.Setup(session, playdata);
                if (_popupWindow) {
                    _popupWindow.Destroy();
                    _popupWindow = null;
                }
                ShowCharactersWindow();
            });
        }

        #endregion

        private void ShowCharactersWindow() {
            var charactersWindow = ModulesManager.Instance.CharactersWindow;
            charactersWindow.onClosed.AddListener(OnCharacersWindowClosed);

            Hide();
            charactersWindow.Open();
        }

        private void OnCharacersWindowClosed() {
            var charactersWindow = ModulesManager.Instance.CharactersWindow;
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => charactersWindow.onClosed.RemoveListener(OnCharacersWindowClosed));

            gameObject.SetActive(true);
            ResetLocalPosition();
        }
    }
}
