using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Extensions;
using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinRT;

namespace BetterWidgets.Services
{
    public sealed class ShareService : IShareService
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private DataTransferManager _dataTransfer;
        #endregion

        public ShareService(ILogger<ShareService> logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        #region Props

        private bool IsSupported => DataTransferManager.IsSupported();
        public bool AllowShare => _settings.AllowSharing;
        private object ShareData { get; set; }
        private string Title { get; set; }
        private string Description { get; set; }

        static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

        #endregion

        #region Utils

        public static DataTransferManager GetDataTransferManager(IntPtr appWindow)
        {
            IDataTransferManagerInterop interop = DataTransferManager.As<IDataTransferManagerInterop>();
            Guid id = new Guid(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);
            IntPtr result;
            result = interop.GetForWindow(appWindow, id);
            DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);
            return (dataTransferManager);
        }

        #endregion

        public (BitmapImage image, Exception ex) CreateWidgetCard(Widget widget)
        {
            try
            {
                if(widget == null) throw new ArgumentNullException(nameof(widget));

                var bitmapView = CaptureHelper.CaptureControl((FrameworkElement)widget.Content);

                return (bitmapView, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public (bool success, Exception ex) RequestShare<T>(T shareData, Widget widget, string title = null, string description = null)
        {
            try
            {
                if(shareData == null) throw new ArgumentNullException(nameof(shareData));
                if(widget == null) throw new InvalidOperationException(Errors.CannotAccessAppShell);
                if(!IsSupported) throw new NotSupportedException(Errors.ShareNotSupported);

                var handle = widget.IsPreview ?
                             ShellHelper.GetAppShellHwnd() : widget.GetHwnd();

                Title = title;
                Description = description;

                _dataTransfer = GetDataTransferManager(handle);
                _dataTransfer.DataRequested += OnDataRequested;

                ShareData = shareData;

                DataTransferManagerInterop.ShowShareUIForWindow(handle);

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if(ShareData == null) return;

            args.Request.Data.Properties.Title = Title;
            args.Request.Data.Properties.Description = Description;

            if(ShareData is string shareText)
               args.Request.Data.SetText(shareText);
            else if(ShareData is BitmapImage image)
            {
                var streamReference = StreamHelpers.ConvertBitmapImageToStreamReference(image);

                args.Request.Data.SetBitmap(streamReference);
            }
            else if(ShareData is StorageFile file)
                args.Request.Data.SetStorageItems([file]);
            else 
                throw new NotSupportedException(Errors.ShareFormatNotSupported);

            _dataTransfer.DataRequested -= OnDataRequested;
        }
    }
}
