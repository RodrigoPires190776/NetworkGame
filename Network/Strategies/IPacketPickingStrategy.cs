using Network.Components;

namespace Network.Strategies
{
    public interface IPacketPickingStrategy
    {
        Packet NextPacket(Router router);
    }
}
