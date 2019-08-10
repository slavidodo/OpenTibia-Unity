using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private void ParseAmbientLight(Internal.ByteArray message) {
            int intensity = message.ReadUnsignedByte();
            int rawColor = message.ReadUnsignedByte();

            var color = Colors.ColorFrom8Bit(rawColor);
            WorldMapStorage.SetAmbientLight(color, intensity);
        }

        private void ParseGraphicalEffect(Internal.ByteArray message) {
            var position = message.ReadPosition();
            byte effectId = message.ReadUnsignedByte();

            var effect = AppearanceStorage.CreateEffectInstance(effectId);
            if (!effect)
                throw new System.Exception("ProtocolGame.ParseGraphicalEffect: Unknown effect id: " + effectId);

            WorldMapStorage.AppendEffect(position, effect);
        }

        private void ParseRemoveGraphicalEffect(Internal.ByteArray message) {

        }

        private void ParseTextEffect(Internal.ByteArray message) {
            var position = message.ReadPosition();
            int color = message.ReadUnsignedByte();
            string text = message.ReadString();

            WorldMapStorage.AddTextualEffect(position, color, text);
        }

        private void ParseMissleEffect(Internal.ByteArray message) {
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
