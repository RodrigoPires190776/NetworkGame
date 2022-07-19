using System.IO;

namespace NetworkGenerator.Generator.PythonScripts
{
    internal static class PythonScriptsResources
    {
        private static string ScriptsFolder = "Generator\\PythonScripts\\";      
        internal static string GetScriptPath(string script)
        {
            return new FileInfo(ScriptsFolder + script).FullName;
        }

        internal static string GenerateNetworkScript = "GenerateNetwork.py";
    }
}
