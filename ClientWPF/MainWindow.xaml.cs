using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using DataMidPoint;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int clientIdCounter = 100;
        private int clientPortNum = 8000;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddClient_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
