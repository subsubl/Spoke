using IXICore.Meta;

namespace Spoke.Services;

/// <summary>
/// Service for centralized error handling and user feedback
/// </summary>
public class ErrorHandler
{
    /// <summary>
    /// Handle an exception with logging and optional user notification
    /// </summary>
    public static void HandleError(Exception ex, string context = "", bool showToUser = true)
    {
        // Log the error
        Logging.error($"Error in {context}: {ex.Message}");
        Logging.error($"Stack trace: {ex.StackTrace}");

        // Show user-friendly message if requested
        if (showToUser)
        {
            ShowUserError(ex, context);
        }
    }

    /// <summary>
    /// Handle an error message with logging and optional user notification
    /// </summary>
    public static void HandleError(string message, string context = "", bool showToUser = true)
    {
        // Log the error
        Logging.error($"Error in {context}: {message}");

        // Show user-friendly message if requested
        if (showToUser)
        {
            ShowUserError(message, context);
        }
    }

    /// <summary>
    /// Show a user-friendly error message
    /// </summary>
    private static void ShowUserError(Exception ex, string context)
    {
        var userMessage = GetUserFriendlyMessage(ex, context);
        ShowUserError(userMessage, context);
    }

    /// <summary>
    /// Show a user-friendly error message
    /// </summary>
    private static void ShowUserError(string message, string context)
    {
        // TODO: Implement user notification (dialog, toast, etc.)
        // For now, just log that we would show to user
        Logging.warn($"Would show user error: {message}");
    }

    /// <summary>
    /// Convert technical exceptions to user-friendly messages
    /// </summary>
    private static string GetUserFriendlyMessage(Exception ex, string context)
    {
        // Network-related errors
        if (ex.Message.Contains("connection") || ex.Message.Contains("network") || ex.Message.Contains("timeout"))
        {
            return "Connection problem. Please check your internet connection and try again.";
        }

        // Authentication errors
        if (ex.Message.Contains("auth") || ex.Message.Contains("credential") || ex.Message.Contains("login"))
        {
            return "Authentication failed. Please check your credentials and try again.";
        }

        // Storage errors
        if (ex.Message.Contains("storage") || ex.Message.Contains("file") || ex.Message.Contains("disk"))
        {
            return "Storage problem. Please check available space and permissions.";
        }

        // Generic fallback
        return $"An error occurred{(string.IsNullOrEmpty(context) ? "" : $" in {context}")}. Please try again or contact support if the problem persists.";
    }

    /// <summary>
    /// Handle async operation errors
    /// </summary>
    public static async Task<T?> HandleAsyncOperation<T>(Func<Task<T>> operation, string context = "", T? defaultValue = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            HandleError(ex, context);
            return defaultValue;
        }
    }

    /// <summary>
    /// Handle async operation errors with void return
    /// </summary>
    public static async Task HandleAsyncOperation(Func<Task> operation, string context = "")
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            HandleError(ex, context);
        }
    }
}