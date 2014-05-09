using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Neural
{
    enum eActions
    {
        ActionOpen,
        ActionExit
    }

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

            // Set filter options and filter index.
            dlg.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;

            // Call the ShowDialog method to show the dialog box.
            dlg.ShowDialog();
        }      

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
