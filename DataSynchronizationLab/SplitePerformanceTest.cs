using DataSynchronizationLab.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSynchronizationLab
{
    [TestClass]
    public class SplitePerformanceTest
    {
        [TestMethod]
        public void PerformanceGet()
        {
            Stopwatch ProcessingTime = new Stopwatch();
            string KeyTime = ServiceKeyTime.Get();
            ProcessingTime.Stop();
            Console.WriteLine($"GetKeyTime");
            Console.WriteLine($"Processing KeyTime       : {ProcessingTime.Elapsed.TotalMilliseconds} ms");
        }
    }
}
