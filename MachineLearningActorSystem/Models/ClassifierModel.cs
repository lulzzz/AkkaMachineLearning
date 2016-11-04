using System;

namespace MachineLearningActorSystem.Models
{
    [Serializable]
    public class ClassifierModel
    {
        public ClassifierModel()
        {
            HyperParameters = new HyperParametersModel();
            Teacher = new TeacherModel();
            Validator = new ValidatorModel();
        }

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