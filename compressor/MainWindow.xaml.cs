using compressor.Neuro;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
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
        private Random rand = new Random();

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
            var prefs = new UserPreferences { 
                WindowLeft = this.Left, 
                WindowTop = this.Top, 
                WindowWidth = this.Width, 
                WindowHeight = this.Height, 
                WindowState = this.WindowState 
            };
            prefs.Save();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            lblStatus.Text = "Обучение";

            BitmapSource bmp = (BitmapSource)imgOrig.Source;
            int H = bmp.PixelHeight,
                W = bmp.PixelWidth,
                stride = (W * bmp.Format.BitsPerPixel + 7) / 8,
                len = H * stride;               
            
            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);

            int NX = 80;
            int NY = 80;          

            #region hidden
           
            // input
            double[] input = new double[4];
            
            for (int i = 0, j = 0; i < len; i+= 4, j++)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                //hexes[j] = (uint)((a << 24) | (r << 16) | (g << 8) | b); 
                //input[i] = (double)pixels[i];
            }

            DistanceNetwork nt = new DistanceNetwork(4, NX * NY);

            SOMLearning	trainer = new SOMLearning( nt );

			

			double	fixedLearningRate = 0.1/*learningRate*/ / 10;
			double	driftingLearningRate = fixedLearningRate * 9;

            int iterations = 1000;
			// iterations
			int k = 0;

			// loop
            while (true)
            {
                trainer.LearningRate = driftingLearningRate * (iterations - k) / iterations + fixedLearningRate;
                trainer.LearningRadius = (double)5/*radius*/ * (iterations - k) / iterations;

                int i = rand.Next(H*W);
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
                if (k >= iterations)
                    break;
            }

            stride = 4 * NX;
            byte[] array = new byte[NY * stride];
            Layer layer = nt[0];

            for (int y = 0; y < NY; y++)
            {
                // for all pixels
                for (int x = 0; x < stride; x+= 4)
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
                WriteableBitmap bm1 = new WriteableBitmap(NX, NY, bmp.DpiX, bmp.DpiY, bmp.Format, null);
                bm1.WritePixels(new Int32Rect(0, 0, NX, NY), array, stride, 0); 
                    
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
    }
}
