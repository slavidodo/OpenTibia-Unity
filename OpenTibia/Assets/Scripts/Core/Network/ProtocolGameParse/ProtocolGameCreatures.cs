using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseCreatureHealth(InputMessage message) {
            uint creatureId = message.GetU32();
            byte healthPercent = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature) {
                throw new System.Exception("ProtocolGame.ParseCreatureHealth: Unknown creature id: " + creatureId);
            } else {
                creature.SetSkill(SkillTypes.HealthPercent, healthPercent);
                m_CreatureStorage.InvalidateOpponents();
            }
        }

        private void ParseCreatureLight(InputMessage message) {
            uint creatureId = message.GetU32();

            byte intensity = message.GetU8();
            byte color = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature) {
                throw new System.Exception("ProtocolGame.ParseCreatureLight: Unknown creature id: " + creatureId);
            } else {
                creature.Brightness = intensity;
                creature.LightColor = Colors.ColorFrom8Bit(color);
            }
        }

        private void ParseCreatureOutfit(InputMessage message) {
            uint creatureId = message.GetU32();
            var outfit = ReadCreatureOutfit(message);
            var mountOutfit = ReadMountOutfit(message);

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature) {
                throw new System.Exception("ProtocolGame.ParseCreatureOutfit: Unknown creature id: " + creatureId);
            } else {
                creature.Outfit = outfit;
                creature.MountOutfit = mountOutfit;
            }
        }

        private void ParseCreatureSpeed(InputMessage message) {
            uint creatureId = message.GetU32();
            ushort baseSpeed = message.GetU16();
            ushort speed = message.GetU16();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature)
                throw new System.Exception("ProtocolGame.ParseCreatureSpeed: Unknown creature id: " + creatureId);
            else
                creature.SetSkill(SkillTypes.Speed, speed, baseSpeed);
        }

        private void ParseCreatureSkull(InputMessage message) {
            uint creatureId = message.GetU32();
            byte pkFlag = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature)
                throw new System.Exception("ProtocolGame.ParseCreatureSkull: Unknown creature id: " + creatureId);
            else
                creature.SetPKFlag((PKFlags)pkFlag);
        }

        private void ParseCreatureShield(InputMessage message) {
            uint creatureId = message.GetU32();
            byte partyFlag = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature)
                throw new System.Exception("ProtocolGame.ParseCreatureShield: Unknown creature id: " + creatureId);
            else
                creature.SetPartyFlag((PartyFlags)partyFlag);
        }

        private void ParseCreatureUnpass(InputMessage message) {
            uint creatureId = message.GetU32();
            bool unpass = message.GetBool();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature)
                throw new System.Exception("ProtocolGame.ParseCreatureUnpass: Unknown creature id: " + creatureId);
            else
                creature.Unpassable = unpass;
        }

        private void ParseCreatureMarks(InputMessage message) {
            uint creatureId = message.GetU32();
            bool permenant = message.GetU8() != 1;
            byte mark = message.GetU8();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature) {
                throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creatureId);
            } else {
                if (permenant)
                    creature.Marks.SetMark(Appearances.Marks.MarkType_Permenant, mark);
                else
                    creature.Marks.SetMark(Appearances.Marks.MarkType_OneSecondTemp, mark);

                m_CreatureStorage.InvalidateOpponents();
            }
        }

        private void ParsePlayerHelpers(InputMessage message) {
            uint creatureId = message.GetU32();
            ushort helpers = message.GetU16();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature)
                throw new System.Exception("ProtocolGame.ParsePlayerHelpers: Unknown creature id: " + creatureId);
            else
                creature.NumberOfPvPHelpers = helpers;
        }

        private void ParseCreatureType(InputMessage message) {
            uint creatureId = message.GetU32();
            byte type = message.GetU8();
            uint master = 0;
            if (type == (int)CreatureTypes.Summon)
                master = message.GetU32();

            var creature = m_CreatureStorage.GetCreature(creatureId);
            if (!creature) {
                throw new System.Exception("ProtocolGame.ParseCreatureType: Unknown creature id: " + creatureId);
            } else {
                creature.Type = (CreatureTypes)type;
                creature.SetSummonerID(master);
            }
        }
    }
}
