using BetterWidgets.Abstractions;
using BetterWidgets.Extensions;
using BetterWidgets.ViewModel.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BetterWidgets.ViewModel
{
    public partial class PermissionsViewModel : ObservableObject
    {
        public PermissionsViewModel() { }
        public PermissionsViewModel(WidgetMetadata widget)
        {
            Widget = widget;
        }

        #region Props

        public string Subtitle => GetSubtitle();
        public string PermissionsLabel => GetPermissionsLabel();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtitle))]
        [NotifyPropertyChangedFor(nameof(PermissionsLabel))]
        [NotifyPropertyChangedFor(nameof(PermissionViews))]
        public WidgetMetadata widget;

        public ObservableCollection<PermissionViewItem> PermissionViews 
            => Widget?.Permissions
               ?.Select(p => new PermissionViewItem(Widget.Id, p))
               ?.ToObservableCollection();

        #endregion

        #region Utils

        private string GetSubtitle() => string.Format
        (
            Resources.Resources.PermissionsSubtitle, 
            Widget?.Title
        );

        private string GetPermissionsLabel()
        {
            string permissions = string.Empty;

            for(int i = 0; i < PermissionViews.Count; i++)
            {
                if(i == PermissionViews.Count - 1)
                   permissions += PermissionViews[i]?.Title;
                else
                   permissions += $"{PermissionViews[i].Title}, ";
            }

            return permissions;
        }

        #endregion
    }
}
