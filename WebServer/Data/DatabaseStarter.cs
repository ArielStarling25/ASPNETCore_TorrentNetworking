using WebServer.Models.DataModels;
using DataMid;

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
            
            JobPostMidcs d1 = new JobPostMidcs();
            d1.FromClient = 01;
            d1.ToClient = 05;
            d1.Job = "Base64EncodedPythonCode";
            d1.JobVariables = "Variables for python to execute with";
            d1.JobSuccess = 1;
            d1.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d1);

            JobPostMidcs d2 = new JobPostMidcs();
            d2.FromClient = 02;
            d2.ToClient = 04;
            d2.Job = "Base64EncodedPythonCode";
            d2.JobVariables = "Variables for python to execute with";
            d2.JobSuccess = 0;
            d2.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d2);

            JobPostMidcs d3 = new JobPostMidcs();
            d3.FromClient = 03;
            d3.ToClient = 02;
            d3.Job = "Base64EncodedPythonCode";
            d3.JobVariables = "Variables for python to execute with";
            d3.JobSuccess = 1;
            d3.JobResult = "ResultOfPythonCode/Shows error if failed";
            DatabaseM.insert(d3);
        }
    }
}
