using System;

namespace MachineLearningActorSystem.Models
{
    [Serializable]
    public class HyperParametersModel
    {
        public int? Join { get; set; }
        public int? SplitStep { get; set; }
        public bool? Diagonal { get; set; }
        public bool? Robust { get; set; }
        public int? K { get; set; }
        public double? LearningRate { get; set; }
        public double? SigmoidAlphaValue { get; set; }
        public int? NeuronsInFirstLayer { get; set; }
        public int? Iterations { get; set; }
        public bool? UseNguyenWidrow { get; set; }
        public bool? UseSameWeights { get; set; }
        public double? Momentum { get; set; }
    }
}