using System;
using System.Collections.Generic;
using MachineLearningActorSystem.Core;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QAction
    {
        public QAction()
        {
            W = new List<double>();
        }

        /// <summary>
        ///     Initialize action for x, y axes with random weights for X,Y,VX,VY
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public QAction(int x, int y)
        {
            X = x;
            Y = y;
            W = new List<double> {1};
            for (var i = 0; i < 4; i++)
                W.Add(GetRandomWeight());
        }

        public int X { get; set; }
        public int Y { get; set; }
        public List<double> W { get; set; }

        /// <summary>
        ///     Update Weight Vector For Action based on current State using default learning and discount rate.
        /// </summary>
        /// <param name="s">Current state</param>
        /// <param name="a">Actions wrapper</param>
        /// <returns></returns>
        public void UpdateWeights(QState s, double maxAQ)
        {
            UpdateWeights(s, maxAQ, Config.ExplorerLearningRate, Config.ExplorerDiscountRate);
        }

        /// <summary>
        ///     Update Weight Vector For Action based on current State.
        /// </summary>
        /// <param name="s">Current state</param>
        /// <param name="a">Actions wrapper</param>
        /// <returns></returns>
        public void UpdateWeights(QState s, double maxAQ, double learningR, double discountR)
        {
            var reward = s.PrevQState != null
                ? Math.Sign(Math.Abs(s.Reward) + Math.Abs(s.PrevQState.Reward) - Math.Abs(s.PrevQState.Reward))
                : s.Reward;
            var nW = new List<double> {reward};
            for (var i = 1; i < 5; i++)
            {
                var w = UpdateWeight(W[i], s, maxAQ, learningR, discountR, reward);
                nW.Add(!double.IsNaN(w) && !double.IsInfinity(w) ? W[i] : w);
            }
            if (nW.Count == W.Count)
                W = new List<double>(nW);
        }


        /// <summary>
        ///     Update Parameter Weight For Action Weight vector based on current State
        /// </summary>
        /// <param name="weight">Weight of current action</param>
        /// <param name="s">Current state</param>
        /// <param name="a">Actions wrapper</param>
        /// <param name="learningR">Learning Rate</param>
        /// <param name="discountR">Discount Rate</param>
        /// <returns></returns>
        private double UpdateWeight(double weight, QState cs, double maxAQ, double learningR, double discountR,
            double reward)
        {
            var currentAQ = GetExpectedQValue(cs);
            //return weight += learningR * (reward + (discountR * maxAQ) - weight);
            return weight += learningR*(reward + discountR*maxAQ - currentAQ);
        }

        /// <summary>
        ///     Get Action QValue for state
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public double GetQValue(QState s)
        {
            return W[0] + W[1]*s.X + W[2]*s.Y + W[3]*s.VX + W[4]*s.VY;
        }

        /// <summary>
        ///     Get Expected Action QValue for state
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public double GetExpectedQValue(QState cs)
        {
            var dx = Math.Abs(Math.Abs(cs.VX) - Math.Abs(cs.PrevQState.VX));
            var dy = Math.Abs(Math.Abs(cs.VY) - Math.Abs(cs.PrevQState.VY));
            dx = (X < 0 ? Math.Sign(dx) : X > 0 ? dx : 0)*(X - cs.PrevQState.AX);
            dy = (Y < 0 ? Math.Sign(dy) : Y > 0 ? dy : 0)*(Y - cs.PrevQState.AY);
            return W[0] + W[1]*(cs.X + dx) + W[2]*(cs.Y + dy) + W[3]*(cs.VX + dx) + W[4]*(cs.VY + dy);
        }

        /// <summary>
        ///     Random number generator for initial weights
        /// </summary>
        /// <returns>A unique random number between [-1.0, 1.0]</returns>
        private double GetRandomWeight()
        {
            var maximum = 1.0;
            var minimum = -1.0;
            var random = new Random();
            var result = 0.0;
            do
            {
                result = random.NextDouble()*(maximum - minimum) + minimum;
            } while (W.Contains(result));
            return result;
        }
    }
}