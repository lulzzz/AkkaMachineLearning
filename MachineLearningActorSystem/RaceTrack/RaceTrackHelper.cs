using System;
using System.IO;
using System.Xml.Serialization;

namespace MachineLearningActorSystem.RaceTrack
{
    public static class RaceTrackHelper
    {
        public static RaceTrack LoadRaceTrack()
        {
            try
            {
                Console.WriteLine("Racetrack load started");
                var raceTrack = Load<RaceTrack>("Racetrack");
                Console.WriteLine("Racetrack load done");
                return raceTrack;
            }
            catch (Exception)
            {
                Console.WriteLine("Racetrack load failed");
                return null;
            }
        }

        public static void SaveRaceTrack(RaceTrack raceTrack)
        {
            try
            {
                Console.WriteLine("Racetrack save started");
                Save(raceTrack, "Racetrack");
                Console.WriteLine("Racetrack save done");
            }
            catch (Exception)
            {
                Console.WriteLine("Racetrack save failed");
            }
        }

        public static QRaceTrack LoadQRaceTrack()
        {
            try
            {
                Console.WriteLine("QRacetrack load started");
                var qRaceTrack = Load<QRaceTrack>("QRacetrack");
                Console.WriteLine("QRacetrack load done");
                return qRaceTrack;
            }
            catch (Exception)
            {
                Console.WriteLine("Racetrack load failed");
                return null;
            }
        }

        public static void SaveQRaceTrack(QRaceTrack qRaceTrack)
        {
            try
            {
                Console.WriteLine("QRacetrack save started");
                Save(qRaceTrack, "QRacetrack");
                Console.WriteLine("QRacetrack save done");
            }
            catch (Exception)
            {
                Console.WriteLine("QRacetrack save failed");
            }
        }

        public static T Load<T>(string filename)
        {
            string file = $".\\{filename}.xml";
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            FileStream aFile = new FileStream(file, FileMode.Open);
            byte[] buffer = new byte[aFile.Length];
            aFile.Read(buffer, 0, (int)aFile.Length);
            MemoryStream stream = new MemoryStream(buffer);
            return (T)formatter.Deserialize(stream);
        }

        public static void Save(object model, string filename)
        {
            string path = $".\\{filename}.xml";
            FileStream outFile = File.Create(path);
            XmlSerializer formatter = new XmlSerializer(model.GetType());
            formatter.Serialize(outFile, model);
        }
    }
}