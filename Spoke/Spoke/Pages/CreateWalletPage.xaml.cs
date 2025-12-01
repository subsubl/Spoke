using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Spoke.Data.Services;

namespace Spoke.Pages;

public partial class CreateWalletPage : ContentPage, INotifyPropertyChanged
{
    private readonly WalletService _walletService;

    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isCreatingWallet = false;
    private string _statusMessage = string.Empty;
    private bool _showStatusMessage = false;
    private Color _statusMessageColor = Colors.Red;

    public CreateWalletPage(WalletService walletService)
    {
        InitializeComponent();
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        BindingContext = this;

        // Subscribe to wallet state changes
        _walletService.WalletStateChanged += OnWalletStateChanged;
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                OnPropertyChanged(nameof(CanCreateWallet));
                OnPropertyChanged(nameof(PasswordStrength));
                OnPropertyChanged(nameof(PasswordStrengthText));
                OnPropertyChanged(nameof(PasswordStrengthColor));
                OnPropertyChanged(nameof(ShowPasswordStrength));
                OnPropertyChanged(nameof(PasswordLengthValid));
                OnPropertyChanged(nameof(PasswordCaseValid));
                OnPropertyChanged(nameof(PasswordNumberValid));
                OnPropertyChanged(nameof(PasswordSpecialValid));
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                OnPropertyChanged(nameof(CanCreateWallet));
            }
        }
    }

    public bool IsCreatingWallet
    {
        get => _isCreatingWallet;
        set => SetProperty(ref _isCreatingWallet, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool ShowStatusMessage
    {
        get => _showStatusMessage;
        set => SetProperty(ref _showStatusMessage, value);
    }

    public Color StatusMessageColor
    {
        get => _statusMessageColor;
        set => SetProperty(ref _statusMessageColor, value);
    }

    public bool CanCreateWallet =>
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        Password == ConfirmPassword &&
        Password.Length >= 8 &&
        !IsCreatingWallet;

    public double PasswordStrength
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return 0;

            double strength = 0;
            if (Password.Length >= 8) strength += 0.25;
            if (Password.Any(char.IsUpper)) strength += 0.25;
            if (Password.Any(char.IsLower)) strength += 0.25;
            if (Password.Any(char.IsDigit)) strength += 0.25;
            if (Password.Any(ch => !char.IsLetterOrDigit(ch))) strength += 0.25;

            return Math.Min(strength, 1.0);
        }
    }

    public string PasswordStrengthText
    {
        get
        {
            var strength = PasswordStrength;
            if (strength < 0.3) return "Weak";
            if (strength < 0.6) return "Fair";
            if (strength < 0.8) return "Good";
            return "Strong";
        }
    }

    public Color PasswordStrengthColor
    {
        get
        {
            var strength = PasswordStrength;
            if (strength < 0.3) return Colors.Red;
            if (strength < 0.6) return Colors.Orange;
            if (strength < 0.8) return Colors.Yellow;
            return Colors.Green;
        }
    }

    public bool ShowPasswordStrength => !string.IsNullOrEmpty(Password);

    public bool PasswordLengthValid => Password.Length >= 8;
    public bool PasswordCaseValid => Password.Any(char.IsUpper) && Password.Any(char.IsLower);
    public bool PasswordNumberValid => Password.Any(char.IsDigit);
    public bool PasswordSpecialValid => Password.Any(ch => !char.IsLetterOrDigit(ch));

    public ICommand CreateWalletCommand => new Command(async () => await CreateWalletAsync());
    public ICommand CancelCommand => new Command(async () => await CancelAsync());

    private async Task CreateWalletAsync()
    {
        if (!CanCreateWallet) return;

        try
        {
            IsCreatingWallet = true;
            ShowStatusMessage = false;

            StatusMessage = "Generating cryptographic keys... This may take a moment.";
            StatusMessageColor = Colors.Blue;
            ShowStatusMessage = true;

            var success = await _walletService.GenerateWalletAsync(Password);

            if (success)
            {
                StatusMessage = "Wallet created successfully!";
                StatusMessageColor = Colors.Green;

                // Wait a moment to show success message
                await Task.Delay(1500);

                // Navigate back or to next step
                await NavigateAfterWalletCreationAsync();
            }
            else
            {
                StatusMessage = "Failed to create wallet. Please try again.";
                StatusMessageColor = Colors.Red;
                ShowStatusMessage = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"An error occurred: {ex.Message}";
            StatusMessageColor = Colors.Red;
            ShowStatusMessage = true;
        }
        finally
        {
            IsCreatingWallet = false;
        }
    }

    private async Task CancelAsync()
    {
        // Navigate back
        await Navigation.PopAsync();
    }

    private async Task NavigateAfterWalletCreationAsync()
    {
        // Check if we're in onboarding flow or standalone
        if (Navigation.NavigationStack.Count > 1)
        {
            // Pop this page to return to previous page
            await Navigation.PopAsync();
        }
        else
        {
            // If this is the root page, navigate to main app
            // This would typically go to the main app shell or home page
            await Shell.Current.GoToAsync("//home");
        }
    }

    private void OnWalletStateChanged(object? sender, WalletStateChangedEventArgs e)
    {
        // Handle wallet state changes if needed
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.State == WalletState.Created)
            {
                // Wallet was created successfully
                // Additional UI updates can be handled here
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe from events
        if (_walletService != null)
        {
            _walletService.WalletStateChanged -= OnWalletStateChanged;
        }
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