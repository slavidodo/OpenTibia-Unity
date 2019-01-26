namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParsePreyFreeListRerollAvailability(InputMessage message) {
            byte slot = message.GetU8();
            ushort minutes = message.GetU16();
        }

        private void ParsePreyTimeLeft(InputMessage message) {
            byte slot = message.GetU8();
            ushort minutes = message.GetU16();
        }

        private void ParsePreyData(InputMessage message) {
            byte slot = message.GetU8();
            byte state = message.GetU8();
            switch (state) {
                case 0: {
                    byte lockType = message.GetU8();
                    break;
                }

                case 1: {
                    break;
                }

                case 2: {
                    message.GetString();
                    ReadCreatureOutfit(message);
                    message.GetU8();
                    message.GetU16();
                    message.GetU8();
                    message.GetU16();
                    break;
                }

                case 3: {
                    byte size = message.GetU8();
                    for (int i = 0; i < size; i++) {
                        message.GetString();
                        ReadCreatureOutfit(message);
                    }
                    break;
                }

                case 4: {
                    message.GetU8();
                    message.GetU16();
                    message.GetU8();
                    byte size = message.GetU8();
                    for (int i = 0; i < size; i++) {
                        message.GetString();
                        ReadCreatureOutfit(message);
                    }
                    break;
                }
                default:
                    break;
            }

            message.GetU16();
        }

        private void ParsePreyRerollPrice(InputMessage message) {
            message.GetU32();
        }
    }
}
