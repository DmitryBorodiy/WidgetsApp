using BetterWidgets.Abstractions;
using BetterWidgets.Enums;
using System.Windows;
using Wpf.Ui.Controls;

namespace BetterWidgets.Controls
{
    public class CardSettingView : Button, ISetting
    {
        public SearchType SearchType => SearchType.Settings;

        #region PropsRegistractions

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            nameof(Id),
            typeof(string),
            typeof(CardSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CardSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(CardSettingView),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
            nameof(IsClickable),
            typeof(bool),
            typeof(CardSettingView),
            new PropertyMetadata(false, OnIsClickableChanged));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(CardSettingView),
            new PropertyMetadata(null));

        private static readonly DependencyPropertyKey IsClickAreaVisiblePropertyKey =
           DependencyProperty.RegisterReadOnly(
               nameof(IsClickAreaVisible),
               typeof(Visibility),
               typeof(CardSettingView),
               new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty IsClickAreaVisibleProperty =
            IsClickAreaVisiblePropertyKey.DependencyProperty;
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

        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set
            {
                SetValue(IsClickableProperty, value);
                SetValue(IsClickAreaVisibleProperty, IsClickAreaVisible);
            }
        }

        private Visibility IsClickAreaVisible
        {
            get => (Visibility)GetValue(IsClickAreaVisibleProperty);
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void OnIsClickableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is CardSettingView control)
            {
                control.SetValue(IsClickAreaVisiblePropertyKey, (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed);
            }
        }
    }
}
