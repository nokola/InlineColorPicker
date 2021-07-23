using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;

namespace InlineColorPicker {
    /// <summary>
    /// Provides color swatch adornments in place of color constants.
    /// </summary>
    internal sealed class ColorAdornmentTagger : IntraTextAdornmentTagTransformer<ColorTag, ColorAdorner> {
        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<ColorTag>> colorTagger) {
            return view.Properties.GetOrCreateSingletonProperty<ColorAdornmentTagger>(
                () => new ColorAdornmentTagger(view, colorTagger.Value));
        }

        private ColorAdornmentTagger(IWpfTextView view, ITagAggregator<ColorTag> colorTagger)
            : base(view, colorTagger) { }

        protected override ColorAdorner CreateAdornment(ColorTag dataTag) {
            return new ColorAdorner(dataTag);
        }

        protected override void UpdateAdornment(ColorAdorner adornment, ColorTag dataTag) {
            adornment.Update(dataTag);
        }

        public override void Dispose() {
            base.Dispose();
            _view.Properties.RemoveProperty(typeof(ColorAdornmentTagger));
        }

    }

}
