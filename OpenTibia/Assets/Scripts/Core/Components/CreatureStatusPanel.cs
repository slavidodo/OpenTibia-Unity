using OpenTibiaUnity.Core.WorldMap.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    internal class CreatureStatusPanel : Base.AbstractComponent
    {
        private static Color HiddenCreatureColor = Colors.ColorFromRGB(192, 192, 192);

        [SerializeField] private TMPro.TextMeshProUGUI m_NamePanel = null;
        [SerializeField] private Slider m_HealthProgressBar = null;
        [SerializeField] private Slider m_ManaProgressBar = null;

        [SerializeField] private VerticalLayoutGroup m_FlagsContainer = null;
        [SerializeField] private HorizontalLayoutGroup m_PartyPKFlagsContainer = null;
        [SerializeField] private Image m_PartyFlagImage = null;
        [SerializeField] private Image m_PKFlagImage = null;
        [SerializeField] private Image m_TypeFlagImage = null;
        [SerializeField] private Image m_SpeechFlagImage = null;
        [SerializeField] private Image m_GuildFlagImage = null;

        internal float CachedWidth { get; private set; } = 0;
        internal float CachedHeight { get; private set; } = 0;
        internal uint CachedRenderCount { get; set; } = 0;

        private bool m_DrawName = true;
        private bool m_DrawHealth = true;
        private bool m_DrawMana = true;
        private bool m_DrawFlags = true;
        
        private Color m_HealthColor = Color.black;
        private Color m_ManaColor = Color.black;

        private PartyFlag m_PartyFlag = PartyFlag.None;
        private PKFlag m_PKFlag = PKFlag.None;
        private SummonTypeFlags m_TypeFlag = SummonTypeFlags.None;
        private SpeechCategory m_SpeechFlag = SpeechCategory.None;
        private GuildFlag m_GuildFlag = GuildFlag.None;

        private bool m_InternallyChanged = false;
        private bool m_LastCreatureVisiblity = true;
        private float m_LastLightFactor = 1f;

        private uint m_CreatureID = 0;

        internal uint CreatureID {
            get => m_CreatureID;
            set { if (m_CreatureID == 0) m_CreatureID = value; }
        }

        internal static Color GetHealthColor(float percent) {
            Color tmpHealthColor;
            if (percent < 0.04) {
                tmpHealthColor = Colors.ColorFromRGB(96, 0, 0);
            } else if (percent < 0.10) {
                tmpHealthColor = Colors.ColorFromRGB(192, 0, 0);
            } else if (percent < 0.30) {
                tmpHealthColor = Colors.ColorFromRGB(192, 48, 48);
            } else if (percent < 0.60) {
                tmpHealthColor = Colors.ColorFromRGB(192, 192, 0);
            } else if (percent < 0.95) {
                tmpHealthColor = Colors.ColorFromRGB(96, 192, 96);
            } else {
                tmpHealthColor = Colors.ColorFromRGB(0, 192, 0);
            }
            return tmpHealthColor;
        }

        internal void UpdateProperties(string name, int healthPercent, int manaPercent) {
            SetCharacterName(name);
            SetHealthPercent(healthPercent);
            SetManaPercent(manaPercent);
            
            m_NamePanel.color = m_HealthColor;
            m_HealthProgressBar.fillRect.GetComponent<RawImage>().color = m_HealthColor;
            m_ManaProgressBar.fillRect.GetComponent<RawImage>().color = Color.blue;
        }
        
        internal void SetDrawingProperties(bool drawName, bool drawHealth, bool drawMana) {
            if (drawName == m_DrawName && drawHealth == m_DrawHealth && drawMana == m_DrawMana)
                return;

            CachedWidth = 0;
            CachedHeight = 0;

            int totalDrawn = 0;

            m_NamePanel.gameObject.SetActive(drawName);
            if (drawName) {
                CachedHeight += 14;
                CachedWidth = Mathf.Max(CachedWidth, m_NamePanel.preferredWidth);
                totalDrawn += 1;
            }

            m_HealthProgressBar.gameObject.SetActive(drawHealth);
            if (drawHealth) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            m_ManaProgressBar.gameObject.SetActive(drawMana);
            if (drawMana) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            CachedHeight += totalDrawn * 1 + 4;
            CachedWidth += 4;
        }

        internal void SetFlags(bool drawFlags, PartyFlag partyFlag, PKFlag pkFlag, SummonTypeFlags typeFlag, SpeechCategory speechFlag, GuildFlag guildFlag) {
            if (!drawFlags) {
                if (m_DrawFlags) {
                    m_FlagsContainer.gameObject.SetActive(false);
                    m_DrawFlags = false;
                }

                return;
            }

            if (m_PartyFlag != partyFlag)
                InternalUpdatePartyFlag(partyFlag);

            if (m_PKFlag != pkFlag)
                InternalUpdatePKFlag(pkFlag);

            if (m_TypeFlag != typeFlag)
                InternalUpdateTypeFlag(typeFlag);

            if (m_SpeechFlag != speechFlag)
                InternalUpdateSpeechFlag(speechFlag);

            if (m_GuildFlag != guildFlag)
                InternalUpdateGuildFlag(guildFlag);

            if (!m_InternallyChanged)
                return;

            m_InternallyChanged = false;

            if (m_PartyFlag != 0 || m_PKFlag != 0 || m_TypeFlag != 0 || m_SpeechFlag != 0 || m_GuildFlag != 0) {
                bool partyOrPK = m_PartyFlag != 0 || m_PKFlag != 0;
                bool partyAndPK = m_PartyFlag != 0 && m_PKFlag != 0;

                if (m_PartyPKFlagsContainer.gameObject.activeSelf != partyOrPK)
                    m_PartyPKFlagsContainer.gameObject.SetActive(partyOrPK);
                
                if (partyAndPK)
                    m_PartyPKFlagsContainer.padding.right = -13;
                else if (partyOrPK)
                    m_PartyPKFlagsContainer.padding.right = 0;
                
                if (!m_DrawFlags) {
                    m_FlagsContainer.gameObject.SetActive(true);
                    m_DrawFlags = true;
                }
            } else if (m_DrawFlags) { // draw flags is set but there were nothing to draw
                m_FlagsContainer.gameObject.SetActive(false);
                m_DrawFlags = false;
            }
        }

        internal void SetCharacterName(string name) {
            m_NamePanel.SetText(name);
        }
        
        internal void SetHealth(int health, int maxHealth) {
            float percent = (float)health / maxHealth;
            m_HealthColor = GetHealthColor(percent);
            m_HealthProgressBar.value = percent * 100;
        }

        internal void SetHealthPercent(int healthPercent) {
            float percent = healthPercent / 100f;
            m_HealthColor = GetHealthColor(percent);
            m_HealthProgressBar.value = healthPercent;
        }

        internal void UpdateHealthColor() {
            if (m_LastCreatureVisiblity) {
                var modifiedColor = ILightmapRenderer.MulColor32(m_HealthColor, m_LastLightFactor);
                m_NamePanel.color = modifiedColor;
                m_HealthProgressBar.fillRect.GetComponent<RawImage>().color = modifiedColor;
            }
        }
        
        internal void SetMana(int mana, int maxMana) {
            float percent = (float)mana / maxMana;
            m_ManaProgressBar.value = percent * 100;
        }

        internal void SetManaPercent(int manaPercent) {
            m_ManaProgressBar.value = manaPercent;
        }
        
        internal void UpdateCreatureMisc(bool visible, float lightfactor) {
            if (m_LastCreatureVisiblity == visible && m_LastLightFactor == lightfactor)
                return;
            
            m_LastCreatureVisiblity = visible;
            m_LastLightFactor = lightfactor;
            if (visible) {
                var modifiedHealthColor = ILightmapRenderer.MulColor32(m_HealthColor, m_LastLightFactor);
                m_NamePanel.color = modifiedHealthColor;
                m_HealthProgressBar.fillRect.GetComponent<RawImage>().color = modifiedHealthColor;
                m_ManaProgressBar.fillRect.GetComponent<RawImage>().color = ILightmapRenderer.MulColor32(Color.blue, m_LastLightFactor);
            } else {
                var hiddenColor = ILightmapRenderer.MulColor32(HiddenCreatureColor, m_LastLightFactor);
                m_NamePanel.color = hiddenColor;
                m_HealthProgressBar.fillRect.GetComponent<RawImage>().color = hiddenColor;
                m_ManaProgressBar.fillRect.GetComponent<RawImage>().color = hiddenColor;
            }
        }
        

        private static PartyFlag s_GetPartyFlag(PartyFlag partyFlag) {
            if (partyFlag < PartyFlag.First || partyFlag > PartyFlag.Last) {
                throw new System.Exception("CharacterStatusPanel.s_GetPartyFlag: Invalid party flag (" + (int)partyFlag + ").");
            }

            if (partyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlag.Leader_SharedXP_Inactive_Guilty;
            else if (partyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlag.Member_SharedXP_Inactive_Guilty; // idk if this is correct

            if (partyFlag == PartyFlag.Other)
                partyFlag = PartyFlag.Member_SharedXP_Inactive_Innocent;

            return partyFlag;
        }

        private void InternalUpdatePartyFlag(PartyFlag partyFlag) {
            partyFlag = s_GetPartyFlag(partyFlag);

            if (partyFlag == PartyFlag.None) {
                if (m_PartyFlag != PartyFlag.None)
                    m_PartyFlagImage.gameObject.SetActive(false);

                m_PartyFlag = PartyFlag.None;
                return;
            }

            m_PartyFlagImage.sprite = OpenTibiaUnity.GameManager.PartySprites[(int)partyFlag - 1];
            if (m_PartyFlag == PartyFlag.None)
                m_PartyFlagImage.gameObject.SetActive(true);

            m_PartyFlag = partyFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdatePKFlag(PKFlag pkFlag) {
            if (pkFlag == PKFlag.None) {
                if (m_PKFlag != PKFlag.None)
                    m_PKFlagImage.gameObject.SetActive(false);

                m_PKFlag = PKFlag.None;
                return;
            }

            m_PKFlagImage.sprite = OpenTibiaUnity.GameManager.PKSprites[(int)pkFlag - 1];
            if (m_PKFlag == PKFlag.None)
                m_PKFlagImage.gameObject.SetActive(true);

            m_PKFlag = pkFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateTypeFlag(SummonTypeFlags typeFlag) {
            if (typeFlag == SummonTypeFlags.None) {
                if (m_TypeFlag != SummonTypeFlags.None)
                    m_TypeFlagImage.gameObject.SetActive(false);

                m_TypeFlag = SummonTypeFlags.None;
                return;
            }

            m_TypeFlagImage.sprite = OpenTibiaUnity.GameManager.TypeSprites[(int)typeFlag - 1];
            if (m_TypeFlag == SummonTypeFlags.None)
                m_TypeFlagImage.gameObject.SetActive(true);

            m_TypeFlag = typeFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateSpeechFlag(SpeechCategory speechFlag) {
            if (speechFlag == SpeechCategory.None) {
                if (m_SpeechFlag != SpeechCategory.None)
                    m_SpeechFlagImage.gameObject.SetActive(false);

                m_SpeechFlag = SpeechCategory.None;
                return;
            }

            m_SpeechFlagImage.sprite = OpenTibiaUnity.GameManager.SpeechSprites[(int)speechFlag - 1];
            if (m_SpeechFlag == SpeechCategory.None)
                m_SpeechFlagImage.gameObject.SetActive(true);

            m_SpeechFlag = speechFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateGuildFlag(GuildFlag guildFlag) {
            if (guildFlag == GuildFlag.None) {
                if (m_GuildFlag != GuildFlag.None)
                    m_GuildFlagImage.gameObject.SetActive(false);

                m_GuildFlag = GuildFlag.None;
                return;
            }

            m_GuildFlagImage.sprite = OpenTibiaUnity.GameManager.GuildSprites[(int)guildFlag - 1];
            if (m_GuildFlag == GuildFlag.None)
                m_GuildFlagImage.gameObject.SetActive(true);

            m_GuildFlag = guildFlag;
            m_InternallyChanged = true;
        }
    }
}