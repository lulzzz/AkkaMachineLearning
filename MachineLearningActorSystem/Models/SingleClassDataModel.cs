using System;
using System.Collections.Generic;

namespace MachineLearningActorSystem.Models
{
    [Serializable]
    public class SingleClassDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[][] Inputs { get; set; }
        public int[] Outputs { get; set; }
        public string[] AttributeNames { get; set; }
        public string ClassAttributeName { get; set; }
        public List<KeyValuePair<int, string>> Classes { get; set; }
    }
}