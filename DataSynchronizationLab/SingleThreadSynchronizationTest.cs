using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using DataSynchronizationLab.Model;

namespace DataSynchronizationLab
{
    [TestClass]
    public class SingleThreadSynchronizationTest
    {
        public const int StorageInsertTime = 1;
        public const int Samping = 500;
        [TestMethod]
        public async Task BlockThread_SimultaneousMessageTest()
        {
            ResourceSingleThread Source = new ResourceSingleThread();
            ServiceSingleThread NodeA = new ServiceSingleThread(Source);
            ServiceSingleThread NodeB = new ServiceSingleThread(Source);

            ClientSingleThread ClientA1 = new ClientSingleThread(NodeA) { KeyName = nameof(ClientA1) };
            ClientSingleThread ClientA2 = new ClientSingleThread(NodeA) { KeyName = nameof(ClientA2) };

            ClientSingleThread ClientB1 = new ClientSingleThread(NodeB) { KeyName = nameof(ClientB1) };
            ClientSingleThread ClientB2 = new ClientSingleThread(NodeB) { KeyName = nameof(ClientB2) };


            await ClientA1.AddObject(new DataObject() { Key = "Fedfe", ValueInt = 123, ValueString = "Arabe2" });
            Assert.AreEqual(ClientA1.DataStorages.Count, 1);
            Assert.AreEqual(ClientA2.DataStorages.Count, 1);

            await ClientA2.AddObject(new DataObject() { Key = "Fedfe33", ValueInt = 12312, ValueString = "Arabe3" });

            Assert.AreEqual(ClientA1.DataStorages.Count, 2);
            Assert.AreEqual(ClientA2.DataStorages.Count, 2);
            Assert.AreEqual(ClientB1.DataStorages.Count, 2);
            Assert.AreEqual(ClientB2.DataStorages.Count, 2);

            Stopwatch PrepairingTime = new Stopwatch();
            PrepairingTime.Start();
            List<Task> Tasks = new List<Task>();
            for (int i = 0; i < Samping; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        Tasks.Add(ClientA1.AddObject(new DataObject() { Key = $"Fedfe3321{i}", ValueInt = 22 + i, ValueString = $"Ar123{i}" }));
                        break;
                    case 1:
                        Tasks.Add(ClientA2.AddObject(new DataObject() { Key = $"Fedfe3321{i}", ValueInt = 22 + i, ValueString = $"Ar123{i}" }));
                        break;
                    case 2:
                        Tasks.Add(ClientB1.AddObject(new DataObject() { Key = $"Fedfe3321{i}", ValueInt = 22 + i, ValueString = $"Ar123{i}" }));
                        break;
                    case 3:
                        Tasks.Add(ClientB2.AddObject(new DataObject() { Key = $"Fedfe3321{i}", ValueInt = 22 + i, ValueString = $"Ar123{i}" }));
                        break;
                }

            }
            Tasks.Add(ClientA2.AddObject(new DataObject() { Key = "Fedfe3321", ValueInt = 22, ValueString = "Ar123" }));
            Tasks.Add(ClientB1.AddObject(new DataObject() { Key = "Fedfe3322", ValueInt = 33, ValueString = "Asdfabe3" }));
            PrepairingTime.Stop();


            Stopwatch ProcessTime = new Stopwatch();
            ProcessTime.Start();
            await Task.WhenAll(Tasks.ToArray());
            ProcessTime.Stop();

            Source.Dispose();

            Assert.AreEqual(ClientA1.DataStorages.Count, 2 + Samping + 2);
            Assert.AreEqual(ClientA2.DataStorages.Count, 2 + Samping + 2);
            Assert.AreEqual(ClientB1.DataStorages.Count, 2 + Samping + 2);
            Assert.AreEqual(ClientB2.DataStorages.Count, 2 + Samping + 2);

            Console.WriteLine($"BlockThread_SimultaneousMessageTest");
            Console.WriteLine($"Storage Execute Time    : {StorageInsertTime} ms");
            Console.WriteLine($"Sampling                : {Samping + 4} t");
            Console.WriteLine($"Prepairing Time         : {PrepairingTime.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"Process Time            : {ProcessTime.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"Transaction per Seconds : {(Samping + 4) / (ProcessTime.Elapsed.TotalMilliseconds / 1000) } t/s");
        }
    }

    public class ClientSingleThread : IClientHashCallback
    {
        public string KeyName { get; set; }
        public List<ILinkRowKey> DataStorages { get; set; } = new List<ILinkRowKey>();
        public override int GetHashCode()
        {
            return new { KeyName }.GetHashCode();
        }


        public async Task AddObject(IHashObject dataObject) => await Server.AddObject(dataObject);

        private ServiceSingleThread Server { get; set; }
        public ClientSingleThread(ServiceSingleThread Server)
        {
            this.Server = Server;
            this.Server.SubscribeService(this);
        }

        public void CallbackHashSync(ILinkRowKey LinkRowKey)
        {
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
        }
    }

    public class ServiceSingleThread
    {
        private ResourceSingleThread Source { get; set; }
        private List<IClientHashCallback> Clients { get; set; }

        public ServiceSingleThread(ResourceSingleThread Source)
        {
            this.Source = Source;
            this.Source.SubscribeResource(this);
            Clients = new List<IClientHashCallback>();
        }

        public void SubscribeService(IClientHashCallback Client)
        {
            if (!Clients.Any(c => c.GetHashCode() == Client.GetHashCode()))
            {
                Clients.Add(Client);
            }
        }

        public async Task AddObject(IHashObject DataObject) => await Source.AddQueueDataAsync(DataObject);

        public void Boardcast(ILinkRowKey HashSync) => Parallel.ForEach(Clients, c => c.CallbackHashSync(HashSync));
    }

    public class ResourceSingleThread : IDisposable
    {
        public Dictionary<int, IHashObject> Storage = new Dictionary<int, IHashObject>();
        public List<ILinkRowKey> HashSync = new List<ILinkRowKey>();

        private Queue<IHashObject> ProofHashSync = new Queue<IHashObject>();
        private SemaphoreSlim WaitingThread = new SemaphoreSlim(1);

        private void StoreData(IHashObject Data)
        {
            Storage.Add(Data.GetHashCode(), Data);
            /*
            // Check Equal Data (unnecessary)
            if (Storage.ContainsKey(Data.GetHashCode()))
            {
                Storage[Data.GetHashCode()] = Data;
            }
            else
            {
                Storage.Add(Data.GetHashCode(), Data);
            }
            */
        }
        private async Task TriggerProofHash()
        {
            await WaitingThread.WaitAsync();
            try
            {
                while (ProofHashSync.Count > 0)
                {
                    // Check is First Sync
                    if (HashSync.Count == 0)
                    {
                        // First Sync
                        var Data = ProofHashSync.Dequeue();
                        HashSync.Add(new LinkHashObject()
                        {
                            PreviousRowKey = "",
                            RowKey = ServiceKeyTime.Get()
                        });
                        NotifyHashSync(HashSync.Last());
                    }
                    else
                    {
                        // After First

                        // Add Data
                        var Data = ProofHashSync.Dequeue();
                        var PreviousHashSync = HashSync.Last();
                        HashSync.Add(new LinkHashObject()
                        {
                            PreviousRowKey = PreviousHashSync.RowKey,
                            RowKey = ServiceKeyTime.Get()
                        });

                        // Validate and Notify
                        var NowHashSync = HashSync.Last();
                        NotifyHashSync(NowHashSync);

                        /*
                        // Check Conflic (unnecessary)
                        if (NowHashSync.PreviousRowKey == PreviousHashSync.RowKey)
                        {
                            // Currect
                            NotifyHashSync(NowHashSync);
                        }
                        else
                        {
                            // Incurrect need to Rollback
                            HashSync.RemoveAt(HashSync.Count - 1);
                        }
                        */
                    }
                    await Task.Delay(SingleThreadSynchronizationTest.StorageInsertTime);
                }
            }
            finally
            {
                WaitingThread.Release();
            }
        }

        private List<ServiceSingleThread> ServiceNodes = new List<ServiceSingleThread>();
        private void NotifyHashSync(ILinkRowKey HashSync) => Parallel.ForEach(ServiceNodes, s => s.Boardcast(HashSync));
        public void SubscribeResource(ServiceSingleThread Service)
        {
            if (!ServiceNodes.Any(s => s == Service)) ServiceNodes.Add(Service);
        }

        public async Task AddQueueDataAsync(IHashObject Data)
        {
            StoreData(Data);
            ProofHashSync.Enqueue(Data);
            await TriggerProofHash();
        }
        public void Dispose()
        {
            WaitingThread.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
