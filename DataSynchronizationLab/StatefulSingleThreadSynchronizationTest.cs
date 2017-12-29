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
    public class StatefulSingleThreadSynchronizationTest
    {
        [TestMethod]
        public async Task StatefulSingleThreadSynchronization()
        {
            StatefulResourceSingleThread Source = new StatefulResourceSingleThread();

            IService NodeA = new SimpleService(Source);
            IService NodeB = new SimpleService(Source);

            IClient ClientA1 = new SimpleClient(NodeA) { KeyName = nameof(ClientA1) };
            IClient ClientA2 = new SimpleClient(NodeA) { KeyName = nameof(ClientA2) };

            IClient ClientB1 = new SimpleClient(NodeB) { KeyName = nameof(ClientB1) };
            IClient ClientB2 = new SimpleClient(NodeB) { KeyName = nameof(ClientB2) };

            Stopwatch PrepairingTime = new Stopwatch();
            PrepairingTime.Start();
            List<Task> Tasks = new List<Task>();
            for (int i = 0; i < TestParameter.Samping; i++)
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
            PrepairingTime.Stop();


            Stopwatch ProcessTime = new Stopwatch();
            ProcessTime.Start();
            await Task.WhenAll(Tasks.ToArray());
            ProcessTime.Stop();

            Source.Dispose();

            Assert.AreEqual(ClientA1.DataStorages.Count, TestParameter.Samping);
            Assert.AreEqual(ClientA2.DataStorages.Count, TestParameter.Samping);
            Assert.AreEqual(ClientB1.DataStorages.Count, TestParameter.Samping);
            Assert.AreEqual(ClientB2.DataStorages.Count, TestParameter.Samping);

            Console.WriteLine($"StatefulSingleThreadSynchronization");
            Console.WriteLine($"Storage Read Time       : {TestParameter.StorageReadTime_ms} ms");
            Console.WriteLine($"Storage Write Time      : {TestParameter.StorageWriteTime_ms} ms");
            Console.WriteLine($"Sampling                : {TestParameter.Samping} t");
            Console.WriteLine($"Prepairing Time         : {PrepairingTime.Elapsed.TotalMilliseconds/1000} s");
            Console.WriteLine($"Process Time            : {ProcessTime.Elapsed.TotalMilliseconds / 1000} s");
            Console.WriteLine($"Transaction per Seconds : {(TestParameter.Samping) / (ProcessTime.Elapsed.TotalMilliseconds / 1000) } t/s");
            Console.WriteLine($"Client Receive          : {ClientA1.DataStorages.Count}, {ClientA2.DataStorages.Count}, {ClientB1.DataStorages.Count}, {ClientB2.DataStorages.Count}");
            Console.WriteLine($"Client Conflic          : {ClientA1.Conflic}, {ClientA2.Conflic}, {ClientB1.Conflic}, {ClientB2.Conflic}");
        }
    }

    public class StatefulResourceSingleThread : IResourceSimple, IDisposable
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

                        // Delay Read from Storage
                        await Task.Delay(TestParameter.StorageReadTime_ms);

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
                    // Delay Write to Storage
                    await Task.Delay(TestParameter.StorageWriteTime_ms);
                }
            }
            finally
            {
                WaitingThread.Release();
            }
        }

        private List<IService> ServiceNodes = new List<IService>();
        private void NotifyHashSync(ILinkRowKey HashSync) => Parallel.ForEach(ServiceNodes, s => s.Boardcast(HashSync));
        public void SubscribeResource(IService Service)
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
