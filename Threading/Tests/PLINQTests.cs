using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tests
{
    /// <summary>
    /// Tests showing the usage of basic Parallel LINQ features
    /// </summary>
    [TestFixture]
    public class PLINQTests
    {
        [Test]
        public void Using_ForAll_ToInvokeActionOnCollectionItemsInParallel()
        {
            var count = 0;
            var collection = Enumerable.Range(1, 10);

            collection.AsParallel().                
                Where(i => i % 3 == 0).
                ForAll(i => // ForAll breaks sorting
                {
                    Interlocked.Increment(ref count);
                });

            Assert.AreEqual(3, count);            
        }   
        
        [Test]
        public void Using_AsSequential_ToKeepOrderOfValues()
        {
            var sb = new StringBuilder(10);
            var collection = Enumerable.Range(1, 10);
            var result= collection.AsParallel()
                .AsOrdered() // This tells parallel execution engine to preserve the order of items
                .Where(i => i % 2 == 0)
                .AsSequential(); // This converts back to usual IEnumerable (sequential, not parallel)

            foreach (var i in result.Take(2)) // Take messes order without .AsSequential()
            {
                sb.Append(i);
            }

            Assert.AreEqual("24", sb.ToString());
        }

        [Test]
        public void ParallelCollection_ThrowsAggregateException()
        {
            var count = 10;
            var collection = Enumerable.Range(1, count);

            var ex = Assert.Throws<AggregateException>(() =>
                collection.AsParallel()
                    .ForAll(i => { throw new Exception(i.ToString()); })
            );

            // it's just my guess that it depends on a number of simultaneous executions            
            Assert.IsTrue(ex.InnerExceptions.Count <= Environment.ProcessorCount);
        }
    }
}