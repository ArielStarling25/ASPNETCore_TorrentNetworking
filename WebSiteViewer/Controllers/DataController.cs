using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;
using DataMid;

namespace WebSiteViewer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly string webServerHttpUrl = "http://localhost:5254";

        //GET: api/data/client
        [HttpGet("client")]
        public IActionResult getClientData()
        {
            RestClient restClient = new RestClient(webServerHttpUrl);
            RestRequest req = new RestRequest("/api/client", Method.Get);
            RestResponse res = restClient.ExecuteGet(req);
            if (res.IsSuccessStatusCode)
            {
                List<ClientInfoMid> clients = JsonConvert.DeserializeObject<List<ClientInfoMid>>(res.Content);
                if(clients != null)
                {
                    return Ok(JsonConvert.SerializeObject(clients));
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //GET: api/data/jobpost
        [HttpGet("jobpost")]
        public IActionResult getJobData() 
        {
            RestClient restClient = new RestClient(webServerHttpUrl);
            RestRequest req = new RestRequest("/api/jobpost", Method.Get);
            RestResponse res = restClient.ExecuteGet(req);
            if (res.IsSuccessStatusCode)
            {
                List<JobPostMidcs> jobPosts = JsonConvert.DeserializeObject<List<JobPostMidcs>>(res.Content);
                if (jobPosts != null)
                {
                    return Ok(JsonConvert.SerializeObject(jobPosts));
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}
