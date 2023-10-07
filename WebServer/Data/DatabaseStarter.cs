using WebServer.Models.DataModels;

namespace WebServer.Data
{
    public class DatabaseStarter
    {
        public static void startDatabase()
        {
            DatabaseM.createTable();
        }

        public static void startAndMockDatabase()
        {
            DatabaseM.createTable();
            
            JobPost d1 = new JobPost();
            d1.FromClient = 01;
            d1.ToClient = 05;
            d1.Job = "Base64EncodedPythonCode";
            d1.JobSuccess = 1;
            d1.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d1);

            JobPost d2 = new JobPost();
            d2.FromClient = 02;
            d2.ToClient = 04;
            d2.Job = "Base64EncodedPythonCode";
            d2.JobSuccess = 0;
            d2.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d2);

            JobPost d3 = new JobPost();
            d3.FromClient = 03;
            d3.ToClient = 02;
            d3.Job = "Base64EncodedPythonCode";
            d3.JobSuccess = 1;
            d3.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d3);
        }
    }
}
