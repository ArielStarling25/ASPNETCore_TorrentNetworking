using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebServer.Models.DataModels;
using WebServer.Models.DataHold;
using DataMid;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        //GET: api/Client
        [HttpGet]
        public IActionResult GetClients()
        {
            List<ClientInfoMid> data = LocalDataHold.GetClients();
            if(data == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(data);
            }
        }

        //GET: api/Client/3
        [HttpGet("{clientId}")]
        public IActionResult GetClient(int clientId)
        {
            ClientInfoMid item = LocalDataHold.getClientById(clientId);
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
        public IActionResult PutClient(int clientId, ClientInfoMid item) 
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
        public IActionResult PostClient(ClientInfoMid item)
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
        [HttpDelete]
        public IActionResult DeleteClient(ClientInfoMid item)
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
