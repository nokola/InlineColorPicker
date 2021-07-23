using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using EasyPainter.Imaging.Silverlight;

namespace InlineColorPicker
{
    internal sealed class ColorTagger : ITagger<ColorTag>
    {
        private ITextBuffer _buffer;
        private bool shouldAllowFreeFloatingColorNames = false; // true for CSS, false otherwise. Used to mark "red" vs red (no quotes) as tag

        Regex _regex = new Regex(@"\#[\dA-F]{3,8}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        internal ColorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            shouldAllowFreeFloatingColorNames = _buffer.ContentType.TypeName == "css";
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ColorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                string text = span.GetText();
                foreach (Match match in ColorSpace.MatchNamedColor(text, shouldAllowFreeFloatingColorNames))
                {
                    object color;
                    try
                    {
                        color = ColorConverter.ConvertFromString(ColorSpace.HexFromColorUpperCase[match.ToString().ToUpperInvariant().Substring(1)]);
                    }
                    catch (Exception)
                    {
                        color = null;
                    }
                    int matchStart = span.Start + match.Index + 1;
                    int matchLength = match.Length - 1;

                    ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(matchStart, matchLength, SpanTrackingMode.EdgeInclusive, TrackingFidelityMode.Forward);

                    ColorTag tag;
                    ColorInfo colorInfo = new ColorInfo() { WasSpecifiedWithAlpha = false };
                    if (color == null)
                    {
                        colorInfo.Color = null;
                    }
                    else
                    {
                        colorInfo.Color = (Color)color;
                    }

                    tag = new ColorTag(colorInfo, trackingSpan);

                    yield return new TagSpan<ColorTag>(new SnapshotSpan(span.Snapshot, matchStart, matchLength), tag);
                }

                foreach (Match match in _regex.Matches(text))
                {

                    object color;
                    try
                    {
                        color = ColorConverter.ConvertFromString(match.ToString());
                    }
                    catch (Exception)
                    {
                        color = null;
                    }
                    int matchStart = span.Start + match.Index;
                    int matchLength = match.Length;

                    ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(matchStart, matchLength, SpanTrackingMode.EdgeInclusive, TrackingFidelityMode.Forward);

                    ColorTag tag;
                    ColorInfo colorInfo = new ColorInfo() { WasSpecifiedWithAlpha = matchLength > 7 };
                    if (color == null)
                    {
                        colorInfo.Color = null;
                    }
                    else
                    {
                        colorInfo.Color = (Color)color;
                    }

                    tag = new ColorTag(colorInfo, trackingSpan);

                    yield return new TagSpan<ColorTag>(new SnapshotSpan(span.Snapshot, matchStart, matchLength), tag);
                }
            }
        }
    }

}
