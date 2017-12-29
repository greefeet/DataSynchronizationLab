using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSynchronizationLab.Model
{
    public class SimpleClient : IClient
    {
        public int Conflic => _Conflic;
        int _Conflic = 0;
        /*
        public int Conflic
        {
            get
            {
                try
                {
                    if (DataStorages.Count == 0) return 0;
                    int Counter = 0;
                    string LastRowKey = DataStorages.First().RowKey;
                    for (int i = 1; i < DataStorages.Count - 1; i++)
                    {
                        if(DataStorages != null)
                        {
                            if (DataStorages[i].PreviousRowKey != DataStorages[i - 1].RowKey)
                            {
                                Counter++;
                            }
                        }
                    }
                    return Counter;
                }
                catch(Exception Ex)
                {
                    return -1;
                }
                
            }
        }
        */
        public string KeyName { get; set; }
        public List<ILinkRowKey> DataStorages { get; set; } = new List<ILinkRowKey>();
        public override int GetHashCode()
        {
            return new { KeyName }.GetHashCode();
        }


        public async Task AddObject(IHashObject dataObject) => await Server.AddObject(dataObject);

        private IService Server { get; set; }
        public SimpleClient(IService Server)
        {
            this.Server = Server;
            this.Server.SubscribeService(this);
        }

        public void CallbackHashSync(ILinkRowKey LinkRowKey)
        {
            DataStorages.Add(LinkRowKey);
            if (DataStorages.Count > 1)
            {
                if(DataStorages[DataStorages.Count-1].PreviousRowKey != DataStorages[DataStorages.Count - 2].RowKey)
                {
                    _Conflic++;
                }
            }
            /*
            if (DataStorages.Count > 0)
            {
                // Only the link data will be accepted.
                if (DataStorages.Last().RowKey == LinkRowKey.PreviousRowKey)
                {
                    DataStorages.Add(LinkRowKey);
                }
            }
            else
            {
                DataStorages = new List<ILinkRowKey>() { LinkRowKey };
            }
            */
        }
    }
}
