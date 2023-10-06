using WebServer.Models.DataModels;

namespace WebServer.Models.DataHold
{
    public class LocalDataHold
    {
        private static List<ClientInfo> clientData;
        private static List<JobPost> jobPostData;

        public static void GoStart()
        {
            clientData = new List<ClientInfo>();
            jobPostData = new List<JobPost>();
        }
    }
}
