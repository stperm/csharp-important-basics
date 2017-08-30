using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingTests
{
    [TestFixture]
    [Ignore("TODO")]
    public class SynchronizationPrimitivesTests
    {
        private readonly object _locker = new object();
        private const string MyGuid = "E4C57CAF-3C69-4262-818C-A7F84CFD2760";

        [Test]
        public void Using_Monitor()
        {
            // TODO take a closer look at Wait and PulseAll here: https://stackoverflow.com/a/530228/4393579          
            // and here: https://msdn.microsoft.com/en-us/library/system.threading.monitor.pulse(v=vs.110).aspx  
            // and here: https://www.codeproject.com/Articles/28785/Thread-synchronization-Wait-and-Pulse-demystified
            Monitor.Enter(_locker);
            Assert.IsTrue(Monitor.IsEntered(_locker));
            
            bool sharedValue = false;
            Task.Factory.StartNew(() =>
            {
                lock (_locker) // the same as Monitor.Enter(_locker)
                    sharedValue = true;
            });
            Thread.Sleep(20);
            Assert.IsFalse(sharedValue);

            Assert.IsTrue(Monitor.TryEnter(_locker, 10)); // repeated enters from the same thread => OK
            Assert.IsFalse(Task.Factory.StartNew(() => Monitor.TryEnter(_locker, 10)).Result);

            Monitor.Exit(_locker);
            Thread.Sleep(20);
            Assert.IsFalse(sharedValue);

            Monitor.Exit(_locker); // we should exit twice because we locked twice
            Thread.Sleep(20);
            Assert.IsTrue(sharedValue);
        }

        [Test]
        public void UsingMutex()
        {
            // TODO: more interesting examples
            bool created;
            var mutex = new Mutex(true, MyGuid, out created);
            Assert.True(created);

            Assert.IsFalse(Task.Factory.StartNew(
            () =>
            {
                bool cr;
                var mut = new Mutex(true, MyGuid, out cr);
                return cr;
            }).Result);
            
    }

        [Test]
        public void Using_Semaphore()
        {
            // TODO: more interesting examples
            var sharedValue = 0;
            var semaphore = new Semaphore(0, 5, MyGuid);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    semaphore.WaitOne();
                    sharedValue++;
                }
            });

            semaphore.Release(3);
            Thread.Sleep(20);
            Assert.AreEqual(3, sharedValue);
        }

        [Test]
        public void Using_SemaphoreSlim()
        {

        }

        [Test]
        public void Using_SpinWait()
        {

        }

        [Test]
        public void Using_ReaderWriterLockSlim()
        {

        }
        
        [Test]
        public void Using_AutoResetEvent()
        {

        }

        [Test]
        public void Using_ManualResetEvent()
        {

        }

        [Test]
        public void Using_Interlocked()
        {
            long sum = 0;

            Parallel.For(0, 1000,
                i => Interlocked.Increment(ref sum));

            Assert.AreEqual(1000, sum);
        }

        [Test]
        public void Using_CountdownEvent()
        {
            var countdown = new CountdownEvent(4);
            countdown.Signal();
            Assert.AreEqual(3, countdown.CurrentCount);
            Task.Factory.StartNew(() => Thread.Sleep(50))
                .ContinueWith(_ => countdown.Signal(3));
            countdown.Wait();
            Assert.AreEqual(0, countdown.CurrentCount);
            Assert.IsTrue(countdown.IsSet);
        }
    }
}
