using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys
{
    public class VapidKeyGenerator
    {
        public static VapidDetails GenerateVapidKeys()
        {
            var results = new VapidDetails();

            var keys = GenerateKeys();
            var publicKey = ((ECPublicKeyParameters)keys.Public).Q.GetEncoded(false);
            var privateKey = ((ECPrivateKeyParameters)keys.Private).D.ToByteArrayUnsigned();

            results.PublicKey = System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(publicKey));
            results.PrivateKey = System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(ByteArrayPadLeft(privateKey, 32)));

            results.PublicKey = results.PublicKey.TrimEnd('.');
            results.PrivateKey = results.PrivateKey.TrimEnd('.');

            return results;
        }

        private static AsymmetricCipherKeyPair GenerateKeys()
        {
            var ecParameters = NistNamedCurves.GetByName("P-256");
            var ecSpec = new ECDomainParameters(ecParameters.Curve, ecParameters.G, ecParameters.N, ecParameters.H,
                ecParameters.GetSeed());
            var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("ECDH");
            keyPairGenerator.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));

            return keyPairGenerator.GenerateKeyPair();
        }

        private static byte[] ByteArrayPadLeft(byte[] src, int size)
        {
            var dst = new byte[size];
            var startAt = dst.Length - src.Length;
            Array.Copy(src, 0, dst, startAt, src.Length);
            return dst;
        }
    }
}
