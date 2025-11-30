using System;
using System.Security.Cryptography;
using System.Text;

namespace Spoke
{
    public class BridgeConnection
    {
        public string BridgeAddress { get; set; }
        public string OurAddress { get; set; }
        public byte[] OurPublicKey { get; set; }
        public DateTime ConnectedAt { get; set; }

        public BridgeConnection(string bridgeAddress, string ourAddress, byte[] ourPublicKey)
        {
            BridgeAddress = bridgeAddress;
            OurAddress = ourAddress;
            OurPublicKey = ourPublicKey;
            ConnectedAt = DateTime.UtcNow;
        }

        public string SignData(string data, RSA privateKey)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        public bool VerifySignature(string data, string signature, byte[] publicKey)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = Convert.FromBase64String(signature);
                return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}