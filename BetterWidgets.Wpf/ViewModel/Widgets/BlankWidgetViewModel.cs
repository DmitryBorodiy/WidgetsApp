using BetterWidgets.Controls;
using BetterWidgets.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class BlankWidgetViewModel : ObservableObject
    {
        private const string _id = "87cd8a59-399a-4843-b467-f709af0f6928";

        #region Props

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StateLabel))]
        public Widget widget;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CounterLabel))]
        public int counter;

        public string CounterLabel => $"Clicks {Counter}";

        public string StateLabel => GetState();

        #endregion

        #region Commands

        public ICommand AppearedCommand => new RelayCommand<Widget>(Loaded);

        public ICommand CountCommand => new RelayCommand(() => Counter++);
        public ICommand CloseCommand => new RelayCommand(Close);

        #endregion

        private void Loaded(Widget widget)
        {
            Widget = widget;
        }

        private void Close()
        {
            if(!Widget.IsPreview)
               Widget?.UnpinDesktop();
        }

        private string GetState()
        {
            return Widget?.State.ToString();
        }
    }
}
