using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseCreatureMark(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            byte mark = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature) {
                creature.Marks.SetMark(MarkType.OneSecondTemp, mark);
                CreatureStorage.InvalidateOpponents();
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creature_id);
            }
        }

        private void ParseTrappers(Internal.ByteArray message) {
            int n = message.ReadUnsignedByte();
            List<Creatures.Creature> trappers = new List<Creatures.Creature>();
            for (int i = 0; i < n; i++) {
                var creature_id = message.ReadUnsignedInt();
                var creature = CreatureStorage.GetCreature(creature_id);
                if (creature)
                    trappers.Add(creature);
            }

            CreatureStorage.SetTrappers(trappers);
        }

        private void ParseCreatureHealth(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            byte healthPercent = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature) {
                creature.SetSkill(SkillType.HealthPercent, healthPercent);
                CreatureStorage.InvalidateOpponents();
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureHealth: Unknown creature id: " + creature_id);
            }
        }

        private void ParseCreatureLight(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();

            byte intensity = message.ReadUnsignedByte();
            byte color = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature) {
                creature.Brightness = intensity;
                creature.LightColor = Colors.ColorFrom8Bit(color);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureLight: Unknown creature id: " + creature_id);
            }
        }

        private void ParseCreatureOutfit(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            var outfit = ReadCreatureOutfit(message);
            
            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature) {
                creature.Outfit = outfit;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                    creature.MountOutfit = ReadMountOutfit(message, creature.MountOutfit);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureOutfit: Unknown creature id: " + creature_id);
            }
        }

        private void ParseCreatureSpeed(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            int baseSpeed = -1;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1059)
                baseSpeed = message.ReadUnsignedShort();

            int speed = message.ReadUnsignedShort();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature)
                creature.SetSkill(SkillType.Speed, speed, baseSpeed);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureSpeed: Unknown creature id: " + creature_id);
        }

        private void ParseCreatureSkull(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            byte pkFlag = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature)
                creature.SetPKFlag((PKFlag)pkFlag);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureSkull: Unknown creature id: " + creature_id);    
        }

        private void ParseCreatureShield(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            byte partyFlag = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature)
                creature.SetPartyFlag((PartyFlag)partyFlag);
            else
                throw new System.Exception("ProtocolGame.ParseCreatureShield: Unknown creature id: " + creature_id);
        }

        private void ParseCreatureUnpass(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            bool unpass = message.ReadBoolean();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature)
                creature.Unpassable = unpass;
            else
                throw new System.Exception("ProtocolGame.ParseCreatureUnpass: Unknown creature id: " + creature_id);
        }

        private void ParseCreatureMarks(Internal.ByteArray message) {
            int length;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1035)
                length = 1;
            else
                length = message.ReadUnsignedByte();

            for (int i = 0; i < length; i++) {
                uint creature_id = message.ReadUnsignedInt();
                bool permenant = message.ReadUnsignedByte() != 1;
                byte mark = message.ReadUnsignedByte();

                var creature = CreatureStorage.GetCreature(creature_id);
                if (!!creature) {
                    creature.Marks.SetMark(permenant ? MarkType.Permenant : MarkType.OneSecondTemp, mark);
                    CreatureStorage.InvalidateOpponents();
                } else {
                    throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creature_id);
                }
            }
        }

        private void ParsePlayerHelpers(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            ushort helpers = message.ReadUnsignedShort();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature)
                creature.NumberOfPvPHelpers = helpers;
            else
                throw new System.Exception("ProtocolGame.ParsePlayerHelpers: Unknown creature id: " + creature_id); 
        }

        private void ParseCreatureType(Internal.ByteArray message) {
            uint creature_id = message.ReadUnsignedInt();
            byte type = message.ReadUnsignedByte();
            uint master = 0;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1120 && type == (int)CreatureType.Summon)
                master = message.ReadUnsignedInt();

            var creature = CreatureStorage.GetCreature(creature_id);
            if (!!creature) {
                creature.Type = (CreatureType)type;
                creature.SetSummoner_id(master);
            } else {
                throw new System.Exception("ProtocolGame.ParseCreatureType: Unknown creature id: " + creature_id);
            }
        }
    }
}
