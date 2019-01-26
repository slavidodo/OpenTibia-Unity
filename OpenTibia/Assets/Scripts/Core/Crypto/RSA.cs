using global::Org.BouncyCastle.Math;
using global::Org.BouncyCastle.Crypto.Engines;
using global::Org.BouncyCastle.Crypto.Parameters;

namespace OpenTibiaUnity.Core.Crypto
{
    class RSA
    {
        public static string OpenTibiaP = "14299623962416399520070177382898895550795403345466153217470516082934737582776038882967213386204600674145392845853859217990626450972452084065728686565928113";
        public static string OpenTibiaQ = "7630979195970404721891201847792002125535401292779123937207447574596692788513647179235335529307251350570728407373705564708871762033017096809910315212884101";
        public static string OpenTibiaDP = "11141736698610418925078406669215087697114858422461871124661098818361832856659225315773346115219673296375487744032858798960485665997181641221483584094519937";
        public static string OpenTibiaDQ = "4886309137722172729208909250386672706991365415741885286554321031904881408516947737562153523770981322408725111241551398797744838697461929408240938369297973";
        public static string OpenTibiaInverseQ = "5610960212328996596431206032772162188356793727360507633581722789998709372832546447914318965787194031968482458122348411654607397146261039733584248408719418";
        public static string OpenTibiaM = "109120132967399429278860960508995541528237502902798129123468757937266291492576446330739696001110603907230888610072655818825358503429057592827629436413108566029093628212635953836686562675849720620786279431090218017681061521755056710823876476444260558147179707119674283982419152118103759076030616683978566631413";
        public static string OpenTibiaE = "65537";

        private static readonly RsaEngine openTibiaDecryptEngine;
        private static readonly RsaEngine openTibiaEncryptEngine;
        private static readonly RsaEngine realTibiaEncryptEngine;
        
        static RSA() {
            var openTibiaDecryptKey = new RsaPrivateCrtKeyParameters(new BigInteger(OpenTibiaM), new BigInteger(OpenTibiaE),
                new BigInteger(OpenTibiaE), new BigInteger(OpenTibiaP), new BigInteger(OpenTibiaQ),
                new BigInteger(OpenTibiaDP), new BigInteger(OpenTibiaDQ), new BigInteger(OpenTibiaInverseQ));

            openTibiaDecryptEngine = new RsaEngine();
            openTibiaDecryptEngine.Init(false, openTibiaDecryptKey);

            var openTibiaEncryptKey = new RsaKeyParameters(false, new BigInteger(OpenTibiaM), new BigInteger(OpenTibiaE));
            openTibiaEncryptEngine = new RsaEngine();
            openTibiaEncryptEngine.Init(true, openTibiaEncryptKey);
        }

        public static int GetRsaSize() {
            return 128;
        }

        public static void EncryptMessage(Network.OutputMessage message) {
            byte[] rsaBuffer = message.GetIndexedBuffer(128);
            if (rsaBuffer == null) {
                throw new System.Exception("Insufficient bytes in buffer to encrypt");
            }

            var encrpted = openTibiaEncryptEngine.ProcessBlock(rsaBuffer, 0, rsaBuffer.Length);
            message.ReplaceIndexedBuffer(encrpted);
        }
    }
}