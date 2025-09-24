using BetterWidgets.Abstractions;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace BetterWidgets.Services
{
    public class DataService<TWidget> : IDataService where TWidget : IWidget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IDataService _data;
        #endregion

        #region Fields
        private readonly string _typeName;
        #endregion

        public DataService(ILogger<DataService<TWidget>> logger, IDataService data)
        {
            _logger = logger;
            _data = data;

            _typeName = typeof(TWidget).Name;
        }

        public async Task<StorageFolder> GetFolderAsync(string subfolderName = null)
            => await _data.GetFolderAsync(_typeName);

        public async Task<StorageFile> CreateFileAsync(string fileName, string subfolderName = null, CreationCollisionOption creationCollisionOption = CreationCollisionOption.OpenIfExists)
            => await _data?.CreateFileAsync(fileName, _typeName, CreationCollisionOption.OpenIfExists);

        public async Task<byte[]> GetBytesFromFileAsync(string fileName, string subfolderName = null)
            => await _data?.GetBytesFromFileAsync(fileName, _typeName);

        public async Task<StorageFile> GetFileAsync(string fileName, string subfolderName = null)
            => await _data?.GetFileAsync(fileName, _typeName);

        public async Task<(T data, Exception ex)> GetFromFileAsync<T>(string fileName, string subfolderName = null)
            => await _data?.GetFromFileAsync<T>(fileName, _typeName);

        public async Task<StorageFile> SaveBytesToFileAsync(string fileName, byte[] bytes, string subfolderName = null)
            => await _data?.SaveBytesToFileAsync(fileName, bytes, _typeName);

        public async Task SetToFileAsync<T>(string fileName, T data, string subfolderName = null)
            => await _data?.SetToFileAsync<T>(fileName, data, _typeName);

        public async Task DeleteFileAsync(string fileName, string subfolderName = null)
            => await _data?.DeleteFileAsync(fileName, _typeName);
    }
}
