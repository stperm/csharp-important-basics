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
        public void Using_ConcurrentDictionary()
        {
            var dict = new ConcurrentDictionary<int, string>();

            bool firstFiveSuccess = false, secondFiveSuccess = false;

            Parallel.Invoke(
                () => dict.TryAdd(4, "four"),
                () => firstFiveSuccess = dict.TryAdd(5, "five"),
                () => secondFiveSuccess = dict.TryAdd(5, "five 2"),
                () => dict.TryAdd(6, "six")
            );

            Assert.AreNotEqual(firstFiveSuccess, secondFiveSuccess);

            string result;
            Assert.IsTrue(dict.TryRemove(6, out result));
            Assert.AreEqual("six", result);

            Parallel.Invoke(
                () => dict.AddOrUpdate(4,
                    i => "four",
                    (i, old) => old == "four" ? "ha-ha" : // we are executed before the second Action
                                old == "updated four" ? "updated twice four" : "oops"), 
                () => dict.TryUpdate(4, "updated four", "four") // we are executed before the first action
            );
                        
            Assert.IsTrue(dict.TryGetValue(4, out result));
            Assert.IsTrue((new[] { "updated four", "updated twice four" }).Contains(result));            
        }

        [Test]
        public void Using_BlockingCollection()
        {
            // bounded capacity, blocking Add until the item is taken,
            // uses concurrentQueue by default, but may be customized 
            var blocking = new BlockingCollection<int>(new ConcurrentStack<int>(new[] { 1, 2, 3 }), 3);            

            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(150);
                int taken;
                var isTaken = blocking.TryTake(out taken);
                Assert.IsTrue(isTaken);
                Assert.AreEqual(3, taken);
            });

            Assert.IsFalse(blocking.TryAdd(4)); // this call is not blocked, but might not succeed
            blocking.Add(5); // this call is blocked until there is a free space in the collection
            var result = blocking.Take();
            Assert.AreEqual(5, result);
        }

        [Test]
        public void Using_ConcurrentBag()
        {
            // one important feature of ConcurrentBag is that
            // values are returned first to the thread which added them
            var bag = new ConcurrentBag<int>();

            var addTask = Task.Factory.StartNew(() =>
            {
                bag.Add(4);
                bag.Add(5);
                bag.Add(6);
            });

            bag.Add(1);
            bag.Add(1); // this value is also stored despite the fact the same value is already in
            bag.Add(2);

            addTask.Wait();

            int result;

            Assert.IsTrue(bag.TryPeek(out result));

            var currentThreadValues = new[] { 1, 2 };

            Assert.IsTrue(currentThreadValues.Contains(result));

            var anotherThreadValues = new[] { 4, 5, 6 };

            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(bag.TryTake(out result));
                Assert.IsTrue(currentThreadValues.Contains(result));
            }

            while (bag.TryTake(out result))
            {
                Assert.IsTrue(anotherThreadValues.Contains(result));
            }
        }

        [Test]
        public void Using_ConcurrentStack()
        {            
            var stack = new ConcurrentStack<int>();

            stack.Push(2);
            stack.Push(4);
            stack.Push(6);

            int result;

            Assert.IsTrue(stack.TryPeek(out result));
            Assert.AreEqual(6, result);

            var sb = new StringBuilder(3);

            while (stack.TryPop(out result))
            {
                sb.Append(result);
            }

            Assert.AreEqual("642", sb.ToString()); // usual stack behavior
        }

        [Test]
        public void Using_ConcurrentQueue()
        {
            var queue = new ConcurrentQueue<int>();

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            int result;

            Assert.IsTrue(queue.TryPeek(out result));
            Assert.AreEqual(1, result);

            var sb = new StringBuilder(3);

            while (queue.TryDequeue(out result))
            {
                sb.Append(result);
            }

            Assert.AreEqual("123", sb.ToString()); // usual queue behavior
        }

        [Test]
        
        public void Using_IProducerConsumerCollection()
        {              
            // IProducerConsumerCollection<T> - abstraction over producer-consumer collections
            // CocncurrentQueue, ConcurrentStack and ConcurrentBag implement this interface

            IProducerConsumerCollection<int> prodConsCollection = new ConcurrentQueue<int>();

            prodConsCollection.TryAdd(1);
            prodConsCollection.TryAdd(2);
            prodConsCollection.TryAdd(3);

            int result;
            var sb = new StringBuilder(3);
            while (prodConsCollection.TryTake(out result))
            {
                sb.Append(result);
            }

            Assert.AreEqual("123", sb.ToString()); // obvious behavior of the real collection
        }        
    }
}
