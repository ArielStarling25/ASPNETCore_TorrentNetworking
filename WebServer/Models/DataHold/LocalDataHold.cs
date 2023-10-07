using WebServer.Models.DataModels;
using WebServer.Data;
using System.Diagnostics.CodeAnalysis;

namespace WebServer.Models.DataHold
{
    public class LocalDataHold
    {
        private static List<ClientInfo> clientData;
        private static List<JobPost> jobPostData;

        public static void GoStart()
        {
            clientData = new List<ClientInfo>();
            jobPostData = DatabaseM.getAll();
            if(jobPostData == null)
            {
                jobPostData = new List<JobPost>();
            }
        }

        public static bool addClient(ClientInfo client)
        {
            bool result = false;
            if (!clientExists(client))
            {
                clientData.Add(client);
                result = true;
            }
            return result;
        }

        public static bool removeClient(ClientInfo client)
        {
            return clientData.Remove(client);
        }

        public static bool clientExists(ClientInfo client)
        {
            bool returnVal = false;
            foreach(ClientInfo clientInfo in clientData)
            {
                if (clientInfo.clientId == client.clientId)
                {
                    returnVal = true;
                    break;
                }
            }
            return returnVal;
        }

        public static ClientInfo getClientById(int clientId)
        {
            ClientInfo returnVal = null;
            foreach(ClientInfo clientInfo in clientData)
            {
                if (clientInfo.clientId == clientId)
                {
                    returnVal = clientInfo;
                    break;
                }
            }
            return returnVal;
        }

        public static bool updateClient(ClientInfo client)
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

        public static void addJobPost(JobPost jobPost)
        {
            jobPostData.Add(jobPost);
        }

        public static List<ClientInfo> GetClients()
        {
            return clientData;
        }
    }
}
