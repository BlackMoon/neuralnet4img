using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace neural
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            dlg.Filter = "Точечные рисунки (*.bmp;*dib)|*.bmp;*.dib|JPEG (*.jpg;*.jpeg;*.jpe;*.jfif)|*.jpg;*.jpeg;*.jpe;*.jfif|GIF (*.gif|*.gif|TIFF (*.tif;*.tiff)|*.tif;*.tiff|PNG (*.png)|*.png|ICO (*.ico)|*.ico|Все файлы изображений|*.bmp;*.dib;*.jpg;*.jpeg;*.jpe;*.jfif;*.gif;*.tif;*.tiff;*.png;*.ico|Все файлы|*.*";
            dlg.FilterIndex = 7;
            
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {   
                    imgOrig.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(dlg.FileName));
                    lblInfo.Text = dlg.SafeFileName;
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
    }
}
