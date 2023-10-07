using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataMid;

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
            Label_Warning.Content = "";
        }

        public void AddClient_Click(object sender, RoutedEventArgs e)
        {
            Label_Warning.Content = "";
            clientIdCounter++;
            clientPortNum = clientPortNum + 100;
            ClientWindow client = new ClientWindow(clientIdCounter, clientPortNum);
            if(client == null)
            {
                Label_Warning.Content = "Failed to create a new Client!";
                clientIdCounter--;
                clientPortNum = clientPortNum - 100;
            }
            else
            {
                client.Show();
            }
        }
    }
}
