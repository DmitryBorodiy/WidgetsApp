using CommunityToolkit.Mvvm.ComponentModel;
using Emoji.Wpf;
using System.Collections.ObjectModel;

namespace BetterWidgets.ViewModel.Components
{
    public partial class EmojiViewModel : ObservableObject
    {
        public EmojiViewModel() { }
        public EmojiViewModel(EmojiData.Emoji emoji)
        {
            Name = emoji.Name;
            Symbol = emoji.Text;
        }
        public EmojiViewModel(string symbol, string name = null)
        {
            Symbol = symbol;
        }

        [ObservableProperty]
        public string name = string.Empty;

        [ObservableProperty]
        public string symbol = string.Empty;
    }

    public partial class EmojiGroupViewModel : ObservableObject
    {
        public EmojiGroupViewModel() { }

        public EmojiGroupViewModel(EmojiData.Group group)
        {
            Title = group.Name;
            Symbol = group.Icon;
            Count = group.EmojiCount;

            Emojis = new ObservableCollection<EmojiViewModel>
                (group.EmojiList.Select(e => new EmojiViewModel(e)));
        }

        public EmojiGroupViewModel(string title, string symbol, IEnumerable<EmojiViewModel> emojis)
        {
            Title = title;
            Symbol = symbol;
            Emojis = new ObservableCollection<EmojiViewModel>(emojis);

            Count = Emojis?.Count ?? 0;
        }

        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public string symbol = string.Empty;

        [ObservableProperty]
        public int count = 0;

        [ObservableProperty]
        public ObservableCollection<EmojiViewModel> emojis;

        public override string ToString() => Title;
    }
}
