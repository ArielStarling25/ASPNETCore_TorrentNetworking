namespace WebServer.Data
{
    public class DatabaseStarter
    {
        public static void startDatabase()
        {
            DatabaseM.createTable();
        }
    }
}
