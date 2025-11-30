using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.UnitTestsClean.UnitTests
{
    public class WalletManagerTests : IDisposable
    {
        private readonly string _tempDir;

        public WalletManagerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "SpokeTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
            }
            catch { }
        }

        [Fact]
        public void GenerateAndLoadWallet_Success()
        {
            var manager = new WalletManager(_tempDir);
            var password = "test-password";
            manager.GenerateNewWallet(password);

            var loaded = manager.LoadWallet(password, out RSA rsa);
            Assert.True(loaded);
            Assert.NotNull(rsa);
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.UnitTestsClean.UnitTests
{
    public class WalletManagerTests : IDisposable
    {
        private readonly string _tempDir;



























































}    }        }            Assert.NotNull(rsa);            Assert.True(restoreResult);
            var restoreResult = manager.RestoreWallet(backupPath, password, out RSA rsa);            if (File.Exists(walletPath)) File.Delete(walletPath);
n            var walletPath = Path.Combine(_tempDir, "wallet.ixi");            manager.BackupWallet(backupPath);
n            var backupPath = Path.Combine(_tempDir, "backup.ixi");            manager.GenerateNewWallet(password);            var password = "backup-pass";            var manager = new WalletManager(_tempDir);        {        public void BackupAndRestoreWallet_Success()
n        [Fact]        }            Assert.Null(rsa);            Assert.False(loaded);
n            var loaded = manager.LoadWallet("wrong-password", out RSA rsa);            manager.GenerateNewWallet(password);            var password = "correct-password";            var manager = new WalletManager(_tempDir);        {        public void LoadWallet_WrongPassword_Fails()
n        [Fact]        }            }                Assert.True(ok);                var ok = verifyRsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);                verifyRsa.ImportRSAPublicKey(publicKey, out _);            {            using (var verifyRsa = RSA.Create())
n            var publicKey = rsa.ExportRSAPublicKey();            var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
n            var data = System.Text.Encoding.UTF8.GetBytes("hello world");            Assert.NotNull(rsa);            Assert.True(loaded);
n            var loaded = manager.LoadWallet(password, out RSA rsa);            manager.GenerateNewWallet(password);            var password = "test-password";            var manager = new WalletManager(_tempDir);        {        public void GenerateAndLoadWallet_Success()
n        [Fact]        }            catch { }            }                if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);            {            try        {
n        public void Dispose()        }            Directory.CreateDirectory(_tempDir);            _tempDir = Path.Combine(Path.GetTempPath(), "SpokeTests", Guid.NewGuid().ToString());        {n        public WalletManagerTests()