using System;
using System.IO;
using IXICore;
using IXICore.Meta;
using Spoke.Meta;
using Xunit;

namespace Spoke.UnitTestsClean.UnitTests
{
    public class ExportViewingWalletTests : IDisposable
    {
        private readonly string _tempDir;

        public ExportViewingWalletTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "SpokeExportTest", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            // Ensure Spoke uses our temp folder for wallet storage
            Config.spokeUserFolder = _tempDir;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
            }
            catch { }
        }

        [Fact]
        public void ExportViewingWallet_CreatesViewFile()
        {
            string walletPath = Path.Combine(_tempDir, Node.walletFile);

            // Create a real WalletStorage and add to IxianHandler
            WalletStorage ws = new WalletStorage(walletPath);
            bool gen = ws.generateWallet("test-password-123");
            Assert.True(gen, "Failed to generate wallet for test");

            bool added = IXICore.Meta.IxianHandler.addWallet(ws);
            Assert.True(added, "Failed to add wallet to IxianHandler");

            string? exported = Node.ExportViewingWallet();
            Assert.False(string.IsNullOrEmpty(exported));
            Assert.True(File.Exists(exported));
            var bytes = File.ReadAllBytes(exported);
            Assert.True(bytes.Length > 0, "Exported view wallet should contain bytes");
        }
    }
}
