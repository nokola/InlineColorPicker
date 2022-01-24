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
                SetTexts(ColorSpace.GetHexCodeOrName(m_selectedColor));
            }
        }

        private void SetTexts(string text)
        {
            HexValue.Text = text;
            byte a, r, g, b;
            if (GetArgb(text, out a, out r, out g, out b))
            {
                AlphaValue.Text = a.ToString();
                RedValue.Text = r.ToString();
                GreenValue.Text = g.ToString();
                BlueValue.Text = b.ToString();
            }

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
            rectHueMonitor.MouseWheel += rectHueMonitor_MouseWheel;

            rectLuminosityMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectLuminosityMonitor_MouseLeftButtonDown);
            rectLuminosityMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectLuminosityMonitor_MouseLeftButtonUp);
            rectLuminosityMonitor.LostMouseCapture += new MouseEventHandler(rectLuminosityMonitor_LostMouseCapture);
            rectLuminosityMonitor.MouseMove += new MouseEventHandler(rectLuminosityMonitor_MouseMove);
            rectLuminosityMonitor.MouseWheel += rectLuminosityMonitor_MouseWheel;

            rectSaturationMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectSaturationMonitor_MouseLeftButtonDown);
            rectSaturationMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectSaturationMonitor_MouseLeftButtonUp);
            rectSaturationMonitor.LostMouseCapture += new MouseEventHandler(rectSaturationMonitor_LostMouseCapture);
            rectSaturationMonitor.MouseMove += new MouseEventHandler(rectSaturationMonitor_MouseMove);
            rectSaturationMonitor.MouseWheel += rectSaturationMonitor_MouseWheel;

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

        void rectHueMonitor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            GeneralTransform trans = rectHueMonitorParent.TransformToDescendant(HueSelector);
            Point p = trans.Transform(new Point(0, -4));
            int yPos = (int)Math.Abs(p.Y);
            if (yPos < 0) yPos = 0;
            if (yPos >= rectHueMonitor.ActualHeight) yPos = (int)rectHueMonitor.ActualHeight - 1;
            if (e.Delta > 0)
			{
                yPos -= 1;
			}
            else if (e.Delta < 0)
			{
                yPos += 1;
            }
            UpdateHueSelection(yPos);
        }

        void rectLuminosityMonitor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            GeneralTransform trans = rectLuminosityMonitorParent.TransformToDescendant(LuminositySelector);
            Point p = trans.Transform(new Point(0, -4));
            int yPos = (int)Math.Abs(p.Y);
            if (yPos < 0) yPos = 0;
            if (yPos >= rectLuminosityMonitor.ActualHeight) yPos = (int)rectLuminosityMonitor.ActualHeight - 1;
            if (e.Delta > 0)
            {
                yPos -= 1;
            }
            else if (e.Delta < 0)
            {
                yPos += 1;
            }
            UpdateLuminositySelection(yPos);
        }

        void rectSaturationMonitor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            GeneralTransform trans = rectSaturationMonitorParent.TransformToDescendant(SaturationSelector);
            Point p = trans.Transform(new Point(0, 0));
            int xPos = (int)Math.Abs(p.X);
            if (xPos < 0) xPos = 0;
            if (xPos >= rectSaturationMonitor.ActualWidth) xPos = (int)rectSaturationMonitor.ActualWidth - 1;
            if (e.Delta > 0)
            {
                xPos += 1;
            }
            else if (e.Delta < 0)
            {
                xPos -= 1;
            }
            UpdateSaturationSelection(xPos);
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

        void rectLuminosityMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }

        void rectSaturationMonitor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _mouseCapture = null;
        }

        void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectHueMonitor.CaptureMouse();
            _mouseCapture = rectHueMonitor;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateHueSelection(yPos);
        }

        void rectLuminosityMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectLuminosityMonitor.CaptureMouse();
            _mouseCapture = rectLuminosityMonitor;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateLuminositySelection(yPos);
        }

        void rectSaturationMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            rectSaturationMonitor.CaptureMouse();
            _mouseCapture = rectSaturationMonitor;
            int xPos = (int)e.GetPosition((UIElement)sender).X;
            UpdateSaturationSelection(xPos);
        }

        void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectHueMonitor.ReleaseMouseCapture();
        }

        void rectLuminosityMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectLuminosityMonitor.ReleaseMouseCapture();
        }

        void rectSaturationMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            rectSaturationMonitor.ReleaseMouseCapture();
        }

        void rectHueMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectHueMonitor) return;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos >= rectHueMonitor.ActualHeight) yPos = (int)rectHueMonitor.ActualHeight - 1;
            UpdateHueSelection(yPos);
        }

        void rectLuminosityMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectLuminosityMonitor) return;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            if (yPos < 0) yPos = 0;
            if (yPos >= rectLuminosityMonitor.ActualHeight) yPos = (int)rectLuminosityMonitor.ActualHeight - 1;
            UpdateLuminositySelection(yPos);
        }

        void rectSaturationMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCapture != rectSaturationMonitor) return;
            int xPos = (int)e.GetPosition((UIElement)sender).X;
            if (xPos < 0) xPos = 0;
            if (xPos >= rectSaturationMonitor.ActualWidth) xPos = (int)rectSaturationMonitor.ActualWidth - 1;
            UpdateSaturationSelection(xPos);
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
            Color maxLum = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, xComponent, 1);
            maxLum.A = a;
            Color maxSat = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, 1, yComponent);
            maxSat.A = a;
            m_selectedColor.Color = c;
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor.Color.Value);
            SetTexts(ColorSpace.GetHexCodeOrName(m_selectedColor));

            ctlAlphaSelect.DisplayColor = m_selectedColor.Color.Value;
            rectLuminosityMonitor.Fill = new LinearGradientBrush(Color.FromArgb(255, maxLum.R, maxLum.G, maxLum.B), Color.FromArgb(255, 0, 0, 0), 90);
            rectSaturationMonitor.Fill = new LinearGradientBrush(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, maxSat.R, maxSat.G, maxSat.B), 0);
            LuminositySelector.Margin = new Thickness(0, yPos - (LuminositySelector.ActualHeight / 2), 0, 0);
            SaturationSelector.Margin = new Thickness(xPos + (SaturationSelector.ActualWidth / 2), 0, 0, 0);
            ColorSelected?.Invoke(m_selectedColor);
        }

        private void UpdateHueSelection(int yPos)
        {
            int huePos = (int)(yPos / rectHueMonitor.ActualHeight * 255);
            int gradientStops = 6;//what is this for?
            Color c = ColorSpace.GetColorFromPosition(huePos * gradientStops);
            rectSample.Fill = new SolidColorBrush(c);
            HueSelector.Margin = new Thickness(0, yPos - (HueSelector.ActualHeight / 2), 0, 0);
            m_selectedHue = (float)(yPos / rectHueMonitor.ActualHeight) * 360;
            UpdateSample(m_sampleX, m_sampleY);
        }

        private void UpdateLuminositySelection(int yPos)
        {
            UpdateSample(m_sampleX, yPos);
        }

        private void UpdateSaturationSelection(int xPos)
        {
            UpdateSample(xPos, m_sampleY);
        }

        private void ctlAlphaSelect_AlphaChanged(byte newAlpha)
        {
            Color c = m_selectedColor.Color.Value;
            c.A = newAlpha;
            m_selectedColor.Color = c;
            SetTexts(ColorSpace.GetHexCodeOrName(m_selectedColor));
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor.Color.Value);

            ColorSelected?.Invoke(m_selectedColor);
        }

        private void Byte_Value_GotFocus(object sender, MouseButtonEventArgs e)
		{
            if (sender != null) ((TextBox)sender).SelectAll();
		}

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = HexValue.Text;
            if (text == ColorSpace.GetHexCodeOrName(m_selectedColor)) return;
            byte a, r, g, b;
            if (!GetArgb(text, out a, out r, out g, out b)) return; // invalid color

            UpdateOnColorChanged(a, r, g, b);
        }

        private void ByteValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (AlphaValue == null || RedValue == null || GreenValue == null || BlueValue == null) return;
                byte a, r, g, b;
				if (byte.TryParse(AlphaValue.Text, out a))
				    if (byte.TryParse(RedValue.Text, out r))
				        if (byte.TryParse(GreenValue.Text, out g))
			            	if (byte.TryParse(BlueValue.Text, out b))
                            {
                                byte[] data = { a, r, g, b };
                                string hex = BitConverter.ToString(data).Replace("-", string.Empty);
                                HexValue.Text = "#" + hex;
                                //changing HexValue will trigger update.
                                //UpdateOnColorChanged(a, r, g, b);
                            }
            }
            catch { };
        }

        private void ByteValue_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            TextBox txt = sender as TextBox;
            byte value;
            if (e.Delta > 0)
			{
                if (byte.TryParse(txt.Text, out value))
				{
                    value += 1;
                    if (value > 255) value = 255;
                    txt.Text = value.ToString();
                }
			}
            else if (e.Delta < 0)
			{
                if (byte.TryParse(txt.Text, out value))
                {
                    value -= 1;
                    if (value < 0) value = 0;
                    txt.Text = value.ToString();
                }
            }
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

            LuminositySelector.Margin = new Thickness(0, yPos - (LuminositySelector.ActualHeight / 2), 0, 0);
            SaturationSelector.Margin = new Thickness(xPos + (SaturationSelector.ActualWidth / 2), 0, 0, 0);

            // Update Hue locator
            HueSelector.Margin = new Thickness(0, (h * rectHueMonitor.ActualHeight) - (HueSelector.ActualHeight / 2), 0, 0);

            // update alpha selector
            if (ctlAlphaSelect != null)
            {
                // TODO: fix - when null should be assigned later
                ctlAlphaSelect.DisplayColor = m_selectedColor.Color.Value;
            }

            // update alpha byte
            if (AlphaValue != null)
            {
                AlphaValue.Text = a.ToString();
            }

            // update alpha byte
            if (RedValue != null)
            {
                RedValue.Text = r.ToString();
            }

            // update alpha byte
            if (GreenValue != null)
            {
                GreenValue.Text = g.ToString();
            }

            // update alpha byte
            if (BlueValue != null)
            {
                BlueValue.Text = b.ToString();
            }

            float yComponent = 1 - (float)(yPos / rectSample.ActualHeight);
            float xComponent = (float)(xPos / rectSample.ActualWidth);
            Color c = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, (float)xComponent, (float)yComponent);
            c.A = a;
            Color maxLum = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, (float)xComponent, 1.0f);
            maxLum.A = a;
            Color maxSat = ColorSpace.ConvertHsvToRgb((float)m_selectedHue, 1.0f, (float)yComponent);
            maxSat.A = a;
            rectLuminosityMonitor.Fill = new LinearGradientBrush(Color.FromArgb(255, maxLum.R, maxLum.G, maxLum.B), Color.FromArgb(255, 0, 0, 0), 90);
            rectSaturationMonitor.Fill = new LinearGradientBrush(Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, maxSat.R, maxSat.G, maxSat.B), 0);

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
    public class SelectAllFocusBehavior
    {
        public static bool GetEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableProperty);
        }
        public static void SetEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableProperty, value);
        }
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(SelectAllFocusBehavior), new PropertyMetadata(false, OnEnableChanged));
        private static void OnEnableChanged(object d, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = d as FrameworkElement;
            if (frameworkElement == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                frameworkElement.GotFocus += SelectAll;
                frameworkElement.PreviewMouseDown += IgnoreMouseButton;
            }
            else
            {
                frameworkElement.GotFocus -= SelectAll;
                frameworkElement.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement is TextBox)
                ((TextBox)frameworkElement).SelectAll();
            else if (frameworkElement is PasswordBox)
                ((PasswordBox)frameworkElement).SelectAll();
        }

        private static void IgnoreMouseButton
                (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || frameworkElement.IsKeyboardFocusWithin) return;
            e.Handled = true;
            frameworkElement.Focus();
        }
    }
    public static class NumericByteValueOnlyBehavior
    {
        public static bool GetIsNumericByteValueOnly(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsNumericByteValueOnlyProperty);
        }
        public static void SetIsNumericByteValueOnly(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumericByteValueOnlyProperty, value);
        }
        public static readonly DependencyProperty IsNumericByteValueOnlyProperty = DependencyProperty.RegisterAttached("IsNumericByteValueOnly", typeof(bool), typeof(NumericByteValueOnlyBehavior), new PropertyMetadata(false, OnIsNumericByteValueOnlyChanged));
        private static void OnIsNumericByteValueOnlyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox txt = (TextBox)sender;
                txt.TextChanged += Txt_TextChanged;
            }
        }
        private static void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox txt = (TextBox)sender;
                foreach (TextChange item in e.Changes)
                {
                    if (item.AddedLength != 0)
                    {
                        foreach (char chr in txt.Text.Substring(item.Offset, item.AddedLength).ToCharArray())
                        {
                            bool scrap = false;
                            if (chr != '1'
                                && chr != '2'
                                && chr != '3'
                                && chr != '4'
                                && chr != '5'
                                && chr != '6'
                                && chr != '7'
                                && chr != '8'
                                && chr != '9'
                                && chr != '0'
                                && chr != '\n'
                                && chr != '\r')
                            {
                                scrap = true;
                            }
                            if (scrap)
                            {
                                int temp = txt.CaretIndex;
                                txt.Text = txt.Text.Remove(item.Offset, item.AddedLength);
                                txt.CaretIndex = temp - 1;
                            }
                        }
                    }
                }
                if (txt.Text.Length > 3)
                {
                    int temp = txt.CaretIndex;
                    txt.Text = txt.Text.Substring(0, 3);
                    if (temp > 3) temp = 3;
                    txt.CaretIndex = temp;
                }
                if (int.TryParse(txt.Text, out int i))
                {
                    if (i > 255)
                    {
                        int temp = txt.CaretIndex;
                        txt.Text = 255.ToString();
                        if (temp > 3) temp = 3;
                        txt.CaretIndex = temp;
                    }
                    if (i < 0)
                    {
                        int temp = txt.CaretIndex;
                        txt.Text = 0.ToString();
                        if (temp > 3) temp = 3;
                        txt.CaretIndex = temp;
                    }
                }
            }
            catch { }
        }
    }
}
