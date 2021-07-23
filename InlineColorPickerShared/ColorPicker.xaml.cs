// 1. added mouse capture capability
// 2. fixed bugs around edge cases (e.g. 0000000 and FFFFFFF colors)
// 3. added Alpha picker
// 4. Added ability to type in hex color
// 5. improved speed and layout

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using InlineColorPicker;
using System.Globalization;

namespace EasyPainter.Imaging.Silverlight
{
    public partial class ColorPicker : UserControl
    {
        float m_selectedHue;
        double m_sampleX;
        double m_sampleY;
        private ColorInfo m_selectedColor = new ColorInfo() { Color = Colors.White, WasSpecifiedWithAlpha = true };
        public delegate void ColorSelectedHandler(ColorInfo info);
        public event ColorSelectedHandler ColorSelected;
        private UIElement _mouseCapture = null;

        internal ColorInfo ColorInfo
        {
            get { return m_selectedColor; }
            set
            {
                m_selectedColor = value;
                Color c = m_selectedColor.Color.Value;
                UpdateOnColorChanged(c.A, c.R, c.G, c.B);
                SetHexText(ColorSpace.GetHexCodeOrName(m_selectedColor));
            }
        }

        private void SetHexText(string text)
        {
            HexValue.Text = text;

            if (text.StartsWith("#"))
            {
                HexValue.Select(1, text.Length - 1);
            }
            else
            {
                HexValue.SelectAll();
            }
        }

        public ColorPicker()
        {
            InitializeComponent();
            rectHueMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonDown);
            rectHueMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonUp);
            rectHueMonitor.LostMouseCapture += new MouseEventHandler(rectHueMonitor_LostMouseCapture);
            rectHueMonitor.MouseMove += new MouseEventHandler(rectHueMonitor_MouseMove);

            rectSampleMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonDown);
            rectSampleMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonUp);
            rectSampleMonitor.LostMouseCapture += new MouseEventHandler(rectSampleMonitor_LostMouseCapture);
            rectSampleMonitor.MouseMove += new MouseEventHandler(rectSampleMonitor_MouseMove);

            ctlAlphaSelect.AlphaChanged += new AlphaSelectControl.AlphaChangedHandler(ctlAlphaSelect_AlphaChanged);
            m_selectedHue = 0;
            m_sampleX = 0;
            m_sampleY = 0;
            this.LayoutUpdated += new EventHandler(ColorPicker_LayoutUpdated);
        }

        bool _firstTime = true;
        void ColorPicker_LayoutUpdated(object sender, EventArgs e)
        {
            if (_firstTime)
            {
                _firstTime = false;
                Color c = m_selectedColor.Color.Value;
                UpdateOnColorChanged(c.A, c.R, c.G, c.B);
            }
        }

        void rectSampleMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }

        void rectHueMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }

        void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectHueMonitor.CaptureMouse();
            _mouseCapture = rectHueMonitor;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateSelection(yPos);
        }

        void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectHueMonitor.ReleaseMouseCapture();
        }

        void rectHueMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectHueMonitor) return;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos >= rectHueMonitor.ActualHeight) yPos = (int)rectHueMonitor.ActualHeight - 1;
            UpdateSelection(yPos);
        }

        void rectSampleMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectSampleMonitor.CaptureMouse();
            _mouseCapture = rectSampleMonitor;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            UpdateSample(m_sampleX, m_sampleY);
        }

        void rectSampleMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectSampleMonitor.ReleaseMouseCapture();
        }

        void rectSampleMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectSampleMonitor) return;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            if (m_sampleY < 0) m_sampleY = 0;
            if (m_sampleY > rectSampleMonitor.ActualHeight) m_sampleY = (int)rectSampleMonitor.ActualHeight;
            if (m_sampleX < 0) m_sampleX = 0;
            if (m_sampleX > rectSampleMonitor.ActualWidth) m_sampleX = (int)rectSampleMonitor.ActualWidth;
            UpdateSample(m_sampleX, m_sampleY);
        }

        private void UpdateSample(double xPos, double yPos)
        {

            SampleSelector.Margin = new Thickness(xPos - (SampleSelector.Height / 2), yPos - (SampleSelector.Height / 2), 0, 0);

            float yComponent = 1 - (float)(yPos / rectSample.ActualHeight);
            float xComponent = (float)(xPos / rectSample.ActualWidth);

            byte a = m_selectedColor.Color.Value.A;
            Color c = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, xComponent, yComponent);
            c.A = a;
            m_selectedColor.Color = c;
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor.Color.Value);
            SetHexText(ColorSpace.GetHexCodeOrName(m_selectedColor));

            ctlAlphaSelect.DisplayColor = m_selectedColor.Color.Value;
            ColorSelected?.Invoke(m_selectedColor);
        }

        private void UpdateSelection(int yPos)
        {
            int huePos = (int)(yPos / rectHueMonitor.ActualHeight * 255);
            int gradientStops = 6;
            Color c = ColorSpace.GetColorFromPosition(huePos * gradientStops);
            rectSample.Fill = new SolidColorBrush(c);
            HueSelector.Margin = new Thickness(0, yPos - (HueSelector.ActualHeight / 2), 0, 0);
            m_selectedHue = (float)(yPos / rectHueMonitor.ActualHeight) * 360;
            UpdateSample(m_sampleX, m_sampleY);
        }

        private void ctlAlphaSelect_AlphaChanged(byte newAlpha)
        {
            Color c = m_selectedColor.Color.Value;
            c.A = newAlpha;
            m_selectedColor.Color = c;
            SetHexText(ColorSpace.GetHexCodeOrName(m_selectedColor));
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor.Color.Value);

            ColorSelected?.Invoke(m_selectedColor);
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = HexValue.Text;
            if (text == ColorSpace.GetHexCodeOrName(m_selectedColor)) return;
            byte a, r, g, b;
            if (!GetArgb(text, out a, out r, out g, out b)) return; // invalid color

            UpdateOnColorChanged(a, r, g, b);
        }

        private void UpdateOnColorChanged(byte a, byte r, byte g, byte b)
        {
            m_selectedColor.Color = Color.FromArgb(a, r, g, b);

            double h, s, v;
            ColorSpace.ConvertRgbToHsv(r / 255.0, g / 255.0, b / 255.0, out h, out s, out v);

            // update selected color
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor.Color.Value);

            // update Saturation and Value locator
            double xPos = s * rectSample.ActualWidth;
            double yPos = (1 - v) * rectSample.ActualHeight;
            m_sampleX = xPos;
            m_sampleY = yPos;
            SampleSelector.Margin = new Thickness(xPos - (SampleSelector.Height / 2), yPos - (SampleSelector.Height / 2), 0, 0);

            m_selectedHue = (float)h;
            h /= 360;
            const int gradientStops = 6;
            rectSample.Fill = new SolidColorBrush(ColorSpace.GetColorFromPosition(((int)(h * 255)) * gradientStops));

            // Update Hue locator
            HueSelector.Margin = new Thickness(0, (h * rectHueMonitor.ActualHeight) - (HueSelector.ActualHeight / 2), 0, 0);

            // update alpha selector
            if (ctlAlphaSelect != null)
            {
                // TODO: fix - when null should be assigned later
                ctlAlphaSelect.DisplayColor = m_selectedColor.Color.Value;
            }

            ColorSelected?.Invoke(m_selectedColor);
        }

        private bool GetArgb(string hexColorOrName, out byte a, out byte r, out byte g, out byte b)
        {
            string text;
            if (!ColorSpace.HexFromColorUpperCase.TryGetValue(hexColorOrName.ToUpperInvariant(), out text))
            {
                text = hexColorOrName;
            }

            a = r = b = g = 0;
            string strA, strR, strG, strB;
            if (text.Length == 9)
            {
                strA = text.Substring(1, 2);
                strR = text.Substring(3, 2);
                strG = text.Substring(5, 2);
                strB = text.Substring(7, 2);
            }
            else if (text.Length == 7)
            {
                strA = "ff";
                strR = text.Substring(1, 2);
                strG = text.Substring(3, 2);
                strB = text.Substring(5, 2);
            }
            else
            {
                return false;
            }

            if (!Byte.TryParse(strA, NumberStyles.HexNumber, null, out a)) return false;
            if (!Byte.TryParse(strR, NumberStyles.HexNumber, null, out r)) return false;
            if (!Byte.TryParse(strG, NumberStyles.HexNumber, null, out g)) return false;
            if (!Byte.TryParse(strB, NumberStyles.HexNumber, null, out b)) return false;
            return true;
        }
    }
}
