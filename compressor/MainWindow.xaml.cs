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

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bmp = (BitmapImage)imgOrig.Source;
            int h = bmp.PixelHeight,
                stride = (bmp.PixelWidth * bmp.Format.BitsPerPixel + 7) / 8,
                len = h * stride,
                len4 = len / 4;
            
            byte[] pixels = new byte[len];
            bmp.CopyPixels(pixels, stride, 0);
            
            // input
            double[] input = new double[len];

            uint [] hexes = new uint[len4];
            for (int i = 0, j = 0; i < len; i+= 4, j++)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                //hexes[j] = (uint)((a << 24) | (r << 16) | (g << 8) | b); 
                input[i] = (double)pixels[i];
            }

            DistanceNetwork nt = new DistanceNetwork(len4, 100);

            SOMLearning	trainer = new SOMLearning( nt );

			

			double	fixedLearningRate = 0.1/*learningRate*/ / 10;
			double	driftingLearningRate = fixedLearningRate * 9;

            int iterations = 100;
			// iterations
			int k = 0;

			// loop
            while (true)
            {
                trainer.LearningRate = driftingLearningRate * (iterations - k) / iterations + fixedLearningRate;
                trainer.LearningRadius = (double)15/*radius*/ * (iterations - k) / iterations;



                /*
                input[0] = rand.Next(256);
                input[1] = rand.Next(256);
                input[2] = rand.Next(256);
                input[3] = rand.Next(256);*/

                trainer.Run(input);


                // increase current iteration
                k++;

                // set current iteration's info
                //currentIterationBox.Text = i.ToString( );

                // stop ?
                if (k >= iterations)
                    break;
            }


            byte[] array = new byte[100];
            for (int i = 0; i < nt.Output.Length; i++)
            {
                array[i] = (byte)nt.Output[i];
            }
            
            try
            {   
                BitmapImage bm = new BitmapImage();                
                //bm.BeginInit();
                bm.CacheOption = BitmapCacheOption.OnLoad;
                bm.StreamSource = new MemoryStream(array);
                //bm.EndInit();

                imgMap.Height = 100;
                imgMap.Width = 100;
                imgMap.Source = bm;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
            
        }
    }
}
