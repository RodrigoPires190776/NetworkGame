#nullable enable
using Network.Components;

namespace Network.Strategies
{
    public interface IPacketCreationStrategy
    {
        public Packet? CreatePacket(Router router);
    }
}
