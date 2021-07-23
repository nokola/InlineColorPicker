using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EasyPainter.Imaging.Silverlight {
    public partial class AlphaSelectControl : UserControl {

        public delegate void AlphaChangedHandler(byte newAlpha);
        public event AlphaChangedHandler AlphaChanged;

        public AlphaSelectControl() {
            InitializeComponent();
        }

        public Color DisplayColor {
            get { return displayColor.Color; }
            set {
                Color color = value;
                color.A = 255;
                displayColor.Color = color;
                UpdateSelectionForAlpha(value.A);
            }
        }

        private UIElement _mouseCapture = null;

        private void rectMonitor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            rectMonitor.CaptureMouse();
            _mouseCapture = rectMonitor;
            UpdateSelection(e.GetPosition((UIElement)sender).Y);
        }

        private void rectMonitor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            rectMonitor.ReleaseMouseCapture();
        }

        private void rectMonitor_MouseMove(object sender, MouseEventArgs e) {
            if (_mouseCapture != rectMonitor) return;
            double yPos = e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos > rectMonitor.ActualHeight) yPos = rectMonitor.ActualHeight;
            UpdateSelection(yPos);
        }

        private void rectMonitor_LostMouseCapture(object sender, MouseEventArgs e) {
            _mouseCapture = null;
        }

        private void UpdateSelectionForAlpha(int alpha) {
            Canvas.SetTop(rectMarker, ((255-alpha) * rectMonitor.ActualHeight)/255.0);
        }

        private void UpdateSelection(double yPos) {
            byte alpha = (byte)(255-(yPos * 255 / rectMonitor.ActualHeight));
            Canvas.SetTop(rectMarker, yPos);
            if (AlphaChanged != null) AlphaChanged(alpha);
        }

    }
}
