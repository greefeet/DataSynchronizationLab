using System.Threading.Tasks;

namespace DataSynchronizationLab.Model
{
    public interface IService
    {
        Task AddObject(IHashObject DataObject);
        void SubscribeService(IClient Client);
        void Boardcast(ILinkRowKey HashSync);
    }
}
