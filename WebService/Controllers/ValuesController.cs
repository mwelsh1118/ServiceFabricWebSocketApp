using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using WorkActor.Interfaces;

namespace WebService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values/abc-123
        [HttpGet("{id}")]
        public async Task<JsonResult> GetAsync(string id)
        {
            IWorkActor proxy = ActorProxy.Create<IWorkActor>(new ActorId(id));
            return Json(await proxy.GetCountAsync(CancellationToken.None));
        }

        // POST api/values/abc-123
        [HttpPost("{id}")]
        public async Task<IActionResult> PostAsync(string id, [FromBody] int value)
        {
            IWorkActor proxy = ActorProxy.Create<IWorkActor>(new ActorId(id));
            await proxy.SetCountAsync(value, CancellationToken.None);
            return Accepted();
        }
    }
}