namespace BetterWidgets.Attributes
{
    public sealed class DevMode : Attribute
    {
        public bool Enabled { get; set; } = true;

        public DevMode(bool enable = true)
        {
            Enabled = enable;
        }
    }
}
