using Wpf.Ui.Controls;

namespace BetterWidgets.Abstractions
{
    public interface ISetting : ISearchable
    {
        string Id { get; set; }
        string Subtitle { get; set; }
        IconElement Icon { get; set; }
    }
}
