using PicNetML.Arff;

namespace Sandbox.Models
{
    public class WeatherRow
    {
        [Nominal("sunny,overcast,rainy")]
        public string outlook { get; set; }

        [IgnoreFeature]
        public double temperature { get; set; }

        [IgnoreFeature]
        public double humidity { get; set; }

        [Nominal("male,female")]
        public string sex { get; set; }

        [Nominal("TRUE, FALSE")]
        public bool windy { get; set; }

        [Nominal("yes,no")]
        public string play { get; set; }
    }
}