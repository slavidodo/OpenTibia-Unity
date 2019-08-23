using OpenTibiaUnity.Core.WorldMap.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class CreatureStatusPanel : Base.AbstractComponent
    {
        private static Color HiddenCreatureColor = Colors.ColorFromRGB(192, 192, 192);

        [SerializeField] private TMPro.TextMeshProUGUI _namePanel = null;
        [SerializeField] private Slider _healthProgressBar = null;
        [SerializeField] private Slider _manaProgressBar = null;

        [SerializeField] private VerticalLayoutGroup _flagsContainer = null;
        [SerializeField] private HorizontalLayoutGroup _partyPKFlagsContainer = null;
        [SerializeField] private Image _partyFlagImage = null;
        [SerializeField] private Image _pKFlagImage = null;
        [SerializeField] private Image _typeFlagImage = null;
        [SerializeField] private Image _speechFlagImage = null;
        [SerializeField] private Image _guildFlagImage = null;

        public float CachedWidth { get; private set; } = 0;
        public float CachedHeight { get; private set; } = 0;
        public uint CachedRenderCount { get; set; } = 0;

        private bool _drawName = true;
        private bool _drawHealth = true;
        private bool _drawMana = true;
        private bool _drawFlags = true;
        
        private Color _healthColor = Color.black;
        private Color _manaColor = Color.black;

        private PartyFlag _partyFlag = PartyFlag.None;
        private PKFlag _pKFlag = PKFlag.None;
        private SummonTypeFlags _typeFlag = SummonTypeFlags.None;
        private SpeechCategory _speechFlag = SpeechCategory.None;
        private GuildFlag _guildFlag = GuildFlag.None;

        private bool _internallyChanged = false;
        private bool _lastCreatureVisiblity = true;
        private float _lastLightFactor = 1f;

        private uint _creature_id = 0;

        public uint Creature_id {
            get => _creature_id;
            set { if (_creature_id == 0) _creature_id = value; }
        }

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
            
            _namePanel.color = _healthColor;
            _healthProgressBar.fillRect.GetComponent<RawImage>().color = _healthColor;
            _manaProgressBar.fillRect.GetComponent<RawImage>().color = Color.blue;
        }
        
        public void SetDrawingProperties(bool drawName, bool drawHealth, bool drawMana) {
            if (drawName == _drawName && drawHealth == _drawHealth && drawMana == _drawMana)
                return;

            CachedWidth = 0;
            CachedHeight = 0;

            int totalDrawn = 0;

            _namePanel.gameObject.SetActive(drawName);
            if (drawName) {
                CachedHeight += 14;
                CachedWidth = Mathf.Max(CachedWidth, _namePanel.preferredWidth);
                totalDrawn += 1;
            }

            _healthProgressBar.gameObject.SetActive(drawHealth);
            if (drawHealth) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            _manaProgressBar.gameObject.SetActive(drawMana);
            if (drawMana) {
                CachedHeight += 4;
                CachedWidth = Mathf.Max(CachedWidth, 27);
                totalDrawn += 1;
            }

            CachedHeight += totalDrawn * 1 + 4;
            CachedWidth += 4;
        }

        public void SetFlags(bool drawFlags, PartyFlag partyFlag, PKFlag pkFlag, SummonTypeFlags typeFlag, SpeechCategory speechFlag, GuildFlag guildFlag) {
            if (!drawFlags) {
                if (_drawFlags) {
                    _flagsContainer.gameObject.SetActive(false);
                    _drawFlags = false;
                }

                return;
            }

            if (_partyFlag != partyFlag)
                InternalUpdatePartyFlag(partyFlag);

            if (_pKFlag != pkFlag)
                InternalUpdatePKFlag(pkFlag);

            if (_typeFlag != typeFlag)
                InternalUpdateTypeFlag(typeFlag);

            if (_speechFlag != speechFlag)
                InternalUpdateSpeechFlag(speechFlag);

            if (_guildFlag != guildFlag)
                InternalUpdateGuildFlag(guildFlag);

            if (!_internallyChanged)
                return;

            _internallyChanged = false;

            if (_partyFlag != 0 || _pKFlag != 0 || _typeFlag != 0 || _speechFlag != 0 || _guildFlag != 0) {
                bool partyOrPK = _partyFlag != 0 || _pKFlag != 0;
                bool partyAndPK = _partyFlag != 0 && _pKFlag != 0;

                if (_partyPKFlagsContainer.gameObject.activeSelf != partyOrPK)
                    _partyPKFlagsContainer.gameObject.SetActive(partyOrPK);
                
                if (partyAndPK)
                    _partyPKFlagsContainer.padding.right = -13;
                else if (partyOrPK)
                    _partyPKFlagsContainer.padding.right = 0;
                
                if (!_drawFlags) {
                    _flagsContainer.gameObject.SetActive(true);
                    _drawFlags = true;
                }
            } else if (_drawFlags) { // draw flags is set but there were nothing to draw
                _flagsContainer.gameObject.SetActive(false);
                _drawFlags = false;
            }
        }

        public void SetCharacterName(string name) {
            _namePanel.SetText(name);
        }
        
        public void SetHealth(int health, int maxHealth) {
            float percent = (float)health / maxHealth;
            _healthColor = GetHealthColor(percent);
            _healthProgressBar.value = percent * 100;
        }

        public void SetHealthPercent(int healthPercent) {
            float percent = healthPercent / 100f;
            _healthColor = GetHealthColor(percent);
            _healthProgressBar.value = healthPercent;
        }

        public void UpdateHealthColor() {
            if (_lastCreatureVisiblity) {
                var modifiedColor = Utils.Utility.MulColor32(_healthColor, _lastLightFactor);
                _namePanel.color = modifiedColor;
                _healthProgressBar.fillRect.GetComponent<RawImage>().color = modifiedColor;
            }
        }
        
        public void SetMana(int mana, int maxMana) {
            float percent = (float)mana / maxMana;
            _manaProgressBar.value = percent * 100;
        }

        public void SetManaPercent(int manaPercent) {
            _manaProgressBar.value = manaPercent;
        }
        
        public void UpdateCreatureMisc(bool visible, float lightfactor) {
            if (_lastCreatureVisiblity == visible && _lastLightFactor == lightfactor)
                return;
            
            _lastCreatureVisiblity = visible;
            _lastLightFactor = lightfactor;
            if (visible) {
                var modifiedHealthColor = Utils.Utility.MulColor32(_healthColor, _lastLightFactor);
                _namePanel.color = modifiedHealthColor;
                _healthProgressBar.fillRect.GetComponent<RawImage>().color = modifiedHealthColor;
                _manaProgressBar.fillRect.GetComponent<RawImage>().color = Utils.Utility.MulColor32(Color.blue, _lastLightFactor);
            } else {
                var hiddenColor = Utils.Utility.MulColor32(HiddenCreatureColor, _lastLightFactor);
                _namePanel.color = hiddenColor;
                _healthProgressBar.fillRect.GetComponent<RawImage>().color = hiddenColor;
                _manaProgressBar.fillRect.GetComponent<RawImage>().color = hiddenColor;
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
                if (_partyFlag != PartyFlag.None)
                    _partyFlagImage.gameObject.SetActive(false);

                _partyFlag = PartyFlag.None;
                return;
            }

            _partyFlagImage.sprite = OpenTibiaUnity.GameManager.PartySprites[(int)partyFlag - 1];
            if (_partyFlag == PartyFlag.None)
                _partyFlagImage.gameObject.SetActive(true);

            _partyFlag = partyFlag;
            _internallyChanged = true;
        }

        private void InternalUpdatePKFlag(PKFlag pkFlag) {
            if (pkFlag == PKFlag.None) {
                if (_pKFlag != PKFlag.None)
                    _pKFlagImage.gameObject.SetActive(false);

                _pKFlag = PKFlag.None;
                return;
            }

            _pKFlagImage.sprite = OpenTibiaUnity.GameManager.PKSprites[(int)pkFlag - 1];
            if (_pKFlag == PKFlag.None)
                _pKFlagImage.gameObject.SetActive(true);

            _pKFlag = pkFlag;
            _internallyChanged = true;
        }

        private void InternalUpdateTypeFlag(SummonTypeFlags typeFlag) {
            if (typeFlag == SummonTypeFlags.None) {
                if (_typeFlag != SummonTypeFlags.None)
                    _typeFlagImage.gameObject.SetActive(false);

                _typeFlag = SummonTypeFlags.None;
                return;
            }

            _typeFlagImage.sprite = OpenTibiaUnity.GameManager.TypeSprites[(int)typeFlag - 1];
            if (_typeFlag == SummonTypeFlags.None)
                _typeFlagImage.gameObject.SetActive(true);

            _typeFlag = typeFlag;
            _internallyChanged = true;
        }

        private void InternalUpdateSpeechFlag(SpeechCategory speechFlag) {
            if (speechFlag == SpeechCategory.None) {
                if (_speechFlag != SpeechCategory.None)
                    _speechFlagImage.gameObject.SetActive(false);

                _speechFlag = SpeechCategory.None;
                return;
            }

            _speechFlagImage.sprite = OpenTibiaUnity.GameManager.SpeechSprites[(int)speechFlag - 1];
            if (_speechFlag == SpeechCategory.None)
                _speechFlagImage.gameObject.SetActive(true);

            _speechFlag = speechFlag;
            _internallyChanged = true;
        }

        private void InternalUpdateGuildFlag(GuildFlag guildFlag) {
            if (guildFlag == GuildFlag.None) {
                if (_guildFlag != GuildFlag.None)
                    _guildFlagImage.gameObject.SetActive(false);

                _guildFlag = GuildFlag.None;
                return;
            }

            _guildFlagImage.sprite = OpenTibiaUnity.GameManager.GuildSprites[(int)guildFlag - 1];
            if (_guildFlag == GuildFlag.None)
                _guildFlagImage.gameObject.SetActive(true);

            _guildFlag = guildFlag;
            _internallyChanged = true;
        }
    }
}