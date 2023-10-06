using System.Data.SQLite;
using WebServer.Models.DataModels;

namespace WebServer.Data
{
    public class DatabaseM
    {
        private static string dataSourceString = "Data Source=PTPS.db;Version=3;";
        private static string tableName = "JobPostTable";

        public static bool createTable()
        {
            bool returnVal = false;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS " + tableName + " (" +
                        "JobId INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "FromClient TEXT, " +
                        "ToClient TEXT, " +
                        "Job TEXT, " +
                        "JobSuccess INTEGER, " +
                        "JobResult TEXT)";

                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                Console.WriteLine("Table has been successfully created :)");
                returnVal = true;
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return returnVal;
        }

        public static bool insert(JobPost data)
        {
            bool returnVal = false;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT INTO " + tableName + " (FromClient, ToClient, Job, JobSuccess, JobResult) " +
                        "VALUES (@FromClient, @ToClient, @Job, @JobSuccess, @JobResult)";

                        //command.Parameters.AddWithValue("@JobId", data.JobId);
                        command.Parameters.AddWithValue("@FromClient", data.FromClient);
                        command.Parameters.AddWithValue("@ToClient", data.ToClient);
                        command.Parameters.AddWithValue("@Job", data.Job);
                        command.Parameters.AddWithValue("@JobSuccess", data.JobSuccess);
                        command.Parameters.AddWithValue("@JobResult", data.JobResult);

                        int rowReturn = command.ExecuteNonQuery();

                        conn.Close();
                        if (rowReturn > 0)
                        {
                            Console.WriteLine("Inserted Data successfully :)");
                            returnVal = true;
                        }
                        else
                        {
                            Console.WriteLine("Data failed to be inserted :(");
                        }
                    }
                }
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return returnVal;
        }

        public static bool delete(int JobId)
        {
            bool returnVal = false;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText =
                            $"DELETE FROM " + tableName + " WHERE JobId = @JobId";
                        command.Parameters.AddWithValue("@JobId", JobId);

                        int rowReturn = command.ExecuteNonQuery();

                        conn.Close();
                        if (rowReturn > 0)
                        {
                            Console.WriteLine("Deleted Data successfully :)");
                            returnVal = true;
                        }
                        else
                        {
                            Console.WriteLine("Data failed to be deleted :(");
                        }
                    }
                }
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return returnVal;
        }

        public static bool update(JobPost data)
        {
            bool returnVal = false;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText =
                            $"UPDATE " + tableName + " SET " +
                            " FromClient = @FromClient," +
                            " ToClient = @ToClient," +
                            " Job = @Job," +
                            " JobSuccess = @JobSuccess" +
                            " JobResult = @JobResult" +
                            " WHERE JobId = @JobId";
                        command.Parameters.AddWithValue("@FromClient", data.FromClient);
                        command.Parameters.AddWithValue("@ToClient", data.ToClient);
                        command.Parameters.AddWithValue("@Job", data.Job);
                        command.Parameters.AddWithValue("@JobSuccess", data.JobSuccess);
                        command.Parameters.AddWithValue("@JobResult", data.JobResult);
                        command.Parameters.AddWithValue("@JobId", data.JobId);

                        int rowReturn = command.ExecuteNonQuery();

                        conn.Close();
                        if (rowReturn > 0)
                        {
                            Console.WriteLine("Updated Data successfully :)");
                            returnVal = true;
                        }
                        else
                        {
                            Console.WriteLine("Data failed to be updated :(");
                        }
                    }
                }
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return returnVal;
        }

        public static List<JobPost> getAll()
        {
            List<JobPost> dataList = null;
            try
            {
                dataList = new List<JobPost>();
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM " + tableName;
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                JobPost data = new JobPost();
                                data.JobId = Convert.ToInt32(reader["JobId"]);
                                data.FromClient = Convert.ToInt32(reader["FromClient"]);
                                data.ToClient = Convert.ToInt32(reader["ToClient"]);
                                data.Job = reader["Job"].ToString();
                                data.JobSuccess = Convert.ToInt32(reader["JobSuccess"]);
                                data.JobResult = reader["JobResult"].ToString();

                                dataList.Add(data);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return dataList;
        }

        public static JobPost getByAccNo(int JobId)
        {
            JobPost data = null;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(dataSourceString))
                {
                    conn.Open();
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM " + tableName + " WHERE JobId = @JobId";
                        command.Parameters.AddWithValue("@JobId", JobId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                data = new JobPost();
                                data.JobId = Convert.ToInt32(reader["JobId"]);
                                data.FromClient = Convert.ToInt32(reader["FromClient"]);
                                data.ToClient = Convert.ToInt32(reader["ToClient"]);
                                data.Job = reader["Job"].ToString();
                                data.JobSuccess = Convert.ToInt32(reader["JobSuccess"]);
                                data.JobResult = reader["JobResult"].ToString();
                            }
                        }
                    }
                }
            }
            catch (SQLiteException sqlER)
            {
                Console.WriteLine("SQL Exception: " + sqlER.Message);
            }
            catch (Exception eR)
            {
                Console.WriteLine("Exception: " + eR.Message);
            }
            return data;
        }
    }
}
