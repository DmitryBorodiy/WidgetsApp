using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph.Models;

namespace BetterWidgets.Model
{
    public partial class Account : ObservableObject
    {
        public Account() { }

        public Account(User user, ImageSource imageSource)
        {
            Id = user.Id;

            DisplayName = user.DisplayName;
            Email = user.UserPrincipalName;
            AvatarImageSource = imageSource;
        }

        public string Id { get; set; }

        [ObservableProperty]
        public string displayName = string.Empty;

        [ObservableProperty]
        public string email = string.Empty;

        [ObservableProperty]
        public ImageSource avatarImageSource;
    }
}
