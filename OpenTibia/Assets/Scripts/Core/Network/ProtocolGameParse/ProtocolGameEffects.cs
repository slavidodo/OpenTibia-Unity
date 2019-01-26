using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseAmbientLight(InputMessage message) {
            byte intensity = message.GetU8();
            Color color = Colors.ColorFrom8Bit(message.GetU8());
            m_WorldMapStorage.SetAmbientLight(color, intensity);
        }

        private void ParseGraphicalEffect(InputMessage message) {
            var position = message.GetPosition();
            byte effectId = message.GetU8();

            var effect = m_AppearanceStorage.CreateEffectInstance(effectId);
            if (!effect)
                throw new System.Exception("Unknown effect id: " + effectId);

            m_WorldMapStorage.AppendEffect(position, effect);
        }

        private void ParseMissleEffect(InputMessage message) {
            var fromPosition = message.GetPosition();
            var toPosition = message.GetPosition();
            byte missleId = message.GetU8();

            var missle = m_AppearanceStorage.CreateMissileInstance(missleId, fromPosition, toPosition);
            if (!missle)
                throw new System.Exception("Unknown missle id: " + missleId);
            
            m_WorldMapStorage.AppendEffect(fromPosition, missle);
        }
    }
}
