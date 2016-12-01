using System.Web.Http;
using ApiApp.Api.Filters;
using ApiApp.Application.Racetrack;
using ApiApp.Application.Racetrack.Models;

namespace ApiApp.Api.Controllers
{
    [Authenticate]
    [RoutePrefix("api/racetracks")]
    public class RacetrackController : ApiController
    {
        private readonly IRacetrackService _racetrackService;

        public RacetrackController(IRacetrackService racetrackService)
        {
            _racetrackService = racetrackService;
        }

        [Route("{pageNumber:int?}/{pageSize:int?}")]
        [HttpGet]
        public RacetrackDtoList All(int? pageNumber=null, int? pageSize=null)
        {
            return _racetrackService.GetRacetrackList(pageNumber, pageSize);
        }

        [Route("{name}")]
        [HttpGet]
        public RacetrackDto Get(string name)
        {
            return _racetrackService.GetRacetrack(name);
        }

        [Route("{id:int}")]
        [HttpGet]
        public RacetrackDto Get(int id)
        {
            return _racetrackService.GetRacetrack(id);
        }

        [Route("")]
        [HttpPost]
        public RacetrackDto Post([FromBody] RacetrackDtoCreate payload)
        {
            return _racetrackService.CreateRacetrack(payload);
        }

        //[HttpPut]
        //[ActionName("Racetrack")]
        //public bool Put(string name, [FromBody] RacetrackDtoUpdate payload)
        //{
        //    return _racetrackService.UpdateRacetrack(name, payload);
        //}

        [Route("{name}")]
        [HttpDelete]
        public bool Delete(string name)
        {
            return _racetrackService.DeleteRacetrack(name);
        }
    }
}
