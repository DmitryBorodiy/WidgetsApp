using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Windows;

namespace BetterWidgets.Services
{
    public partial class WidgetManager : ObservableObject, IWidgetManager
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        #endregion

        public WidgetManager()
        {
            _logger = App.Services?.GetRequiredService<ILogger<WidgetManager>>();
            _settings = App.Services?.GetRequiredService<Settings>();
        }

        #region Fields
        private static WidgetManager _widgetManager;
        private Dictionary<Guid, IWidget> ActivatedWidgets = new Dictionary<Guid, IWidget>();
        #endregion

        #region Props

        public static WidgetManager Current
        {
            get
            {
                if (_widgetManager == null)
                    _widgetManager = new WidgetManager();

                return _widgetManager;
            }
        }

        public Widget CurrentPreview { get; private set; }

        [ObservableProperty]
        public Dictionary<Guid, WidgetMetadata> widgets;

        #endregion

        #region Events
        public event EventHandler<IEnumerable<WidgetMetadata>> AllWidgetsGetted;
        #endregion

        #region Methods

        public WidgetMetadata GetWidgetById(Guid id)
        {
            if(Widgets == null) return null;

            return Widgets[id];
        }

        public WidgetMetadata GetWidgetByType<T>() where T : IWidget
            => Widgets?.Values.FirstOrDefault(w => w.Type.Name == typeof(T).Name);

        public Widget CreatePreview(Type widgetType)
        {
            if(widgetType == null) throw new ArgumentNullException(Errors.TypeWasNull);
            if(CurrentPreview != null &&
               widgetType == CurrentPreview.GetType()) return CurrentPreview;

            CurrentPreview = (Widget)Activator.CreateInstance(widgetType);

            CurrentPreview.TitleBar = null;
            CurrentPreview.IsPreview = true;
            CurrentPreview.ActivateWidget(false);

            if(CurrentPreview.Content is FrameworkElement content)
            {
                content.DataContext = CurrentPreview.DataContext;
                content.Loaded += delegate
                { 
                    CurrentPreview.AppearedCommand?.Execute
                        (CurrentPreview.AppearedCommandParameter); 
                };
            }

            return CurrentPreview;
        }

        public IEnumerable<WidgetMetadata> GetWidgets()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly?.GetTypes();
                var widgets = types?.Where(w => w.IsSubclassOf(typeof(Widget)) &&
                                                w.GetType() != typeof(Widget))
                                    .ToList();

                Widgets = new Dictionary<Guid, WidgetMetadata>();

                for(int i = 0; i < widgets.Count; i++)
                {
                    var type = widgets[i];
                    WidgetMetadata widget = new WidgetMetadata(type);

                    Widgets.Add(widget.Id, widget);

                    if(widget.IsPinnedDesktop)
                    {
                        var activationResult = ActivateWidget(widget, true);

                        if(activationResult.ex != null)
                           _logger?.LogError(activationResult.ex, activationResult.ex.Message, activationResult.ex.StackTrace);
                    }
                }

                AllWidgetsGetted?.Invoke(this, Widgets.Values);

                return Widgets.Values;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Enumerable.Empty<WidgetMetadata>();
            }
        }

        public WidgetMetadata GetWidgetByType(Type widgetType)
            => Widgets?.Values.FirstOrDefault(t => t.Type.Name == widgetType.Name);

        public IEnumerable<WidgetMetadata> GetWidgetsPinned()
            => Widgets.Values.Where(w => w.IsPinnedDesktop);

        public void PinToDesktop(Guid id)
        {
            if(Widgets == null) return;
            if(!Widgets.ContainsKey(id)) throw new InvalidOperationException(Errors.WidgetWithSpecifiedIdIsNotExists);

            var metadata = Widgets[id];
            var activationResult = ActivateWidget(metadata);

            if(activationResult.widget != null)
               activationResult.widget.PinOnDesktop();

            if(activationResult.ex != null)
               _logger?.LogError(activationResult.ex, activationResult.ex.Message, activationResult.ex.StackTrace);
        }

        public void UnpinFromDesktop(Guid id)
        {
            if(Widgets == null) return;
            if(!Widgets.ContainsKey(id)) throw new InvalidOperationException(Errors.WidgetWithSpecifiedIdIsNotExists);
            if(!ActivatedWidgets.ContainsKey(id)) return;

            ActivatedWidgets[id].UnpinDesktop();
            DeactivateWidget(id);
        }

        public bool IsActivated(Guid id) => ActivatedWidgets.ContainsKey(id);

        public IEnumerable<IWidget> GetActivatedWidgets() => ActivatedWidgets.Values;

        public IWidget GetActivatedWidget(Guid id)
        {
            if(ActivatedWidgets == null) return null;
            if(!ActivatedWidgets.ContainsKey(id)) return null;

            return ActivatedWidgets[id];
        }

        public T GetActivatedWidgetByType<T>() where T : IWidget
        {
            if(ActivatedWidgets == null) return default;
            if(ActivatedWidgets.Count == 0) return default;
            
            return (T)ActivatedWidgets.Values.FirstOrDefault(w => w.GetType().Name == typeof(T).Name);
        }

        public (Widget widget, Exception ex) ActivateWidget(WidgetMetadata metadata, bool activateView = false)
        {
            try
            {
                if(metadata == null) throw new ArgumentNullException(Errors.WidgetMetadataCannotBeNull);
                if(metadata.Id == default) throw new ArgumentException(Errors.IdNullOrEmpty);
                if(metadata.Type == default) throw new ArgumentException(Errors.TypeWasNull);
                if(ActivatedWidgets.ContainsKey(metadata.Id)) return ((Widget)ActivatedWidgets[metadata.Id], null);

                var widget = (Widget)Activator.CreateInstance(metadata.Type);

                if(widget == null) throw new InvalidOperationException(Errors.CannotActivateWidgetInstance);

                if(activateView) widget.ActivateWidget(activateView);

                ActivatedWidgets.Add(metadata.Id, widget);

                return (widget, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public (bool success, Exception ex) DeactivateWidget(Guid id)
        {
            try
            {
                if(id == default) throw new ArgumentException(Errors.IdNullOrEmpty);
                if(!ActivatedWidgets.ContainsKey(id)) throw new InvalidOperationException(Errors.WidgetWithSpecifiedIdIsNotExists);

                ActivatedWidgets.Remove(id);

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        #endregion
    }
}
