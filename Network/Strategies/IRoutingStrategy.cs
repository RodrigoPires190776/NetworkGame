using Network.Components;

namespace Network.Strategies
{
    public interface IRoutingStrategy
    {
        public Link NextLink(Router router, Packet packet);
    }
}
