using WebServer.Models.DataModels;
using WebServer.Data;

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

        public static void addClient(ClientInfo client)
        {
            clientData.Add(client);
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
                if (clientInfo.ipAddr.Equals(client.ipAddr) && (clientInfo.portNum == client.portNum))
                {
                    returnVal = true;
                    break;
                }
            }
            return returnVal;
        }

        public static void addJobPost(JobPost jobPost)
        {
            jobPostData.Add(jobPost);
        }

    }
}
