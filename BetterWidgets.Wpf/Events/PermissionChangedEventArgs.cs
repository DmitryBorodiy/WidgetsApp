using BetterWidgets.Model;

namespace BetterWidgets.Events
{
    public class PermissionChangedEventArgs : EventArgs
    {
        public PermissionChangedEventArgs(Permission permission)
        {
            Permission = permission;
        }

        public Permission Permission { get; set; }
    }
}
