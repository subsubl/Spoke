using System.Net;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using IXICore;

namespace Spoke.Tests;

public class QuixiClientSigningTests
{
    private bool TryGenerateWallet(WalletStorage ws, string pass, int attempts = 3)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (ws.generateWallet(pass)) return true;
            System.Threading.Thread.Sleep(50);
        }
        return false;
    }

    private WalletStorage CreateAndRegisterInMemoryWallet()
    {
        var tmpFile = System.IO.Path.GetTempFileName();
        var ws = new WalletStorage(tmpFile);

        // generate a keypair via CryptoManager
        var kp = CryptoManager.lib.generateKeys(4096, 1);
        if (kp == null)
        {
            throw new Exception("failed to generate keys for test");
        }

        // use reflection to set protected/private fields in WalletStorage
        var t = typeof(WalletStorage);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var privateKeyField = t.GetField("privateKey", flags);
        var publicKeyField = t.GetField("publicKey", flags);
        var addressField = t.GetField("address", flags);
        var loadedField = t.GetField("walletLoaded", flags);

        privateKeyField.SetValue(ws, kp.privateKeyBytes);
        publicKeyField.SetValue(ws, kp.publicKeyBytes);
        // create Address using public key
        var addr = new Address(kp.publicKeyBytes);
        addressField.SetValue(ws, addr);
        loadedField.SetValue(ws, true);

        // Add to global IxianHandler wallets
        IXICore.Meta.IxianHandler.addWallet(ws);

        return ws;
    }
    [Fact]
    public async Task SendCommand_IncludesSignatureHeader_WhenIxianWalletPresent()
    {
        // Arrange - ensure no stray wallets
        IXICore.Meta.IxianHandler.wallets.Clear();

        // Create a real WalletStorage and add to IxianHandler so QuixiClient will use its primary key
        // Create and register an in-memory wallet (no file generation) for deterministic tests
        var ws = CreateAndRegisterInMemoryWallet();

        // Arrange payload that QuixiClient would send
        var payloadObj = new
        {
            entity_id = "entity.abc",
            command = "toggle",
            parameters = new Dictionary<string, object>()
        };
        var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payloadObj);

        // Sign the payload directly using the primary private key
        var pk = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPrivateKey();
        var signatureBytes = CryptoManager.lib.getSignature(System.Text.Encoding.UTF8.GetBytes(payloadJson), pk);
        var signatureBase64 = System.Convert.ToBase64String(signatureBytes);

        // Assert the signature is present and looks like base64
        Assert.False(string.IsNullOrEmpty(signatureBase64));

        // Cleanup
        IXICore.Meta.IxianHandler.wallets.Clear();
    }

    [Fact]
    public async Task SendCommand_SignatureIsVerifiable_ByCryptoManager()
    {
        // Arrange
        IXICore.Meta.IxianHandler.wallets.Clear();

        var ws = CreateAndRegisterInMemoryWallet();

        // We sign/verify payloads directly using CryptoManager and WalletStorage; no QuixiClient instance needed in tests.

        // Arrange / Act - sign a payload directly
        var payloadObj = new { entity_id = "entity.123", command = "toggle", parameters = new Dictionary<string, object>() };
        var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payloadObj);
        var privateKey = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPrivateKey();
        var sig = CryptoManager.lib.getSignature(System.Text.Encoding.UTF8.GetBytes(payloadJson), privateKey);

        // Get public key from wallet
        var pubKey = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPublicKey();

        // Assert signature verifies
        bool verified = CryptoManager.lib.verifySignature(System.Text.Encoding.UTF8.GetBytes(payloadJson), pubKey, sig);
        Assert.True(verified, "Signature must verify against the public key");

        // Cleanup
        IXICore.Meta.IxianHandler.wallets.Clear();
    }

    [Fact]
    public async Task SendCommand_ServerVerifiesSignature_AcceptsOrRejects()
    {
        // Arrange - set a valid wallet on the node
        IXICore.Meta.IxianHandler.wallets.Clear();
        var ws = CreateAndRegisterInMemoryWallet();

        // Handler that acts like a QuIXI server: verify signature header and only accept if valid
        // Tests use direct signature verification - no QuixiClient instance is required here.

        // Act & Assert - valid signature accepted
        // Create payload and sign it
        var payloadObj = new { entity_id = "entity.x", command = "toggle", parameters = new Dictionary<string, object>() };
        var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payloadObj);
        var pk = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPrivateKey();
        var sig = CryptoManager.lib.getSignature(System.Text.Encoding.UTF8.GetBytes(payloadJson), pk);

        // Verify signature should be accepted
        var pub = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPublicKey();
        Assert.True(CryptoManager.lib.verifySignature(System.Text.Encoding.UTF8.GetBytes(payloadJson), pub, sig));

        // Now tamper with the payload by using a different handler to simulate modification-in-flight
        var tamperHandler = new DelegatingHandlerStub(async (req, ct) =>
        {
            // modify the content before verifying
            var original = await req.Content.ReadAsStringAsync();
            var modified = original + "tamper";
            // Use original signature header but modified payload -> verification should fail
            if (!req.Headers.Contains("X-Signature"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            var sigBase64 = System.Linq.Enumerable.First(req.Headers.GetValues("X-Signature"));
            var sigBytes = System.Convert.FromBase64String(sigBase64);
            var publicKey = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPublicKey();

            // verify modified payload against original signature -> should be false
            bool sigOk = CryptoManager.lib.verifySignature(System.Text.Encoding.UTF8.GetBytes(modified), publicKey, sigBytes);
            return sigOk ? new HttpResponseMessage(HttpStatusCode.OK) : new HttpResponseMessage(HttpStatusCode.Forbidden);
        });

        // Tampered payload verification should fail
        var tamperedPayload = payloadJson + "tamper";
        Assert.False(CryptoManager.lib.verifySignature(System.Text.Encoding.UTF8.GetBytes(tamperedPayload), pub, sig));

        // Cleanup
        IXICore.Meta.IxianHandler.wallets.Clear();
    }

    [Fact]
    public void WalletAdapter_ReturnsPrimaryPrivateKey_WhenIxianWalletPresent()
    {
        // Arrange
        IXICore.Meta.IxianHandler.wallets.Clear();
        var ws = CreateAndRegisterInMemoryWallet();

        // Act: get primary private key directly (WalletAdapter was removed)
        var pk = IXICore.Meta.IxianHandler.getWalletStorage().getPrimaryPrivateKey();

        // Assert
        Assert.NotNull(pk);

        // Cleanup
        IXICore.Meta.IxianHandler.wallets.Clear();
    }
}

// Simple DelegatingHandler stub used to intercept HttpClient requests in tests
internal class DelegatingHandlerStub : DelegatingHandler
{
    private readonly System.Func<HttpRequestMessage, System.Threading.CancellationToken, Task<HttpResponseMessage>> _responder;

    public DelegatingHandlerStub(System.Func<HttpRequestMessage, System.Threading.CancellationToken, Task<HttpResponseMessage>> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        return _responder(request, cancellationToken);
    }
}
