using System;
using System.IO;
using System.Xml.Serialization;
using log4net;
using MachineLearningActorSystem.Models;
using MachineLearningActorSystem.RaceTrack;

namespace MachineLearningActorSystem.Core
{
    public static class XmlHelper
    {
        [NonSerialized] private static readonly ILog Logger = LogManager.GetLogger(typeof(XmlHelper));

        public static RaceTrack.RaceTrack LoadRaceTrack()
        {
            try
            {
                Logger.Info("Racetrack load started...");
                var raceTrack = Load<RaceTrack.RaceTrack>("Racetrack");
                Logger.Info("Done");
                return raceTrack;
            }
            catch (Exception)
            {
                try
                {
                    var raceTrack = Load<RaceTrack.RaceTrack>("Resources\\Racetrack");
                    Logger.Info("Done");
                    return raceTrack;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Failed", ex);
                    return null;
                }
            }
        }

        public static void SaveRaceTrack(RaceTrack.RaceTrack raceTrack)
        {
            try
            {
                Logger.Info("Racetrack save started...");
                Save(raceTrack, "Racetrack");
                Logger.Info("Done");
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed", ex);
            }
        }

        public static QRaceTrack LoadQRaceTrack()
        {
            try
            {
                Logger.Info("QRacetrack load started...");
                var qRaceTrack = Load<QRaceTrack>("QRacetrack");
                Logger.Info("Done");
                return qRaceTrack;
            }
            catch (Exception)
            {
                try
                {
                    var qRaceTrack = Load<QRaceTrack>("Resources\\QRacetrack");
                    Logger.Info("Done");
                    return qRaceTrack;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Failed", ex);
                    return null;
                }
            }
        }

        public static void SaveQRaceTrack(QRaceTrack qRaceTrack)
        {
            try
            {
                Logger.Info("QRacetrack save started...");
                Save(qRaceTrack, "QRacetrack");
                Logger.Info("Done");
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed", ex);
            }
        }

        public static Classifiers LoadClassifiers()
        {
            try
            {
                Logger.Info("Classifiers load started...");
                var dataModel = Load<Classifiers>("Classifiers");
                Logger.Info("Done");
                return dataModel;
            }
            catch (Exception)
            {
                try
                {
                    var dataModel = Load<Classifiers>("Resources\\Classifiers");
                    Logger.Info("Done");
                    return dataModel;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Failed", ex);
                    return null;
                }
            }
        }

        public static void SaveClassifiers(Classifiers dataModel)
        {
            try
            {
                Logger.Info("Classifiers save started...");
                Save(dataModel, "Classifiers");
                Logger.Info("Done");
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed", ex);
            }
        }

        private static T Load<T>(string filename)
        {
            string file = $".\\{filename}.xml";
            var formatter = new XmlSerializer(typeof(T));
            using (var aFile = new FileStream(file, FileMode.Open))
            {
                var buffer = new byte[aFile.Length];
                aFile.Read(buffer, 0, (int) aFile.Length);
                var stream = new MemoryStream(buffer);
                return (T) formatter.Deserialize(stream);
            }
        }

        private static void Save(object model, string filename)
        {
            string path = $".\\{filename}.xml";
            using (var outFile = File.Create(path))
            {
                var formatter = new XmlSerializer(model.GetType());
                formatter.Serialize(outFile, model);
            }
        }
    }
}