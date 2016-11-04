using System;

namespace MachineLearningActorSystem.Models
{
    [Serializable]
    public class ValidatorModel
    {
        public string Type { get; set; } // CrossValidation, ...
        public int Samples { get; set; }
        public int? Folds { get; set; }
        public int? SubSamples { get; set; }
    }
}