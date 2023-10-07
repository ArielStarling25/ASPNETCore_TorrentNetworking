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

namespace ClientWPF
{
    public partial class MainWindow : Window
    {
        private int clientCounter = 100;
        private int portNum = 8000;
        public MainWindow()
        {
            InitializeComponent();
            Label_WarningLabel.Content = "";
        }

        public void AddClient_Click(object sender, RoutedEventArgs e)
        {
            Label_WarningLabel.Content = "";
            clientCounter++;
            portNum = portNum + 100;
            clientWindow client = new clientWindow(clientCounter, portNum);
            if(client == null)
            {
                clientCounter--;
                portNum = portNum - 100;
                Label_WarningLabel.Content = "Error in launching new client";
            }
            else
            {
                client.Show();
            }
        }
    }
}
