using System.IO;
using Windows.Storage;
using Microsoft.Extensions.Logging;
using BetterWidgets.Consts;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace BetterWidgets.Services
{
    public class DataService : IDataService
    {
        private readonly ILogger _logger;

        public DataService(ILogger<DataService> logger)
        {
            _logger = logger;
        }

        public async Task<StorageFolder> GetFolderAsync(string subfolderName = null)
        {
            var folder = string.IsNullOrEmpty(subfolderName) ?
                               ApplicationData.Current.LocalFolder :
                               await ApplicationData.Current.LocalFolder.CreateFolderAsync(subfolderName, CreationCollisionOption.OpenIfExists);

            return folder;
        }

        private async Task<StorageFile> GetCreateFileAsync(string fileName, StorageFolder folder)
        {
            var file = await folder.TryGetItemAsync(fileName);

            return file != null ? 
                   (StorageFile)file :
                   await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        }

        public async Task<(T data, Exception ex)> GetFromFileAsync<T>(string fileName, string subfolderName = null)
        {
            try
            {
                if(string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(Errors.FileNameIsNullOrEmpty);

                var folder = await GetFolderAsync(subfolderName);
                var file = await folder.TryGetItemAsync(fileName);

                if(file is not StorageFile) return (default, null);

                string content = await FileIO.ReadTextAsync(file as StorageFile, UnicodeEncoding.Utf8);

                if(string.IsNullOrEmpty(content)) throw new InvalidDataException(string.Format(Errors.FileContentIsEmpty, file.Name));

                return (JsonConvert.DeserializeObject<T>(content), null);
            }
            catch(Exception ex)
            {
                return (default, ex);
            }
        }

        public async Task SetToFileAsync<T>(string fileName, T data, string subfolderName = null)
        {
            try
            {
                if(data == null) throw new ArgumentNullException(Errors.DataIsNull);
                if(string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(Errors.FileNameIsNullOrEmpty);

                var folder = await GetFolderAsync(subfolderName);
                var file = await GetCreateFileAsync(fileName, folder);

                string content = JsonConvert.SerializeObject(data);

                await FileIO.WriteTextAsync(file, content, UnicodeEncoding.Utf8);
            }
            catch(COMException) {}
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        public async Task<StorageFile> GetFileAsync(string fileName, string subfolderName = null)
        {
            try
            {
                var folder = await GetFolderAsync(subfolderName);
                var item = await folder.TryGetItemAsync(fileName);

                if(item is StorageFile file) return file;
                else return null;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task<StorageFile> CreateFileAsync(string fileName, string subfolderName = null, CreationCollisionOption creationCollisionOption = CreationCollisionOption.OpenIfExists)
        {
            try
            {
                var folder = await GetFolderAsync(subfolderName);

                return await folder.CreateFileAsync(fileName, creationCollisionOption);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task<byte[]> GetBytesFromFileAsync(string fileName, string subfolderName = null)
        {
            try
            {
                var folder = await GetFolderAsync(subfolderName);
                var item = await folder.TryGetItemAsync(fileName);

                if(item is StorageFile storageFile)
                {
                    var buffer = await FileIO.ReadBufferAsync(storageFile);
                    byte[] data = new byte[buffer.Length];

                    using var reader = DataReader.FromBuffer(buffer);

                    reader.ReadBytes(data);

                    return data;
                }
                else return null;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task<StorageFile> SaveBytesToFileAsync(string fileName, byte[] bytes, string subfolderName = null)
        {
            try
            {
                var folder = await GetFolderAsync(subfolderName);
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

                await FileIO.WriteBytesAsync(file, bytes);

                return file;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task DeleteFileAsync(string fileName, string subfolderName = null)
        {
            try
            {
                var file = await GetFileAsync(fileName, subfolderName);

                if(file == null) return;
                if(!File.Exists(file.Path)) return;

                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
