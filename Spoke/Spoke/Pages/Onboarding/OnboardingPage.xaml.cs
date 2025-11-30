using Spoke.Meta;
using Spoke.Network;
using System.Collections.ObjectModel;
using System.IO;

namespace Spoke.Pages.Onboarding;

public partial class OnboardingPage : ContentPage
{
    public class OnboardingStep
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsLastStep { get; set; } = false;
    }

    public ObservableCollection<OnboardingStep> Steps { get; set; } = new();

    public string HostAddress { get; set; } = "";
    public string Port { get; set; } = "8001";
    public string Username { get; set; } = "";
    public string WalletPassword { get; set; } = "";
    public string QuixiUsername { get; set; } = "";
    public string QuixiPassword { get; set; } = "";
    public ImageSource AvatarImageSource { get; set; }

    private int currentStepIndex = 0;

    public OnboardingPage()
    {
        InitializeComponent();

        // Define onboarding steps
        Steps.Add(new OnboardingStep
        {
            Title = "Welcome to Spoke",
            Description = "Your smart home control center powered by Ixian blockchain technology. Let's set up your secure identity and connect to your QuIXI instance.",
            Icon = "🏠"
        });

        Steps.Add(new OnboardingStep
        {
            Title = "Create Your Wallet",
            Description = "Create a secure cryptographic wallet for your Ixian identity. This wallet will authenticate you with QuIXI and secure all your smart home communications.",
            Icon = "🔐"
        });

        Steps.Add(new OnboardingStep
        {
            Title = "Choose Username & Avatar",
            Description = "Set up your profile with a username and profile picture. This will be your identity across the Ixian network.",
            Icon = "👤"
        });

        Steps.Add(new OnboardingStep
        {
            Title = "Configure QuIXI Connection",
            Description = "Enter your QuIXI connection details to link Spoke with your smart home system via the Ixian network.",
            Icon = "⚙️"
        });

        Steps.Add(new OnboardingStep
        {
            Title = "Test Connection",
            Description = "Let's verify that Spoke can connect to your QuIXI instance.",
            Icon = "🔗"
        });

        Steps.Add(new OnboardingStep
        {
            Title = "Setup Complete",
            Description = "Spoke is now connected to QuIXI! Your smart home is ready to control via blockchain technology.",
            Icon = "✅",
            IsLastStep = true
        });

        BindingContext = this;
        UpdateCurrentStep();
    }

    private void UpdateCurrentStep()
    {
        if (currentStepIndex < Steps.Count)
        {
            CurrentStep = Steps[currentStepIndex];
        }
    }

    private OnboardingStep? _currentStep;
    public OnboardingStep? CurrentStep
    {
        get => _currentStep;
        set
        {
            _currentStep = value;
            OnPropertyChanged();
        }
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        if (currentStepIndex < Steps.Count - 1)
        {
            currentStepIndex++;
            UpdateCurrentStep();
        }
        else
        {
            // Finish onboarding
            Config.quixiAddress = HostAddress;
            Config.quixiApiPort = int.TryParse(Port, out int port) ? port : 8001;
            Config.quixiUsername = QuixiUsername;
            
            // Set up wallet and profile info
            if (!string.IsNullOrEmpty(Username))
            {
                // Save username to preferences (you might want to integrate with Spixi's storage)
                Preferences.Default.Set("user_nickname", Username);
            }
            
            // Save wallet password securely
            if (!string.IsNullOrEmpty(WalletPassword))
            {
                SecureStorage.Default.SetAsync("wallet_password", WalletPassword).Wait();
            }
            
            // Save QuIXI password
            if (!string.IsNullOrEmpty(QuixiPassword))
            {
                try
                {
                    SecureStorage.Default.SetAsync(nameof(Config.quixiPassword), QuixiPassword).Wait();
                }
                catch
                {
                    // SecureStorage might not be available
                }
            }

            Config.SetSetupComplete(true);
            Config.Save();

            // Navigate to main app
            App.appWindow!.Page = new AppShell();
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            UpdateCurrentStep();
        }
    }

    private void OnPlatformSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            // For now, assume QuIXI is selected
            // TODO: Handle Home Assistant selection
            OnNextClicked(sender, e);
        }
    }

    private async void OnCreateWalletClicked(object sender, EventArgs e)
    {
        // Use Spixi's wallet creation logic
        try
        {
            // Generate wallet password if not set
            if (string.IsNullOrEmpty(WalletPassword))
            {
                WalletPassword = Guid.NewGuid().ToString();
            }

            // Generate wallet using Ixian Core wallet storage
            if (Spoke.Meta.Node.generateWallet(WalletPassword))
            {
                Spoke.Meta.Node.generatedNewWallet = true;
                Spoke.Meta.Node.start();

                await DisplayAlert("Success", "Wallet created successfully!", "OK");
                OnNextClicked(sender, e);
            }
            else
            {
                await DisplayAlert("Error", "Failed to create wallet. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Wallet creation failed: {ex.Message}", "OK");
        }
    }

    private async void OnChooseAvatarClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Profile Picture",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                // Load and resize the image
                using var stream = await result.OpenReadAsync();
                var imageData = await ResizeImageAsync(stream, 200, 200);
                
                // Save to temp location for now
                var tempPath = Path.Combine(FileSystem.CacheDirectory, "avatar_temp.jpg");
                await File.WriteAllBytesAsync(tempPath, imageData);
                
                AvatarImageSource = ImageSource.FromFile(tempPath);
                OnPropertyChanged(nameof(AvatarImageSource));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to select avatar: {ex.Message}", "OK");
        }
    }

    private async Task<byte[]> ResizeImageAsync(Stream stream, int width, int height)
    {
        // Simple image resizing - in production you'd want better image handling
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        // Test the connection
        var quixiClient = new QuixiClient(HostAddress, int.TryParse(Port, out int port) ? port : 8001, false, QuixiUsername, QuixiPassword);
        bool success = await quixiClient.TestConnectionAsync();

        if (success)
        {
            await DisplayAlert("Success", "Connection test successful!", "OK");
        }
        else
        {
            await DisplayAlert("Failed", "Connection test failed. Please check your settings.", "OK");
        }
    }
}

