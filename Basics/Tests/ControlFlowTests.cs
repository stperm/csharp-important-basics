using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.ExceptionServices;

namespace Tests
{
    [TestFixture]
    public class ControlFlowTests
    {
        [Test]
        public void Using_TwoVariablesWithinFor()
        {
            var sum = 0;

            for (int i = 0, j = 3; i < 15 && j > 0; i += 2, j--)
            {
                if (i == 4)
                    continue;
                if (j == 1)
                    break;
                sum += i + j;
            }

            Assert.AreEqual(7, sum);
        }

        [Test]
        public void Using_SwitchAndGoTo()
        {
            var result = string.Empty;
            var ch = 'c';

            labelBeforeSwitch:
            switch (ch)
            {
                case 'a':
                case 'b':
                    result = "a or b";
                    break;
                case 'c':
                    result = "case c";
                    goto default;
                default:
                    result = "default";
                    break;
            }

            if (ch == 'c')
            {
                Assert.AreEqual("default", result);
                ch = 'b';
                goto labelBeforeSwitch;
            }

            Assert.AreEqual("a or b", result);
        }

        [Test]
        public void Using_ChainedNullCoalescingOperator()
        {
            int? nullableInt1 = null;
            int? nullableInt2 = null;
            Assert.AreEqual(-10, 
                nullableInt1 ?? nullableInt2 ?? -10);
        }

        [Test]
        public void Using_InfiniteFor()
        {
            bool isStopped = false;
            Task.Run(() =>
            {
                Thread.Sleep(60);
                isStopped = true;
            });

            var count = 0;

            for (;;)
            {
                count++;
                Thread.Sleep(40);
                if (isStopped)
                    break;
            }

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Using_ExceptionDispatchInfo()
        {            
            var ex = Assert.Throws<Exception>(() => GetExceptionDispatchInfo().Throw());
            Assert.IsTrue(ex.StackTrace.Contains("GetExceptionDispatchInfo")); // still contains info about initial place of throw
            Assert.IsTrue(ex.StackTrace.Contains("Using_ExceptionDispatchInfo")); // contains info about the second place of throw
        }

        private ExceptionDispatchInfo GetExceptionDispatchInfo()
        {
            try
            {
                throw new Exception("some exception");
            }
            catch (Exception ex)
            {
                return ExceptionDispatchInfo.Capture(ex);                
            }            
        }

        [Test]
        [Explicit("This one is just to remember the FailFast method")]
        public void Using_EnvironmentFailFast()
        {
            try
            {
                Environment.FailFast("failing fast");
            }
            finally
            {
                Assert.Fail(); // not executed
            }
        }
    }
}
