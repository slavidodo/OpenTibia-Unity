using OpenTibiaUnity.Core.InputManagment;
using OpenTibiaUnity.Core.Network;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    public sealed class CharactersWindow : Core.Components.Base.Window
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private LoginWindow m_LoginWindow;
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Button m_OkButton;
        [SerializeField] private Button m_CancelButton;
        [SerializeField] private Button m_GetPremiumButton;
#pragma warning restore CS0649 // never assigned to

        private Core.Components.PopupWindow m_LoggerWindow;

        private string m_SessionKey;
        private string m_AccountName;
        private string m_Password;
        private string m_AuthenticatorToken;
        private CharacterList m_CharacterList;
        private int m_LastReaction;

        private int m_CharacterIndex = -1;

        protected override void Start() {
            base.Start();

            m_OkButton.onClick.AddListener(OnOkClicked);
            m_CancelButton.onClick.AddListener(CloseWindow);

            m_LoggerWindow = Instantiate(OpenTibiaUnity.GameManager.PopupWindowPrefab, transform.parent);
            m_LoggerWindow.name = "PopupWindow_CharactersWindow";
            m_LoggerWindow.gameObject.SetActive(false);
            m_LoggerWindow.PopupType = PopupMenuType.Cancel;

            m_LoggerWindow.OnClickCancel.AddListener(() => {
                m_LoggerWindow.gameObject.SetActive(false);
                if (!!OpenTibiaUnity.ProtocolGame && OpenTibiaUnity.ProtocolGame.IsConnected) {
                    OpenTibiaUnity.ProtocolGame.Disconnect();
                    OpenTibiaUnity.ProtocolGame.onDisconnect.AddListener(() => {
                        gameObject.SetActive(true);
                        EventSystem.current.SetSelectedGameObject(gameObject);
                    });
                } else {
                    gameObject.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            });

            OpenTibiaUnity.InputHandler.AddKeyUpListener((Event e, bool repeat) => {
                if (!InputHandler.IsHighlighted(this))
                    return;

                switch (e.keyCode) {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        OnOkClicked();
                        break;

                    case KeyCode.Escape:
                        CloseWindow();
                        break;

                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                        m_LastReaction = OpenTibiaUnity.TicksMillis;
                        break;
                }
            });
        }

        private void Update() {
            if (OpenTibiaUnity.TicksMillis - m_LastReaction > 60000)
                CloseWindow();
        }

        public void Show() {
            LockToOverlay();
            gameObject.SetActive(true);
            m_LastReaction = OpenTibiaUnity.TicksMillis;
            
            m_ScrollRect.content
                .GetChild(m_CharacterIndex > -1 ? m_CharacterIndex : 0)
                ?.GetComponent<Core.Components.CharacterPanel>()
                ?.ToggleComponent
                ?.Select();
        }

        private void OnOkClicked() {
            if (!!OpenTibiaUnity.ProtocolGame && OpenTibiaUnity.ProtocolGame.IsGameRunning) {
                int characterIndex = -1;
                var content = m_ScrollRect.content;
                for (int i = 0; i < content.childCount; i++) {
                    var characterPanel = content.GetChild(i).GetComponent<Core.Components.CharacterPanel>();
                    if (characterPanel && EventSystem.current.currentSelectedGameObject == characterPanel.gameObject) {
                        characterIndex = i;
                        break;
                    }
                }

                OpenTibiaUnity.GameManager.PendingCharacterIndex = characterIndex;
                if (characterIndex != -1) {
                    OpenTibiaUnity.GameManager.ProtocolGame.Disconnect();
                }

                UnlockFromOverlay();
                gameObject.SetActive(false);
            } else {
                SubmitEnterGame();
            }
        }

        public void SubmitEnterGame() {
            SubmitEnterGame(-1);
        }

        private void SubmitEnterGame(int characterIndex) {
            var content = m_ScrollRect.content;

            if (characterIndex == -1) {
                for (int i = 0; i < content.childCount; i++) {
                    var characterPanel = content.GetChild(i).GetComponent<Core.Components.CharacterPanel>();
                    if (characterPanel && EventSystem.current.currentSelectedGameObject == characterPanel.gameObject) {
                        m_CharacterIndex = i;
                        ConnectToGame(characterPanel.characterName.text, m_CharacterList.FindWorld(characterPanel.worldName.text));
                        break;
                    }
                }

                return;
            }

            m_CharacterIndex = characterIndex;
            var characterInfo = m_CharacterList.Characters[characterIndex];
            ConnectToGame(characterInfo.Name, m_CharacterList.FindWorld(characterInfo.World));
        }

        private void CloseWindow() {
            gameObject.SetActive(false);
            if (!!OpenTibiaUnity.ProtocolGame && OpenTibiaUnity.ProtocolGame.IsGameRunning)
                UnlockFromOverlay();
            else
                m_LoginWindow.Show();
        }

        public void Setup(string sessionKey, string accountname, string password, string authToken, CharacterList characterList) {
            m_SessionKey = sessionKey;
            m_AccountName = accountname;
            m_Password = password;
            m_AuthenticatorToken = authToken;
            m_CharacterList = characterList;
            m_CharacterIndex = 0;

            var content = m_ScrollRect.content;

            foreach (Transform child in content) {
                Destroy(child.gameObject);
            }

            int i = 0;
            bool reversed = characterList.Characters.Count % 2 == 0;

            foreach (var character in characterList.Characters) {
                var characterPanel = Instantiate(OpenTibiaUnity.GameManager.CharacterPanelPrefab);
                characterPanel.transform.SetParent(content);
                characterPanel.characterName.text = character.Name;
                characterPanel.worldName.text = characterList.FindWorld(character.World).Name;
                
                characterPanel.ColorSwitched = reversed ? (i++ % 2 == 0) : (++i % 2 == 0);
                characterPanel.onDoubleClick.AddListener(SubmitEnterGame);
            }
        }
        
        public void TryToEnterGame(int characterIndex) {
            SubmitEnterGame(characterIndex);
        }

        void ConnectToGame(string characterName, CharacterList.World world) {
            m_LoggerWindow.SetTitle("Connecting");
            m_LoggerWindow.SetMessage("Connecting to the game world. Please wait.");
            m_LoggerWindow.LockToOverlay();
            m_LoggerWindow.ResetToCenter();
            m_LoggerWindow.gameObject.SetActive(true);
            gameObject.SetActive(false);

            OpenTibiaUnity.ChatStorage.Reset();
            //gameManager.ContainerStorage.Reset();
            OpenTibiaUnity.CreatureStorage.Reset();
            //gameManager.SpellStorage.Reset();
            OpenTibiaUnity.WorldMapStorage.Reset();

            OpenTibiaUnity.ProtocolGame = new ProtocolGame() {
                SessionKey = m_SessionKey,
                CharacterName = characterName,
                AccountName = m_AccountName,
                Password = m_Password,
                AuthenticatorToken = m_AuthenticatorToken,
                WorldName = world.Name,
                WorldIp = world.HostName,
                WorldPort = world.Port,
            };

            OpenTibiaUnity.ProtocolGame.Connect();
        }

        public void ProcessGameStart() {
            m_LoggerWindow.UnlockFromOverlay();
            m_LoggerWindow.gameObject.SetActive(false);

            UnlockFromOverlay();
            gameObject.SetActive(false);
        }
    }
}