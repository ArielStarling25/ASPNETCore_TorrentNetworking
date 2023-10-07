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
using System.Windows.Shapes;

using DataMid;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private readonly string webServerHttpUrl = "http://localhost:5254";
        private string addressName = "Client-[clientId]";
        private string netAddress = "net.tcp://localhost:[clientPortNum]/";

        private ClientInfoMid clientInfo;
        public ClientWindow(int clientId, int portNum)
        {
            InitializeComponent();
            addressName = "Client-" + clientId;
            netAddress = "net.tcp://localhost:" + portNum + "/";
            clientInfo = new ClientInfoMid();
            clientInfo.clientId = clientId;
            clientInfo.portNum = portNum;
            clientInfo.ipAddr = "localhost";
            startClient();
        }

        private void startClient()
        {
            
        }
    }
}
