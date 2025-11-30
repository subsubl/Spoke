using Xunit;
using Spoke.Wallet;

namespace Spoke.Tests.UnitTests
{
    public class WalletManagerTests
    {
        [Fact]
        public void GetPrimaryKeys_DefaultsToNull_WhenNoIxianWallet()
        {
            // In a test environment without Ixian-Core backing, WalletAdapter should return nulls.
            var priv = WalletAdapter.GetPrimaryPrivateKey();
            var pub = WalletAdapter.GetPrimaryPublicKey();
            var addr = WalletAdapter.GetPrimaryAddress();

            Assert.Null(priv);
            Assert.Null(pub);
            Assert.Null(addr);
        }
    }
}
