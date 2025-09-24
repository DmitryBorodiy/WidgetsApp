using Windows.Storage;
using BetterWidgets.Services;
using System.Reflection;
using Newtonsoft.Json;

namespace BetterWidgets.Tests.Services
{
    public class DataService : IDataService
    {
        private string GetAppDataFolder()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetExecutingAssembly().GetName().Name);

            if(!Path.Exists(folder)) Directory.CreateDirectory(folder);

            return folder;
        }

        public Task<StorageFile> CreateFileAsync(string fileName, string subfolderName = null, CreationCollisionOption creationCollisionOption = CreationCollisionOption.OpenIfExists)
        {
            throw new PlatformNotSupportedException();
        }

        public async Task<byte[]> GetBytesFromFileAsync(string fileName, string subfolderName = null)
        {
            string folder = GetAppDataFolder();
            string path = Path.Combine(folder, subfolderName, fileName);
            
            if(!Path.Exists(path)) return null;

            return await File.ReadAllBytesAsync(path);
        }

        public Task<StorageFile> GetFileAsync(string fileName, string subfolderName = null)
        {
            throw new PlatformNotSupportedException();
        }

        public Task<StorageFolder> GetFolderAsync(string subfolderName = null)
        {
            throw new PlatformNotSupportedException();
        }

        public async Task<(T data, Exception ex)> GetFromFileAsync<T>(string fileName, string subfolderName = null)
        {
            try
            {
                var directoryPath = GetAppDataFolder();

                if(!string.IsNullOrEmpty(subfolderName))
                   directoryPath = Path.Combine(directoryPath, subfolderName);

                var filePath = Path.Combine(directoryPath, fileName);

                if(!File.Exists(filePath)) return (default, null);

                string content = await File.ReadAllTextAsync(filePath);

                T data = JsonConvert.DeserializeObject<T>(content);

                return (data, null);
            }
            catch(Exception ex)
            {
                return (default, ex);
            }
        }

        public async Task<StorageFile> SaveBytesToFileAsync(string fileName, byte[] bytes, string subfolderName = null)
        {
            string folder = GetAppDataFolder();

            if(!string.IsNullOrEmpty(subfolderName))
            {
                folder = Path.Combine(folder, subfolderName);

                if(!Path.Exists(folder)) Directory.CreateDirectory(folder);
            }

            string filePath = Path.Combine(folder, fileName);

            if(!Path.Exists(filePath)) File.Create(filePath);

            await File.WriteAllBytesAsync(filePath, bytes);

            return null;
        }

        public async Task SetToFileAsync<T>(string fileName, T data, string subfolderName = null)
        {
            var directoryPath = GetAppDataFolder();

            if(!string.IsNullOrEmpty(subfolderName))
               directoryPath = Path.Combine(directoryPath, subfolderName);

            if(!Directory.Exists(directoryPath)) 
               Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName);
            
            string dataContent = JsonConvert.SerializeObject(data);

            await File.WriteAllTextAsync(filePath, dataContent);
        }

        public Task DeleteFileAsync(string fileName, string subfolderName = null)
        {
            var directoryPath = GetAppDataFolder();

            if(!string.IsNullOrEmpty(subfolderName))
               directoryPath = Path.Combine(directoryPath, subfolderName);

            if(!Directory.Exists(directoryPath)) 
               Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName);

            if(File.Exists(filePath)) File.Delete(filePath);

            return Task.FromResult(Task.CompletedTask);
        }
    }
}
