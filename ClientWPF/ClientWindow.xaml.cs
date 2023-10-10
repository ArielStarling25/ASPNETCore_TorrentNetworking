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
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime.Exceptions;
using static IronPython.Modules.PythonIOModule;

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

        //Data fields for buttons in GUI Job List
        List<string> base64PyDataHolder;
        List<string> varPyHolder;
        List<string> resultHolder;

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
            public void getJob(int clientId, out int jobId, out string base64Py, out string base64VarStr)
            {
                base64Py = "";
                base64VarStr = "";
                jobId = -1;
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
                            base64Py = mod.Job;
                            base64VarStr = mod.JobVariables;
                            jobId = mod.JobId;
                            break;
                        }
                    }
                }
                //return jobObtain;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool completeJob(int clientId, int jobSucceed, string jobResult)
            {
                bool returnComp = false;
                for(int i = 0; i < context.myJobList.Count; i++)
                {
                    //finding the job with the matching receiving client and marked as 'In progress' flag '0'
                    if (context.myJobList[i].ToClient == clientId && context.myJobList[i].JobSuccess == 0)
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
                ClientInfoMid savedClient = null;
                string base64PythonCode = "";
                string base64VarStr = "";
                int jobId = -1;
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
                            bool pythonDataObtained = false;
                            foreach (ClientInfoMid client in otherClients)
                            {
                                connectToClient(client.clientId, client.portNum, client.ipAddr);
                                if (foobFactory != null)
                                {
                                    foob.getJob(clientInfo.clientId, out jobId, out base64PythonCode, out base64VarStr);
                                    if (!String.IsNullOrEmpty(base64PythonCode))
                                    {
                                        //could probably apply hash checks here
                                        pythonDataObtained = true;
                                        savedClient = client;
                                        break;
                                    }
                                }
                            }

                            if(foobFactory != null)
                            {
                                foobFactory.Close();
                            }

                            if (pythonDataObtained && savedClient != null)
                            {
                                //Convert and Execute
                                string pythonScript = convertToCode(base64PythonCode);
                                string[] pySplit = pythonScript.Split(' ','(');
                                string funcName = pySplit[1]; // Should be at index 1 at least...
                                string finalResult = "N/A";

                                try
                                {
                                    //Prepare for execution of code
                                    VarHolder[] varHolders = new VarHolder[0];
                                    ScriptEngine engine = Python.CreateEngine();
                                    ScriptScope scope = engine.CreateScope();
                                    engine.Execute(pythonScript, scope);

                                    //int numVals = 0;
                                    if (!String.IsNullOrEmpty(base64VarStr))
                                    {
                                        string varString = convertToCode(base64VarStr);
                                        string[] varStrSplt = varString.Split('|');
                                        //numVals = varStrSplt.Length;
                                        varHolders = new VarHolder[varStrSplt.Length];

                                        for (int i = 0; i < varStrSplt.Length; i++)
                                        {
                                            string[] splt = varStrSplt[i].Split('=');
                                            if (int.TryParse(splt[1], out int result))
                                            {
                                                varHolders[i].intValue = result;
                                            }
                                            else // it is a string value
                                            {
                                                varHolders[i].intValue = null;
                                                varHolders[i].strValue = splt[1];
                                            }
                                        }
                                    }

                                    //Couldnt find a more dynamic way to apply variables to the script func
                                    //Execute code
                                    dynamic pyFunc = scope.GetVariable(funcName);
                                    var resultOfScript = "N/A";
                                    if (varHolders.Length == 0)
                                    {
                                        resultOfScript = pyFunc();
                                    }
                                    else if (varHolders.Length == 1)
                                    {
                                        if (varHolders[0].isInt())
                                        {
                                            resultOfScript = pyFunc(varHolders[0].intValue);
                                        }
                                        else
                                        {
                                            resultOfScript = pyFunc(varHolders[0].strValue);
                                        }
                                    }
                                    else if (varHolders.Length == 2)
                                    {
                                        if (varHolders[0].isInt())
                                        {
                                            if (varHolders[1].isInt())
                                            {
                                                resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].intValue);
                                            }
                                            else
                                            {
                                                resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].strValue);
                                            }
                                        }
                                        else
                                        {
                                            if (varHolders[1].isInt())
                                            {
                                                resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].intValue);
                                            }
                                            else
                                            {
                                                resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].strValue);
                                            }
                                        }
                                    }
                                    else if(varHolders.Length == 3)
                                    {
                                        if (varHolders[0].isInt())
                                        {
                                            if (varHolders[1].isInt())
                                            {
                                                if (varHolders[2].isInt())
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].intValue, varHolders[2].intValue);
                                                }
                                                else
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].intValue, varHolders[2].strValue);
                                                }
                                            }
                                            else
                                            {
                                                if (varHolders[2].isInt())
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].strValue, varHolders[2].intValue);
                                                }
                                                else
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].intValue, varHolders[1].strValue, varHolders[2].strValue);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (varHolders[1].isInt())
                                            {
                                                if (varHolders[2].isInt())
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].intValue, varHolders[2].intValue);
                                                }
                                                else
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].intValue, varHolders[2].strValue);
                                                }
                                            }
                                            else
                                            {
                                                if (varHolders[2].isInt())
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].strValue, varHolders[2].intValue);
                                                }
                                                else
                                                {
                                                    resultOfScript = pyFunc(varHolders[0].strValue, varHolders[1].strValue, varHolders[2].strValue);
                                                }
                                            }
                                        }

                                        finalResult = (string)resultOfScript;

                                    }
                                    else
                                    {
                                        throw new Exception("Too many arguments! (Max 3)");
                                    }
                                }
                                catch (IronPython.Runtime.Exceptions.ImportException pyIE)
                                {
                                    finalResult = pyIE.Message;
                                }
                                catch(Exception pyE)
                                {
                                    finalResult = pyE.Message;
                                }


                                connectToClient(savedClient.clientId, savedClient.portNum, savedClient.ipAddr);
                                if(foob.completeJob(clientInfo.clientId, 1, finalResult))
                                {
                                    foobFactory.Close();
                                    savedClient = null;
                                }
                                else
                                {
                                    foobFactory.Close();
                                    throw new Exception("Failed to complete a job!");
                                }

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
            List<JobPostMidcs> savedJobList = new List<JobPostMidcs>();
            try
            {
                //WIP
                while (onGoingAccess(-1))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (!savedJobList.Equals(myJobList))
                        {

                        }
                    }));
                    Thread.Sleep(500);
                }
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

        private void updateGUIJobList()
        {
            ListView_ClientJobList.Items.Clear();
            List<JobPostMidcs> jobList = myJobList;
            int buttonIdCounter = 1;
            if(jobList.Count > 0)
            {
                base64PyDataHolder = new List<string>();
                varPyHolder = new List<string>();
                resultHolder = new List<string>();

                for (int i = 0; i < jobList.Count; i++)
                {
                    ListViewItem listItem = new ListViewItem();
                    StackPanel itemStackHori = new StackPanel();
                    itemStackHori.Orientation = Orientation.Horizontal;

                    //Section 1
                    StackPanel itemStackVert1 = new StackPanel();
                    itemStackVert1.Orientation = Orientation.Vertical;
                    TextBlock jobIdTxt = new TextBlock();
                    jobIdTxt.Text = "Job ID: " + jobList[i].JobId.ToString();
                    jobIdTxt.FontWeight = FontWeights.Bold;
                    TextBlock fromClientTxt = new TextBlock();
                    fromClientTxt.Text = "Sending Client: " + jobList[i].FromClient.ToString();
                    TextBlock toClientTxt = new TextBlock();
                    toClientTxt.Text = "Recieving Client: " + jobList[i].ToClient.ToString();
                    TextBlock jobSuccessTxt = new TextBlock();
                    jobSuccessTxt.Text = "Job Status: ";
                    if (jobList[i].JobSuccess == -1)
                    {
                        jobSuccessTxt.Text += "Incomplete";
                    }
                    else if (jobList[i].JobSuccess == 0)
                    {
                        jobSuccessTxt.Text += "In Progress";
                    }
                    else if (jobList[i].JobSuccess == 1)
                    {
                        jobSuccessTxt.Text += "Complete";
                    }
                    else
                    {
                        jobSuccessTxt.Text += "Unknown";
                    }

                    itemStackVert1.Children.Add(jobIdTxt);
                    itemStackVert1.Children.Add(fromClientTxt);
                    itemStackVert1.Children.Add(toClientTxt);
                    itemStackVert1.Children.Add(jobSuccessTxt);

                    //Section 2
                    StackPanel itemStackVert2 = new StackPanel();
                    itemStackVert2.Orientation = Orientation.Vertical;

                    string buttonName = "";
                    for (int x = 0; x < buttonIdCounter; x++)
                    {
                        buttonName += "c";
                    }

                    Button Button_Job = new Button();
                    Button_Job.Height = 20;
                    Button_Job.Width = 70;
                    Button_Job.Content = "View Code";
                    Button_Job.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Button_Job.Name = buttonName;
                    Button_Job.Click += new RoutedEventHandler(Job_Click);
                    base64PyDataHolder.Add(jobList[i].Job);

                    Button Button_Var = new Button();
                    Button_Var.Height = 20;
                    Button_Var.Width = 70;
                    Button_Var.Content = "View Variables";
                    Button_Var.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Button_Var.Name = buttonName;
                    Button_Var.Click += new RoutedEventHandler(Var_Click);
                    varPyHolder.Add(jobList[i].JobVariables);

                    Button Button_Res = new Button();
                    Button_Var.Height = 20;
                    Button_Var.Width = 70;
                    Button_Var.Content = "View Result";
                    Button_Var.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Button_Var.Name = buttonName;
                    Button_Var.Click += new RoutedEventHandler(Var_Click);
                    varPyHolder.Add(jobList[i].JobResult);

                    buttonIdCounter++;
                    itemStackVert2.Children.Add(Button_Job);
                    itemStackVert2.Children.Add(Button_Var);
                    itemStackVert2.Children.Add(Button_Res);

                    itemStackHori.Children.Add(itemStackVert1);
                    itemStackHori.Children.Add(itemStackVert2);

                    listItem.Content = itemStackHori;
                    ListView_ClientJobList.Items.Add(listItem);
                }
            }
        }

        private void Job_Click(object sender, RoutedEventArgs e)
        {
            Label_Warning.Content = "";
            Button btnAccess = (Button)sender;
            if(btnAccess != null)
            {
                int btnId = btnAccess.Name.Length;
                MessageBox.Show(convertToCode(base64PyDataHolder[btnId-1]));
            }
        }

        private void Var_Click(object sender, RoutedEventArgs e)
        {
            Label_Warning.Content = "";
            Button btnAccess = (Button)sender;
            if (btnAccess != null)
            {
                int btnId = btnAccess.Name.Length;
                MessageBox.Show(convertToCode(varPyHolder[btnId - 1]));
            }
        }

        private void Res_Click(object sender, RoutedEventArgs e)
        {
            Label_Warning.Content = "";
            Button btnAccess = (Button)sender;
            if (btnAccess != null)
            {
                int btnId = btnAccess.Name.Length;
                string result = resultHolder[btnId-1];
                if (String.IsNullOrEmpty(result))
                {
                    Label_Warning.Content = " Results are not available ";
                }
                else
                {
                    MessageBox.Show(result);
                }
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
            Label_Warning.Content = "";
            if (!String.IsNullOrEmpty(TextBox_CodeBlock.Text) || !TextBox_CodeBlock.Text.Equals("Enter Python code here..."))
            {
                if(!TextBox_VariableInput.Text.Equals("Variable Input here..."))
                {
                    JobPostMidcs jobPost = new JobPostMidcs();
                    jobPost.FromClient = clientInfo.clientId;
                    jobPost.ToClient = 0;
                    jobPost.JobSuccess = -1;
                    jobPost.Job = convertToBase64(TextBox_CodeBlock.Text);//Convert to Base64 then send
                    jobPost.JobVariables = convertToBase64(TextBox_VariableInput.Text);
                    jobPost.JobResult = "";

                    modJobList("add", 0, jobPost);
                }
                else
                {
                    Label_Warning.Content = "Enter valid variable input please";
                }
            }
            else
            {
                Label_Warning.Content = "There should be code to submit...";
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
            Label_Warning.Content = "";
            RestClient restClient = new RestClient(webServerHttpUrl);
            RestRequest req = new RestRequest("/api/client", Method.Delete);
            req.RequestFormat = RestSharp.DataFormat.Json;
            req.AddBody(clientInfo);
            RestResponse res = restClient.Delete(req);
            if (res.IsSuccessStatusCode)
            {
                host.Close();
                onGoingAccess(0);
                //Join threads and exit
                miniServerThread.Join();
                networkingThread.Join();
                jobListRefresherThread.Join();

                if(foobFactory != null)
                {
                    foobFactory.Close();
                }
                this.Close();
            }
            else
            {
                Label_Warning.Content = "Failed to close client!";
            }
            
        }

        private void window_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (onGoingAccess(-1))
            {
                e.Cancel = true;
                Label_Warning.Content = "Close Client Properly Please...";
            }
        }

        //Sends updates to the webServer and obtains the data from webserver to update the job list
        //add, indexMod
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void modJobList(string type, int index, JobPostMidcs item)
        {
            if (type.Equals("add"))
            {
                //myJobList.Add(item);
                RestClient restClient = new RestClient(webServerHttpUrl);
                RestRequest req = new RestRequest("/api/jobpost", Method.Post);
                req.RequestFormat = RestSharp.DataFormat.Json;
                req.AddBody(item);
                RestResponse response = restClient.ExecutePost(req);
                if (response.IsSuccessStatusCode)
                {
                    List<JobPostMidcs> modJobList;
                    RestClient restClient2 = new RestClient(webServerHttpUrl);
                    RestRequest req2 = new RestRequest("/api/jobpost/getbyclient/" + clientInfo.clientId, Method.Get);
                    RestResponse res = restClient2.ExecuteGet(req2);
                    if (res.IsSuccessStatusCode)
                    {
                        modJobList = JsonConvert.DeserializeObject<List<JobPostMidcs>>(res.Content);
                        if(modJobList != null)
                        {
                            myJobList = modJobList;
                            Label_Warning.Content = "Successfully added job item to server!";
                        }
                        else
                        {
                            Label_Warning.Content = "Error occured trying to deserialise job list from server database!";
                        }
                    }
                    else
                    {
                        Label_Warning.Content = "Failed to obtain job list from server!";
                    }
                }
                else
                {
                    Label_Warning.Content = "Failed to post job to server!";
                }
            }
            else if (type.Equals("indexMod"))
            {
                //myJobList[index] = item;
                RestClient restClient = new RestClient(webServerHttpUrl);
                RestRequest req = new RestRequest("/api/jobpost/" + myJobList[index].JobId, Method.Put);
                req.RequestFormat = RestSharp.DataFormat.Json;
                req.AddBody(item);
                RestResponse response = restClient.ExecutePut(req);
                if (response.IsSuccessStatusCode)
                {
                    List<JobPostMidcs> modJobList;
                    RestClient restClient2 = new RestClient(webServerHttpUrl);
                    RestRequest req2 = new RestRequest("/api/jobpost/getbyclient/" + clientInfo.clientId, Method.Get);
                    RestResponse res = restClient2.ExecuteGet(req2);
                    if (res.IsSuccessStatusCode)
                    {
                        modJobList = JsonConvert.DeserializeObject<List<JobPostMidcs>>(res.Content);
                        if (modJobList != null)
                        {
                            myJobList = modJobList;
                        }
                        else
                        {
                            Label_Warning.Content = "Error occured trying to deserialise job list from server database!";
                        }
                    }
                    else
                    {
                        Label_Warning.Content = "Failed to obtain job list from server!";
                    }
                }
                else
                {
                    Label_Warning.Content = "Failed to update job on server database!";
                }
            }
        }
    }
}
