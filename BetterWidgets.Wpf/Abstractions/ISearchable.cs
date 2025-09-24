using BetterWidgets.Enums;

namespace BetterWidgets.Abstractions
{
    public interface ISearchable
    {
        string Title { get; set; }
        SearchType SearchType { get; }
    }
}
