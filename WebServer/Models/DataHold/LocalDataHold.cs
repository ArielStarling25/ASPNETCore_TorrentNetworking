using WebServer.Models.DataModels;
using WebServer.Data;
using System.Diagnostics.CodeAnalysis;
using DataMid;

namespace WebServer.Models.DataHold
{
    public class LocalDataHold
    {
        private static List<ClientInfoMid> clientData;
        private static List<JobPostMidcs> jobPostData;

        public static void GoStart()
        {
            clientData = new List<ClientInfoMid>();
            jobPostData = DatabaseM.getAll();
            if(jobPostData == null)
            {
                jobPostData = new List<JobPostMidcs>();
            }
        }

        public static void GoMockAndStart() 
        {
            clientData = new List<ClientInfoMid>();

            ClientInfoMid c1 = new ClientInfoMid();
            c1.clientId = 01;
            c1.ipAddr = "net.tcp://localhost:";
            c1.portNum = 1000;
            clientData.Add(c1);

            ClientInfoMid c2 = new ClientInfoMid();
            c2.clientId = 02;
            c2.ipAddr = "net.tcp://localhost:";
            c2.portNum = 2000;
            clientData.Add(c2);

            ClientInfoMid c3 = new ClientInfoMid();
            c3.clientId = 03;
            c3.ipAddr = "net.tcp://localhost:";
            c3.portNum = 3000;
            clientData.Add(c3);

            jobPostData = DatabaseM.getAll();
            if (jobPostData == null)
            {
                jobPostData = new List<JobPostMidcs>();
            }
        }

        public static bool addClient(ClientInfoMid client)
        {
            bool result = false;
            if (!clientExists(client))
            {
                clientData.Add(client);
                result = true;
            }
            return result;
        }

        public static bool removeClient(ClientInfoMid client)
        {
            bool result = false;
            for(int i = 0; i < clientData.Count; i++)
            {
                if (clientData[i].clientId == client.clientId)
                {
                    clientData.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static bool clientExists(ClientInfoMid client)
        {
            bool returnVal = false;
            foreach(ClientInfoMid clientInfo in clientData)
            {
                if (clientInfo.clientId == client.clientId)
                {
                    returnVal = true;
                    break;
                }
            }
            return returnVal;
        }

        public static ClientInfoMid getClientById(int clientId)
        {
            ClientInfoMid returnVal = null;
            foreach(ClientInfoMid clientInfo in clientData)
            {
                if (clientInfo.clientId == clientId)
                {
                    returnVal = clientInfo;
                    break;
                }
            }
            return returnVal;
        }

        public static bool updateClient(ClientInfoMid client)
        {
            bool returnVal = false;
            if (clientExists(client))
            {
                for (int x = 0; x < clientData.Count; x++)
                {
                    if (clientData[x].clientId == client.clientId)
                    {
                        clientData[x] = client;
                        returnVal = true;
                        break;
                    }
                }
            }
            return returnVal;
        }

        public static void addJobPost(JobPostMidcs jobPost)
        {
            jobPostData.Add(jobPost);
        }

        public static List<ClientInfoMid> GetClients()
        {
            return clientData;
        }
    }
}
