using System.IO;

namespace NetworkGenerator.NetworkImporter.NetworkFile
{
    public sealed class NetworkFileImporter : NetworkImporter<Stream>
    {
        private StreamReader Reader { get; set; }
        protected override void Initialize(Stream networkData, int smallestLink)
        {
            SmallestLink = smallestLink;
            Reader = new StreamReader(networkData);
        }
        protected override string GetNextLine()
        {
           return Reader.ReadLine();
        }

        public override Network.Network Import(Stream networkData, int smallestLink)
        {
            Initialize(networkData, smallestLink);
            return base.Import();
        }
    }
}
