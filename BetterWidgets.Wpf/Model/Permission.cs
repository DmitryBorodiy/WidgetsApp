using BetterWidgets.Consts;
using BetterWidgets.Enums;
using Wpf.Ui.Controls;

namespace BetterWidgets.Model
{
    public class Permission
    {
        public Permission() { }
        public Permission(string scope, PermissionLevel level = PermissionLevel.HighLevel)
        {
            Scope = scope;
            Level = level;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Scope { get; set; }

        public PermissionLevel Level { get; set; } = PermissionLevel.Undefined;
        public PermissionState State { get; set; } = PermissionState.Undefined;

        public string GetKey(Guid id) => $"{id}:{Scope}";

        public SymbolRegular GetIcon()
        {
            switch(Scope)
            {
                case Scopes.SystemInformation:
                    return SymbolRegular.DeveloperBoard20;
                case Scopes.Clipboard:
                    return SymbolRegular.Clipboard20;
                case Scopes.Location:
                    return SymbolRegular.Location20;
                case Scopes.AccountInfo:
                    return SymbolRegular.Person20;
                case Scopes.Appointments:
                    return SymbolRegular.CalendarEmpty20;
                case Scopes.Tasks:
                    return SymbolRegular.TaskListLtr20;
                case Scopes.Notes:
                    return SymbolRegular.Note20;
                default:
                    return SymbolRegular.Shield20;
            }
        }
    }
}
