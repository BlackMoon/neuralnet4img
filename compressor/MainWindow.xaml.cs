using compressor.Neuro;
using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace compressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _rate    = 0.1;

        private int _nx         = 10;
        private int _ny         = 10;
        private int _radius     = 10;
        private int _iterations = 100;

        public MainWindow()
        {
            InitializeComponent();

            var prefs = new UserPreferences();
            this.Height = prefs.WindowHeight;
            this.Width = prefs.WindowWidth;
            this.Top = prefs.WindowTop;
            this.Left = prefs.WindowLeft;
            this.WindowState = prefs.WindowState;
        }

        private void PrepareParams(){

            double.TryParse(txtRate.Text, out _rate);

            int.TryParse(txtNX.Text, out _nx);
            int.TryParse(txtNY.Text, out _ny);
            int.TryParse(txtRadius.Text, out _radius);
            int.TryParse(txtIter.Text, out _iterations);
        }

        private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Точечные рисунки (*.bmp;*dib)|*.bmp;*.dib|JPEG (*.jpg;*.jpeg;*.jpe;*.jfif)|*.jpg;*.jpeg;*.jpe;*.jfif|TIFF (*.tif;*.tiff)|*.tif;*.tiff|PNG (*.png)|*.png|ICO (*.ico)|*.ico|Все файлы изображений|*.bmp;*.dib;*.jpg;*.jpeg;*.jpe;*.jfif;*.tif;*.tiff;*.png;*.ico|Все файлы|*.*";
            dlg.FilterIndex = 6;

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(dlg.FileName);
                    bi.EndInit();

                    imgOrig.Source = bi;
                    lblInfo.Text = dlg.SafeFileName + " (" + bi.PixelWidth + " x " + bi.PixelHeight + ")";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StartCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            PrepareParams();

            this.Cursor = Cursors.Wait;
            lblStatus.Text = "Обучение";

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;

            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);
           

            #region hidden

            // input
            double[] input = new double[4];

            for (int i = 0, j = 0; i < len; i += 4, j++)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];               
            }

            DistanceNetwork nt = new DistanceNetwork(4, _nx * _ny);
            SOMLearning trainer = new SOMLearning(nt);

            double fixedLearningRate = _rate / 10;
            double driftingLearningRate = fixedLearningRate * 9;
            
            // iterations
            int k = 0;
            
            Random rand = new Random();
            // loop
            while (true)
            {
                trainer.LearningRate = driftingLearningRate * (_iterations - k) / _iterations + fixedLearningRate;
                trainer.LearningRadius = (double)_radius * (_iterations - k) / _iterations;

                int i = rand.Next(H * W);
                input[0] = pixels[i];
                input[1] = pixels[i + 1];
                input[2] = pixels[i + 2];
                input[3] = pixels[i + 3];

                trainer.Run(input);


                // increase current iteration
                k++;

                // set current iteration's info
                //currentIterationBox.Text = i.ToString( );

                // stop ?
                if (k >= _iterations)
                    break;
            }

            stride = 4 * _nx;
            byte[] array = new byte[_ny * stride];
            Layer layer = nt[0];

            for (int y = 0; y < _ny; y++)
            {
                // for all pixels
                for (int x = 0; x < stride; x += 4)
                {
                    Neuron neuron = layer[y];
                    array[stride * y + x] = (byte)(byte.MaxValue * neuron[0]);
                    array[stride * y + x + 1] = (byte)(byte.MaxValue * neuron[1]);
                    array[stride * y + x + 2] = (byte)(byte.MaxValue * neuron[2]);
                    array[stride * y + x + 3] = (byte)(byte.MaxValue * neuron[3]);
                }
            }

            try
            {
                WriteableBitmap bm1 = new WriteableBitmap(_nx, _ny, bmp.DpiX, bmp.DpiY, bmp.Format, null);
                bm1.WritePixels(new Int32Rect(0, 0, _nx, _ny), array, stride, 0);

                imgMap.Source = bm1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion

            lblStatus.Text = "";
            this.Cursor = Cursors.Arrow;
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName an = assembly.GetName();

            MessageBox.Show("вер. " + an.Version, "О программе");
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var prefs = new UserPreferences
            {
                WindowLeft = this.Left,
                WindowTop = this.Top,
                WindowWidth = this.Width,
                WindowHeight = this.Height,
                WindowState = this.WindowState
            };
            prefs.Save();
        }

        private void txtNum_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {   
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.NumLock:
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.Back:
                case Key.OemComma:
                case Key.OemPeriod:
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand Start = new RoutedUICommand("Start", "Start", typeof(CustomCommands));
    }
}
