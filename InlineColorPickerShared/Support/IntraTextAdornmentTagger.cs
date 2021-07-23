using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System.Diagnostics;

namespace InlineColorPicker
{
    /// <summary>
    /// Helper class for interspersing adornments into text.
    /// </summary>
    /// <remarks>
    /// To avoid an issue around intra-text adornment support and its interaction with text buffer changes,
    /// this tagger reacts to text and color tag changes with a delay. It waits to send out its own TagsChanged
    /// event until the WPF Dispatcher is running again and it takes care to report adornments
    /// that are consistent with the latest sent TagsChanged event by storing that particular snapshot
    /// and using it to query for the data tags.
    /// </remarks>
    internal abstract class IntraTextAdornmentTagger<TData, TAdornment>
        : ITagger<IntraTextAdornmentTag>
        where TData : ITag
        where TAdornment : UIElement
    {
        protected readonly IWpfTextView _view;
        private Dictionary<SnapshotSpan, TAdornment> _adornmentCache = new Dictionary<SnapshotSpan, TAdornment>();
        protected ITextSnapshot _snapshot { get; private set; }
        private readonly List<SnapshotSpan> _invalidatedSpans = new List<SnapshotSpan>();

        protected IntraTextAdornmentTagger(IWpfTextView view)
        {
            _view = view;
            _snapshot = view.TextBuffer.CurrentSnapshot;

            _view.LayoutChanged += HandleLayoutChanged;
            _view.TextBuffer.Changed += HandleBufferChanged;
        }

        protected abstract TAdornment CreateAdornment(TData dataTag);
        protected abstract void UpdateAdornment(TAdornment adornment, TData dataTag);
        protected abstract IEnumerable<Tuple<SnapshotSpan, TData>> GetAdorenmentData(NormalizedSnapshotSpanCollection spans);

        private void HandleBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            var editedSpans = args.Changes.Select(change => new SnapshotSpan(args.After, change.NewSpan)).ToList();
            InvalidateSpans(editedSpans);
        }

        protected void InvalidateSpans(IList<SnapshotSpan> spans)
        {
            lock (_invalidatedSpans)
            {
                bool wasEmpty = _invalidatedSpans.Count == 0;
                _invalidatedSpans.AddRange(spans);

                if (wasEmpty && _invalidatedSpans.Count > 0)
                {
                    _ = _view.VisualElement.Dispatcher.BeginInvoke(new Action(AsyncUpdate));
                }
            }
        }

        private void AsyncUpdate()
        {
            // Store the snapshot that we're now current with and send an event
            // for the text that has changed.
            if (_snapshot != _view.TextBuffer.CurrentSnapshot)
            {
                _snapshot = _view.TextBuffer.CurrentSnapshot;

                Dictionary<SnapshotSpan, TAdornment> translatedAdornmentCache = new Dictionary<SnapshotSpan, TAdornment>();

                foreach (var keyValuePair in _adornmentCache)
                {
                    translatedAdornmentCache[keyValuePair.Key.TranslateTo(_snapshot, SpanTrackingMode.EdgeExclusive)] = keyValuePair.Value;
                }

                _adornmentCache = translatedAdornmentCache;
            }

            List<SnapshotSpan> translatedSpans;
            lock (_invalidatedSpans)
            {
                translatedSpans = _invalidatedSpans.Select(s => s.TranslateTo(_snapshot, SpanTrackingMode.EdgeInclusive)).ToList();
                _invalidatedSpans.Clear();
            }

            if (translatedSpans.Count == 0)
                return;

            var start = translatedSpans.Select(span => span.Start).Min();
            var end = translatedSpans.Select(span => span.End).Max();


            RaiseTagsChanged(new SnapshotSpan(start, end));
        }

        protected void RaiseTagsChanged(SnapshotSpan span)
        {
            var handler = this.TagsChanged;
            if (handler != null)
                handler(this, new SnapshotSpanEventArgs(span));
        }

        private void HandleLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            SnapshotSpan visibleSpan = _view.TextViewLines.FormattedSpan;

            // Filter out the adornments that are no longer visible.
            List<SnapshotSpan> toRemove = new List<SnapshotSpan>(
                from keyValuePair
                in _adornmentCache
                where !keyValuePair.Key.TranslateTo(visibleSpan.Snapshot, SpanTrackingMode.EdgeExclusive).IntersectsWith(visibleSpan)
                select keyValuePair.Key);

            foreach (var span in toRemove)
                _adornmentCache.Remove(span);
        }


        // Produces tags on the snapshot that the tag consumer asked for.
        public virtual IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
                yield break;

            // Translate the request to the snapshot that this tagger is current with.

            ITextSnapshot requestedSnapshot = spans[0].Snapshot;

            var translatedSpans = new NormalizedSnapshotSpanCollection(spans.Select(span => span.TranslateTo(_snapshot, SpanTrackingMode.EdgeExclusive)));

            // Grab the adornments.
            foreach (var tagSpan in GetAdornmentTagsOnSnapshot(translatedSpans))
            {
                // Translate each adornment to the snapshot that the tagger was asked about.
                SnapshotSpan span = tagSpan.Span.TranslateTo(requestedSnapshot, SpanTrackingMode.EdgeExclusive);
                PositionAffinity? affinity = span.Length == 0 ? (PositionAffinity?)PositionAffinity.Successor : null; // Affinity is needed only for zero-length adornments.

                IntraTextAdornmentTag tag = new IntraTextAdornmentTag(tagSpan.Tag.Adornment, tagSpan.Tag.RemovalCallback, affinity);
                yield return new TagSpan<IntraTextAdornmentTag>(span, tag);
            }
        }

        // Produces tags on the snapshot that this tagger is current with.
        private IEnumerable<TagSpan<IntraTextAdornmentTag>> GetAdornmentTagsOnSnapshot(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            System.Diagnostics.Debug.Assert(snapshot == _snapshot);

            // Since WPF UI objects have state (like mouse hover or animation) and are relatively expensive to create and lay out,
            // this code tries to reuse controls as much as possible.
            // The controls are stored in _adornmentCache between the calls.

            // Mark which adornments fall inside the requested spans with Keep=false
            // so that they can be removed from the cache if they no longer correspond to data tags.
            HashSet<SnapshotSpan> toRemove = new HashSet<SnapshotSpan>();
            foreach (var ar in _adornmentCache)
                if (spans.IntersectsWith(new NormalizedSnapshotSpanCollection(ar.Key)))
                    toRemove.Add(ar.Key);

            foreach (var spanDataPair in GetAdorenmentData(spans))
            {
                // Look up the corresponding adornment or create one if it's new.
                TAdornment adornment;
                SnapshotSpan snapshotSpan = spanDataPair.Item1;
                TData adornmentData = spanDataPair.Item2;
                if (_adornmentCache.TryGetValue(snapshotSpan, out adornment))
                {
                    UpdateAdornment(adornment, adornmentData);
                    toRemove.Remove(snapshotSpan);
                }
                else
                {
                    adornment = CreateAdornment(adornmentData);

                    // Get the adornment to measure itself. Its DesiredSize property is used to determine
                    // how much space to leave between text for this adornment.
                    // Note: If the size of the adornment changes, the line will be reformatted to accommodate it.
                    // Note: Some adornments may change size when added to the view's visual tree due to inherited
                    // dependency properties that affect layout. Such options can include SnapsToDevicePixels,
                    // UseLayoutRounding, TextRenderingMode, TextHintingMode, and TextFormattingMode. Making sure
                    // that these properties on the adornment match the view's values before calling Measure here
                    // can help avoid the size change and the resulting unnecessary re-format.
                    adornment.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    _adornmentCache.Add(snapshotSpan, adornment);
                }

                yield return new TagSpan<IntraTextAdornmentTag>(snapshotSpan, new IntraTextAdornmentTag(adornment, null, null));
            }

            foreach (var snapshotSpan in toRemove)
                _adornmentCache.Remove(snapshotSpan);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
