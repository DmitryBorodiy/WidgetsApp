using System.IO;
using Windows.Storage;

namespace BetterWidgets.Helpers
{
    public class StorageFileHelper
    {
        public static async Task<(StorageFile file, Exception ex)> GetStorageFileFromPathAsync(string filePath)
        {
            try
            {
                if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if(!File.Exists(filePath)) throw new FileNotFoundException();

                var file = await StorageFile.GetFileFromPathAsync(filePath);

                return (file, null);
            }
            catch(Exception ex)
            {
                return (null, ex);
            }
        } 
    }
}
