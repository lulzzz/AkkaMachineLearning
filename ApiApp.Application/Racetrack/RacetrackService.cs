using System;
using System.Collections.Generic;
using ApiApp.Application.Racetrack.Models;
using ApiApp.Data.Racetrack;
using ApiApp.Data.Racetrack.Models;

namespace ApiApp.Application.Racetrack
{
    public class RacetrackService : IRacetrackService
    {
        private readonly IRacetrackRepository _racetrackRepository;

        public RacetrackService(IRacetrackRepository racetrackRepository)
        {
            _racetrackRepository = racetrackRepository;
        }

        public RacetrackDtoList GetRacetrackList(int? pageNumber, int? pageSize)
        {
            try
            {
                var response = _racetrackRepository.GetRacetrackList(pageNumber, pageSize);

                var responseDtoList = new RacetrackDtoList();
                foreach (var racetrackDao in response.Data)
                {
                    var responseDto = new RacetrackDto
                    {
                        Name = racetrackDao.Name,
                        CreatedDate = racetrackDao.CreatedDate,
                        ExecutionTime = racetrackDao.ExecutionTime,
                        RacetrackType = racetrackDao.RacetrackType,
                        RunResults = new List<RunResultsDto>()
                    };

                    foreach (var r in racetrackDao.RunResults)
                    {
                        responseDto.RunResults.Add(new RunResultsDto
                        {
                            CreatedDate = r.CreatedDate,
                            DiscountRate = r.DiscountRate,
                            ExecutionTime = r.ExecutionTime,
                            ExplorationRate = r.ExplorationRate,
                            LearningRate = r.LearningRate,
                            Reward = r.Reward,
                            Steps = r.Steps,
                            SuccessRate = r.SuccessRate
                        });
                    }

                    responseDtoList.Data.Add(responseDto);
                }

                return responseDtoList;
            }
            catch (Exception)
            {
                return new RacetrackDtoList();
            }
        }

        public RacetrackDto GetRacetrack(string name)
        {
            try
            {
                var responseDao = _racetrackRepository.GetRacetrack(name);

                var responseDto = new RacetrackDto
                {
                    Name = responseDao.Name,
                    CreatedDate = responseDao.CreatedDate,
                    ExecutionTime = responseDao.ExecutionTime,
                    RacetrackType = responseDao.RacetrackType,
                    RunResults = new List<RunResultsDto>()
                };

                foreach (var r in responseDao.RunResults)
                {
                    responseDao.RunResults.Add(new RunResultsDao
                    {
                        CreatedDate = r.CreatedDate,
                        DiscountRate = r.DiscountRate,
                        ExecutionTime = r.ExecutionTime,
                        ExplorationRate = r.ExplorationRate,
                        LearningRate = r.LearningRate,
                        Reward = r.Reward,
                        Steps = r.Steps,
                        SuccessRate = r.SuccessRate
                    });
                }

                return responseDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public RacetrackDto GetRacetrack(int id)
        {
            try
            {
                var responseDao = _racetrackRepository.GetRacetrack(id);

                var responseDto = new RacetrackDto
                {
                    Name = responseDao.Name,
                    CreatedDate = responseDao.CreatedDate,
                    ExecutionTime = responseDao.ExecutionTime,
                    RacetrackType = responseDao.RacetrackType,
                    RunResults = new List<RunResultsDto>()
                };

                foreach (var r in responseDao.RunResults)
                {
                    responseDao.RunResults.Add(new RunResultsDao
                    {
                        CreatedDate = r.CreatedDate,
                        DiscountRate = r.DiscountRate,
                        ExecutionTime = r.ExecutionTime,
                        ExplorationRate = r.ExplorationRate,
                        LearningRate = r.LearningRate,
                        Reward = r.Reward,
                        Steps = r.Steps,
                        SuccessRate = r.SuccessRate
                    });
                }

                return responseDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public RacetrackDto CreateRacetrack(RacetrackDtoCreate payloadDto)
        {
            try
            {
                var payloadDao = new RacetrackDaoCreate
                {
                    Name = payloadDto.Name,
                    CreatedDate = payloadDto.CreatedDate,
                    ExecutionTime = payloadDto.ExecutionTime,
                    RacetrackType = payloadDto.RacetrackType,
                    RunResults = new List<RunResultsDao>()
                };

                foreach (var r in payloadDto.RunResults)
                {
                    payloadDao.RunResults.Add(new RunResultsDao
                    {
                        CreatedDate = r.CreatedDate,
                        DiscountRate = r.DiscountRate,
                        ExecutionTime = r.ExecutionTime,
                        ExplorationRate = r.ExplorationRate,
                        LearningRate = r.LearningRate,
                        Reward = r.Reward,
                        Steps = r.Steps,
                        SuccessRate = r.SuccessRate
                    });
                }

                var responseDao = _racetrackRepository.CreateRacetrack(payloadDao);

                var responseDto = new RacetrackDto
                {
                    Name = responseDao.Name,
                    CreatedDate = responseDao.CreatedDate,
                    ExecutionTime = responseDao.ExecutionTime,
                    RacetrackType = responseDao.RacetrackType,
                    RunResults = new List<RunResultsDto>()
                };

                foreach (var r in responseDao.RunResults)
                {
                    responseDao.RunResults.Add(new RunResultsDao
                    {
                        CreatedDate = r.CreatedDate,
                        DiscountRate = r.DiscountRate,
                        ExecutionTime = r.ExecutionTime,
                        ExplorationRate = r.ExplorationRate,
                        LearningRate = r.LearningRate,
                        Reward = r.Reward,
                        Steps = r.Steps,
                        SuccessRate = r.SuccessRate
                    });
                }

                return responseDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool DeleteRacetrack(string name)
        {
            return _racetrackRepository.DeleteRacetrack(name);
        }

        public bool DeleteRacetrack(int id)
        {
            return _racetrackRepository.DeleteRacetrack(id);
        }
    }
}
