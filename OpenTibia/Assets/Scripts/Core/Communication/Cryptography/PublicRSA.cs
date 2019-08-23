using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;

namespace OpenTibiaUnity.Core.Communication.Cryptography
{
    public class PublicRSA
    {
        public const string OpenTibiaM = "109120132967399429278860960508995541528237502902798129123468757937266291492576446330739696001110603907230888610072655818825358503429057592827629436413108566029093628212635953836686562675849720620786279431090218017681061521755056710823876476444260558147179707119674283982419152118103759076030616683978566631413";
        public const string OpenTibiaE = "65537";

        public const string RealTibiaM = "132127743205872284062295099082293384952776326496165507967876361843343953435544496682053323833394351797728954155097012103928360786959821132214473291575712138800495033169914814069637740318278150290733684032524174782740134357629699062987023311132821016569775488792221429527047321331896351555606801473202394175817";
        public const string RealTibiaE = "65537";

        public const int RSABlockSize = 128;
        
        private static readonly RsaEngine OpenTibiaEncryptEngine;
        private static readonly RsaEngine RealTibiaEncryptEngine;
        private static readonly Random s_Random;

        static PublicRSA() {
            var openTibiaEncryptKey = new RsaKeyParameters(false, new BigInteger(OpenTibiaM), new BigInteger(OpenTibiaE));
            OpenTibiaEncryptEngine = new RsaEngine();
            OpenTibiaEncryptEngine.Init(true, openTibiaEncryptKey);

            var realTibiaEncruptKey = new RsaKeyParameters(false, new BigInteger(RealTibiaM), new BigInteger(RealTibiaE));
            RealTibiaEncryptEngine = new RsaEngine();
            RealTibiaEncryptEngine.Init(true, realTibiaEncruptKey);

            s_Random = new Random();
        }

        public static void EncryptMessage(Internal.ByteArray message, int payloadStart, int blockSize) {
            blockSize = Math.Min(blockSize, message.Length - payloadStart);
            message.Position = payloadStart + blockSize;
            
            int length = (int)(Math.Floor((blockSize + RSABlockSize - 1D) / RSABlockSize) * RSABlockSize);
            if (length > blockSize) {
                var tmp = new byte[length - blockSize];
                s_Random.NextBytes(tmp);
                message.WriteBytes(tmp);
                blockSize = length;
            }

            message.Position = payloadStart;
            var bytes = ProcessBlock(message.Buffer, payloadStart, RSABlockSize);
            message.WriteBytes(bytes, 0, bytes.Length);
        }

        private static byte[] ProcessBlock(byte[] buffer, int offset, int length) {
            RsaEngine engine;
            if (OpenTibiaUnity.GameManager.IsRealTibia)
                engine = RealTibiaEncryptEngine;
            else
                engine = OpenTibiaEncryptEngine;

            return engine.ProcessBlock(buffer, offset, length);
        }
    }
}
