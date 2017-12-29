﻿using DataSynchronizationLab.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataSynchronizationLab
{
    [TestClass]
    public class SingleThreadBlockSynchronizationTest
    {
        public const int StorageReadTime_ms = 10;
        public const int StorageWriteTime_ms = 40;
        public const int Samping = 500;
        public const int BlockSize = 100;

        [TestMethod]
        public async Task SingleThread_Block_SimultaneousMessageTest()
        {
            ResourceSingleThreadBlock Source = new ResourceSingleThreadBlock();
            ServiceSingleThreadBlock NodeA = new ServiceSingleThreadBlock(Source);
            ServiceSingleThreadBlock NodeB = new ServiceSingleThreadBlock(Source);

            ClientSingleThreadBlock ClientA1 = new ClientSingleThreadBlock(NodeA) { KeyName = nameof(ClientA1) };
            ClientSingleThreadBlock ClientA2 = new ClientSingleThreadBlock(NodeA) { KeyName = nameof(ClientA2) };

            ClientSingleThreadBlock ClientB1 = new ClientSingleThreadBlock(NodeB) { KeyName = nameof(ClientB1) };
            ClientSingleThreadBlock ClientB2 = new ClientSingleThreadBlock(NodeB) { KeyName = nameof(ClientB2) };

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
            PrepairingTime.Stop();


            Stopwatch ProcessTime = new Stopwatch();
            ProcessTime.Start();
            await Task.WhenAll(Tasks.ToArray());
            ProcessTime.Stop();

            Source.Dispose();

            Assert.AreEqual(ClientA1.DataStorages.Count, Samping);
            Assert.AreEqual(ClientA2.DataStorages.Count, Samping);
            Assert.AreEqual(ClientB1.DataStorages.Count, Samping);
            Assert.AreEqual(ClientB2.DataStorages.Count, Samping);

            Console.WriteLine($"SingleThread_Block_SimultaneousMessageTest");
            Console.WriteLine($"Storage Read Time       : {StorageReadTime_ms} ms");
            Console.WriteLine($"Storage Write Time      : {StorageWriteTime_ms} ms");
            Console.WriteLine($"Block Size              : {BlockSize} t");
            Console.WriteLine($"Sampling                : {Samping + 4} t");
            Console.WriteLine($"Prepairing Time         : {PrepairingTime.Elapsed.TotalMilliseconds / 1000} s");
            Console.WriteLine($"Process Time            : {ProcessTime.Elapsed.TotalMilliseconds / 1000} s");
            Console.WriteLine($"Transaction per Seconds : {(Samping + 4) / (ProcessTime.Elapsed.TotalMilliseconds / 1000) } t/s");
        }
    }

    public class ClientSingleThreadBlock : IClientHashCallback
    {
        public string KeyName { get; set; }
        public List<ILinkRowKey> DataStorages { get; set; } = new List<ILinkRowKey>();
        public override int GetHashCode()
        {
            return new { KeyName }.GetHashCode();
        }


        public async Task AddObject(IHashObject dataObject) => await Server.AddObject(dataObject);

        private ServiceSingleThreadBlock Server { get; set; }
        public ClientSingleThreadBlock(ServiceSingleThreadBlock Server)
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

    public class ServiceSingleThreadBlock
    {
        private ResourceSingleThreadBlock Source { get; set; }
        private List<IClientHashCallback> Clients { get; set; }

        public ServiceSingleThreadBlock(ResourceSingleThreadBlock Source)
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

    public class ResourceSingleThreadBlock : IDisposable
    {
        public Dictionary<int, IHashObject> Storage = new Dictionary<int, IHashObject>();
        public List<ILinkRowKey> HashSync = new List<ILinkRowKey>();

        private Queue<IHashObject> ProofHashSync = new Queue<IHashObject>();
        private SemaphoreSlim WaitingThread = new SemaphoreSlim(1);

        private int CounterSimBlock = 0;

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
                    CounterSimBlock++;

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
                        await Task.Delay(SingleThreadSynchronizationTest.StorageReadTime_ms);

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
                    
                    if(CounterSimBlock >= SingleThreadBlockSynchronizationTest.BlockSize)
                    {
                        CounterSimBlock = 0;
                        await Task.Delay(SingleThreadSynchronizationTest.StorageWriteTime_ms);
                    }
                }
            }
            finally
            {
                WaitingThread.Release();
            }
        }

        private List<ServiceSingleThreadBlock> ServiceNodes = new List<ServiceSingleThreadBlock>();
        private void NotifyHashSync(ILinkRowKey HashSync) => Parallel.ForEach(ServiceNodes, s => s.Boardcast(HashSync));
        public void SubscribeResource(ServiceSingleThreadBlock Service)
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
