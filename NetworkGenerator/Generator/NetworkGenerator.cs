using PythonIntegration;
using NetworkGenerator.Generator.PythonScripts;
using System;
using NetworkGenerator.NetworkImporter.NetworkFileImporter;
using System.Collections.Generic;
using NetworkUtils;

namespace NetworkGenerator.Generator
{
    public class NetworkGenerator
    {
        public static Dictionary<string, Property> GetProperties()
        {
            var dictionaryProperties = new Dictionary<string, Property>();

            dictionaryProperties.Add(Property.NumberOfNodes, new Property(Property.PropertyType.Integer, 
                new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INITIAL_VALUE, 2),
                        new Tuple<string, object>(Property.INTEGER_MIN, 2)
                    }));

            dictionaryProperties.Add(Property.ProbabilityOfLink, new Property(Property.PropertyType.Decimal,
                new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.DECIMAL_MIN, 0m),
                        new Tuple<string, object>(Property.DECIMAL_MAX, 1m)
                    }));

            dictionaryProperties.Add(Property.ShortestLinkLength, new Property(Property.PropertyType.Integer,
                new List<Tuple<string, object>>()
                    {
                        new Tuple<string, object>(Property.INITIAL_VALUE, 1),
                        new Tuple<string, object>(Property.INTEGER_MIN, 1)
                    }));

            return dictionaryProperties;
        }

        public NetworkGenerator()
        {

        }

        public Network.Network GenerateNetwork(Dictionary<string, Property> properties)
        {
            var output = Python.Run(PythonScriptsResources.GetScriptPath(PythonScriptsResources.GenerateNetworkScript), 
                new List<string> { 
                    properties[Property.NumberOfNodes].Value.ToString(), 
                    properties[Property.ProbabilityOfLink].Value.ToString(), 
                    "-u" } 
                );

            if (!output.Success) throw new Exception("Failed to run python script!"); 
            
            return new NetworkStringImporter().Import(output.Output, (int)properties[Property.ShortestLinkLength].Value);
        }
    }
}
