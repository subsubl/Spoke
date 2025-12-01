using System;
using IXICore.Meta;

namespace Spoke.Wallet
{
    /// <summary>
    /// Thin adapter that exposes primary wallet access via Ixian-Core (IxianHandler). 
    /// This project no longer includes the legacy WalletManager; callers should use IxianHandler.
    /// Use this class as the single place to migrate callers to Ixian-Core.
    /// </summary>
    public static class WalletAdapter
    {
        public static byte[]? GetPrimaryPrivateKey()
        {
            try
            {
                if (IxianHandler.getWalletList().Count > 0)
                {
                    return IxianHandler.getWalletStorage().getPrimaryPrivateKey();
                }
            }
            catch { }

            // If no Ixian-Core wallet is present, return null (no legacy fallback).
            return null;
        }

        public static byte[]? GetPrimaryPublicKey()
        {
            if (IxianHandler.getWalletList().Count > 0)
            {
                return IxianHandler.getWalletStorage().getPrimaryPublicKey();
            }
            return null;
        }

        public static IXICore.Address? GetPrimaryAddress()
        {
            if (IxianHandler.getWalletList().Count > 0)
            {
                return IxianHandler.getWalletStorage().getPrimaryAddress();
            }
            return null;
        }

            /// <summary>
            /// Export a view-only wallet using the Node helper.
            /// Returns the exported file path on success, null on failure.
            /// </summary>
            public static string? ExportViewingWallet(string? destPath = null)
            {
                try
                {
                    return Spoke.Meta.Node.ExportViewingWallet(destPath);
                }
                catch
                {
                    return null;
                }
            }
    }
}
