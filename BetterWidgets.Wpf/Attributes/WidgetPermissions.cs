namespace BetterWidgets.Attributes
{
    public class WidgetPermissions : Attribute
    {
        public WidgetPermissions(string[] permissions)
        {
            Permissions = permissions;
        }

        public readonly string[] Permissions;
    }
}
