using Wpf.Ui.Controls;
using System.Windows.Media;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Color = System.Windows.Media.Color;

namespace BetterWidgets.Views.Dialogs
{
    [ObservableObject]
    public partial class ColorPickerDialog : FluentWindow
    {
        public ColorPickerDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        [ObservableProperty]
        public Color selectedColor = Colors.Black;

        public ContentDialogResult Result { get; set; }

        public ICommand CloseCommand => new RelayCommand(Close);

        [RelayCommand]
        private void Primary()
        {
            Result = ContentDialogResult.Primary;
            CloseCommand.Execute(default);
        }

        [RelayCommand]
        private void Secondary()
        {
            Result = ContentDialogResult.Secondary;
            CloseCommand.Execute(default);
        }
    }
}
