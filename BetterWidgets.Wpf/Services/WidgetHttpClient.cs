using System.Net.Http;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Services
{
    public class WidgetHttpClient<TWidget> : HttpClient where TWidget : IWidget
    {
        public WidgetHttpClient() { }
    }
}
