using System;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.Tests.UnitTests
{
    public class BridgeConnectionTests
    {
        [Fact]
        public void SignAndVerify_Works()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, publicKey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongData_Fails()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature("other", signature, publicKey);
                Assert.False(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongPublicKey_Fails()
        {
            using (var rsa = RSA.Create(2048))
            using (var rsa2 = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var otherPublicKey = rsa2.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, otherPublicKey);
                Assert.False(ok);
            }
        }
    }
}
using System;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.Tests.UnitTests
{
    public class BridgeConnectionTests
    {
        [Fact]
        public void SignAndVerify_Works()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, publicKey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongData_Fails()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature("other", signature, publicKey);
                Assert.False(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongPublicKey_Fails()
        {
            using (var rsa = RSA.Create(2048))
            using (var rsa2 = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var otherPublicKey = rsa2.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, otherPublicKey);
                Assert.False(ok);
            }
        }
    }
}
using System;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.Tests.UnitTests
{
    public class BridgeConnectionTests
    {
        [Fact]
        public void SignAndVerify_Works()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, publicKey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongData_Fails()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature("other", signature, publicKey);
                Assert.False(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongPublicKey_Fails()
        {
            using (var rsa = RSA.Create(2048))
            using (var rsa2 = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var otherPublicKey = rsa2.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, otherPublicKey);
                Assert.False(ok);
            }
        }
    }
}
using System;
using System.Security.Cryptography;
using Xunit;

namespace Spoke.Tests.UnitTests
{
    public class BridgeConnectionTests
    {
        [Fact]
        public void SignAndVerify_Works()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, publicKey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongData_Fails()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature("other", signature, publicKey);
                Assert.False(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongPublicKey_Fails()
        {
            using (var rsa = RSA.Create(2048))
            using (var rsa2 = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var otherPublicKey = rsa2.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, otherPublicKey);
                Assert.False(ok);
            }
        }
    }
}
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Spoke.Tests.UnitTests
{
    public class BridgeConnectionTests
    {
        [Fact]
        public void SignAndVerify_Works()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature(data, signature, publicKey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void VerifySignature_WrongData_Fails()
        {
            using (var rsa = RSA.Create(2048))
            {
                var publicKey = rsa.ExportSubjectPublicKeyInfo();
                var bc = new BridgeConnection("bridge", "our", publicKey);
                var data = "command:1";
                var signature = bc.SignData(data, rsa);
                Assert.False(string.IsNullOrEmpty(signature));

                var ok = bc.VerifySignature("other", signature, publicKey);
                Assert.False(ok);
            }
        }




















}    }        }            }                Assert.False(ok);                var ok = bc.VerifySignature(data, signature, otherPublicKey);                Assert.False(string.IsNullOrEmpty(signature));                var signature = bc.SignData(data, rsa);                var data = "command:1";                var bc = new BridgeConnection("bridge", "our", publicKey);                var otherPublicKey = rsa2.ExportSubjectPublicKeyInfo();                var publicKey = rsa.ExportSubjectPublicKeyInfo();            {            using (var rsa2 = RSA.Create(2048))            using (var rsa = RSA.Create(2048))        {        public void VerifySignature_WrongPublicKey_Fails()        [Fact]