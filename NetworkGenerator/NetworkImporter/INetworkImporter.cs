using Network.Strategies;

namespace NetworkGenerator.NetworkImporter
{
    public interface INetworkImporter<T>
    {
        Network.Network Import(T file, int smallestLink);
    }
}
