using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Threading;

namespace ThreadingTests
{
    [TestFixture]
    public class ThreadingTests
    {
        [Test]
        public void UsingThreadStatic()
        {
            ThreadStaticExample.ThreadStaticField = 5;
            int valueOnAnotherThread = 5;

            var thread = new Thread(() => valueOnAnotherThread = ThreadStaticExample.ThreadStaticField);
            thread.Start();
            thread.Join();

            Assert.AreNotEqual(ThreadStaticExample.ThreadStaticField, valueOnAnotherThread);
            Assert.AreEqual(5, ThreadStaticExample.ThreadStaticField);
            Assert.AreEqual(0, valueOnAnotherThread);
        }

        [Test]
        public void UsingThreadLocal()
        {
            Func<int> getManagedThreadId = () => Thread.CurrentThread.ManagedThreadId;

            var firstInstance = ThreadLocalExampe<int>.Get(getManagedThreadId);
            ThreadLocalExampe<int> secondInstance = null;
            var thread = new Thread(obj =>
            {
                secondInstance = ThreadLocalExampe<int>.Get((Func<int>)obj);
            });

            thread.Start(getManagedThreadId);
            thread.Join();

            Assert.AreNotEqual(firstInstance.ThreadLocalField, secondInstance.ThreadLocalField);
        }

        [Test]
        public void UsingThreadPool()
        {
            var array = new int[1];
            var isCompleted = false;
            ThreadPool.QueueUserWorkItem(obj => 
            {
                var arr = (int[])obj;
                for(int i = 0; i < 8; i++)
                {
                    Thread.Sleep(50);
                    arr[0]++;
                }
                isCompleted = true;
            }, array);

            while (!isCompleted)
            {
                Thread.Sleep(200);
            }

            Assert.AreEqual(8, array[0]);
        }

        [Test]
        [Timeout(5000)]
        public void ThreadPoolWait()
        {
            var semaphore = new Semaphore(0, 1); // we are owning it
            var executedCount = 0;
            Action<object, bool> callback = (state, timedOut) => 
            {
                if (!timedOut)
                    executedCount++;
                (state as Semaphore)?.Release();
            };

            var waitHandle = ThreadPool.RegisterWaitForSingleObject(semaphore, new WaitOrTimerCallback(callback),
                state: semaphore, 
                millisecondsTimeOutInterval: 10000, 
                executeOnlyOnce: false);

            Thread.Sleep(100);
            semaphore.Release();            
            semaphore.WaitOne();
            Thread.Sleep(50);

            Assert.AreEqual(1, executedCount);

            semaphore.Release();
            semaphore.WaitOne();
            Thread.Sleep(50);

            Assert.AreEqual(2, executedCount);
            waitHandle.Unregister(semaphore);

            semaphore.Release();
            Assert.AreEqual(2, executedCount);
        }

        [Test]
        [Timeout(1000)]
        public void UsingThreadStartDelegate()
        {
            var cancelled = false;
            var counter = 0;
            var thread = new Thread(new ThreadStart(() =>
            {
                while (!cancelled)
                {
                    counter++;
                    Thread.Sleep(50);
                }
            }));
            thread.Start();

            Thread.Sleep(75);
            cancelled = true;
            thread.Join();

            Assert.AreEqual(2, counter);
        }

        [Test]
        public void UsingParameterizedThreadStartDelegate()
        {            
            var thread = new Thread(new ParameterizedThreadStart(ThreadMethod));
            thread.IsBackground = false; // false by defaul, just to memorize the property
            thread.Name = "my thread";
            
            var result = new int[1];
            thread.Start(result);
            thread.Join();
                        
            Assert.AreEqual(5, result[0]);
        }
        
        private static void ThreadMethod(object obj)
        {
            var arr = obj as int[];
            if (arr != null && arr.Length > 0)
            {
                arr[0] = 5;
            }
        }
    }
}