namespace PythonIntegration
{
    public class PythonResult
    {
        public string Output { get; }
        public bool Success { get; }
        public PythonResult(string output, bool success)
        {
            Output = output;
            Success = success;
        }
    }
}
