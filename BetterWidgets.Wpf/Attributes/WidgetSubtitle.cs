namespace BetterWidgets.Attributes
{
    public class WidgetSubtitle : Attribute
    {
        public bool UseResources { get; set; }
        public string Subtitle { get; set; }

        public WidgetSubtitle(string subtitle, bool useResources = false)
        {
            Subtitle = subtitle;
            UseResources = useResources;
        }
    }
}
