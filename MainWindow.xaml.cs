using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleMNIST
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MNISTEvaluator _evaluator = new MNISTEvaluator();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void recognizeButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = GetHandWrittenImage();
            Tensor<float> imageData = ConvertImageToTensorData(bitmap);

            List<MNISTResult> results = _evaluator.Evaluate(imageData);

            // the result is sorted by confidence. so the first is the highest
            numberLabel.Text = results.FirstOrDefault()?.Digit.ToString() ?? "N/A";
        }

        private static Tensor<float> ConvertImageToTensorData(Bitmap image)
        {
            int width = image.Size.Width;
            int height = image.Size.Height;
            Tensor<float> tensor = new DenseTensor<float>(new[] { width, height }, true); // CNTK uses ColumnMajor layout
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var color = image.GetPixel(x, y);
                    float value = (color.R + color.G + color.B) / 3;

                    // Turn to black background and white digit like MNIST dataset
                    tensor[x, y] = (255 - value);
                }
            }
            return tensor;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Strokes.Clear();
            numberLabel.Text = "";
        }

        private Bitmap GetHandWrittenImage()
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96d, 96d, PixelFormats.Default);
            renderBitmap.Render(inkCanvas);

            int imageHeight = (int)Math.Sqrt(_evaluator.ExpectedImageInputSize);
            int imageWidth = imageHeight;

            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Flush();

                //get the bitmap bytes from the memory stream
                ms.Position = 0;
                bitmap = new Bitmap(Image.FromStream(ms));
            }

            bitmap = ResizeImage(bitmap, new System.Drawing.Size(imageWidth, imageHeight));
            return bitmap;
        }

        private static Bitmap ResizeImage(Bitmap imgToResize, System.Drawing.Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            }
            return b;
        }

        #region HideIcon
        protected override void OnSourceInitialized(EventArgs e)
        {
            const int GWL_EXSTYLE = -20;
            const int WS_EX_DLGMODALFRAME = 0x0001;
            const int SWP_NOSIZE = 0x0001;
            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOZORDER = 0x0004;
            const int SWP_FRAMECHANGED = 0x0020;

            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE |
                  SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
            int x, int y, int width, int height, uint flags);
        #endregion
    }
}
