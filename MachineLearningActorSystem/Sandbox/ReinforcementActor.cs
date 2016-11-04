//using System;
//using System.Threading;
//using Accord.MachineLearning;
//using MachineLearningActorSystem.Core;
//using MachineLearningActorSystem.Events;
//using MachineLearningActorSystem.Models;

//namespace MachineLearningActorSystem.Actors.Explorers
//{
//    public class ReinforcementActor : BaseActor
//    {
//        // Q-Learning algorithm
//        private QLearning qLearning = null;
//        // Sarsa algorithm
//        private Sarsa sarsa = null;
//        // worker thread
//        private Thread workerThread = null;

//        private bool needToStop = false;

//        public ReinforcementActor()
//        {
//            Receive<QLearningEvent>(qLearningEvent =>
//            {
//                logger.Info($"{qLearningEvent.GetType().Name} Received");
//                // create new QLearning algorithm's instance
//                qLearning = new QLearning(256, 4, new TabuSearchExploration(4, new EpsilonGreedyExploration(Constants.ReinforcementExplorationRate)));
//                sarsa = null;
//                workerThread = new Thread(new ThreadStart(QLearningThread));
//                needToStop = false;
//                workerThread.Start();
//                var model = ReachGoal(qLearningEvent.Data);
//                Sender.Tell(model, Self);
//            });

//            Receive<SarsaLearningEvent>(sarsaLearningEvent =>
//            {
//                logger.Info($"{sarsaLearningEvent.GetType().Name} Received");
//                // create new Sarsa algorithm's instance
//                qLearning = null;
//                sarsa = new Sarsa(256, 4, new TabuSearchExploration(4, new EpsilonGreedyExploration(Constants.ReinforcementExplorationRate)));
//                workerThread = new Thread(new ThreadStart(SarsaThread));
//                needToStop = false;
//                workerThread.Start();
//                var model = ReachGoal(sarsaLearningEvent.Data);
//                Sender.Tell(model, Self);
//            });
//        }

//        private ExplorerResultEvent ReachGoal(RaceTrackModel mapDataModel)
//        {
//            var batchId = Guid.NewGuid();

//            return new ExplorerResultEvent(batchId.ToString());
//        }

//        private void QLearningThread()
//        {
//            int iteration = 0;
//            // curent coordinates of the agent
//            int agentCurrentX, agentCurrentY;
//            // exploration policy
//            TabuSearchExploration tabuPolicy = (TabuSearchExploration)qLearning.ExplorationPolicy;
//            EpsilonGreedyExploration explorationPolicy = (EpsilonGreedyExploration)tabuPolicy.BasePolicy;

//            // loop
//            while ((!needToStop) && (iteration < Constants.ReinforcementLearningIterations))
//            {
//                // set exploration rate for this iteration
//                explorationPolicy.Epsilon = Constants.ReinforcementExplorationRate - ((double)iteration / Constants.ReinforcementLearningIterations) * Constants.ReinforcementExplorationRate;
//                // set learning rate for this iteration
//                qLearning.LearningRate = Constants.ReinforcementLearningRate - ((double)iteration / Constants.ReinforcementLearningIterations) * Constants.ReinforcementLearningRate;
//                // clear tabu list
//                tabuPolicy.ResetTabuList();

//                // reset agent's coordinates to the starting position
//                agentCurrentX = agentStartX;
//                agentCurrentY = agentStartY;

//                // steps performed by agent to get to the goal
//                int steps = 0;

//                while ((!needToStop) && ((agentCurrentX != agentStopX) || (agentCurrentY != agentStopY)))
//                {
//                    steps++;
//                    // get agent's current state
//                    int currentState = GetStateNumber(agentCurrentX, agentCurrentY);
//                    // get the action for this state
//                    int action = qLearning.GetAction(currentState);
//                    // update agent's current position and get his reward
//                    double reward = UpdateAgentPosition(ref agentCurrentX, ref agentCurrentY, action);
//                    // get agent's next state
//                    int nextState = GetStateNumber(agentCurrentX, agentCurrentY);
//                    // do learning of the agent - update his Q-function
//                    qLearning.UpdateState(currentState, action, reward, nextState);

//                    // set tabu action
//                    tabuPolicy.SetTabuAction((action + 2) % 4, 1);
//                }

//                System.Diagnostics.Debug.WriteLine(steps);

//                iteration++;
//            }
//        }

//        // Sarsa thread
//        private void SarsaThread()
//        {
//            int iteration = 0;
//            // curent coordinates of the agent
//            int agentCurrentX, agentCurrentY;
//            // exploration policy
//            TabuSearchExploration tabuPolicy = (TabuSearchExploration)sarsa.ExplorationPolicy;
//            EpsilonGreedyExploration explorationPolicy = (EpsilonGreedyExploration)tabuPolicy.BasePolicy;

//            // loop
//            while ((!needToStop) && (iteration < Constants.ReinforcementLearningIterations))
//            {
//                // set exploration rate for this iteration
//                explorationPolicy.Epsilon = Constants.ReinforcementExplorationRate - ((double)iteration / Constants.ReinforcementLearningIterations) * Constants.ReinforcementExplorationRate;
//                // set learning rate for this iteration
//                sarsa.LearningRate = Constants.ReinforcementLearningRate - ((double)iteration / Constants.ReinforcementLearningIterations) * Constants.ReinforcementLearningRate;
//                // clear tabu list
//                tabuPolicy.ResetTabuList();

//                // reset agent's coordinates to the starting position
//                agentCurrentX = agentStartX;
//                agentCurrentY = agentStartY;

//                // steps performed by agent to get to the goal
//                int steps = 1;
//                // previous state and action
//                int previousState = GetStateNumber(agentCurrentX, agentCurrentY);
//                int previousAction = sarsa.GetAction(previousState);
//                // update agent's current position and get his reward
//                double reward = UpdateAgentPosition(ref agentCurrentX, ref agentCurrentY, previousAction);

//                while ((!needToStop) && ((agentCurrentX != agentStopX) || (agentCurrentY != agentStopY)))
//                {
//                    steps++;

//                    // set tabu action
//                    tabuPolicy.SetTabuAction((previousAction + 2) % 4, 1);

//                    // get agent's next state
//                    int nextState = GetStateNumber(agentCurrentX, agentCurrentY);
//                    // get agent's next action
//                    int nextAction = sarsa.GetAction(nextState);
//                    // do learning of the agent - update his Q-function
//                    sarsa.UpdateState(previousState, previousAction, reward, nextState, nextAction);

//                    // update agent's new position and get his reward
//                    reward = UpdateAgentPosition(ref agentCurrentX, ref agentCurrentY, nextAction);

//                    previousState = nextState;
//                    previousAction = nextAction;
//                }

//                if (!needToStop)
//                {
//                    // update Q-function if terminal state was reached
//                    sarsa.UpdateState(previousState, previousAction, reward);
//                }

//                System.Diagnostics.Debug.WriteLine(steps);

//                iteration++;

//            }
//        }
//    }
//}

