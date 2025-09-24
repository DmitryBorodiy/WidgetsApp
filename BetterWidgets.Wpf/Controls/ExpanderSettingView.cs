using BetterWidgets.Abstractions;
using BetterWidgets.Enums;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace BetterWidgets.Controls
{
    public class ExpanderSettingView : Expander, ISetting
    {
        public ExpanderSettingView()
        {
            
        }

        public SearchType SearchType => SearchType.Settings;

        #region PropsRegistractions

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            nameof(Id),
            typeof(string),
            typeof(ExpanderSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ExpanderSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(ExpanderSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(ExpanderSettingView),
            new PropertyMetadata(null));

        #endregion

        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    }
}
