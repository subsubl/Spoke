using System;
using System.IO;
using System.Threading.Tasks;
using IXICore;
using Spoke.Data.Models;
using Spoke.Services;

namespace Spoke.Data.Services;

/// <summary>
/// Service for managing Ixian blockchain wallets
/// </summary>
public class WalletService
{
    private readonly ISecureStorage _secureStorage;
    private IXICore.WalletStorage? _walletStorage;
    private Data.Models.Wallet? _currentWallet;

    /// <summary>
    /// Event raised when wallet state changes
    /// </summary>
    public event EventHandler<WalletStateChangedEventArgs>? WalletStateChanged;

    /// <summary>
    /// Gets whether a wallet is currently loaded
    /// </summary>
    public bool IsWalletLoaded => _walletStorage?.isLoaded() ?? false;

    /// <summary>
    /// Gets the current wallet information
    /// </summary>
    public Data.Models.Wallet? CurrentWallet => _currentWallet;

    /// <summary>
    /// Gets the wallet address
    /// </summary>
    public string? WalletAddress => _currentWallet?.Address;

    /// <summary>
    /// Gets the public key
    /// </summary>
    public byte[]? PublicKey => _currentWallet?.PublicKey;

    public WalletService(ISecureStorage secureStorage)
    {
        _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
    }

    /// <summary>
    /// Generates a new wallet with the specified password
    /// </summary>
    /// <param name="password">Password to encrypt the wallet</param>
    /// <returns>True if wallet was generated successfully</returns>
    public async Task<bool> GenerateWalletAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorHandler.HandleError("Wallet password cannot be empty", "Please provide a valid password for your wallet.");
                return false;
            }

            if (password.Length < 8)
            {
                ErrorHandler.HandleError("Wallet password too short", "Password must be at least 8 characters long for security.");
                return false;
            }

            // Check if wallet already exists
            if (await CheckWalletExistsAsync())
            {
                ErrorHandler.HandleError("Wallet already exists", "A wallet already exists. Use a different password or delete the existing wallet first.");
                return false;
            }

            // Generate wallet using Ixian-Core
            var walletPath = GetWalletFilePath();
            EnsureWalletDirectoryExists(walletPath);

            _walletStorage = new WalletStorage(walletPath);

            if (!_walletStorage.generateWallet(password))
            {
                ErrorHandler.HandleError("Wallet generation failed", "Failed to generate cryptographic keys for the wallet.");
                return false;
            }

            // Create wallet model from generated wallet
            _currentWallet = new Data.Models.Wallet
            {
                PublicKey = _walletStorage.getPrimaryPublicKey(),
                EncryptedPrivateKey = null, // Private key is stored encrypted in the wallet file
                Address = _walletStorage.getPrimaryAddress()?.ToString() ?? string.Empty,
                Username = null,
                ProfileImage = null,
                Created = DateTime.UtcNow
            };

            // Store wallet password securely
            await StoreWalletPasswordAsync(password);

            // Raise wallet state changed event
            OnWalletStateChanged(WalletState.Created, _currentWallet);

            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleError("Wallet generation error", $"An unexpected error occurred while generating the wallet: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads an existing wallet with the specified password
    /// </summary>
    /// <param name="password">Password to decrypt the wallet</param>
    /// <returns>True if wallet was loaded successfully</returns>
    public async Task<bool> LoadWalletAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorHandler.HandleError("Wallet password required", "Please provide the password for your wallet.");
                return false;
            }

            var walletPath = GetWalletFilePath();
            if (!File.Exists(walletPath))
            {
                ErrorHandler.HandleError("Wallet not found", "No wallet file exists. Please create a new wallet first.");
                return false;
            }

            _walletStorage = new WalletStorage(walletPath);

            if (!_walletStorage.readWallet(password))
            {
                ErrorHandler.HandleError("Invalid password", "The provided password is incorrect or the wallet file is corrupted.");
                return false;
            }

            // Create wallet model from loaded wallet
            _currentWallet = new Data.Models.Wallet
            {
                PublicKey = _walletStorage.getPrimaryPublicKey(),
                EncryptedPrivateKey = null, // Private key remains encrypted in wallet file
                Address = _walletStorage.getPrimaryAddress()?.ToString() ?? string.Empty,
                Username = null,
                ProfileImage = null,
                Created = DateTime.UtcNow // TODO: Load actual creation date from wallet metadata
            };

            // Store wallet password securely for future operations
            await StoreWalletPasswordAsync(password);

            // Raise wallet state changed event
            OnWalletStateChanged(WalletState.Loaded, _currentWallet);

            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleError("Wallet loading error", $"An unexpected error occurred while loading the wallet: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Unloads the current wallet
    /// </summary>
    public void UnloadWallet()
    {
        _walletStorage = null;
        _currentWallet = null;
        OnWalletStateChanged(WalletState.Unloaded, null);
    }

    /// <summary>
    /// Checks if a wallet file exists
    /// </summary>
    /// <returns>True if wallet file exists</returns>
    public async Task<bool> CheckWalletExistsAsync()
    {
        var walletPath = GetWalletFilePath();
        return File.Exists(walletPath);
    }

    /// <summary>
    /// Validates a wallet password without loading the wallet
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if password is valid</returns>
    public async Task<bool> ValidateWalletPasswordAsync(string password)
    {
        try
        {
            var walletPath = GetWalletFilePath();
            if (!File.Exists(walletPath))
            {
                return false;
            }

            var tempWallet = new WalletStorage(walletPath);
            return tempWallet.isValidPassword(password);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the wallet file path
    /// </summary>
    private string GetWalletFilePath()
    {
        // Use the same path as the existing Spoke implementation
        var userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spoke");
        return Path.Combine(userFolder, "wallet.ixi");
    }

    /// <summary>
    /// Ensures the wallet directory exists
    /// </summary>
    private void EnsureWalletDirectoryExists(string walletPath)
    {
        var directory = Path.GetDirectoryName(walletPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Stores the wallet password securely
    /// </summary>
    private async Task StoreWalletPasswordAsync(string password)
    {
        const string passwordKey = "wallet_password";
        await _secureStorage.SetAsync(passwordKey, password);
    }

    /// <summary>
    /// Retrieves the stored wallet password
    /// </summary>
    private async Task<string?> GetStoredWalletPasswordAsync()
    {
        const string passwordKey = "wallet_password";
        return await _secureStorage.GetAsync(passwordKey);
    }

    /// <summary>
    /// Raises the wallet state changed event
    /// </summary>
    private void OnWalletStateChanged(WalletState state, Data.Models.Wallet? wallet)
    {
        WalletStateChanged?.Invoke(this, new WalletStateChangedEventArgs(state, wallet));
    }
}

/// <summary>
/// Wallet state enumeration
/// </summary>
public enum WalletState
{
    Created,
    Loaded,
    Unloaded
}

/// <summary>
/// Event arguments for wallet state changes
/// </summary>
public class WalletStateChangedEventArgs : EventArgs
{
    public WalletState State { get; }
    public Data.Models.Wallet? Wallet { get; }

    public WalletStateChangedEventArgs(WalletState state, Data.Models.Wallet? wallet)
    {
        State = state;
        Wallet = wallet;
    }
}