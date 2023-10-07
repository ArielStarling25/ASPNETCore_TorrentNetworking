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

        //PUT: api/Client/3
        [HttpPut("{clientId}")]
        public IActionResult PutClient(int clientId, ClientInfo item) 
        {
            if(clientId != item.clientId)
            {
                return BadRequest();
            }
            else
            {
                if (LocalDataHold.updateClient(item))
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(item);
                }
            }
        }

        //POST: api/Client
        [HttpPost]
        public IActionResult PostClient(ClientInfo item)
        {
            if(item == null)
            {
                return BadRequest();
            }

            if (LocalDataHold.addClient(item))
            {
                return NoContent();
            }
            else
            {
                return BadRequest(item);
            }
        }

        //DELETE: api/Client
        public IActionResult DeleteClient(ClientInfo item)
        {
            if(item == null)
            {
                return BadRequest();
            }

            if (LocalDataHold.removeClient(item))
            {
                return NoContent();
            }
            else
            {
                return NotFound(item);
            }
        }
    }
}
