using System.Windows.Media;
using System.Windows;

namespace BetterWidgets.Extensions
{
    public static class UIElementExtensions
    {
        public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            if(parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for(int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && ((FrameworkElement)child).Name == childName)
                    return typedChild;

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null) return foundChild;
            }

            return null;
        }
    }
}
