using OpenTibiaUnity.Core.WorldMap.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class CreatureStatusPanel : Base.AbstractComponent
    {
        private static Color HiddenCreatureColor = Core.Colors.ColorFromRGB(192, 192, 192);
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private TMPro.TextMeshProUGUI m_NamePanel;
        [SerializeField] private RectTransform m_HealthProgressPanel;
        [SerializeField] private RectTransform m_ManaProgressPanel;

        [SerializeField] private VerticalLayoutGroup m_FlagsContainer;
        [SerializeField] private HorizontalLayoutGroup m_PartyPKFlagsContainer;
        [SerializeField] private Image m_PartyFlagImage;
        [SerializeField] private Image m_PKFlagImage;
        [SerializeField] private Image m_TypeFlagImage;
        [SerializeField] private Image m_SpeechFlagImage;
        [SerializeField] private Image m_GuildFlagImage;
#pragma warning restore CS0649 // never assigned to
        public float CachedWidth { get; private set; } = 0;
        public float CachedHeight { get; private set; } = 0;
        public uint CachedRenderCount { get; set; } = 0;

        private bool m_DrawName = true;
        private bool m_DrawHealth = true;
        private bool m_DrawMana = true;
        private bool m_DrawFlags = true;
        
        private Color m_HealthColor = Color.black;
        private Color m_ManaColor = Color.black;

        private PartyFlags m_PartyFlag = 0;
        private PKFlags m_PKFlag = 0;
        private SummonTypeFlags m_TypeFlag = 0;
        private SpeechCategories m_SpeechFlag = 0;
        private GuildFlags m_GuildFlag = 0;

        private bool m_InternallyChanged = false;
        private bool m_LastCreatureVisiblity = true;
        private float m_LastLightFactor = 1f;

        public static Color GetHealthColor(float percent) {
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

        public void UpdateProperties(string name, int healthPercent, int manaPercent) {
            SetCharacterName(name);
            SetHealthPercent(healthPercent);
            SetManaPercent(manaPercent);
            
            m_NamePanel.color = m_HealthColor;
            m_HealthProgressPanel.GetComponent<RawImage>().color = m_HealthColor;
            m_ManaProgressPanel.GetComponent<RawImage>().color = Color.blue;
        }
        
        public void SetDrawingProperties(bool drawName, bool drawHealth, bool drawMana) {
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

            m_HealthProgressPanel.parent.gameObject.SetActive(drawHealth);
            if (drawHealth) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            m_ManaProgressPanel.parent.gameObject.SetActive(drawMana);
            if (drawMana) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            CachedHeight += totalDrawn * 1 + 4;
            CachedWidth += 4;
        }

        public void SetFlags(bool drawFlags, PartyFlags partyFlag, PKFlags pkFlag, SummonTypeFlags typeFlag, SpeechCategories speechFlag, GuildFlags guildFlag) {
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

        public void SetCharacterName(string name) {
            m_NamePanel.SetText(name);
        }
        
        public void SetHealth(int health, int maxHealth) {
            float percent = (float)health / maxHealth;
            m_HealthColor = GetHealthColor(percent);
            InternalUpdateProgress(m_HealthProgressPanel, health / (float)maxHealth);
        }

        public void SetHealthPercent(int healthPercent) {
            float percent = healthPercent / 100f;
            m_HealthColor = GetHealthColor(percent);
            InternalUpdateProgress(m_HealthProgressPanel, healthPercent / 100f);
        }

        public void UpdateHealthColor() {
            if (m_LastCreatureVisiblity) {
                var modifiedColor = ILightmapRenderer.MulColor32(m_HealthColor, m_LastLightFactor);
                m_NamePanel.color = modifiedColor;
                m_HealthProgressPanel.GetComponent<RawImage>().color = modifiedColor;
            }
        }
        
        public void SetMana(int mana, int maxMana) {
            InternalUpdateProgress(m_ManaProgressPanel, mana / (float)maxMana);
        }

        public void SetManaPercent(int manaPercent) {
            InternalUpdateProgress(m_ManaProgressPanel, manaPercent / 100f);
        }
        
        public void UpdateCreatureMisc(bool visible, float lightfactor) {
            if (m_LastCreatureVisiblity == visible && m_LastLightFactor == lightfactor)
                return;
            
            m_LastCreatureVisiblity = visible;
            m_LastLightFactor = lightfactor;
            if (visible) {
                var modifiedHealthColor = ILightmapRenderer.MulColor32(m_HealthColor, m_LastLightFactor);
                m_NamePanel.color = modifiedHealthColor;
                m_HealthProgressPanel.GetComponent<RawImage>().color = modifiedHealthColor;
                m_ManaProgressPanel.GetComponent<RawImage>().color = ILightmapRenderer.MulColor32(Color.blue, m_LastLightFactor);
            } else {
                var hiddenColor = ILightmapRenderer.MulColor32(HiddenCreatureColor, m_LastLightFactor);
                m_NamePanel.color = hiddenColor;
                m_HealthProgressPanel.GetComponent<RawImage>().color = hiddenColor;
                m_ManaProgressPanel.GetComponent<RawImage>().color = hiddenColor;
            }
        }

        private void InternalUpdateProgress(RectTransform rectTransform, float percent) {
            int baseWidth = 27 - 2;
            var offsetMax = m_HealthProgressPanel.offsetMax;
            offsetMax.x = -1 - baseWidth * Mathf.Min(1f, 1f - percent);
            m_HealthProgressPanel.offsetMax = offsetMax;
        }

        private static PartyFlags s_GetPartyFlag(PartyFlags partyFlag) {
            if (partyFlag < PartyFlags.None || partyFlag > PartyFlags.Other) {
                throw new System.Exception("CharacterStatusPanel.s_GetPartyFlag: Invalid flag.");
            }

            if (partyFlag == PartyFlags.Leader_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlags.Leader_SharedXP_Inactive_Guilty;
            else if (partyFlag == PartyFlags.Member_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlags.Member_SharedXP_Inactive_Guilty; // idk if this is correct

            if (partyFlag == PartyFlags.Other)
                partyFlag = PartyFlags.Member_SharedXP_Inactive_Innocent;

            return partyFlag;
        }

        private void InternalUpdatePartyFlag(PartyFlags partyFlag) {
            partyFlag = s_GetPartyFlag(partyFlag);

            if (partyFlag == 0) {
                if (m_PartyFlag != 0)
                    m_PartyFlagImage.gameObject.SetActive(false);

                m_PartyFlag = 0;
                return;
            }

            m_PartyFlagImage.sprite = OpenTibiaUnity.GameManager.PartySprites[(int)partyFlag - 1];
            if (m_PartyFlag == 0)
                m_PartyFlagImage.gameObject.SetActive(true);

            m_PartyFlag = partyFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdatePKFlag(PKFlags pkFlag) {
            if (pkFlag == 0) {
                if (m_PKFlag != 0)
                    m_PKFlagImage.gameObject.SetActive(false);

                m_PKFlag = 0;
                return;
            }

            m_PKFlagImage.sprite = OpenTibiaUnity.GameManager.PKSprites[(int)pkFlag - 1];
            if (m_PKFlag == 0)
                m_PKFlagImage.gameObject.SetActive(true);

            m_PKFlag = pkFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateTypeFlag(SummonTypeFlags typeFlag) {
            if (typeFlag == 0) {
                if (m_TypeFlag != 0)
                    m_TypeFlagImage.gameObject.SetActive(false);

                m_TypeFlag = 0;
                return;
            }

            m_TypeFlagImage.sprite = OpenTibiaUnity.GameManager.TypeSprites[(int)typeFlag - 1];
            if (m_TypeFlag == 0)
                m_TypeFlagImage.gameObject.SetActive(true);

            m_TypeFlag = typeFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateSpeechFlag(SpeechCategories speechFlag) {
            if (speechFlag == 0) {
                if (m_SpeechFlag != 0)
                    m_SpeechFlagImage.gameObject.SetActive(false);

                m_SpeechFlag = 0;
                return;
            }

            m_SpeechFlagImage.sprite = OpenTibiaUnity.GameManager.SpeechSprites[(int)speechFlag - 1];
            if (m_SpeechFlag == 0)
                m_SpeechFlagImage.gameObject.SetActive(true);

            m_SpeechFlag = speechFlag;
            m_InternallyChanged = true;
        }

        private void InternalUpdateGuildFlag(GuildFlags guildFlag) {
            if (guildFlag == 0) {
                if (m_GuildFlag != 0)
                    m_GuildFlagImage.gameObject.SetActive(false);

                m_GuildFlag = 0;
                return;
            }

            m_GuildFlagImage.sprite = OpenTibiaUnity.GameManager.GuildSprites[(int)guildFlag - 1];
            if (m_GuildFlag == 0)
                m_GuildFlagImage.gameObject.SetActive(true);

            m_GuildFlag = guildFlag;
            m_InternallyChanged = true;
        }
    }
}