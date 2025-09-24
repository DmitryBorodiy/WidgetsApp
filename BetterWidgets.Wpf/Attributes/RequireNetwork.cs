namespace BetterWidgets.Attributes
{
    public class RequireNetwork : Attribute
    {
        public bool IsRequire { get; set; }

        public RequireNetwork(bool isRequire = true)
        {
            IsRequire = isRequire;
        }
    }
}
