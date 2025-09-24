using System.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BetterWidgets.ViewModel.Components
{
    public partial class SystemSoundView : ObservableObject
    {
        public SystemSoundView() {}
        public SystemSoundView(SystemSound sound, string key)
        {
            Sound = sound;
            DisplayName = Resources.Resources.ResourceManager.GetString(key);
        }

        [ObservableProperty]
        public SystemSound sound;

        [ObservableProperty]
        public string displayName;

        [RelayCommand]
        private void Play() => Sound?.Play();
    }
}
