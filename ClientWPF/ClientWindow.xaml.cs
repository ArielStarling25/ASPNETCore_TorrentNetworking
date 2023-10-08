using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ServiceModel;

using DataMid;
using System.Runtime.CompilerServices;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        //Connection Fields
        private readonly string webServerHttpUrl = "http://localhost:5254";
        private string addressName = "Client-[clientId]";
        private string netAddress = "net.tcp://localhost:[clientPortNum]/";

        //Client Data Field
        private ClientInfoMid clientInfo;

        //Thread Fields
        private Thread miniServerThread;
        private Thread networkingThread;

        //Other Data Fields
        public List<JobPostMidcs> myJobList = new List<JobPostMidcs>();

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

        private void miniServerT()
        {
            ServiceHost host;
            NetTcpBinding tcp = new NetTcpBinding();
            string url = netAddress + addressName;

            tcp.OpenTimeout = new TimeSpan(0, 0, 5);
            tcp.CloseTimeout = new TimeSpan(0, 0, 5);
            tcp.ReceiveTimeout = new TimeSpan(0, 0, 10);
            tcp.SendTimeout = new TimeSpan(0, 0, 30);
            tcp.MaxBufferPoolSize = 5000000; //5MB
            tcp.MaxReceivedMessageSize = 5000000; //5MB
            tcp.MaxBufferSize = 5000000; //5MB
            //tcp.ReaderQuotas.MaxArrayLength = 100000;
            //tcp.ReaderQuotas.MaxDepth = 10;
            //tcp.ReaderQuotas.MaxBytesPerRead = 10000000; //10MB 
            //tcp.ReaderQuotas.MaxStringContentLength = 1000000; //1MB

            //Creates port for connection
            MiniClientServer mini = new MiniClientServer(this);
            host = new ServiceHost(mini);
            host.AddServiceEndpoint(typeof(MiniClientServerInt), tcp, url);
            host.Open();
        }

        //Imbedded class for handling .net networking tasks
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = true)]
        private class MiniClientServer : MiniClientServerInt
        {
            private ClientWindow context;
            public MiniClientServer(ClientWindow context)
            {
                this.context = context;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public string getJob(int clientId)
            {
                string jobObtain = "";
                for(int i = 0; i < context.myJobList.Count; i++)
                {
                    //if a jobItem is found to have no reciever yet
                    if (context.myJobList[i].ToClient == null || context.myJobList[i].ToClient == 0)
                    {
                        JobPostMidcs mod = context.myJobList[i];
                        mod.ToClient = clientId;
                        mod.JobSuccess = 0; // In progress
                        context.modJobList("indexMod", i, mod);
                        jobObtain = mod.Job;
                        break;
                    }
                }
                return jobObtain;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool completeJob(int client, int jobSucceed, string jobResult)
            {
                bool returnComp = false;
                for(int i = 0; i < context.myJobList.Count; i++)
                {
                    //finding the job with the matching receiving client and marked as 'In progress' flag '0'
                    if (context.myJobList[i].ToClient == client && context.myJobList[i].JobSuccess == 0)
                    {
                        JobPostMidcs mod = context.myJobList[i];
                        mod.JobSuccess = jobSucceed; //Complete
                        mod.JobResult = jobResult;
                        context.modJobList("indexMod", i, mod);
                        returnComp = true;
                        break;
                    }
                }
                return returnComp;
            }
        }

        private void networkingT()
        {

        }

        private void startClient()
        {
            
        }

        //add, indexMod
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void modJobList(string type, int index, JobPostMidcs item)
        {
            if (type.Equals("add"))
            {
                myJobList.Add(item);
            }
            else if (type.Equals("indexMod"))
            {
                myJobList[index] = item;
            }
        }
    }
}
