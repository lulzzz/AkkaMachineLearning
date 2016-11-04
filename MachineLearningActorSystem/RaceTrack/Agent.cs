using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class Agent
    {
        public Agent()
        {
            Episode = new List<State>();
        }

        public Agent(State startState)
        {
            AgentName = $"Agent.X{startState.X}.Y{startState.Y}";
            Episode = new List<State>() {startState};
        }

        public string AgentName { get; set; }
        public string RaceTime { get; set; }
        public int TotalReward { get; set; }
        public int NoSlideSteps { get; set; }
        public int SlideRightSteps { get; set; }
        public int SlideUpSteps { get; set; }
        public int OffWorldSteps { get; set; }
        public int GoodSteps { get; set; }
        public int TotalSteps { get; set; }
        public bool FailedRace { get; set; }
        public List<State> Episode { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"AgentName: {AgentName}");
            sb.AppendLine($"RaceTime: {RaceTime}");
            sb.AppendLine($"TotalReward: {TotalReward}");
            sb.AppendLine($"NoSlideSteps: {NoSlideSteps}");
            sb.AppendLine($"SlideRightSteps: {SlideRightSteps}");
            sb.AppendLine($"SlideUpSteps: {SlideUpSteps}");
            sb.AppendLine($"OffWorldSteps: {OffWorldSteps}");
            sb.AppendLine($"GoodSteps: {GoodSteps}");
            sb.AppendLine($"TotalSteps: {TotalSteps}");
            sb.AppendLine("Episode:");
            foreach (var state in Episode)
            {
                sb.AppendLine($" {state}");
            }

            return sb.ToString();
        }

        public void Race(ref List<State> states)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var currentState = Episode.First();
            var currentAction = currentState.Policy.First();
            var randomNumberGenerator = new Random(DateTime.Now.Millisecond);
            do
            {
                int dvx = currentState.VX + currentAction.X;
                int dvy = currentState.VY + currentAction.Y;

                // Max velocity -4 || 4
                dvx = dvx < -4 ? -4 : dvx > 4 ? 4 : dvx;
                dvy = dvy < -4 ? -4 : dvy > 4 ? 4 : dvy;

                int dx = currentState.X + dvx;
                int dy = currentState.Y + dvy;

                State nextState = null;
                switch (randomNumberGenerator.Next(0,3))
                {
                    // 50% chance no slide
                    case 0:
                    case 1:
                        NoSlideSteps++;
                        nextState = states.SingleOrDefault(s => s.X == dx && s.Y == dy && s.VX == dvx && s.VY == dvy);
                        if (nextState == null)
                        {
                            TotalReward += -5;
                            OffWorldSteps++;
                            nextState = states.SingleOrDefault(s => s.X == dx && s.Y == dy && s.VX == 0 && s.VY == 0);
                        }
                        else
                        {
                            TotalReward += -1;
                            GoodSteps++;
                        }

                        break;
                    // 25% chance slide up
                    case 2:
                        SlideUpSteps++;
                        // If the slide up takes the agent off track, then move it back on track with 0 velocity
                        nextState = states.SingleOrDefault(s => s.X == dx && s.Y == dy - 1 && s.VX == dvx && s.VY == dvy);
                        if (nextState == null)
                        {
                            TotalReward += -5;
                            OffWorldSteps++;
                            nextState = states.SingleOrDefault(s => s.X == dx && s.Y == dy && s.VX == 0 && s.VY == 0);
                        }
                        else
                        {
                            TotalReward += -1;
                            GoodSteps++;
                        }
                                    
                        break;
                    // 25% chance slide right
                    case 3:
                        // If the slide right takes the agent off track, then move it back on track with 0 velocity
                        nextState = states.SingleOrDefault(s => s.X == dx + 1 && s.Y == dy && s.VX == dvx && s.VY == dvy);
                        SlideRightSteps++;
                        if (nextState == null)
                        {
                            TotalReward += -5;
                            OffWorldSteps++;
                            nextState = states.SingleOrDefault(s => s.X == dx && s.Y == dy && s.VX == 0 && s.VY == 0);
                        }
                        else
                        {
                            TotalReward += -1;
                            GoodSteps++;
                        }

                        break;
                }

                if (nextState == null 
                    || Episode.Exists(
                        e => e.X == nextState.X 
                        && e.Y == nextState.Y 
                        && e.VX == nextState.VX 
                        && e.VY == nextState.VY)
                    || nextState.Policy == null 
                    || !nextState.Policy.Any())
                {
                    FailedRace = true;
                    Episode.Add(nextState);
                    break;
                }

                Episode.Add(nextState);
                currentState = nextState;
                currentAction = nextState.Policy.First();

            } while (currentState.Type != StateType.Goal);

            stopwatch.Stop();
            RaceTime = stopwatch.Elapsed.ToString();
            TotalSteps = Episode.Count;
        }
    }
}