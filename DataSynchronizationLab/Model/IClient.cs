using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataSynchronizationLab.Model
{
    public interface IClient
    {
        void CallbackHashSync(ILinkRowKey LinkRowKey);
        Task AddObject(IHashObject dataObject);
        List<ILinkRowKey> DataStorages { get; }
        int Conflic { get; }
    }
}
