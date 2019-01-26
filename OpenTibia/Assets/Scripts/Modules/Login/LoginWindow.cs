using OpenTibiaUnity.Core.InputManagment;
using OpenTibiaUnity.Core.Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    [DisallowMultipleComponent]
    public sealed class LoginWindow : Core.Components.Base.Window
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private TMPro.TMP_InputField m_AccountNameInputField;
        [SerializeField] private TMPro.TMP_InputField m_PasswordInputField;
        [SerializeField] private TMPro.TMP_InputField m_IPAddressInputField;
        [SerializeField] private Button m_LoginButton;
        [SerializeField] private TMPro.TMP_Dropdown m_VersionsDropdown;
        [SerializeField] private CharactersWindow m_CharactersWindow;
        
#pragma warning restore CS0649 // never assigned to

        private Core.Components.PopupWindow m_LoggerWindow;
        private ProtocolLogin m_ProtocolLogin;
        private string m_SessionKey;
        private int m_MotdNum;
        private string m_Motd;
        private string m_LoginError;

        private Queue<UnityAction> m_LoggerActionsQueue = new Queue<UnityAction>();
        private bool m_LoggerWindowActive = false;
        private bool m_LoggerWindowDisposable = false;

        public string AccountName {
            get => m_AccountNameInputField.text;
            set => m_AccountNameInputField.text = value;
        }

        public string Password {
            get => m_PasswordInputField.text;
            set => m_PasswordInputField.text = value;
        }

        public string IPAddress {
            get => m_IPAddressInputField.text;
            set => m_IPAddressInputField.text = value;
        }

        protected override void Start() {
            base.Start();
            var gameManager = OpenTibiaUnity.GameManager;
            var inputHandler = OpenTibiaUnity.InputHandler;
            
            inputHandler.AddKeyUpListener((Event e, bool repeat) => {
                if (e.alt || e.shift || e.control)
                    return;

                switch (e.keyCode) {
                    case KeyCode.Tab:
                        if (InputHandler.IsHighlighted(m_AccountNameInputField)) {
                            m_PasswordInputField.Select();
                            m_PasswordInputField.MoveTextEnd(false);
                        } else if (InputHandler.IsHighlighted(m_PasswordInputField)) {
                            m_IPAddressInputField.Select();
                            m_IPAddressInputField.MoveTextEnd(false);
                        } else if (InputHandler.IsHighlighted(m_IPAddressInputField)) {
                            m_AccountNameInputField.Select();
                            m_AccountNameInputField.MoveTextEnd(false);
                        }
                        
                        break;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (InputHandler.IsGameObjectHighlighted(gameObject) && AccountName.Length > 0)
                            SubmitLogin();
                        break;
                }
            });
            
            gameObject.SetActive(true);
            m_LoginButton.onClick.AddListener(OnLoginButtonClicked);

            m_LoggerWindow = Instantiate(gameManager.PopupWindowPrefab, transform.parent);
            m_LoggerWindow.gameObject.SetActive(false);

            m_LoggerWindow.OnClickOk.AddListener(() => {
                RemoveLogger();
                m_AccountNameInputField.Select();
            });

            m_LoggerWindow.OnClickCancel.AddListener(() => {
                RemoveLogger();
                m_ProtocolLogin?.Disconnect();
                m_ProtocolLogin = null;
                m_AccountNameInputField.Select();
            });

            var supportedVersions = OpenTibiaUnity.GetSupportedVersions();
            List<TMPro.TMP_Dropdown.OptionData> options = new List<TMPro.TMP_Dropdown.OptionData>();
            foreach (var version in supportedVersions) {
                options.Add(new TMPro.TMP_Dropdown.OptionData(version.ToString()));
            }

            m_VersionsDropdown.options = options;
        }

        protected override void OnEnable() {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => m_AccountNameInputField.Select());
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void RemoveLogger() {
            m_LoggerWindowActive = false;
            m_LoggerWindowDisposable = false;
            m_LoggerWindow.UnlockFromOverlay();
            m_LoggerWindow.gameObject.SetActive(false);

            PostLoggerRemoval();
        }

        public void PostLoggerRemoval() {
            if (m_LoggerActionsQueue.Count == 0) {
                return;
            }

            while (!m_LoggerWindowActive && m_LoggerActionsQueue.Count > 0) {
                var action = m_LoggerActionsQueue.Dequeue();
                action.Invoke();
            }

            if (!m_LoggerWindowActive && !m_CharactersWindow.gameObject.activeSelf) {
                m_AccountNameInputField.Select();
            }
        }

        public void PopupInfo(string title, string message, PopupMenuType popupType = PopupMenuType.OK, TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.MidlineGeoAligned) {
            // any popup can override the current popup
            // i.e motd can override waiting message

            // this will be discarded if characters' window is currently visible;
            if (m_CharactersWindow.gameObject.activeSelf) {
                return;
            }
            
            m_LoggerWindow.Show();
            m_LoggerWindow.LockToOverlay();
            m_LoggerWindow.ResetToCenter();

            m_LoggerWindow.PopupType = popupType;

            m_LoggerWindow.SetTitle(title);
            m_LoggerWindow.SetMessage(message, 500, 250);
            m_LoggerWindow.SetMessageAlignment(alignment);
            
            m_LoggerWindowActive = true;
        }

        public bool CanTraceWindow() {
            if (m_LoggerWindowActive) {
                if (m_LoggerWindowDisposable) {
                    OpenTibiaUnity.GameManager.InvokeOnMainThread(RemoveLogger);
                    if (m_LoggerActionsQueue.Count == 0) {
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        void OnLoginButtonClicked() {
            if (AccountName.Length > 0) {
                SubmitLogin();
            }
        }

        void SubmitLogin() {
            string accountName = AccountName;
            string password = Password;

            AccountName = string.Empty;
            Password = string.Empty;

            TryToLogin(accountName, password);
            PopupInfo("Loading...", "Please wait while we fetch your characters list.", PopupMenuType.Cancel);
            m_LoggerWindowDisposable = true;
        }

        void TryToLogin(string accountName, string password) {
            string ip_port = IPAddress;

            string ip;
            int port;
            if (ip_port.Length == 0) {
                ip = Constants.LocalHostIP;
                port = Constants.LocalHostLoginPort;
            } else {
                var split = ip_port.Split(':');
                ip = split[0];
                if (split.Length != 2 || !int.TryParse(split[1], out port))
                    port = Constants.LocalHostLoginPort;
            }

            var version = OpenTibiaUnity.GetSupportedVersions()[m_VersionsDropdown.value];
            OpenTibiaUnity.GameManager.SetClientVersion(version);
            OpenTibiaUnity.GameManager.SetProtocolVersion(GetClientProtocolVersion(version));

            m_ProtocolLogin = new ProtocolLogin();
            m_ProtocolLogin.AccountName = accountName;
            m_ProtocolLogin.Password = password;

            m_ProtocolLogin.onCustomLoginError.AddListener(TriggerCustomLoginError);
            m_ProtocolLogin.onLoginError.AddListener(TriggerLoginError);
            m_ProtocolLogin.onLoginTokenError.AddListener(TriggerLoginTokenError);
            m_ProtocolLogin.onMotd.AddListener(TriggerLoginMotd);
            m_ProtocolLogin.onUpdateRequired.AddListener(TriggerUpdateRequired);
            m_ProtocolLogin.onSessionKey.AddListener(TriggerLoginSessionKey);
            m_ProtocolLogin.onCharacterList.AddListener(TriggerLoginCharacterList);

            m_ProtocolLogin.Connect(ip, port);
        }

        private int GetClientProtocolVersion(int version) {
            switch (version) {
                case 980: return 971;
                case 981: return 973;
                case 982: return 974;
                case 983: return 975;
                case 984: return 976;
                case 985: return 977;
                case 986: return 978;
                case 1001: return 979;
                case 1002: return 980;
                default: return version;
            }
        }

        void TriggerCustomLoginError(string message) {
            m_LoginError = message;

            // no checks are needed, usually those are the first errors
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                PopupInfo("Login Error", m_LoginError, PopupMenuType.OK, TMPro.TextAlignmentOptions.MidlineLeft);
            });
        }
        void TriggerLoginError(string message) {
            m_LoginError = message;

            // no checks are needed, usually those are the first errors
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                PopupInfo("Login Error", m_LoginError);
            });
        }
        void TriggerLoginTokenError(int unknown) {
            m_LoginError = "You have entered incorrect authenticator token.";

            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                PopupInfo("Authentication Error", m_LoginError);
            });
        }
        void TriggerLoginMotd(int num, string motd) {
            m_MotdNum = num;
            m_Motd = motd;
            UnityAction showMotdWindow = () => {
                PopupInfo("Message of the Day", m_Motd);
            };

            if (!CanTraceWindow()) {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                    m_LoggerActionsQueue.Enqueue(showMotdWindow);
                });
            } else {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(showMotdWindow);
            }
        }
        void TriggerUpdateRequired() {
            m_LoginError = "Your client is outdated, consider updating it!";

            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                PopupInfo("Client Error", m_LoginError);
            });
        }
        void TriggerLoginSessionKey(string sessionKey) {
            m_SessionKey = sessionKey;
        }
        void TriggerLoginCharacterList(CharacterList characterList) {
            UnityAction showCharactersWindow = () => {
                gameObject.SetActive(false);

                // TODO; support auth token
                m_CharactersWindow.Setup(m_SessionKey, AccountName, Password, string.Empty, characterList);
                m_CharactersWindow.Show();
            };

            if (!CanTraceWindow()) {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                    m_LoggerActionsQueue.Enqueue(showCharactersWindow);
                });
            } else {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(showCharactersWindow);
            }
        }
    }
}