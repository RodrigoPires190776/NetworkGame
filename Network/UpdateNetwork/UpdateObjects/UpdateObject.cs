using System;

namespace Network.UpdateNetwork.UpdateObjects
{
    public abstract class UpdateObject
    {
        public Guid ID { get; }
        protected UpdateObject(Guid id)
        {
            ID = id;
        }
    }
}
