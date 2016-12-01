using ApiApp.Data.Racetrack.Models;

namespace ApiApp.Data.Racetrack
{
    public interface IRacetrackRepository
    {
        RacetrackDaoList GetRacetrackList(int? pageNumber, int? pageSize);
        RacetrackDao GetRacetrack(string name);
        RacetrackDao GetRacetrack(int id);
        RacetrackDao CreateRacetrack(RacetrackDaoCreate payload);
        bool DeleteRacetrack(string name);
        bool DeleteRacetrack(int id);
    }
}
