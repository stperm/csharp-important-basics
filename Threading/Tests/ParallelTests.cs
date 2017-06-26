using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Tests showing the basic usage of Parallel class
    /// </summary>
    [TestFixture]
    public class ParallelTests
    {
        [Test]
        public void Using_Parallel_For()
        {
            var counter = 0;

            Parallel.For(0, 10, i =>
            {
                Interlocked.Increment(ref counter);
            });

            Assert.AreEqual(10, counter);
        }

        [Test]
        public void Using_Parallel_ForEach()
        {
            var collection = Enumerable.Range(1, 10);
            var locker = new object();
            var sum = 0;

            Parallel.ForEach(collection, i => 
            {
                lock (locker)
                    sum += i;
            });

            Assert.AreEqual(55, sum);
        }

        [Test]
        public void Using_Parallel_Invoke()
        {
            var counter = 10;
            var actions = new Action[3];

            for (int i = 0; i < 3; i++)
            {
                var tmp = i;
                actions[i] = () => 
                {
                    for (int j = 0; j < tmp; j++)
                    {
                        Interlocked.Decrement(ref counter);
                    }
                };
            }

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Parallel.Invoke(new ParallelOptions() {
                    MaxDegreeOfParallelism = 2,
                    CancellationToken = token }, // just showing the opportunity to use a CancellationToken
                actions);

            Assert.AreEqual(7, counter);
        }

        [Test]
        public void Breaking_Parallel()
        {            
            Parallel.For(0, 1000, ((int i, ParallelLoopState loopState) =>
                {
                    Console.WriteLine(i);
                    if (i == 500)
                    {
                        loopState.Break(); // Break ensures that all iterations that are currently running will be finished. 
                        Assert.IsFalse(loopState.IsStopped);
                        Assert.IsFalse(loopState.IsExceptional);
                        Assert.AreEqual(500, loopState.LowestBreakIteration);
                    }
                }));
        }

        [Test]
        public void Stopping_Parallel()
        {
            Parallel.For(0, 1000, ((int i, ParallelLoopState loopState) =>
            {
                Console.WriteLine(i);
                if (i == 500)
                {   
                    loopState.Stop(); // Stop just terminates everything.
                    Assert.IsTrue(loopState.IsStopped);
                    Assert.IsFalse(loopState.IsExceptional);
                    Assert.AreEqual(null, loopState.LowestBreakIteration);
                }
            }));
        }
    }
}
