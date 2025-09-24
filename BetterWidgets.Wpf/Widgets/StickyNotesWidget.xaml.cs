using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Attributes;
using System.Runtime.InteropServices;

namespace BetterWidgets.Widgets
{
    [Guid("63066b73-ff6f-4868-91d6-fbb7b6c71f59")]
    [WidgetPermissions([Scopes.Notes])]
    [WidgetTitle(Notes, true)]
    [WidgetSubtitle(NotesSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class StickyNotesWidget : Widget
    {
        private const string Notes = nameof(Notes);
        private const string NotesSubtitle = nameof(NotesSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/Notes/icon-48.png";

        public StickyNotesWidget() => InitializeComponent();
    }
}
