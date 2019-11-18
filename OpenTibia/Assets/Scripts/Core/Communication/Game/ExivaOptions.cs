using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseMessageExivaSuppressed(Internal.CommunicationStream message) {
            // no payload
        }

        private void ParseUpdateExivaOptions(Internal.CommunicationStream message) {
            message.ReadBoolean();
            message.ReadBoolean();
            message.ReadBoolean();
            message.ReadBoolean();
            message.ReadBoolean();
            message.ReadBoolean();

            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();

            int count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++)
                list1.Add(message.ReadString());

            count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++)
                list2.Add(message.ReadString());

            count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++)
                list3.Add(message.ReadString());

            count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++)
                list4.Add(message.ReadString());
        }
    }
}
