using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Alerts;

namespace Spoke.Platforms
{
    public static class SFileOperations
    {
        public static async Task<bool> share(string filepath, string title)
        {
            if (!File.Exists(filepath))
            {
                return false;
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            try
            {
                string fileName = Path.GetFileName(filepath);
                using FileStream fileStream = File.OpenRead(filepath);
                var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, fileStream, cancellationTokenSource.Token);
                if (!fileSaverResult.IsSuccessful)
                {
                    await Toast.Make($"The file was not saved. Error: {fileSaverResult.Exception?.Message}").Show(cancellationTokenSource.Token);
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
