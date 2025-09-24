using BetterWidgets.Consts;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace BetterWidgets.Views.Dialogs
{
    public class RequestAccessDialog
    {
        public RequestAccessDialog(Permission permission)
        {
            _resources = Resources.Resources.ResourceManager;

            Permission = permission;
        }

        #region Services
        private readonly ResourceManager _resources;
        #endregion

        #region Props

        private Permission Permission { get; set; }

        #endregion

        #region Methods

        private string CreateTitle()
            => string.Format(Resources.Resources.RequestAccessTitle,
                             _resources.GetString(Permission.Scope));

        private string CreateSubtitle()
            => string.Format(Resources.Resources.RequestAccessSubtitle,
                             _resources.GetString(Permission.Scope));

        private TextBlock CreateTitleText() => new TextBlock()
        {
            Text = CreateTitle(),
            Style = (Style)Application.Current.Resources[KnownResources.PermissionDialogTitle]
        };

        private TextBlock CreateSubtitleText() => new TextBlock()
        {
            Text = CreateSubtitle(),
            Style = (Style)Application.Current.Resources[KnownResources.PermissionDialogSubtitle]
        };

        private IconElement CreateTitleIcon()
        {
            var icon = Permission.GetIcon();
            var iconElement = IconHelper.CreateFilled(icon);

            iconElement.Style = (Style)Application.Current.Resources[KnownResources.PermissionDialogIcon];

            return iconElement;
        }

        private StackPanel CreateTitleRoot()
        {
            var root = new StackPanel();

            root.Children.Add(CreateTitleIcon());
            root.Children.Add(CreateTitleText());

            return root;
        }

        private StackPanel CreateRootLayout()
        {
            var stack = new StackPanel();

            stack.Children.Add(CreateTitleRoot());
            stack.Children.Add(CreateSubtitleText());

            return stack;
        }

        public MessageBox CreateDialog() => new MessageBox()
        {
            Content = CreateRootLayout(),
            Style = (Style)Application.Current.Resources[KnownResources.PermissionMessageBoxStyle]
        };

        #endregion
    }
}
