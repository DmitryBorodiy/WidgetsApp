using Windows.Storage;

namespace BetterWidgets.Services
{
    public interface IDataService
    {
        Task<StorageFolder> GetFolderAsync(string subfolderName = null);
        Task<(T data, Exception ex)> GetFromFileAsync<T>(string fileName, string subfolderName = default);
        Task SetToFileAsync<T> (string fileName, T data, string subfolderName = default);

        Task<StorageFile> GetFileAsync(string fileName, string subfolderName = default);
        Task<StorageFile> CreateFileAsync(string fileName, string subfolderName = default, CreationCollisionOption creationCollisionOption = CreationCollisionOption.OpenIfExists);

        Task<byte[]> GetBytesFromFileAsync(string fileName, string subfolderName = default);
        Task<StorageFile> SaveBytesToFileAsync(string fileName, byte[] bytes, string subfolderName = default);
        Task DeleteFileAsync(string fileName, string subfolderName = default);
    }
}
