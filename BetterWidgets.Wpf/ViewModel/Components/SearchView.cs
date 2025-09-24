using BetterWidgets.Abstractions;
using BetterWidgets.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Components
{
    public partial class SearchView : ObservableObject
    {
        public SearchView() { }
        public SearchView(ISearchable searchable)
        {
            Title = searchable.Title;
            Type = searchable.SearchType;

            GetView(searchable);
        }

        #region Props

        public SearchType Type { get; set; } = SearchType.Everything;

        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public IconElement icon;

        [ObservableProperty]
        public object tag;

        #endregion

        #region Utils

        private void GetView(ISearchable searchable)
        {
            switch(searchable)
            {
                case WidgetMetadata widget:
                    Tag = widget;
                    Icon = new ImageIcon()
                    {
                        Source = new BitmapImage(widget.Icon)
                    };
                    break;
                case ISetting setting:
                    Tag = setting;
                    Icon = setting.Icon;
                    break;
            }
        }

        #endregion
    }
}
