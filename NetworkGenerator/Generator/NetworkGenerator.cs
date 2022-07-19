using PythonIntegration;
using NetworkGenerator.Generator.PythonScripts;
using System;
using NetworkGenerator.NetworkImporter.NetworkFileImporter;
using System.Collections.Generic;

namespace NetworkGenerator.Generator
{
    public class NetworkGenerator
    {
        
        public NetworkGenerator()
        {

        }

        public Network.Network GenerateNetwork()
        {
            var output = Python.Run(PythonScriptsResources.GetScriptPath(PythonScriptsResources.GenerateNetworkScript), 
                new List<string> { "200", "0.05", "-u" } );

            if (!output.Success) throw new Exception("Failed to run python script!"); 
            
            return new NetworkStringImporter().Import(output.Output, 1);
        }
    }
}
