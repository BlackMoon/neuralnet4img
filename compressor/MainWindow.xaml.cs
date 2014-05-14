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
            if (net == null || !net.Trained)
            {
                MessageBox.Show("Сеть не обучена!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblStatus.Text = "Сжатие";
            lblStatus.Refresh();

            this.Cursor = Cursors.Wait;

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;

            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);           

             // restore
            byte[] array = new byte[H * stride];
            int offset = _ns * _s + 1;
            
            int x, y;
            for (y = 0; y < H; y++)
            {
                for (x = 0; x < W; x++)
                {
                    double[] vA = new double[DIM];
                    double[] vR = new double[DIM];
                    double[] vG = new double[DIM];
                    double[] vB = new double[DIM];
                    
                    int n = 0;
                    // окрестность точки
                    for (int j = y - _s; j <= y + _s; j++)
                    {
                        if (j >= 0 && j < H)
                        {
                            for (int i = 4 * (W * j + x - _s); i <= 4 * (W * j + x + _s); i += 4)
                            {
                                if (i >= 4 * j * W && i < 4 * (j + 1) * W)
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

                    // поиск наиболее близкого по весу нейрона
                    Neuron nA = net.SomA.GetWinner(vA);
                    Neuron nR = net.SomR.GetWinner(vR);
                    Neuron nG = net.SomG.GetWinner(vG);
                    Neuron nB = net.SomB.GetWinner(vB);
                    
                    // запись в сжатое изображение
                    array[stride * y + 4 * x + 3] = (byte)nA[offset];
                    array[stride * y + 4 * x + 2] = (byte)nR[offset];
                    array[stride * y + 4 * x + 1] = (byte)nG[offset];
                    array[stride * y + 4 * x] = (byte)nB[offset];
                }

                lblProgr.Text = "Выполнено: " + Math.Round((double)100 * y / H).ToString() + "%";
                lblProgr.Refresh();
            }

            try
            {
                WriteableBitmap wbm = new WriteableBitmap(W, H, bmp.DpiX, bmp.DpiY, bmp.Format, null);
                wbm.WritePixels(new Int32Rect(0, 0, W, H), array, stride, 0);

                imgCompress.Source = wbm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            lblProgr.Text = "Готово";
            lblStatus.Text = "";
            this.Cursor = Cursors.Arrow;
        }

        private void StartCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            PrepareParams();

            lblStatus.Text = "Обучение";
            lblStatus.Refresh();

            this.Cursor = Cursors.Wait;

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;

            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);
            
            if (_ns % 2 == 0) 
                _ns--;                              // нужен нечетн.
            
            _s = (_ns - 1) / 2;
            
            DIM = _ns * _ns;                        // размер нейрона
            int NW = _ns * _nx;                     // ширина карты Кохонена
            int NH = _ns * _ny;                     // высота карты Кохонена

            net = new Network(_nx, _ny, DIM);

            int k, x, y;
            Random rand = new Random();
                  
            for (k = 0; k < _iterations; k++)
            {
                // рандомная точка
                x = _s + (int)Math.Floor(rand.NextDouble() * (W - _ns));
                y = _s + (int)Math.Floor(rand.NextDouble() * (H - _ns));

                // входые (input) - ARGB
                double[] vA = new double[DIM];
                double[] vR = new double[DIM];
                double[] vG = new double[DIM];
                double[] vB = new double[DIM];

                int i, j, n = 0;
                // окрестность точки
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

                net.SomA.Train(vA);
                net.SomR.Train(vR);
                net.SomG.Train(vG);
                net.SomB.Train(vB); 

                if (k % 20 == 0) {
                    lblProgr.Text = "Итерация: " + k.ToString();
                    lblProgr.Refresh();
                }
            }

            net.Trained = true;

            // рисование карт (SOM)
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
                                        
                    // преобразование координат
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

                WriteableBitmap wbm = new WriteableBitmap(NW, NH, g.DpiX, g.DpiY, bmp.Format, null);
                wbm.WritePixels(new Int32Rect(0, 0, NW, NH), array, stride, 0);

                imgMap.Source = wbm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            lblStatus.Text = lblProgr.Text = "";
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

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
