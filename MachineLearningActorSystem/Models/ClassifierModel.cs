using System;

namespace MachineLearningActorSystem.Models
{
    public class ClassifierModel
    {
        public int Id { get; set; }
        public string BatchId { get; set; } 
        public string DataSetName { get; set; }
        public string Type { get; set; } // DecisionTree, ...
        public double TrainingError { get; set; }
        public double ValidationError { get; set; }
        public byte[] TrainedModelBinary { get; set; }
        public HyperParametersModel HyperParameters { get; set; }
        public TeacherModel Teacher { get; set; }
        public ValidatorModel Validator { get; set; }
    }
}
