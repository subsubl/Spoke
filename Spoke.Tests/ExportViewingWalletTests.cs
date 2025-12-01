using System;
using System.IO;
using System.Linq;
using IXICore;
using Xunit;

namespace Spoke.Tests;

public class ExportViewingWalletTests : IDisposable
{
    private readonly string _tempDir;

    public ExportViewingWalletTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SpokeExportTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    [Fact]
    public void WalletStorage_GeneratesViewingWalletBytes()
    {
        string walletPath = Path.Combine(_tempDir, "wallet.ixi");
        WalletStorage wallet = new WalletStorage(walletPath);
        bool generated = wallet.generateWallet("test-password-123");
        Assert.True(generated, "Wallet generation failed");

        string walletIdSegment = new string(wallet.getMyAddressesBase58().First().Take(8).ToArray());
        string expectedBackupPath = walletPath + "." + walletIdSegment + ".bak";
        wallet.backup();
        Assert.True(File.Exists(expectedBackupPath), "Wallet backup was not created");

        byte[] viewingBytes = wallet.getRawViewingWallet();
        Assert.NotNull(viewingBytes);
        Assert.True(viewingBytes.Length > 0, "Viewing wallet export should contain bytes");
    }
}
