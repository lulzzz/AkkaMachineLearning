namespace Sandbox
{
    public interface IDemo
    {
        void Execute();
        void SetClassifier(string name, string[] options);
        void SetFilter(string name, string[] options);
        void SetTraining(string name);
        string ToString();
        string Usage();
    }
}