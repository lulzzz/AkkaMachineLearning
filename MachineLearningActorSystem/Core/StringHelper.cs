using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.MachineLearning;
using Accord.Statistics.Analysis;
using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Core
{
    public static class StringHelper
    {
        public static string PrintConfusionMatrixString(List<ConfusionMatrix> cms,
            List<KeyValuePair<int, string>> classes, string dataSetName, string classifierType, string splittinMethod)
        {
            var sb = new StringBuilder();

            sb.AppendLine("------------------------------------------------------------------------------------------");
            sb.AppendLine($" {dataSetName}, {classifierType}, {splittinMethod}, Confusion Matrix");
            sb.AppendLine("------------------------------------------------------------------------------------------");
            sb.AppendLine(" Class | T Pos | F Pos | T Neg | F Neg | Sensitivity | Specificity | Efficiency | Accuracy");
            sb.AppendLine("------------------------------------------------------------------------------------------");

            for (var i = 0; i < cms.Count; i++)
                sb.AppendLine(
                    classes.Single(x => x.Key == i).Value.PadLeft(5)
                    + cms[i].TruePositives.ToString().PadLeft(8)
                    + cms[i].FalsePositives.ToString().PadLeft(8)
                    + cms[i].TrueNegatives.ToString().PadLeft(8)
                    + cms[i].FalseNegatives.ToString().PadLeft(8)
                    + cms[i].Sensitivity.ToString("0.0000").PadLeft(14)
                    + cms[i].Specificity.ToString("0.0000").PadLeft(14)
                    + cms[i].Efficiency.ToString("0.0000").PadLeft(13)
                    + cms[i].Accuracy.ToString("0.0000").PadLeft(11)
                );

            sb.AppendLine("------------------------------------------------------------------------------------------");

            sb.AppendLine("        "
                          + cms.Select(x => x.TruePositives).Sum().ToString("0").PadLeft(5)
                          + cms.Select(x => x.FalsePositives).Sum().ToString("0").PadLeft(8)
                          + cms.Select(x => x.TrueNegatives).Sum().ToString("0").PadLeft(8)
                          + cms.Select(x => x.FalseNegatives).Sum().ToString("0").PadLeft(8)
                          + cms.Select(x => x.Sensitivity).Average().ToString("0.0000").PadLeft(14)
                          + cms.Select(x => x.Specificity).Average().ToString("0.0000").PadLeft(14)
                          + cms.Select(x => x.Efficiency).Average().ToString("0.0000").PadLeft(13)
                          + cms.Select(x => x.Accuracy).Average().ToString("0.0000").PadLeft(11)
            );

            sb.AppendLine("------------------------------------------------------------------------------------------");

            return sb.ToString();
        }

        public static string PrintValidationResultString(List<ClassifierModel> classifierModels, string dataSetName,
            object oResult)
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------------------------------------------");
            sb.AppendLine($" {dataSetName}, {classifierModels.First().Type}");
            sb.AppendLine($" {classifierModels.First().Teacher.Type}, {classifierModels.First().Validator.Type}");
            sb.AppendLine("----------------------------------------------");
            sb.AppendLine(" Iteration | Training Error | Validation Error");
            sb.AppendLine("----------------------------------------------");

            foreach (var cm in classifierModels)
                sb.AppendLine(
                    "".PadLeft(9)
                    + cm.TrainingError.ToString("0.0000").PadLeft(17)
                    + cm.ValidationError.ToString("0.0000").PadLeft(19)
                );

            sb.AppendLine("----------------------------------------------");

            if (oResult is BootstrapResult)
                sb.AppendLine("         "
                              + ((BootstrapResult) oResult).Training.Mean.ToString("0.0000").PadLeft(17)
                              + ((BootstrapResult) oResult).Validation.Mean.ToString("0.0000").PadLeft(19)
                );
            else if (oResult is CrossValidationResult<object>)
                sb.AppendLine("         "
                              + ((CrossValidationResult<object>) oResult).Training.Mean.ToString("0.0000").PadLeft(17)
                              + ((CrossValidationResult<object>) oResult).Validation.Mean.ToString("0.0000").PadLeft(19)
                );

            sb.AppendLine("----------------------------------------------");

            return sb.ToString();
        }
    }
}