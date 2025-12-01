using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Spoke.Data.Models;
using Spoke.Data.Services;

namespace Spoke.ViewModels;

/// <summary>
/// View model for wallet management and display
/// </summary>
public class WalletViewModel : INotifyPropertyChanged
{
    private readonly WalletService _walletService;
    private readonly ErrorHandler _errorHandler;

    private Wallet? _currentWallet;
    private bool _isWalletLoaded;
    private string _walletAddress = string.Empty;
    private string _publicKeyDisplay = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _showStatusMessage;
    private Color _statusMessageColor = Colors.Red;
    private bool _isLoading;

    public WalletViewModel(WalletService walletService, ErrorHandler errorHandler)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

        // Subscribe to wallet state changes
        _walletService.WalletStateChanged += OnWalletStateChanged;

        // Initialize commands
        LoadWalletCommand = new Command(async () => await LoadWalletAsync());
        UnloadWalletCommand = new Command(UnloadWallet);
        CopyAddressCommand = new Command(async () => await CopyAddressToClipboardAsync());
        CopyPublicKeyCommand = new Command(async () => await CopyPublicKeyToClipboardAsync());

        // Initialize state
        UpdateWalletState();
    }

    /// <summary>
    /// Gets or sets the current wallet
    /// </summary>
    public Wallet? CurrentWallet
    {
        get => _currentWallet;
        set => SetProperty(ref _currentWallet, value);
    }

    /// <summary>
    /// Gets whether a wallet is currently loaded
    /// </summary>
    public bool IsWalletLoaded
    {
        get => _isWalletLoaded;
        set => SetProperty(ref _isWalletLoaded, value);
    }

    /// <summary>
    /// Gets the wallet address for display
    /// </summary>
    public string WalletAddress
    {
        get => _walletAddress;
        set => SetProperty(ref _walletAddress, value);
    }

    /// <summary>
    /// Gets the public key for display (truncated)
    /// </summary>
    public string PublicKeyDisplay
    {
        get => _publicKeyDisplay;
        set => SetProperty(ref _publicKeyDisplay, value);
    }

    /// <summary>
    /// Gets the status message
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Gets whether to show the status message
    /// </summary>
    public bool ShowStatusMessage
    {
        get => _showStatusMessage;
        set => SetProperty(ref _showStatusMessage, value);
    }

    /// <summary>
    /// Gets the status message color
    /// </summary>
    public Color StatusMessageColor
    {
        get => _statusMessageColor;
        set => SetProperty(ref _statusMessageColor, value);
    }

    /// <summary>
    /// Gets whether the view model is loading
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Command to load the wallet
    /// </summary>
    public ICommand LoadWalletCommand { get; }

    /// <summary>
    /// Command to unload the wallet
    /// </summary>
    public ICommand UnloadWalletCommand { get; }

    /// <summary>
    /// Command to copy wallet address to clipboard
    /// </summary>
    public ICommand CopyAddressCommand { get; }

    /// <summary>
    /// Command to copy public key to clipboard
    /// </summary>
    public ICommand CopyPublicKeyCommand { get; }

    /// <summary>
    /// Loads the wallet with stored password
    /// </summary>
    private async Task LoadWalletAsync()
    {
        try
        {
            IsLoading = true;
            ShowStatusMessage = false;

            // Check if wallet exists
            var walletExists = await _walletService.CheckWalletExistsAsync();
            if (!walletExists)
            {
                ShowError("No wallet found. Please create a wallet first.");
                return;
            }

            // Try to load with stored password
            var success = await _walletService.LoadWalletAsync(null); // null means use stored password

            if (success)
            {
                ShowSuccess("Wallet loaded successfully");
            }
            else
            {
                ShowError("Failed to load wallet. The stored password may be incorrect.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error loading wallet: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Unloads the current wallet
    /// </summary>
    private void UnloadWallet()
    {
        _walletService.UnloadWallet();
        ShowSuccess("Wallet unloaded");
    }

    /// <summary>
    /// Copies the wallet address to clipboard
    /// </summary>
    private async Task CopyAddressToClipboardAsync()
    {
        if (!string.IsNullOrEmpty(WalletAddress))
        {
            await Clipboard.SetTextAsync(WalletAddress);
            ShowSuccess("Address copied to clipboard");
        }
    }

    /// <summary>
    /// Copies the public key to clipboard
    /// </summary>
    private async Task CopyPublicKeyToClipboardAsync()
    {
        if (_walletService.PublicKey != null)
        {
            var publicKeyHex = Convert.ToHexString(_walletService.PublicKey);
            await Clipboard.SetTextAsync(publicKeyHex);
            ShowSuccess("Public key copied to clipboard");
        }
    }

    /// <summary>
    /// Updates the wallet state properties
    /// </summary>
    private void UpdateWalletState()
    {
        CurrentWallet = _walletService.CurrentWallet;
        IsWalletLoaded = _walletService.IsWalletLoaded;

        if (IsWalletLoaded && CurrentWallet != null)
        {
            WalletAddress = CurrentWallet.Address ?? string.Empty;
            PublicKeyDisplay = CurrentWallet.PublicKey != null
                ? $"{Convert.ToHexString(CurrentWallet.PublicKey).Substring(0, 16)}..."
                : "Not available";
        }
        else
        {
            WalletAddress = "No wallet loaded";
            PublicKeyDisplay = "No wallet loaded";
        }
    }

    /// <summary>
    /// Shows a success message
    /// </summary>
    private void ShowSuccess(string message)
    {
        StatusMessage = message;
        StatusMessageColor = Colors.Green;
        ShowStatusMessage = true;

        // Auto-hide after 3 seconds
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            MainThread.BeginInvokeOnMainThread(() => ShowStatusMessage = false);
        });
    }

    /// <summary>
    /// Shows an error message
    /// </summary>
    private void ShowError(string message)
    {
        StatusMessage = message;
        StatusMessageColor = Colors.Red;
        ShowStatusMessage = true;
    }

    /// <summary>
    /// Handles wallet state changes
    /// </summary>
    private void OnWalletStateChanged(object? sender, WalletStateChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateWalletState();

            switch (e.State)
            {
                case WalletState.Created:
                    ShowSuccess("Wallet created successfully");
                    break;
                case WalletState.Loaded:
                    ShowSuccess("Wallet loaded successfully");
                    break;
                case WalletState.Unloaded:
                    ShowSuccess("Wallet unloaded");
                    break;
            }
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}