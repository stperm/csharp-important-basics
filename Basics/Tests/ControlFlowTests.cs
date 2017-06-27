using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.ExceptionServices;

namespace Tests
{
    [TestFixture]
    public class ControlFlowTests
    {
        [Test]
        public void Todo()
        {
            for (int i = 0, j = 15; i < 15 && j > 0; i += 2, j--)
            {
                if (i == 6)
                    continue;
                if (j == 13)
                    break;
                Console.WriteLine(i + j);
            }

            var ch = 'c';

            switch (ch)
            {
                case 'a':
                case 'b':
                    {   // does this look better with curly braces?
                        Console.WriteLine("a or b");
                        break;
                    }
                case 'c':
                    Console.WriteLine("case c");
                    goto default;
                //break;
                default:
                    Console.WriteLine("default");
                    break;
            }

            int? nullableInt1 = null;
            int? nullableInt2 = null;
            Console.WriteLine(nullableInt1 ?? nullableInt2 ?? -10); // null-coalescing operator (chained)

            goto label;
            Console.WriteLine("not written"); // unreachable
            label:
            Console.WriteLine("written");

            bool isStopped = false;
            Task.Run(() =>
            {
                Console.WriteLine("press any key to stop");
                Console.ReadKey();
                isStopped = true;
            });

            for (;;)
            {
                Console.Write("*");
                Thread.Sleep(500);
                if (isStopped)
                {
                    Console.WriteLine();
                    break;
                }
            }

            var captured = ExceptionDispatchInfo.Capture(new Exception("some exception"));
            // captured.Throw(); // throws as good as await (preserving the original info)

            try
            {
                Environment.FailFast("failing fast");
            }
            finally
            {
                Console.WriteLine("finally"); // not written to Console
            }
        }
    }
}
