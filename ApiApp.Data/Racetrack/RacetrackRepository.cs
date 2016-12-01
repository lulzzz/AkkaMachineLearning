using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using ApiApp.Data.Racetrack.Models;
using NanoApi;

namespace ApiApp.Data.Racetrack
{
    public class RacetrackRepository : IRacetrackRepository
    {
        private JsonFile<RacetrackDao> _raceTrackDb = JsonFile<RacetrackDao>.GetInstance(".\\", $"{typeof(RacetrackDao).Name}.json", Encoding.UTF8);

        public RacetrackDaoList GetRacetrackList(int? pageNumber, int? pageSize)
        {
            try
            {
                var data = GetCachedRacetrackDaos(false);

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    int index = pageNumber.Value * pageSize.Value;
                    int count = pageSize.Value;
                    int endOfIndex = index + count;

                    // TODO: Add (NEXT and PREVIOUS restapi controls)
                    if (data.Count < endOfIndex)
                    {
                        data.GetRange(index, data.Count);
                        return new RacetrackDaoList
                        {
                            Data = data
                        };
                    }

                    if (data.Count >= endOfIndex)
                    {
                        data.GetRange(index, count);
                        return new RacetrackDaoList
                        {
                            Data = data
                        };
                    }

                }

                return new RacetrackDaoList
                {
                    Data = data
                };
            }
            catch (Exception ex)
            {
                // TODO: Log exception
            }

            return new RacetrackDaoList
            {
                Data = new List<RacetrackDao>()
            };
        }

        public RacetrackDao GetRacetrack(string name)
        {
            try
            {
                return GetCachedRacetrackDaos(false).FirstOrDefault(x => x.Name == name);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return null;
            }
        }

        public RacetrackDao GetRacetrack(int id)
        {
            try
            {
                return GetCachedRacetrackDaos(false).FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return null;
            }
        }

        public RacetrackDao CreateRacetrack(RacetrackDaoCreate payload)
        {
            try
            {

                if (GetCachedRacetrackDaos(false).Where(x => x.Name == payload.Name).Any())
                {
                    throw new Exception(string.Format("Item name {0} already exists in the db.", payload.Name));
                }

                var id =_raceTrackDb.Insert(new RacetrackDao()
                {
                    Name = payload.Name,
                    CreatedDate = payload.CreatedDate,
                    ExecutionTime = payload.ExecutionTime,
                    RacetrackType = payload.RacetrackType,
                    RunResults = new List<RunResultsDao>(payload.RunResults)
                });
                return GetCachedRacetrackDaos(true).Last();

            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return null;
            }
        }

        // Nothing to update, keep as template
        //public bool UpdateRacetrack(string name, RacetrackDaoUpdate putRequest)
        //{
        //    try
        //    {
        //        if (!GetCachedRacetrackDaos(false).Exists(x => x.Name == name))
        //        {
        //            throw new Exception(string.Format("Item name {0} does not exists in the db.", name));
        //        }
        //        else
        //        {
        //            var id = _raceTrackDb.Update(x => x.Name == name, null);
        //            GetCachedRacetrackDaos(true).FirstOrDefault(x => x.Id == id);
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: Log exception
        //        return false;
        //    }
        //}

        public bool DeleteRacetrack(string name)
        {
            try
            {
                if (!GetCachedRacetrackDaos(false).Exists(x => x.Name == name))
                {
                    throw new Exception(string.Format("Item name {0} does not exists in the db.", name));
                }
                else
                {
                    int id = _raceTrackDb.Delete(x => x.Name == name);
                    GetCachedRacetrackDaos(true).FirstOrDefault(x => x.Id == id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return false;
            }
        }

        public bool DeleteRacetrack(int id)
        {
            try
            {
                if (!GetCachedRacetrackDaos(false).Exists(x => x.Id == id))
                {
                    throw new Exception(string.Format("Item name {0} does not exists in the db.", id));
                }
                else
                {
                    id = _raceTrackDb.Delete(x => x.Id == id);
                    GetCachedRacetrackDaos(true).FirstOrDefault(x => x.Id == id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return false;
            }
        }

        public List<RacetrackDao> GetCachedRacetrackDaos(bool clearFirst)
        {
            ObjectCache cache = MemoryCache.Default;
            if (clearFirst)
            {
                cache.Remove("RacetrackDao");
            }

            var data = cache["RacetrackDao"] as List<RacetrackDao>;

            if (Equals(data, default(List<RacetrackDao>)))
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                List<string> filePaths = new List<string>();
                filePaths.Add($"{System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)}\\{typeof(RacetrackDao).Name}.json");
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
                data = _raceTrackDb.Select();
                cache.Set("RacetrackDao", data, policy);
            }

            return data;
        }
    }
}
