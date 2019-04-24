using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseCreatureMark(InputMessage message) {
            uint creatureID = message.GetU32();
            byte mark = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature) {
                creature.Marks.SetMark(MarkTypes.OneSecondTemp, mark);
                m_CreatureStorage.InvalidateOpponents();
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creatureID);
            }
        }

        private void ParseTrappers(InputMessage message) {
            int n = message.GetU8();
            List<Creatures.Creature> trappers = new List<Creatures.Creature>();
            for (int i = 0; i < n; i++) {
                var creatureID = message.GetU32();
                var creature = m_CreatureStorage.GetCreature(creatureID);
                if (creature)
                    trappers.Add(creature);
            }

            m_CreatureStorage.SetTrappers(trappers);
        }

        private void ParseCreatureHealth(InputMessage message) {
            uint creatureID = message.GetU32();
            byte healthPercent = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature) {
                creature.SetSkill(SkillTypes.HealthPercent, healthPercent);
                m_CreatureStorage.InvalidateOpponents();
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureHealth: Unknown creature id: " + creatureID);
            }
        }

        private void ParseCreatureLight(InputMessage message) {
            uint creatureID = message.GetU32();

            byte intensity = message.GetU8();
            byte color = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature) {
                creature.Brightness = intensity;
                creature.LightColor = Colors.ColorFrom8Bit(color);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureLight: Unknown creature id: " + creatureID);
            }
        }

        private void ParseCreatureOutfit(InputMessage message) {
            uint creatureID = message.GetU32();
            var outfit = ReadCreatureOutfit(message);

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature) {
                creature.Outfit = outfit;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerMounts))
                    creature.MountOutfit = ReadMountOutfit(message, creature.MountOutfit);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureOutfit: Unknown creature id: " + creatureID);
            }
        }

        private void ParseCreatureSpeed(InputMessage message) {
            uint creatureID = message.GetU32();
            int baseSpeed = -1;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1059)
                baseSpeed = message.GetU16();

            int speed = message.GetU16();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature)
                creature.SetSkill(SkillTypes.Speed, speed, baseSpeed);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureSpeed: Unknown creature id: " + creatureID);
        }

        private void ParseCreatureSkull(InputMessage message) {
            uint creatureID = message.GetU32();
            byte pkFlag = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature)
                creature.SetPKFlag((PKFlags)pkFlag);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureSkull: Unknown creature id: " + creatureID);    
        }

        private void ParseCreatureShield(InputMessage message) {
            uint creatureID = message.GetU32();
            byte partyFlag = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature)
                creature.SetPartyFlag((PartyFlags)partyFlag);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureShield: Unknown creature id: " + creatureID);
        }

        private void ParseCreatureUnpass(InputMessage message) {
            uint creatureID = message.GetU32();
            bool unpass = message.GetBool();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature)
                creature.Unpassable = unpass;
            else
                throw new System.Exception("ProtocolGame.ParseCreatureUnpass: Unknown creature id: " + creatureID);
        }

        private void ParseCreatureMarks(InputMessage message) {
            int length;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1035)
                length = 1;
            else
                length = message.GetU8();

            for (int i = 0; i < length; i++) {
                uint creatureID = message.GetU32();
                bool permenant = message.GetU8() != 1;
                byte mark = message.GetU8();

                var creature = m_CreatureStorage.GetCreature(creatureID);
                if (!!creature) {
                    creature.Marks.SetMark(permenant ? MarkTypes.Permenant : MarkTypes.OneSecondTemp, mark);
                    m_CreatureStorage.InvalidateOpponents();
                } else {
                    throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creatureID);
                }
            }
        }

        private void ParsePlayerHelpers(InputMessage message) {
            uint creatureID = message.GetU32();
            ushort helpers = message.GetU16();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature)
                creature.NumberOfPvPHelpers = helpers;
            else
                throw new System.Exception("ProtocolGame.ParsePlayerHelpers: Unknown creature id: " + creatureID); 
        }

        private void ParseCreatureType(InputMessage message) {
            uint creatureID = message.GetU32();
            byte type = message.GetU8();
            uint master = 0;
            if (type == (int)CreatureTypes.Summon)
                master = message.GetU32();

            var creature = m_CreatureStorage.GetCreature(creatureID);
            if (!!creature) {
                creature.Type = (CreatureTypes)type;
                creature.SetSummonerID(master);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureType: Unknown creature id: " + creatureID);
            }
        }
    }
}
