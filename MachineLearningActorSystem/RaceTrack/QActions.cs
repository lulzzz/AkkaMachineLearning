using System;
using System.Collections.Generic;
using System.Linq;
using MachineLearningActorSystem.Core;

namespace MachineLearningActorSystem.RaceTrack
{
    /// <summary>
    ///     Wrapper for available actions
    /// </summary>
    [Serializable]
    public class QActions
    {
        public QActions()
        {
            Actions = new List<QAction>();
        }

        /// <summary>
        ///     Initialize actions with random weight vectors
        /// </summary>
        /// <param name="intitWithRandomWeights"></param>
        public QActions(bool intitWithRandomWeights)
        {
            Actions = new List<QAction>();
            for (var ax = -1; ax <= 1; ax++)
                for (var ay = -1; ay <= 1; ay++)
                    Actions.Add(new QAction(ax, ay));
        }

        public List<QAction> Actions { get; set; }

        public double GetMaxExpectedQValue(QState cs)
        {
            double? maxQ = null;
            foreach (var a in Actions)
                maxQ = maxQ.HasValue ? Math.Max(maxQ.Value, a.GetExpectedQValue(cs)) : maxQ = a.GetExpectedQValue(cs);
            return maxQ ?? 0;
        }

        public QAction GetActionWithMaxExpectedQValue(QState cs)
        {
            var qS = new List<Tuple<int, double>>();
            for (var i = 0; i < Actions.Count; i++)
                if (!((cs.VX == 0) && (cs.VY == 0) && (Actions[i].X == 0) && (Actions[i].Y == 0)))
                {
                    var currentQ = Actions[i].GetExpectedQValue(cs);
                    qS.Add(new Tuple<int, double>(i, currentQ));
                }

            var maxQ = qS.Max(x => x.Item2);
            var random = new Random();
            var maxActions = qS.Where(x => x.Item2 == maxQ).ToList();
            var ri = random.Next(0, maxActions.Count());
            return Actions[maxActions[ri].Item1];
        }

        public QAction GetActionWithMaxQValue(QState cs)
        {
            var qS = new List<Tuple<int, double>>();
            for (var i = 0; i < Actions.Count; i++)
                if (!((cs.VX == 0) && (cs.VY == 0) && (Actions[i].X == 0) && (Actions[i].Y == 0)))
                {
                    var currentQ = Actions[i].GetQValue(cs);
                    qS.Add(new Tuple<int, double>(i, currentQ));
                }

            var maxQ = qS.Max(x => x.Item2);
            var random = new Random();
            var maxActions = qS.Where(x => x.Item2 == maxQ).ToList();
            var ri = random.Next(0, maxActions.Count());
            return Actions[maxActions[ri].Item1];
        }

        /// <summary>
        ///     Gets the action with the highest expected Q value.
        ///     Will pick random actions based on exploration rate.
        ///     Exploration rate is currently only supported at rate of tenths
        /// </summary>
        /// <param name="state">Current State</param>
        /// <param name="explorationR">-1 to force randomness</param>
        /// <returns></returns>
        public QAction GetAction(QState cs, double explorationR)
        {
            var ri = 0;
            var random = new Random();
            if ((explorationR == -1) || ((explorationR > 0) && (random.Next(1, 10)/explorationR*10 == 1)))
            {
                ri = random.Next(0, Actions.Count);
                return Actions[ri];
            }
            if (cs.PrevQState == null)
                return GetActionWithMaxQValue(cs);
            return GetActionWithMaxExpectedQValue(cs);
        }

        public QAction GetAction(QState cs)
        {
            return GetAction(cs, Config.ExplorerExplorationRate);
        }

        public void UpdateWeights(QState s)
        {
            var maxAQ = GetMaxExpectedQValue(s);
            foreach (var action in Actions)
                action.UpdateWeights(s, maxAQ);
        }
    }
}