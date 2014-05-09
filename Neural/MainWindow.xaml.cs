using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Neural
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Точечный рисунок Windows (BMP)|*.bmp|Формат JPEG|*.jpg|Portable Network Graphics (PNG)|*.png|Все файлы|*.*";
            dlg.FilterIndex = 1;
            
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    System.Windows.Media.ImageSource src = new BitmapImage(new Uri(dlg.FileName));                    
                    imgOrig.Source = src;

                    lblInfo.Text = dlg.SafeFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }      

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
