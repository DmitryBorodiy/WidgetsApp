namespace BetterWidgets.Attributes
{
    public class WidgetIcon : Attribute
    {
        public string Source { get; set; }

        public WidgetIcon(string source)
        {
            Source = source;
        }
    }
}
