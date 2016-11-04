namespace MachineLearningActorSystem.Events
{
    public class FileExplorerDataSourceEvent
    {
        public FileExplorerDataSourceEvent(string dataSourceFilePath, bool printLastRun)
        {
            DataSourceFilePath = dataSourceFilePath;
            PrintLastRun = printLastRun;
        }

        public string DataSourceFilePath { get; set; }

        public bool PrintLastRun { get; set; }
    }
}