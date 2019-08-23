using pbr = Google.Protobuf.Reflection;
using pb = Google.Protobuf;

namespace OpenTibiaUnity.Protobuf.Protocol
{
    public abstract class ProtocolMessage<T> : pb::IMessage<T> where T : pb::IMessage<T>
    {
        public pbr.MessageDescriptor Descriptor => throw new System.NotImplementedException();

        public abstract int CalculateSize();

        public virtual T Clone() {
            throw new System.NotImplementedException();
        }

        public virtual bool Equals(T other) {
            throw new System.NotImplementedException();
        }

        public void MergeFrom(T message) {
            throw new System.NotImplementedException();
        }

        public void MergeFrom(pb.CodedInputStream input) {
            throw new System.NotImplementedException();
        }

        public void WriteTo(pb.CodedOutputStream output) {
            throw new System.NotImplementedException();
        }
    }
}
