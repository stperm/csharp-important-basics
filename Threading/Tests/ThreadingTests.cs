using NUnit.Framework;
using System;
using System.Threading;

namespace Tests
{
    /// <summary>
    /// Tests showing the basic functionality of raw threads and ThreadPool
    /// </summary>
    [TestFixture]
    public class ThreadingTests
    {
        private ThreadLocal<int> _threadLocalManagedThreadId = new ThreadLocal<int>(() => Thread.CurrentThread.ManagedThreadId);

        [ThreadStatic]
        private static int _threadStaticField;
        
        [Test]
        public void UsingThreadStatic_GivesDifferentStaticFieldValuesOnDifferentThreads()
        {
            _threadStaticField = 5;
            int valueOnAnotherThread = 5;

            var thread = new Thread(() => valueOnAnotherThread = _threadStaticField);
            thread.Start();
            thread.Join();
            
            Assert.AreEqual(5, _threadStaticField);
            Assert.AreEqual(0, valueOnAnotherThread);
        }

        [Test]
        public void UsingThreadLocal_ForRunningThreadSpecificFieldInitializationLogic()
        {
            int firstValue = -1, secondValue = -1;

            var t1 = new Thread(() => { firstValue = _threadLocalManagedThreadId.Value; });
            t1.Start();

            var t2 = new Thread(() => { secondValue = _threadLocalManagedThreadId.Value; });
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.AreNotEqual(firstValue, secondValue);            
        }

        [Test]
        public void Using_ThreadPool_QueueUserWorkItem_Method_SchedulesWorkOnAThreadPoolThread()
        {
            var array = new int[1];
            var isCompleted = false;
            ThreadPool.QueueUserWorkItem(obj => 
            {
                var arr = (int[])obj;
                for(int i = 0; i < 4; i++)
                {
                    Thread.Sleep(20);
                    arr[0]++;
                }
                isCompleted = true;
            }, array);

            while (!isCompleted)
            {
                Thread.Sleep(90);
            }

            Assert.AreEqual(4, array[0]);
        }

        [Test]
        [Timeout(5000)]
        public void Using_ThreadPool_RegisterWaitForSingleObject_Method_AllowsToScheduleContinuationForAWaitHandle()
        {
            var semaphore = new Semaphore(0, 1); // we are owning it
            var executedCount = 0;
            Action<object, bool> callback = (state, timedOut) => 
            {
                if (!timedOut)
                    executedCount++;
                (state as Semaphore)?.Release(); // releasing as we got this locked implicitly when entering the method
            };

            var waitHandle = ThreadPool.RegisterWaitForSingleObject(semaphore, new WaitOrTimerCallback(callback),
                state: semaphore, // a WaitHandle, using a Mutex is not recommended
                millisecondsTimeOutInterval: 10000, // how long we wait for a WaitHandle
                executeOnlyOnce: false); // do we want to execute every time the WaitHandle is released

            Thread.Sleep(50);
            semaphore.Release(); // releasing 1st           
            semaphore.WaitOne(); // if we are not locking it back, our handler will be called continously...
            Thread.Sleep(50);
            Assert.AreEqual(1, executedCount);

            semaphore.Release(); // releasing 2nd
            semaphore.WaitOne();
            Thread.Sleep(50);
            Assert.AreEqual(2, executedCount);

            waitHandle.Unregister(semaphore); // unregistering our Wait

            semaphore.Release(); // releasing 3rd, shouldn't be habdled
            Assert.AreEqual(2, executedCount);
        }

        [Test]
        [Timeout(1000)]
        public void Using_ThreadStart_Delegate_WhenPassingNothingToTheMethod()
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
        public void Using_ParameterizedThreadStart_Delegate_WhenPassingParameterToTheMethod()
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