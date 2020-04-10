using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseCreatureMark(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte mark = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.Marks.SetMark(MarkType.OneSecondTemp, mark);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureMark: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseTrappers(Internal.CommunicationStream message) {
            int n = message.ReadUnsignedByte();
            List<Creatures.Creature> trappers = new List<Creatures.Creature>();
            for (int i = 0; i < n; i++) {
                var creatureId = message.ReadUnsignedInt();
                var creature = CreatureStorage.GetCreatureById(creatureId);
                if (creature)
                    trappers.Add(creature);
            }

            CreatureStorage.SetTrappers(trappers);
        }

        private void ParseCreatureHealth(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte healthPercent = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.SetSkill(SkillType.HealthPercent, healthPercent);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureHealth: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureLight(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();

            byte intensity = message.ReadUnsignedByte();
            byte color = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.Brightness = intensity;
                creature.LightColor = Colors.ColorFrom8Bit(color);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureLight: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureOutfit(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            var outfit = ProtocolGameExtentions.ReadCreatureOutfit(message);
            
            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.Outfit = outfit;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                    creature.MountOutfit = ProtocolGameExtentions.ReadMountOutfit(message, creature.MountOutfit);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureOutfit: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureSpeed(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            int baseSpeed = -1;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1059)
                baseSpeed = message.ReadUnsignedShort();

            int speed = message.ReadUnsignedShort();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.SetSkill(SkillType.Speed, speed, baseSpeed);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureSpeed: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureSkull(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte pkFlag = message.ReadUnsignedByte();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.SetPKFlag((PkFlag)pkFlag);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureSkull: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureShield(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            var partyFlag = message.ReadEnum<PartyFlag>();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.SetPartyFlag(partyFlag);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureShield: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureUnpass(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            bool unpass = message.ReadBoolean();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.Unpassable = unpass;
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureUnpass: Unknown creature id: " + creatureId);
            }*/
        }

        private void ParseCreatureMarks(Internal.CommunicationStream message) {
            int length;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1035)
                length = 1;
            else
                length = message.ReadUnsignedByte();

            for (int i = 0; i < length; i++) {
                uint creatureId = message.ReadUnsignedInt();
                bool permenant = message.ReadUnsignedByte() != 1;
                byte mark = message.ReadUnsignedByte();

                var creature = CreatureStorage.GetCreatureById(creatureId);
                if (!!creature) {
                    creature.Marks.SetMark(permenant ? MarkType.Permenant : MarkType.OneSecondTemp, mark);
                }/*else {
                    throw new System.Exception("ProtocolGame.ParseCreatureMarks: Unknown creature id: " + creatureId);
                }*/
            }
        }

        private void ParsePlayerHelpers(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            ushort helpers = message.ReadUnsignedShort();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature)
                creature.NumberOfPvPHelpers = helpers;
            /*else
                throw new System.Exception("ProtocolGame.ParsePlayerHelpers: Unknown creature id: " + creatureId);*/
        }

        private void ParseCreatureType(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte type = message.ReadUnsignedByte();
            uint master = 0;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1120 && type == (int)CreatureType.Summon)
                master = message.ReadUnsignedInt();

            var creature = CreatureStorage.GetCreatureById(creatureId);
            if (!!creature) {
                creature.Type = (CreatureType)type;
                creature.SetSummonerId(master);
            }/*else {
                throw new System.Exception("ProtocolGame.ParseCreatureType: Unknown creature id: " + creatureId);
            }*/
        }
    }
}
