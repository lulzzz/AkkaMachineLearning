using System;
using System.Collections.Generic;

namespace MachineLearningActorSystem.Models
{
    [Serializable]
    public class Classifiers
    {
        public Classifiers()
        {
            ClassifierModels = new List<ClassifierModel>();
            SingleClassDataModels = new List<SingleClassDataModel>();
        }

        public List<ClassifierModel> ClassifierModels { get; set; }
        public List<SingleClassDataModel> SingleClassDataModels { get; set; }
    }
}