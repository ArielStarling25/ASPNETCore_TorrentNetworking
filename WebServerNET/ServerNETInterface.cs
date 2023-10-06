using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace WebServerNET
{
    [ServiceContract]
    public interface ServerNETInterface
    {
        [OperationContract]
        void RegisterClient(string clientIp, int port);

        [OperationContract]
        string GetJob(string fromClientIp, string toClientIp);

        [OperationContract]
        int GetNumClients();

        [OperationContract]
        string
    }
}
