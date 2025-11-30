using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Spoke
{
    public class WalletManager
    {
        private const string WalletFileName = "wallet.ixi";
        private const int KeySize = 2048;

        private static WalletManager _instance;
        public static WalletManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WalletManager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Spoke"));
                }
                return _instance;
            }
        }

        public string WalletDirectory { get; private set; }

        private byte[] _privateKey;
        public byte[] GetPrivateKey()
        {
            return _privateKey;
        }

        public WalletManager(string walletDirectory)
        {
            WalletDirectory = walletDirectory;
            if (!Directory.Exists(WalletDirectory))
            {
                Directory.CreateDirectory(WalletDirectory);
            }
        }

        public void GenerateNewWallet(string password)
        {
            // Generate RSA key pair
            using (var rsa = RSA.Create(KeySize))
            {
                var privateKey = rsa.ExportRSAPrivateKey();
                var publicKey = rsa.ExportRSAPublicKey();

                // Encrypt private key with password
                var encryptedPrivateKey = EncryptPrivateKey(privateKey, password);

                // Save wallet file
                SaveWalletFile(encryptedPrivateKey, publicKey);
            }
        }

        private byte[] EncryptPrivateKey(byte[] privateKey, string password)
        {
            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SpokeSalt"), 10000);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(privateKey, 0, privateKey.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private void SaveWalletFile(byte[] encryptedPrivateKey, byte[] publicKey)
        {
            var walletPath = Path.Combine(WalletDirectory, WalletFileName);
            using (var fs = new FileStream(walletPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(publicKey.Length);
                writer.Write(publicKey);
                writer.Write(encryptedPrivateKey.Length);
                writer.Write(encryptedPrivateKey);
            }
        }

        public bool LoadWallet(string password, out RSA rsa)
        {
            rsa = null;
            var walletPath = Path.Combine(WalletDirectory, WalletFileName);
            if (!File.Exists(walletPath))
            {
                return false;
            }

            using (var fs = new FileStream(walletPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                var publicKeyLength = reader.ReadInt32();
                var publicKey = reader.ReadBytes(publicKeyLength);
                var encryptedPrivateKeyLength = reader.ReadInt32();
                var encryptedPrivateKey = reader.ReadBytes(encryptedPrivateKeyLength);

                var privateKey = DecryptPrivateKey(encryptedPrivateKey, password);
                if (privateKey == null)
                {
                    return false;
                }

                _privateKey = privateKey; // Store the decrypted private key

                rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKey, out _);
                return true;
            }
        }

        private byte[] DecryptPrivateKey(byte[] encryptedPrivateKey, string password)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SpokeSalt"), 10000);
                    aes.Key = key.GetBytes(32);
                    aes.IV = key.GetBytes(16);

                    using (var ms = new MemoryStream(encryptedPrivateKey))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var output = new MemoryStream())
                    {
                        cs.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public void BackupWallet(string backupPath)
        {
            var walletPath = Path.Combine(WalletDirectory, WalletFileName);
            if (!File.Exists(walletPath))
            {
                throw new FileNotFoundException("Wallet file not found.");
            }

            File.Copy(walletPath, backupPath, overwrite: true);
        }

        public bool RestoreWallet(string backupPath, string password, out RSA rsa)
        {
            rsa = null;
            if (!File.Exists(backupPath))
            {
                return false;
            }

            var walletPath = Path.Combine(WalletDirectory, WalletFileName);
            File.Copy(backupPath, walletPath, overwrite: true);

            return LoadWallet(password, out rsa);
        }
    }
}