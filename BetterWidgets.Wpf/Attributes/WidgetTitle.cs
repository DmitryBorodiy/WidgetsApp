namespace BetterWidgets.Attributes
{
    public class WidgetTitle : Attribute
    {
        public bool UseResources { get; set; }
        public string Title { get; private set; }

        public WidgetTitle(string title, bool useResources = false)
        {
            Title = title;
            UseResources = useResources;
        }
    }
}
