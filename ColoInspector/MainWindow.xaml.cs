using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Point = System.Drawing.Point;

namespace ColoInspector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        public MainWindow()
        {
            InitializeComponent();
        }

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

        public Color BackColor { get; private set; }
        public Action<System.Windows.Media.SolidColorBrush> Action;

        public Color GetColorAt(Point location)
        {
            try
            {
                using (Graphics gdest = Graphics.FromImage(screenPixel))
                {
                    using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        IntPtr hSrcDC = gsrc.GetHdc();
                        IntPtr hDC = gdest.GetHdc();
                        int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                        gdest.ReleaseHdc();
                        gsrc.ReleaseHdc();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return screenPixel.GetPixel(0, 0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread insepctThread = new Thread(this.GetColorByCursorPos);
            insepctThread.Name = "farberkennungCursorThread";
            insepctThread.IsBackground = true;
            insepctThread.Start();
        }

        private void GetColorByCursorPos()
        {
            while (true)
            {
                Point cursor = new Point();

                GetCursorPos(ref cursor);

                var c = GetColorAt(cursor);

                System.Windows.Media.Color color = new System.Windows.Media.Color();

                color.A = c.A;
                color.R = c.R;
                color.B = c.B;
                color.G = c.G;

                var newBrush = new System.Windows.Media.SolidColorBrush(color);

                this.UpdateColorView(color);

                Thread.Sleep(1);
            }
        }

        private void UpdateColorView(System.Windows.Media.Color color)
        {
            if (Application.Current != null)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => this.UpdateColorView(color)));
                }
                else
                {
                    var solidColor = new System.Windows.Media.SolidColorBrush(color);
                    this.ColorShowName.Text = solidColor.ToString();
                    this.colorShow.Background = solidColor;
                }
            }
        }
    }
}