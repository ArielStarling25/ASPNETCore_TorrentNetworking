using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebServer.Models.DataModels;
using WebServer.Models.DataHold;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        //GET: api/Client
        [HttpGet]
        public IEnumerable<ClientInfo> GetClients()
        {
            return LocalDataHold.GetClients();
        }

        //GET: api/Client/3
        [HttpGet("{clientId}")]
        public IActionResult GetClient(int clientId)
        {
            ClientInfo item = LocalDataHold.getClientById(clientId);
            if(item == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(item);
            }
        }
    }
}
