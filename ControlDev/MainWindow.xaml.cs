using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlDev
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            aaa.ItemsSource = source;
        }

        System.Collections.ObjectModel.ObservableCollection<string> source =
            new System.Collections.ObjectModel.ObservableCollection<string>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            source.Add(DateTime.Now.ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            source.RemoveAt(source.Count - 1);
        }
    }
}
