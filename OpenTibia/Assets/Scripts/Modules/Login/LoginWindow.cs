using OpenTibiaUnity.Core.Input;
using System;
using UnityEngine;
using UnityEngine.UI;
using OpenTibiaUnity.Core.Communication.Login;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Modules.Login
{
    internal class LoginWindow : Core.Components.Base.Window
    {
        // Serializable Fields
        [SerializeField] protected RawImage m_BackgroundImageComponent = null;
        [SerializeField] protected AspectRatioFitter m_BackgroundAspectRatioFitter = null;

        [SerializeField] protected TMPro.TextMeshProUGUI m_AccountIdentifierLabel = null;
        [SerializeField] protected TMPro.TMP_InputField m_AccountIdentifierInput = null;
        [SerializeField] protected TMPro.TMP_InputField m_PasswordInput = null;
        [SerializeField] protected TMPro.TMP_InputField m_TokenInput = null;
        [SerializeField] protected TMPro.TMP_InputField m_AddressInput = null; // ip-address or login web address

        [SerializeField] protected TMPro.TextMeshProUGUI m_AddressLabel = null;

        [SerializeField] protected TMPro.TMP_Dropdown m_ClientVersionDropdown = null;
        [SerializeField] protected TMPro.TMP_Dropdown m_BuildVersionDropdown = null;

        [SerializeField] private Button m_AuthButton = null;
        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;

        [SerializeField] private RectTransform m_PanelAccountManagementV11 = null;
        [SerializeField] private Core.Components.Draggable m_DraggableComponent = null;

        [SerializeField] private Texture2D m_ClientArtwork700 = null;
        [SerializeField] private Texture2D m_ClientArtwork800 = null;
        [SerializeField] private Texture2D m_ClientArtwork910 = null;
        [SerializeField] private Texture2D m_ClientArtwork1020 = null;
        [SerializeField] private Texture2D m_ClientArtwork1100 = null;
        [SerializeField] private Texture2D m_ClientArtwork1103 = null;
        [SerializeField] private Texture2D m_ClientArtwork1140 = null;
        [SerializeField] private Texture2D m_ClientArtwork1150 = null;
        [SerializeField] private Texture2D m_ClientArtwork1180 = null;
        [SerializeField] private Texture2D m_ClientArtwork1200 = null;

        // Fields
        protected string m_SessionKey = string.Empty;
        protected string m_ProtocolAccountIdentifier = string.Empty;
        protected string m_ProtocolPassword = string.Empty;
        protected string m_ProtocolToken = string.Empty;

        protected ProtocolLogin m_ProtocolLogin = null;
        protected LoginWebClient m_LoginWebClient = null;

        protected Core.Components.PopupWindow m_PopupWindow;
        protected bool m_PopupIsMOTD = false;


        // Properties
        protected string AcccountIdentifier {
            get => m_AccountIdentifierInput.text;
            set => m_AccountIdentifierInput.text = value;
        }
        protected string Password {
            get => m_PasswordInput.text;
            set => m_PasswordInput.text = value;
        }
        protected string Token {
            get => m_TokenInput.text;
            set => m_TokenInput.text = value;
        }
        protected string Address {
            get => m_AddressInput.text;
            set => m_AddressInput.text = value;
        }


        protected override void Start() {
            base.Start();
            
            // setup input
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Core.Utility.EventImplPriority.High, OnKeyDown);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utility.EventImplPriority.High, OnKeyUp);

            // setup events
            m_AuthButton.onClick.AddListener(OnAuthButtonClick);
            m_OKButton.onClick.AddListener(OnOkButtonClick);
            m_CancelButton.onClick.AddListener(OnCancelButtonClick);
            m_ClientVersionDropdown.onValueChanged.AddListener(OnClientVersionDropdownValueChanged);
            m_BuildVersionDropdown.onValueChanged.AddListener(OnBuildVersionDropdownValueChanged);

            // setup popup message
            m_PopupWindow = Instantiate(OpenTibiaUnity.GameManager.PopupWindowPrefab, transform.parent);
            m_PopupWindow.name = "PopupWindow_LoginWindow";
            m_PopupWindow.HideWindow();

            m_PopupWindow.onOKClick.AddListener(OnPopupOkClick);
            m_PopupWindow.onCancelClick.AddListener(OnPopupCancelClick);

            // setup client versions
            var options = new System.Collections.Generic.List<TMPro.TMP_Dropdown.OptionData>();
            foreach (int version in OpenTibiaUnity.GetClientVersions())
                options.Add(new TMPro.TMP_Dropdown.OptionData(version.ToString()));
            m_ClientVersionDropdown.options = options;

            // setup our client with loaded options
            SetupWithOptions();
        }

        protected override void OnEnable() {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => m_AccountIdentifierInput.Select());
        }

        protected void OnKeyDown(Event e, bool repeat) {
            if (repeat)
                OnKeyUp(e, false);
        }

        protected void OnKeyUp(Event e, bool _) {
            if (e.alt || e.shift || e.control || !InputHandler.IsHighlighted(this))
                return;
            
            switch (e.keyCode) {
                case KeyCode.Tab:
                    e.Use();

                    if (InputHandler.IsHighlighted(m_AccountIdentifierInput)) {
                        m_PasswordInput.Select();
                        m_PasswordInput.MoveTextEnd(false);
                    } else if (InputHandler.IsHighlighted(m_PasswordInput)) {
                        if (m_TokenInput.transform.parent.gameObject.activeSelf) {
                            m_TokenInput.Select();
                            m_TokenInput.MoveTextEnd(false);
                        } else {
                            m_AddressInput.Select();
                            m_AddressInput.MoveTextEnd(false);
                        }
                    } else if (InputHandler.IsHighlighted(m_TokenInput)) {
                        m_AddressInput.Select();
                        m_AddressInput.MoveTextEnd(false);
                    } else { // to fix any error //
                        m_AccountIdentifierInput.Select();
                        m_AccountIdentifierInput.MoveTextEnd(false);
                    }
                    
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
            var oldValidation = m_AccountIdentifierInput.characterValidation;
            if (gameManager.GetFeature(GameFeature.GameAccountNames))
                m_AccountIdentifierInput.characterValidation = TMPro.TMP_InputField.CharacterValidation.None;
            else
                m_AccountIdentifierInput.characterValidation = TMPro.TMP_InputField.CharacterValidation.Integer;

            if (oldValidation != m_AccountIdentifierInput.characterValidation)
                m_AccountIdentifierInput.text = string.Empty;
            
            // check authenticator button
            // on tibia 11, authenticator button was hidden until it's required
            // on tibia 10, the player has the ability to switch it on/off
            bool authButtonSupported = gameManager.GetFeature(GameFeature.GameAuthenticator) && newVersion < 1100;
            m_AuthButton.gameObject.SetActive(authButtonSupported);
            if (authButtonSupported && optionStorage.AuthenticatorTokenOn) {
                m_TokenInput.transform.parent.gameObject.SetActive(true);
                m_TokenInput.text = "";
                m_AuthButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_AUTHBUTTON_ON);
            } else {
                m_TokenInput.transform.parent.gameObject.SetActive(false);
                m_AuthButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_AUTHBUTTON_OFF);
                optionStorage.AuthenticatorTokenOn = false;
            }
            
            // check address label, okButton & cancelButton
            // on tibia 11 & later versions, we use a login web client
            // on earlier versions (10, 9, 8, 7.x) we use protocols, thus requires address & port
            if ((oldVersion < 1100 && newVersion >= 1100) || (oldVersion >= 1100 && newVersion < 1100)) {
                m_CancelButton.gameObject.SetActive(newVersion < 1100);
                m_OKButton.GetComponent<LayoutElement>().preferredWidth = newVersion < 1100 ? 50 : 100;
                if (newVersion < 1100) {
                    m_OKButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_OKBUTTON_LEGACY);
                    m_AddressLabel.SetText(TextResources.IP_ADDRESS_LABEL);
                    m_AddressInput.placeholder.GetComponent<TMPro.TextMeshProUGUI>().SetText(Constants.OpenTibiaDefaultFullIPAddress);
                } else {
                    m_OKButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(TextResources.LOGIN_OKBUTTON_NEW);
                    m_AddressLabel.SetText(TextResources.LOGIN_WEB_ADDRESS_LABEL);
                    m_AddressInput.placeholder.GetComponent<TMPro.TextMeshProUGUI>().SetText(Constants.OpenTibiaDefaultClientServicesAddress);
                }
            }

            // check abilities of the login window
            // reset the window the the center of the screen
            // the player can drag the window if it's not tibia 11
            ResetLocalPosition();
            m_DraggableComponent.enabled = newVersion < 1100;

            // check build versions
            // build version is supported from tibia 11 and later versions
            var buildVersions = OpenTibiaUnity.GetBuildVersions(newVersion); // == null on tibia 10
            m_BuildVersionDropdown.gameObject.SetActive(buildVersions != null);

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
                
                m_BuildVersionDropdown.ClearOptions();
                m_BuildVersionDropdown.AddOptions(options);
                OpenTibiaUnity.GameManager.SetBuildVersion(buildVersions[0]);
                buildVersion = buildVersions[0];
            }

            // now update protocol version
            // this is required for login purposes later
            gameManager.SetProtocolVersion(OpenTibiaUnity.GetProtocolVersion(newVersion, buildVersion));

            // update the layout of clientVersions dropdown
            var clientVersionsRectTransform = m_ClientVersionDropdown.GetComponent<RectTransform>();
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
            m_PanelAccountManagementV11.gameObject.SetActive(newVersion >= 1100);

            // update background image
            Texture2D newTexture;
            if (newVersion < 800)
                newTexture = m_ClientArtwork700;
            else if (newVersion < 910)
                newTexture = m_ClientArtwork800;
            else if (newVersion < 1020)
                newTexture = m_ClientArtwork910;
            else if (newVersion < 1100)
                newTexture = m_ClientArtwork1020;
            else if (newVersion < 1103)
                newTexture = m_ClientArtwork1100;
            else if (newVersion < 1140)
                newTexture = m_ClientArtwork1103;
            else if (newVersion < 1150)
                newTexture = m_ClientArtwork1140;
            else if (newVersion < 1180)
                newTexture = m_ClientArtwork1150;
            else if (newVersion < 1200)
                newTexture = m_ClientArtwork1180;
            else
                newTexture = m_ClientArtwork1200;

            m_BackgroundImageComponent.texture = newTexture;
            m_BackgroundImageComponent.enabled = true;

            float aspectRatio = 1.6f;
            if (newVersion < 1020)
                aspectRatio = Screen.width / Screen.height;

            m_BackgroundAspectRatioFitter.aspectRatio = aspectRatio;

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

            if (m_AccountIdentifierLabel.text != newAccountIdentifierLabel)
                m_AccountIdentifierLabel.text = newAccountIdentifierLabel;
        }

        protected void OnPopupOkClick() {
            if (m_LoginWebClient) {
                RemoveLoginWebClientListeners();
                m_LoginWebClient = null;
            }

            if (m_ProtocolLogin) {
                RemoveProtocolLoginListeners();
                m_ProtocolLogin.Disconnect();
                m_ProtocolLogin = null;
            }

            if (m_PopupIsMOTD) { // up to 10.99, motd can be displayed!
                m_PopupIsMOTD = false;
                HideWindow();

                ModulesManager.Instance.CharactersWindow.OpenWindow();
            } else {
                ShowWindow();

                m_AccountIdentifierInput.Select();
            }
        }

        protected void OnPopupCancelClick() {
            if (m_LoginWebClient) {
                RemoveLoginWebClientListeners();
                m_LoginWebClient = null;
            }

            if (m_ProtocolLogin) {
                RemoveProtocolLoginListeners();
                m_ProtocolLogin.Disconnect();
                m_ProtocolLogin = null;
            }

            gameObject.SetActive(true);
            m_AccountIdentifierInput.Select();
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
                    m_ClientVersionDropdown.value = foundIndex;
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
                    m_BuildVersionDropdown.value = foundIndex;
                } else {
                    OnBuildVersionDropdownValueChanged(0);
                    optionStorage.SelectedBuildVersion = gameManager.ClientVersion;
                }
            } else {
                OnBuildVersionDropdownValueChanged(0);
            }

            // update login address
            m_AddressInput.text = optionStorage.LoginAddress;

            // todo: save account name & password encrypted //
        }

        protected void PopupMessage(string title, string message, PopupMenuType popupType = PopupMenuType.OK, TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.MidlineGeoAligned) {
            m_PopupWindow.OpenWindow();
            m_PopupWindow.LockToOverlay();
            m_PopupWindow.ResetLocalPosition();

            m_PopupWindow.PopupType = popupType;

            m_PopupWindow.SetTitle(title);
            m_PopupWindow.SetMessage(message, 500, 250);
            m_PopupWindow.SetMessageAlignment(alignment);
        }

        protected async void DoLogin(string accountIdentifier, string password, string token, string address) {
            m_ProtocolAccountIdentifier = accountIdentifier;
            m_ProtocolPassword = password;
            m_ProtocolToken = token;

            AcccountIdentifier = string.Empty;
            Password = string.Empty;
            Token = string.Empty;

            OpenTibiaUnity.OptionStorage.LoginAddress = address;

            var gameManager = OpenTibiaUnity.GameManager;
            int clientVersion = gameManager.ClientVersion;
            int buildVersion = gameManager.BuildVersion;

            ClientSpecification specification;
            string rawAddress = address.ToLower();
            if (rawAddress == "cipsoft")
                specification = ClientSpecification.Cipsoft;
            else
                specification = ClientSpecification.OpenTibia;

            gameManager.SetClientSpecification(specification);

            if (clientVersion >= 1200) {
                gameObject.SetActive(false);
                gameManager.LoadingAppearancesWindow.OpenWindow();
                bool loaded = await gameManager.LoadThingsAsyncAwaitable(clientVersion, buildVersion, specification);
                gameManager.LoadingAppearancesWindow.CloseWindow();
                gameObject.SetActive(true);

                if (!loaded) {
                    PopupMessage("Sorry", string.Format("Couldn't load appearances for version {0}.{1}.", clientVersion / 100f, buildVersion));
                    return;
                }
            } else {
                if (!gameManager.CanLoadThings(clientVersion, buildVersion, specification)) {
                    PopupMessage("Sorry", string.Format("Couldn't load appearances for version {0}.{1}.", clientVersion / 100f, buildVersion));
                    return;
                }

                gameManager.LoadThingsAsync(clientVersion, buildVersion, specification);
            }
            
            if (clientVersion >= 1100) {
                m_LoginWebClient = new LoginWebClient(clientVersion, buildVersion) {
                    AccountIdentifier = m_ProtocolAccountIdentifier,
                    Password = m_ProtocolPassword,
                    Token = m_ProtocolToken,
                };

                if (specification == ClientSpecification.Cipsoft)
                    address = Constants.RealTibiaClientServicesAddress;
                else if (address.Length == 0)
                    address = Constants.OpenTibiaDefaultClientServicesAddress;
                
                AddLoginWebClientListeners();
                new Task(() => m_LoginWebClient.LoadDataAsync(address)).Start();
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
                
                m_ProtocolLogin = new ProtocolLogin() {
                    AccountName = m_ProtocolAccountIdentifier,
                    Password = m_ProtocolPassword,
                    Token = m_ProtocolToken
                };

                AddProtocolLoginListners();
                new Task(() => m_ProtocolLogin.Connect(ip, port)).Start();
            }

            gameObject.SetActive(clientVersion >= 1100);
            PopupMessage("Connecting", "Your character list is being loaded. Please wait.", PopupMenuType.Cancel);
        }

        internal void DoLoginWithNewToken(string token) {
            DoLogin(m_ProtocolAccountIdentifier, m_ProtocolPassword, token, OpenTibiaUnity.OptionStorage.LoginAddress);
        }

        private void AddLoginWebClientListeners() {
            if (m_LoginWebClient) {
                m_LoginWebClient.onTechnicalError.AddListener(OnLoginWebClientTechnicalError);
                m_LoginWebClient.onLoginError.AddListener(OnLoginWebClientLoginError);
                m_LoginWebClient.onTokenError.AddListener(OnLoginWebClientTokenError);
                m_LoginWebClient.onLoginSuccess.AddListener(OnLoginWebClientSuccess);
            }
        }

        private void RemoveLoginWebClientListeners() {
            if (m_LoginWebClient) {
                m_LoginWebClient.onTechnicalError.RemoveListener(OnLoginWebClientTechnicalError);
                m_LoginWebClient.onLoginError.RemoveListener(OnLoginWebClientLoginError);
                m_LoginWebClient.onTokenError.RemoveListener(OnLoginWebClientTokenError);
                m_LoginWebClient.onLoginSuccess.RemoveListener(OnLoginWebClientSuccess);
            }
        }

        private void AddProtocolLoginListners() {
            if (m_ProtocolLogin) {
                m_ProtocolLogin.onInternalError.AddListener(OnProtocolLoginInternalError);
                m_ProtocolLogin.onLoginError.AddListener(OnProtocolLoginError);
                m_ProtocolLogin.onLoginTokenError.AddListener(OnProtocolLoginTokenError);
                m_ProtocolLogin.onMessageOfTheDay.AddListener(OnProtocolLoginMOTD);
                m_ProtocolLogin.onSessionKey.AddListener(OnProtocolLoginSessionKey);
                m_ProtocolLogin.onCharacterList.AddListener(OnProtocolLoginCharacterList);
                m_ProtocolLogin.onUpdateRequired.AddListener(OnProtocolUpdateRequired);
            }
        }

        private void RemoveProtocolLoginListeners() {
            if (m_ProtocolLogin) {
                m_ProtocolLogin.onInternalError.RemoveListener(OnProtocolLoginInternalError);
                m_ProtocolLogin.onLoginError.RemoveListener(OnProtocolLoginError);
                m_ProtocolLogin.onLoginTokenError.RemoveListener(OnProtocolLoginTokenError);
                m_ProtocolLogin.onMessageOfTheDay.RemoveListener(OnProtocolLoginMOTD);
                m_ProtocolLogin.onSessionKey.RemoveListener(OnProtocolLoginSessionKey);
                m_ProtocolLogin.onCharacterList.RemoveListener(OnProtocolLoginCharacterList);
                m_ProtocolLogin.onUpdateRequired.RemoveListener(OnProtocolUpdateRequired);
            }
        }

        #region ProtocolLoginListeners
        private void OnProtocolLoginInternalError(string message) {
            PopupMessage("Sorry", message, PopupMenuType.OK, TMPro.TextAlignmentOptions.MidlineLeft);
        }

        private void OnProtocolLoginError(string message) {
            PopupMessage("Sorry", message);
        }
        
        private void OnProtocolLoginTokenError(int tries) {
            PopupMessage("Authentication Error", TextResources.ERRORMSG_AUTHENTICATION_ERROR);
        }

        private void OnProtocolLoginMOTD(int number, string message) {
            PopupMessage("Message of the day", message);
            m_PopupIsMOTD = true;
        }

        private void OnProtocolLoginSessionKey(string sessionKey) {
            m_SessionKey = sessionKey;
        }

        private void OnProtocolLoginCharacterList(CharacterList characterList) {
            ModulesManager.Instance.CharactersWindow.Setup(m_SessionKey, m_ProtocolAccountIdentifier, m_ProtocolPassword, m_ProtocolToken, characterList);

            if (m_PopupWindow.Visible) {
                if (m_PopupIsMOTD)
                    return;
                else
                    m_PopupWindow.CloseWindow();
            }

            ShowCharactersWindow();
        }

        private void OnProtocolUpdateRequired() {
            throw new NotImplementedException();
        }
        #endregion

        #region LoginWebClientListeners
        private void OnLoginWebClientTechnicalError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupMessage("Sorry", message));
        }

        private void OnLoginWebClientLoginError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => PopupMessage("Sorry", message));
        }
        
        private void OnLoginWebClientTokenError(string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => ModulesManager.Instance.AuthenticatorWindow.OpenWindow());
        }

        private void OnLoginWebClientSuccess(Session session, Playdata playdata) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                ModulesManager.Instance.CharactersWindow.Setup(session, playdata);
                m_PopupWindow.CloseWindow();
                ShowCharactersWindow();
            });
        }

        #endregion

        private void ShowCharactersWindow() {
            var charactersWindow = ModulesManager.Instance.CharactersWindow;
            charactersWindow.onClosed.AddListener(OnCharacersWindowClosed);

            HideWindow();
            charactersWindow.OpenWindow();
        }

        private void OnCharacersWindowClosed() {
            var charactersWindow = ModulesManager.Instance.CharactersWindow;
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => charactersWindow.onClosed.RemoveListener(OnCharacersWindowClosed));

            gameObject.SetActive(true);
            ResetLocalPosition();
        }
    }
}
