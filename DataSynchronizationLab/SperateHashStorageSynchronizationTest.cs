using DataSynchronizationLab.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace DataSynchronizationLab
{
    [TestClass]
    public class SperateHashStorageSynchronizationTest
    {
        [TestMethod]
        public void SperateHashStorageSynchronization()
        {
            DataHashContract CreateDummy = new DataHashContract()
            {
                KeyTime = ServiceKeyTime.Get(),
                ValueInt = 123,
                ValueString = "SDFSDF"
            };
        }
    }

    public class StorageResource
    {

    }

    public class StorageHash
    {

    }

    public class EventContract
    {

    }

    public class ServiceNode
    {
        private StorageResource Storage { get; set; }
        private StorageHash StorageHash { get; set; }
        public ServiceNode(StorageResource Storage, StorageHash StorageHash)
        {
            this.Storage = Storage;
            this.StorageHash = StorageHash;


            //ประเด็นวิเคราะห์
            // 1. Hight Perfomance
            // 2. Hight Reliable 
            // 3. Light weight

            //จำลองสถานการณ์
            // - เริ่มต้นซิงค์ในระหว่างที่มี Transaction ขณะนั้นสูง ให้ได้อย่างไม่มีปัญหา
            // - ล็อกอินหลายเครื่อง Sync ได้อย่างไม่มีปัญหา
            // - เปิดโปรแกรมพร้อมกันหลายหน้าต่างได้อย่างไม่มีปัญหา

        }

        public void Create(DataHashContract Arabe)
        {

        }

        public void Update(string KeyTime, DataHashContract Arabe)
        {

        }

        public void Delete(string KeyTime)
        {

        }
    }

    [DataContract]
    public class DataEventHashContract : IHashable
    {
        [DataMember(Order = 1)]
        public string KeyTime { get; set; }

        [DataMember(Order = 2)]
        public string PreviousKeyTime { get; set; }

        [DataMember(Order = 3)]
        public int Hash { get; set; }

        public int GetHash() => new { KeyTime, PreviousKeyTime }.GetHashCode();
        public bool IsValidHash() => Hash == GetHash();
    }

    [DataContract]
    public class DataHashContract : IHashable
    {
        [DataMember(Order = 1)]
        public string KeyTime { get; set; }         // Client สร้างขึ้น

        [DataMember(Order = 2)]
        public string ValueString { get; set; }

        [DataMember(Order = 3)]
        public int ValueInt { get; set; }

        [DataMember(Order = 4)]
        public int Hash { get; set; }

        public int GetHash() => new { KeyTime, ValueString, ValueInt }.GetHashCode();
        public bool IsValidHash() => Hash == GetHash();
    }

    public enum EventType
    {
        Create = 0,
        Update = 1,
        Delete = 2
    }

    public interface IHashable
    {
        int GetHash();
        bool IsValidHash();
    }
}
