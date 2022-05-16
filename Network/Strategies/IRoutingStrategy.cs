using Network.Components;

namespace Network.Strategies
{
    public interface IRoutingStrategy
    {
        Link NextLink(Router router, Packet packet);
    }
}
