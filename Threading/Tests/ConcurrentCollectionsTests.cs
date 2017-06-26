using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Tests showing basic methods of standard concurrent collections
    /// </summary>
    [TestFixture]
    public class ConcurrentCollectionsTests
    {
        [Test]
        [Ignore("TODO")]
        public void ToDo()
        {
            // CONCURRENT DICTIONARY -----------------------------------------------

            var dict = new ConcurrentDictionary<int, string>();

            dict.TryAdd(4, "four");
            dict.TryAdd(5, "five");
            dict.TryAdd(5, "five 2");
            dict.TryAdd(6, "six");

            Parallel.Invoke(
                () => dict.AddOrUpdate(4,
                    i => "four",
                    (i, old) => old == "four" ? "ha-ha" :
                                old == "updated four" ? "updated twice four" : "oops"),
                () => dict.TryUpdate(4, "updated four", "four")
            );

            PrintCollection(dict);

            // BLOCKING COLLECTION ----------------------------------------------------------

            // bounded capacity, blocking Add until the item is taken,
            // uses concurrentQueue by default, but may be customized 
            var blocking = new BlockingCollection<int>(new ConcurrentStack<int>(new[] { 1, 2, 3 }), 3);

            //blocking.TryAdd(1);
            //blocking.TryAdd(2);
            //blocking.TryAdd(3);

            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(3000);
                int taken;
                var isTaken = blocking.TryTake(out taken);
                Console.WriteLine("{0}taken - {1}", isTaken ? "" : "not ", taken);
            });

            Console.WriteLine("trying...");
            if (!blocking.TryAdd(4))
                Console.WriteLine("failed");
            blocking.Add(5);
            Console.WriteLine("done");

            PrintCollection(blocking);

            // CONCURRENT BAG ----------------------------------------------------

            var bag = new ConcurrentBag<int>();

            var addTask = Task.Factory.StartNew(() =>
            {
                bag.Add(4);
                bag.Add(5);
                bag.Add(6);
            });

            addTask.Wait();

            bag.Add(1);
            bag.Add(1);
            bag.Add(2);

            int result;

            if (bag.TryPeek(out result))
                Console.WriteLine("bag peek: {0}", result);

            while (bag.TryTake(out result))
            {
                Console.WriteLine("bag: {0}", result);
            }

            // CONCURRENT STACK ---------------------------------------------------------- 

            var stack = new ConcurrentStack<int>();

            stack.Push(2);
            stack.Push(4);
            stack.Push(6);

            if (stack.TryPeek(out result))
                Console.WriteLine("stack peek: {0}", result);

            while (stack.TryPop(out result))
            {
                Console.WriteLine("stack: {0}", result);
            }

            // CONCURRENT QUEUE ----------------------------------------------------------

            var queue = new ConcurrentQueue<int>();

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);

            if (queue.TryPeek(out result))
                Console.WriteLine("queue peek: {0}", result);

            while (queue.TryDequeue(out result))
            {
                Console.WriteLine("queue: {0}", result);
            }

            // IProducerConsumerCollection<T> - abstraction over producer-consumer collections
            // queue, stack and bag implement this interface

            IProducerConsumerCollection<int> prodConsCollection = new ConcurrentQueue<int>();

            prodConsCollection.TryAdd(1);
            prodConsCollection.TryAdd(2);
            prodConsCollection.TryAdd(3);

            while (prodConsCollection.TryTake(out result))
            {
                Console.WriteLine("prodCons {0}", result);
            }
        }

        private static void PrintCollection<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine("{0}", item);
            }
        }
    }
}
