namespace MachineLearningActorSystem.Events
{
    public class FileExplorerDataSourceEvent
    {
        public FileExplorerDataSourceEvent(string dataSourceFilePath)
        {
            DataSourceFilePath = dataSourceFilePath;
        }

        public string DataSourceFilePath { get; set; }
    }
}
