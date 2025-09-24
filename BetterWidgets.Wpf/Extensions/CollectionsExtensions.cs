using System.Collections.ObjectModel;

namespace BetterWidgets.Extensions
{
    public static class CollectionsExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
            => new ObservableCollection<T>(source);
    }
}
