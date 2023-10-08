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
using RestSharp;
using Newtonsoft.Json;

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
        MiniClientServerInt foob;
        ChannelFactory<MiniClientServerInt> foobFactory;
        ServiceHost host;

        //Client Data Field
        private ClientInfoMid clientInfo;

        //Thread Fields
        private Thread miniServerThread;
        private Thread networkingThread;
        private Thread jobListRefresherThread;

        //Other Data Fields
        public List<JobPostMidcs> myJobList = new List<JobPostMidcs>();
        private bool onGoing;

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
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    NetTcpBinding tcp = new NetTcpBinding();
                    string url = netAddress + addressName;

                    tcp.OpenTimeout = new TimeSpan(0, 0, 5);
                    tcp.CloseTimeout = new TimeSpan(0, 0, 5);
                    tcp.ReceiveTimeout = new TimeSpan(0, 0, 10);
                    tcp.SendTimeout = new TimeSpan(0, 0, 30);
                    tcp.MaxBufferPoolSize = 5000000; //5MB
                    tcp.MaxReceivedMessageSize = 5000000; //5MB
                    tcp.MaxBufferSize = 5000000; //5MB

                    //Creates port for connection
                    MiniClientServer mini = new MiniClientServer(this);
                    host = new ServiceHost(mini);
                    host.AddServiceEndpoint(typeof(MiniClientServerInt), tcp, url);
                    host.Open();
                }));
            }
            catch (ThreadAbortException tAE)
            {
                MessageBox.Show(tAE.Message);
            }
            catch (ThreadInterruptedException tIE)
            {
                MessageBox.Show(tIE.Message);
            }
            catch (TaskCanceledException tCE)
            {
                MessageBox.Show(tCE.Message);
            }
            catch (Exception eR)
            {
                MessageBox.Show("Fatal Error:" + eR.Message);
            }
        }

        //Inner class for handling the server queries for the miniServer per client
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
                if (!listIsEmpty())
                {
                    for (int i = 0; i < context.myJobList.Count; i++)
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

            private bool listIsEmpty()
            {
                bool returnVal = true;
                if(context.myJobList.Count > 0)
                {
                    returnVal = false;
                }
                return returnVal;
            }
        }

        private void networkingT()
        {
            try
            {
                List<ClientInfoMid> otherClients;
                string base64PythonCode = "";
                while (onGoingAccess(-1))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        RestClient restClient = new RestClient(webServerHttpUrl);
                        RestRequest req = new RestRequest("/api/client", Method.Get);
                        RestResponse res = restClient.ExecuteGet(req);
                        otherClients = JsonConvert.DeserializeObject<List<ClientInfoMid>>(res.Content);
                        if (otherClients != null)
                        {
                            foreach (ClientInfoMid client in otherClients)
                            {
                                connectToClient(client.clientId, client.portNum, client.ipAddr);
                                if (foobFactory != null)
                                {
                                    base64PythonCode = foob.getJob(clientInfo.clientId);
                                    if (!base64PythonCode.Equals(""))
                                    {
                                        break;
                                    }
                                }
                            }

                            if (!base64PythonCode.Equals(""))
                            {
                                //Convert and Execute
                            }
                        }
                    }));
                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException tAE)
            {
                MessageBox.Show(tAE.Message);
            }
            catch(ThreadInterruptedException tIE)
            {
                MessageBox.Show(tIE.Message);
            }
            catch(TaskCanceledException tCE)
            {
                MessageBox.Show(tCE.Message);
            }
            catch(Exception eR)
            {
                MessageBox.Show("Fatal Error:" + eR.Message);
            }
        }

        private void connectToClient(int clientId, int portNum, string ip)
        {
            if(foobFactory != null)
            {
                foobFactory.Close();
            }

            NetTcpBinding tcpBinding = new NetTcpBinding();
            tcpBinding.OpenTimeout = new TimeSpan(0, 0, 5);
            tcpBinding.CloseTimeout = new TimeSpan(0, 0, 5);
            tcpBinding.ReceiveTimeout = new TimeSpan(0, 0, 10);
            tcpBinding.SendTimeout = new TimeSpan(0, 0, 30);
            tcpBinding.MaxBufferPoolSize = 5000000; //5MB
            tcpBinding.MaxReceivedMessageSize = 5000000; //5MB
            tcpBinding.MaxBufferSize = 5000000; //5MB

            string tcpUrl = "net.tcp://" + ip + ":" + portNum + "/Client-" + clientId;
            foobFactory = new ChannelFactory<MiniClientServerInt>(tcpBinding, tcpUrl);
            foob = foobFactory.CreateChannel();
        }

        private void jobListRefresherT()
        {
            try
            {

            }
            catch (ThreadAbortException tAE)
            {
                MessageBox.Show(tAE.Message);
            }
            catch (ThreadInterruptedException tIE)
            {
                MessageBox.Show(tIE.Message);
            }
            catch (TaskCanceledException tCE)
            {
                MessageBox.Show(tCE.Message);
            }
            catch (Exception eR)
            {
                MessageBox.Show("Fatal Error:" + eR.Message);
            }
        }

        private void startClient()
        {
            Label_Warning.Content = "";
            onGoingAccess(1);

            RestClient restClient = new RestClient(webServerHttpUrl);
            RestRequest req = new RestRequest("/api/client", Method.Post);
            req.RequestFormat = RestSharp.DataFormat.Json;
            req.AddBody(clientInfo);
            RestResponse response = restClient.ExecutePost(req);
            if (!response.IsSuccessStatusCode)
            {
                Label_Warning.Content = "Failed to send client data to Server! Close and Reopen";
                onGoingAccess(0);
                Button_CloseClient.Visibility = Visibility.Collapsed;
                Button_CloseClient.IsEnabled = false;
            }
            else
            {
                miniServerThread = new Thread(new ThreadStart(miniServerT));
                networkingThread = new Thread(new ThreadStart(networkingT));
                miniServerThread.Start();
                networkingThread.Start();
            }
        }

        public void SubmitCode_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextBox_CodeBlock.Text))
            {
                JobPostMidcs jobPost = new JobPostMidcs();
                jobPost.FromClient = clientInfo.clientId;
                jobPost.ToClient = 0;
                jobPost.JobSuccess = -1;
                jobPost.Job = convertToBase64(TextBox_CodeBlock.Text);//Convert to Base64 then send
                jobPost.JobResult = "";

                modJobList("add", 0, jobPost);
                //STOPPED HERE
            }
        }

        private string convertToBase64(string codeBlock)
        {
            string base64Str = "";
            if (!String.IsNullOrEmpty(codeBlock))
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(codeBlock);
                base64Str = Convert.ToBase64String(textBytes);
            }
            return base64Str;
        }

        private string convertToCode(string base64)
        {
            string code = "";
            if (!String.IsNullOrEmpty(base64))
            {
                byte[] encodedBytes = Convert.FromBase64String(base64);
                code = Encoding.UTF8.GetString(encodedBytes);
            }
            return code;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool onGoingAccess(int state)
        {
            //bool returnVal;
            if(state == -1) //returns current value of onGoing
            {
                
            }
            else if(state == 0) //mod to false and returns it
            {
                onGoing = false;
            }
            else if(state == 1) //mod to true and returns it
            {
                onGoing = true;
            }

            return onGoing; 
        }

        public void CloseClient_Click(object sender, RoutedEventArgs e)
        {
            host.Close();
            onGoingAccess(0);
        }

        private void window_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (onGoingAccess(-1))
            {
                e.Cancel = true;
                Label_Warning.Content = "Close Client Properly Please...";
            }
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
