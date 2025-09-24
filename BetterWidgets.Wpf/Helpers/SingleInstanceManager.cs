using System.IO.Pipes;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Helpers
{
    public static class SingleInstanceManager
    {
        private static Mutex _mutex;
        private static bool _ownsMutex;
        private static readonly ILogger _logger;
        private const string PipeNamePrefix = "BetterWidgetsPipe_";
        private static CancellationTokenSource _cts;

        static SingleInstanceManager()
        {
            _logger = App.Services?.GetService<ILogger<App>>();
        }

        public static bool EnsureSingleInstance(string id, Action<string> onCommandReceived)
        {
            _mutex = new Mutex(true, $"Local\\{id}", out _ownsMutex);

            if(_ownsMutex)
            {
                _cts = new CancellationTokenSource();

                Task.Run(() => ListenAsync(PipeNamePrefix + id, onCommandReceived, _cts.Token));
            }

            return _ownsMutex;
        }

        public static void Release()
        {
            if(_ownsMutex)
               _mutex?.ReleaseMutex();

            _mutex?.Dispose();
        }

        private static async Task ListenAsync(string pipeName, Action<string> onCommandReceived, CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
                try
                {
                    await server.WaitForConnectionAsync(token);
                    using var reader = new StreamReader(server, Encoding.UTF8);
                    var command = await reader.ReadLineAsync();

                    if(!string.IsNullOrWhiteSpace(command))
                    {
                        App.Current.Dispatcher.Invoke(() => onCommandReceived(command));
                    }
                }
                catch(Exception ex)
                {
                    _logger?.LogError(ex, ex.Message, ex.StackTrace);  
                }
            }
        }

        public static void SendCommandToExistingInstance(string id, string command)
        {
            try
            {
                if(string.IsNullOrEmpty(command)) command = "addwidget";

                using var client = new NamedPipeClientStream(".", PipeNamePrefix + id, PipeDirection.Out);
                client.Connect(1000);
                
                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(command);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }

}
