using System.IO;

namespace NetworkGenerator.NetworkImporter.NetworkFileImporter
{
    public class NetworkStringImporter : NetworkImporter<string>
    {
        private StringReader Reader { get; set; }
        protected override void Initialize(string networkData, int smallestLink)
        {
            SmallestLink = smallestLink;
            Reader = new StringReader(networkData);
        }
        protected override string GetNextLine()
        {
            return Reader.ReadLine();
        }

        public override Network.Network Import(string networkData, int smallestLink)
        {
            Initialize(networkData, smallestLink);
            return base.Import();
        }
    }
}
