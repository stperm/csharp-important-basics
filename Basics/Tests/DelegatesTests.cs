using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [Ignore("TODO")]
    [TestFixture]
    public class DelegatesTests
    {

        private delegate int MyIntBinaryOperationDelegate(int x, int y);

        private delegate A AnotherDelegate(B b);

        [Test]
        public void Todo()
        {
            MyIntBinaryOperationDelegate del = (x, y) => x + y;
            Console.WriteLine(del(1, 2));

            del = Multiply;
            Console.WriteLine(del(1, 2));

            del += Multiply; // MulticastDelegate => we can add more methods, they will be chained
            Console.WriteLine("Num of methods: {0}", del.GetInvocationList().Count());
            del(1, 2);

            // TODO: implement async call using old-styled pattern
            // del.BeginInvoke()
            // del.EndInvoke()

            AnotherDelegate another = (B b) => new A(); // exact types
            another = (B b) => new B(); // can return more derived type => covariance
            another = AnotherMethod; // can use method with less derived parameters => contravariance

            int n = 5;
            Func<int, int> multByVar = i => i * n; // n captured by lambda - closure
            Console.WriteLine(multByVar(5));

            var pub1 = new Publisher1();
            pub1.OnChange += () => Console.WriteLine("First subscr");
            pub1.OnChange += () => Console.WriteLine("Second subscr");
            pub1.Raise();
            pub1.OnChange = () => Console.WriteLine("First subscr 2"); // direct assignment removes all other subscribers
            pub1.OnChange(); // as OnChange is just a public field, we can invoke it by explicit call

            var pub2 = new Publisher2();
            pub2.OnChange += () => Console.WriteLine("1");
            pub2.OnChange += () => Console.WriteLine("2");
            pub2.Raise();
            // pub2.OnChange(); - it's event now, not able to invoke it by calling

            // proper event raise pattern
            var pub4 = new Publisher4();
            pub4.OnChange += Pub4_OnChange;
            pub4.Raise();

            // exception handling
            var pub6 = new Publisher6();
            pub6.OnChange += () => Console.WriteLine(1);
            pub6.OnChange += () => { throw new Exception("FUUUUUU"); };
            pub6.OnChange += () => Console.WriteLine(2);

            try
            {
                pub6.Raise();
            }
            catch (AggregateException agrEx)
            {
                Console.WriteLine("Caugth aggregate exception with {0} children", agrEx.InnerExceptions.Count);
            }
        }

        private static void Pub4_OnChange(object sender, MyArgs e)
        {
            Console.WriteLine("{0} sent: {1}", sender, e.Value);
        }

        static A AnotherMethod(A a) => new A();

        static int Multiply(int x, int y)
        {
            Console.WriteLine("Mult");
            return x * y;
        }
    }

    class A
    {

    }

    class B : A
    {

    }

    class C : B
    {

    }

    class Publisher1
    {
        public Action OnChange;

        public void Raise()
        {
            OnChange?.Invoke();
        }
    }

    class Publisher2
    {
        public event Action OnChange = delegate { }; // empty delegate => it's never null

        public void Raise()
        {
            OnChange();
        }
    }

    class Publisher3
    {
        public event EventHandler OnChange;

        public void Raise()
        {
            var e = new EventArgs();
            OnChange(this, e);
        }
    }

    class MyArgs : EventArgs
    {
        public MyArgs(int x)
        {
            Value = x;
        }

        public int Value { get; private set; }
    }

    class Publisher4
    {
        public event EventHandler<MyArgs> OnChange = delegate { };

        public void Raise()
        {
            OnChange(this, new MyArgs(8));
        }
    }

    /// <summary>
    /// Class with custom event accessor
    /// </summary>
    class Publisher5
    {
        private event EventHandler<MyArgs> onChange = delegate { };

        public event EventHandler<MyArgs> OnChange
        {
            add
            {
                lock (onChange) // locking is important to ensure that it's thread safe
                    onChange += value;
            }
            remove
            {
                lock (onChange)
                    onChange -= value;
            }
        }

        public void Raise()
        {
            onChange(this, new MyArgs(5));
        }
    }

    /// <summary>
    /// Shows an examle of exception handling
    /// </summary>
    class Publisher6
    {
        public event Action OnChange = delegate { };

        public void Raise()
        {
            var exList = new List<Exception>();

            foreach (var del in OnChange.GetInvocationList())
            {
                try
                {
                    del.DynamicInvoke();
                }
                catch (Exception ex)
                {
                    exList.Add(ex);
                }
            }

            if (exList.Any())
                throw new AggregateException(exList);
        }

    }
}
