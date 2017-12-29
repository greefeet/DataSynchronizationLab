using System.Threading.Tasks;

namespace DataSynchronizationLab.Model
{
    public interface IResourceSimple
    {
        void SubscribeResource(IService Service);
        Task AddQueueDataAsync(IHashObject Data);
    }
}
