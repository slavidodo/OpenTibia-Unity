using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseAmbientLight(Internal.CommunicationStream message) {
            int intensity = message.ReadUnsignedByte();
            int rawColor = message.ReadUnsignedByte();

            var color = Colors.ColorFrom8Bit(rawColor);
            WorldMapStorage.SetAmbientLight(color, intensity);
        }

        private void ParseGraphicalEffects(Internal.CommunicationStream message) {
            var initialPosition = message.ReadPosition();
            int modifier = message.ReadUnsignedByte();

            var fromPosition = initialPosition;
            ushort unclampedOffset = 0;
            while (modifier != 0) { // 0: end loop
                if (modifier == 1) { // delta -> used to define a change in position
                    unclampedOffset = message.ReadUnsignedShort();
                    int offset = unclampedOffset % 256;
                    fromPosition.x += offset % Constants.MapSizeX;
                    fromPosition.y += offset / Constants.MapSizeX;
                }

                // the effect is far away from the initial position
                while (fromPosition.x - initialPosition.x >= Constants.MapSizeX) {
                    fromPosition.x -= Constants.MapSizeX;
                    fromPosition.y++;
                }

                byte effectId;
                Appearances.AppearanceInstance effect;
                if ((unclampedOffset >= 1024 && modifier == 1) || (modifier & 3) == 0) {
                    effectId = message.ReadUnsignedByte();
                    int deltaX = message.ReadSignedByte();
                    int deltaY = message.ReadSignedByte();
                    var toPosition = new Vector3Int(fromPosition.x + deltaX, fromPosition.y + deltaY, fromPosition.z);
                    effect = AppearanceStorage.CreateMissileInstance(effectId, fromPosition, toPosition);
                } else if (unclampedOffset >= 768 || modifier == 3) {
                    effectId = message.ReadUnsignedByte();
                    effect = AppearanceStorage.CreateEffectInstance(effectId);
                } else {
                    throw new System.NotImplementedException();
                }

                if (!effect)
                    throw new System.Exception("ProtocolGame.ParseGraphicalEffects: Unknown effect: " + effectId);

                WorldMapStorage.AppendEffect(fromPosition, effect);
                modifier = message.ReadUnsignedByte();
            }
        }

        private void ParseGraphicalEffect(Internal.CommunicationStream message) {
            var position = message.ReadPosition();
            byte effectId = message.ReadUnsignedByte();

            var effect = AppearanceStorage.CreateEffectInstance(effectId);
            if (!effect)
                throw new System.Exception("ProtocolGame.ParseGraphicalEffect: Unknown effect id: " + effectId);

            WorldMapStorage.AppendEffect(position, effect);
        }

        private void ParseRemoveGraphicalEffect(Internal.CommunicationStream message) {

        }

        private void ParseTextEffect(Internal.CommunicationStream message) {
            var position = message.ReadPosition();
            int color = message.ReadUnsignedByte();
            string text = message.ReadString();

            WorldMapStorage.AddTextualEffect(position, color, text);
        }

        private void ParseMissleEffect(Internal.CommunicationStream message) {
            var fromPosition = message.ReadPosition();
            var toPosition = message.ReadPosition();
            byte missleId = message.ReadUnsignedByte();

            var missle = AppearanceStorage.CreateMissileInstance(missleId, fromPosition, toPosition);
            if (!missle)
                throw new System.Exception("ProtocolGame.ParseMissleEffect: Unknown missle id: " + missleId);
            
            WorldMapStorage.AppendEffect(fromPosition, missle);
        }
    }
}
