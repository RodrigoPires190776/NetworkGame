using Network.Components;

namespace Network.Strategies
{
    public interface IPacketPickingStrategy
    {
        public Packet NextPacket(Router router);
    }
}
