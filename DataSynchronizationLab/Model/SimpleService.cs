using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSynchronizationLab.Model
{
    public class SimpleService : IService
    {
        private IResourceSimple Resource { get; set; }
        private List<IClient> Clients { get; set; }

        public SimpleService(IResourceSimple Resource)
        {
            this.Resource = Resource;
            this.Resource.SubscribeResource(this);
            Clients = new List<IClient>();
        }

        public void SubscribeService(IClient Client)
        {
            if (!Clients.Any(c => c.GetHashCode() == Client.GetHashCode()))
            {
                Clients.Add(Client);
            }
        }

        public async Task AddObject(IHashObject DataObject) => await Resource.AddQueueDataAsync(DataObject);

        public void Boardcast(ILinkRowKey HashSync) => Parallel.ForEach(Clients, c => c.CallbackHashSync(HashSync));
    }
}
