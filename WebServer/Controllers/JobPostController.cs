using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebServer.Data;
using WebServer.Models.DataModels;
using DataMid;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        //GET: api/JobPost
        [HttpGet]
        public IEnumerable<JobPostMidcs> GetJobPosts()
        {
            List<JobPostMidcs> list = DatabaseM.getAll();
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list;
                }
            }
            return null;
        }

        //GET: api/JobPost/getbyid/10
        [HttpGet("getbyid/{jobId}")]
        public IActionResult GetJobPostById(int jobId)
        {
            JobPostMidcs item = DatabaseM.getByJobId(jobId);
            if (item == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(item);
            }
        }

        //GET: api/JobPost/getbyclient/10
        [HttpGet("getbyclient/{clientId}")]
        public IActionResult GetJobPostByClient(int clientId)
        {
            List<JobPostMidcs> items = DatabaseM.getByClientId(clientId);
            if (items == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(items);
            }
        }

        //PUT: api/JobPost/10
        [HttpPut("{jobId}")]
        public IActionResult PutJobPost(int jobId, JobPostMidcs item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            else
            {
                if (jobId != item.JobId)
                {
                    return BadRequest();
                }
            }


            if (DatabaseM.update(item))
            {
                return NoContent();
            }
            else
            {
                return NotFound(item);
            }
        }
        
        //POST: api/JobPost
        [HttpPost]
        public IActionResult PostJobPost(JobPostMidcs item)
        {
            if(item == null)
            {
                return BadRequest();
            }
            else
            {
                if (item.FromClient == 0)
                {
                    return BadRequest();
                }
            }

            if (DatabaseM.insert(item))
            {
                return NoContent();
            }
            else
            {
                return BadRequest(item);
            }
        }

        //DELETE: api/JobPost
        [HttpDelete("{jobId}")]
        public IActionResult DeleteJobPost(int jobId) 
        {
            if (DatabaseM.delete(jobId))
            {
                return NoContent();
            }
            else
            {
                return NotFound(jobId);
            }
        }
    }
}
