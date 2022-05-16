using Network.Components;

namespace Network.Strategies
{
    public interface IPacketCreationStrategy
    {
        Packet CreatePacket(Router router);
    }
}
