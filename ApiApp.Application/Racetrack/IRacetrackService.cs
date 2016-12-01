using ApiApp.Application.Racetrack.Models;

namespace ApiApp.Application.Racetrack
{
    public interface IRacetrackService
    {
        RacetrackDtoList GetRacetrackList(int? pageNumber, int? pageSize);
        RacetrackDto GetRacetrack(string name);
        RacetrackDto GetRacetrack(int id);
        RacetrackDto CreateRacetrack(RacetrackDtoCreate payload);
        bool DeleteRacetrack(string name);
        bool DeleteRacetrack(int id);
    }
}
