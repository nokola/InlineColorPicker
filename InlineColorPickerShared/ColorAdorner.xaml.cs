using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;
using EasyPainter.Imaging.Silverlight;
using System.Diagnostics;
using System.Windows.Threading;

namespace InlineColorPicker {
    /// <summary>
    /// Interaction logic for ColorAdorner.xaml
    /// </summary>
    public partial class ColorAdorner : UserControl {
        ColorTag _tag = null;
        DispatcherTimer _timer = new DispatcherTimer();
        public ColorAdorner(ColorTag tag) {
            InitializeComponent();
            Update(tag);
            _timer.Interval = _updateColorSpan;
            _timer.Tick += new EventHandler(_timer_Tick);
        }

        TimeSpan _updateColorSpan = TimeSpan.FromMilliseconds(200);
        void _timer_Tick(object sender, EventArgs e) {
            if ((DateTime.Now - _lastColorUpdateTime) <= _updateColorSpan) return;
            ApplyColorIfNeeded();
        }

        private ColorInfo _currentColor = new ColorInfo() { Color = Colors.Red, WasSpecifiedWithAlpha = true };
        
        /// <summary>
        /// 
        /// </summary>
        internal ColorInfo ColorInfo {
            get { return _currentColor; }
            set {
                _currentColor = value;
                if (_currentColor.Color == null) {
                    rectColor.Fill = null;
                    line1.Visibility = Visibility.Collapsed;
                    line2.Visibility = Visibility.Collapsed;
                }
                else 
                {
                    rectColor.Fill = new SolidColorBrush(_currentColor.Color.Value);
                    line1.Visibility = Visibility.Visible;
                    line2.Visibility = Visibility.Visible;
                }
            }
        }

        public void Update(ColorTag tag) {
            _tag = tag;
            this.ColorInfo = _tag.ColorInfo;
        }

        static ColorPopup _popup = null;

        private void InitPopupIfNeeded() {
            if (_popup != null) return;
            _popup = new ColorPopup();
            _popup.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            _popup.HorizontalOffset = 12;
            _popup.VerticalOffset = 12;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            InitPopupIfNeeded();

            _popup.PlacementTarget = this;
            if (_currentColor == null) {
                _popup.picker.ColorInfo = ColorInfo.White;
            }
            else {
                _popup.picker.ColorInfo = _currentColor;
            }
            _mustApplyColor = false;
            _popup.picker.ColorSelected += picker_ColorSelected;
            _popup.Closed += _popup_Closed;

            _popup.IsOpen = true;
            _popup.picker.HexValue.Focus();
            _timer.Start();
        }

        bool _mustApplyColor = false;
        DateTime _lastColorUpdateTime = DateTime.MinValue;

        void ApplyColorIfNeeded() {
            if (!_mustApplyColor) return;
            _mustApplyColor = false;
            ITextBuffer buffer = _tag.TrackingSpan.TextBuffer;
            SnapshotSpan span = _tag.TrackingSpan.GetSpan(buffer.CurrentSnapshot);
            buffer.Replace(span, ColorSpace.GetHexCodeOrName(_tag.ColorInfo));
        }

        void picker_ColorSelected(ColorInfo info) {
            _tag.ColorInfo = info;
            _mustApplyColor = true;
            if ((DateTime.Now - _lastColorUpdateTime) > _updateColorSpan) {
                ApplyColorIfNeeded();
            }
            _lastColorUpdateTime = DateTime.Now;
            //Debug.WriteLine("Span: " + span.ToString());
            //_tag.TrackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive, TrackingFidelityMode.Forward);
            //Debug.WriteLine("Tracking: " + span.ToString());
        }

        void _popup_Closed(object sender, EventArgs e) {
            _timer.Stop();
            _popup.Closed -= _popup_Closed;
            _popup.picker.ColorSelected -= picker_ColorSelected;
            ApplyColorIfNeeded();
        }
    }
}
