using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace InlineColorPicker
{
    /// <summary>
    /// Helper class for translating given tags into intra-text adornments.
    /// </summary>
    internal abstract class IntraTextAdornmentTagTransformer<TDataTag, TAdornment>
        : IntraTextAdornmentTagger<TDataTag, TAdornment>, IDisposable
        where TDataTag : ITag
        where TAdornment : UIElement
    {
        protected readonly ITagAggregator<TDataTag> _dataTagger;

        protected IntraTextAdornmentTagTransformer(IWpfTextView view, ITagAggregator<TDataTag> dataTagger)
            : base(view)
        {
            _dataTagger = dataTagger;

            _dataTagger.TagsChanged += HandleDataTagsChanged;
        }

        protected override IEnumerable<Tuple<SnapshotSpan, TDataTag>> GetAdorenmentData(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (IMappingTagSpan<TDataTag> dataTagSpan in _dataTagger.GetTags(spans))
            {
                NormalizedSnapshotSpanCollection dataTagSpans = dataTagSpan.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (dataTagSpans.Count != 1)
                    continue;

                yield return Tuple.Create(new SnapshotSpan(dataTagSpans[0].Start, 0), dataTagSpan.Tag);
            }
        }

        private void HandleDataTagsChanged(object sender, TagsChangedEventArgs args)
        {
            var changedSpans = args.Span.GetSpans(_view.TextBuffer.CurrentSnapshot);
            InvalidateSpans(changedSpans);
        }

        public virtual void Dispose()
        {
            _dataTagger.Dispose();
        }
    }
}
