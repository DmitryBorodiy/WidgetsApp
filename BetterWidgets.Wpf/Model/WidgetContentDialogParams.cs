using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace BetterWidgets.Model
{
    public class WidgetContentDialogParams
    {
        public string Title { get; set; } = string.Empty;
        public bool StaysOpen { get; set; } = false;
        public object Content { get; set; }
        public object PrimaryButtonContent { get; set; }
        public object PrimaryButtonParameter { get; set; }
        public object SecondaryButtonContent { get; set; }
        public object SecondaryButtonParameter { get; set; }

        public ControlAppearance PrimaryButtonAppearance { get; set; } = ControlAppearance.Primary;
        public ControlAppearance SecondaryButtonAppearance { get; set; } = ControlAppearance.Secondary;

        public Brush Background { get; set; }
        public Brush Foreground { get; set; }

        public Visibility TitleBarVisibility { get; set; }
        public Visibility SecondaryButtonVisibility { get; set; }
        public Visibility PrimaryButtonVisibility { get; set; }

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand SecondaryButtonCommand { get; set; }
    }
}
