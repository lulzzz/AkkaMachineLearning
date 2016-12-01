using NanoApi.JsonFile;

namespace ApiApp.Data.Racetrack.Models
{
    public class BaseRacetrackDao
    {
        [PrimaryKey]
        public int Id { get; set; }
    }
}
