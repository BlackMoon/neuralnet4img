using compressor.Kohonen;
using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace compressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int DIM         = 0;

        private int _ns         = 3;
        private int _s          = 1;
        private int _nx         = 10;
        private int _ny         = 10;
        
        private int _iterations = 1000;

        private Network net;

        public MainWindow()
        {
            InitializeComponent();

            var prefs = new UserPreferences();
            this.Height = prefs.WindowHeight;
            this.Width = prefs.WindowWidth;
            this.Top = prefs.WindowTop;
            this.Left = prefs.WindowLeft;
            this.WindowState = prefs.WindowState;

            PrepareParams(false);            
        }

        private void PrepareParams(bool get = true){

            if (get)
            {
                // ns
                try
                {
                    _ns = Math.Max(1, Math.Min(50, int.Parse(txtNS.Text)));
                }
                catch
                {
                }

                // nx
                try
                {
                    _nx = Math.Max(1, Math.Min(1000, int.Parse(txtNX.Text)));
                }
                catch
                {
                }

                // ny
                try
                {
                    _ny = Math.Max(1, Math.Min(1000, int.Parse(txtNY.Text)));
                }
                catch
                {
                }             

                // iterations
                try
                {
                    _iterations = Math.Max(10, Math.Min(1000000, int.Parse(txtIter.Text)));
                }
                catch
                {
                }
            }
            else
            {   
                txtNX.Text = _nx.ToString();
                txtNY.Text = _ny.ToString();
                txtNS.Text = _ns.ToString();
                txtIter.Text = _iterations.ToString();
            }
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

        private void CompressCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            lblStatus.Text = "Сжатие";
            this.Cursor = Cursors.Wait;

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;

            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);           

             // restore
            byte[] array1 = new byte[H * stride];
            int offset = _ns * _s + 1;
            stride = 4 * W;
            
            double[] vA = new double[DIM];
            double[] vR = new double[DIM];
            double[] vG = new double[DIM];
            double[] vB = new double[DIM];

            int x, y;
            for (y = 0; y < H; y++)
            {
                for (x = 0; x < W; x++)
                {
                    int n = 0;
                    for (int j = y - _s; j <= y + _s; j++)
                    {
                        if (j >= 0 && j < H)
                        {
                            for (int i = 4 * (W * j + x - _s); i <= 4 * (W * j + x + _s); i += 4)
                            {
                                if (i >= 0 && i < 4 * H * W)
                                {
                                    vA[n] = pixels[i + 3];
                                    vR[n] = pixels[i + 2];
                                    vG[n] = pixels[i + 1];
                                    vB[n] = pixels[i];
                                }
                                n++;
                            }
                        }
                        else
                            n += _ns;
                    }

                    Neuron nA = net.SomA.FindBmu1(vA);
                    Neuron nR = net.SomR.FindBmu1(vR);
                    Neuron nG = net.SomG.FindBmu1(vG);
                    Neuron nB = net.SomB.FindBmu1(vB);

                    pixels[stride * y + x + 3] = (byte)nA[offset];
                    pixels[stride * y + x + 2] = (byte)nR[offset];
                    pixels[stride * y + x + 1] = (byte)nG[offset];
                    pixels[stride * y + x] = (byte)nB[offset];

                }
            }

            try
            {

                WriteableBitmap bm1 = new WriteableBitmap(W, H, bmp.DpiX, bmp.DpiY, bmp.Format, null);
                bm1.WritePixels(new Int32Rect(0, 0, W, H), pixels, stride, 0);

                imgReconstr.Source = bm1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            lblStatus.Text = "";
            this.Cursor = Cursors.Arrow;
        }

        private void StartCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            PrepareParams();

            lblStatus.Text = "Обучение";
            this.Cursor = Cursors.Wait;

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;

            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);

            
            if (_ns % 2 == 0) 
                _ns--;
            
            _s = (_ns - 1) / 2;
            
            DIM = _ns * _ns;            
            int NW = _ns * _nx;
            int NH = _ns * _ny;

            int k, x, y;

            net = new Network(_nx, _ny, DIM);
            Random rand = new Random();

            double[] vA = new double[DIM];
            double[] vR = new double[DIM];
            double[] vG = new double[DIM];
            double[] vB = new double[DIM];

            for (k = 0; k < _iterations; k++)
            {
                x = _s + (int)Math.Floor(rand.NextDouble() * (W - _ns));
                y = _s + (int)Math.Floor(rand.NextDouble() * (H - _ns));

                

                int i, j, n = 0;

                for (j = y - _s; j <= y + _s; j++)
                {
                    for (i = 4 * (W * j + x - _s); i <= 4 * (W * j + x + _s); i += 4)
                    {
                        vA[n] = pixels[i + 3];
                        vR[n] = pixels[i + 2];
                        vG[n] = pixels[i + 1];
                        vB[n] = pixels[i];

                        n++;
                    }                                     
                }

                net.SomA.Learn(vA);
                net.SomR.Learn(vR);
                net.SomG.Learn(vG);
                net.SomB.Learn(vB); 
            }

            // map
            stride = 4 * NW;
            byte[] array = new byte[NH * stride];

            for (y = 0; y < _ny; y++)
            {
                for (x = 0; x < _nx; x++)
                {
                    Neuron nA = net.SomA[y, x];
                    Neuron nR = net.SomR[y, x];
                    Neuron nG = net.SomG[y, x];
                    Neuron nB = net.SomB[y, x];
                                        
                    int x1 = (2 * _s + 1) * x + _s;
                    int y1 = (2 * _s + 1) * y + _s;

                    k = 0;
                    for (int j = y1 - _s; j <= y1 + _s; j++)
                    {
                        for (int i = 4 * (NW * j + x1 - _s); i <= 4 * (NW * j + x1 + _s); i += 4)
                        {
                            array[i + 3] = (byte)nA[k];
                            array[i + 2] = (byte)nR[k];
                            array[i + 1] = (byte)nG[k];
                            array[i] = (byte)nB[k];

                            k++;
                        }
                    }                        
                }
            }

            try
            {
                System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

                WriteableBitmap bm1 = new WriteableBitmap(NW, NH, g.DpiX, g.DpiY, bmp.Format, null);
                bm1.WritePixels(new Int32Rect(0, 0, NW, NH), array, stride, 0);

                imgMap.Source = bm1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
           
            #region map
            /*

            // input
            double[] input = new double[4];

            for (int i = 0; i < len; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];               
            }

            Neuron.RandRange = new Range<double>(0, 255);
            nt = new DistanceNetwork(4, _nx * _ny);            

            SOMLearning trainer = new SOMLearning(nt, _nx, _ny);

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
                
                // stop ?
                if (k >= _iterations)
                    break;
            }

            stride = 4 * _nx;
            byte[] array = new byte[_ny * stride];
            Layer layer = nt[0];

            for (int y = 0, i = 0; y < _ny; y++)
            {
                // for all pixels
                for (int x = 0; x < stride; i++, x += 4)
                {
                    Neuron neuron = layer[i];
                    array[stride * y + x] = (byte)Math.Max(0, Math.Min(255, neuron[0]));
                    array[stride * y + x + 1] = (byte)Math.Max(0, Math.Min(255, neuron[1]));
                    array[stride * y + x + 2] = (byte)Math.Max(0, Math.Min(255, neuron[2]));
                    array[stride * y + x + 3] = (byte)Math.Max(0, Math.Min(255, neuron[3]));
                }
            }
                 * */

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
        public static readonly RoutedUICommand Compress = new RoutedUICommand("Compress", "Compress", typeof(CustomCommands));
        public static readonly RoutedUICommand Start = new RoutedUICommand("Start", "Start", typeof(CustomCommands));
    }
}
