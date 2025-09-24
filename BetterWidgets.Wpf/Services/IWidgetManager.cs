using BetterWidgets.Controls;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Services
{
    public interface IWidgetManager
    {
        Widget CurrentPreview { get; }

        Dictionary<Guid, WidgetMetadata> Widgets { get; protected set; }

        event EventHandler<IEnumerable<WidgetMetadata>> AllWidgetsGetted;

        Widget CreatePreview(Type widgetType);
        WidgetMetadata GetWidgetById(Guid id);
        WidgetMetadata GetWidgetByType(Type widgetType);
        WidgetMetadata GetWidgetByType<T>() where T : IWidget;

        IEnumerable<WidgetMetadata> GetWidgetsPinned();
        void PinToDesktop(Guid id);
        void UnpinFromDesktop(Guid id);

        bool IsActivated(Guid id);
        IEnumerable<IWidget> GetActivatedWidgets();
        IWidget GetActivatedWidget(Guid id);
        T GetActivatedWidgetByType<T>() where T : IWidget;

        (Widget widget, Exception ex) ActivateWidget(WidgetMetadata metadata, bool activateView = false);
        (bool success, Exception ex) DeactivateWidget(Guid id);

        IEnumerable<WidgetMetadata> GetWidgets();
    }
}
