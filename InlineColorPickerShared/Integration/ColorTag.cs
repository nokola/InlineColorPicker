using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;

namespace InlineColorPicker
{
    public class ColorTag : ITag {
        internal ColorTag(ColorInfo colorInfo, ITrackingSpan span) {
            this.ColorInfo = colorInfo;
            this.TrackingSpan = span;
        }

        internal ITrackingSpan TrackingSpan;
        internal ColorInfo ColorInfo;
    }
}
 