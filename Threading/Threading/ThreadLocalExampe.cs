using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading
{
    public class ThreadLocalExampe<T>
    {
        public ThreadLocal<T> ThreadLocalField;

        private ThreadLocalExampe(Func<T> threadLocalInitFunc)
        {
            ThreadLocalField = new ThreadLocal<T>(threadLocalInitFunc);
        }

        public static ThreadLocalExampe<T> Get(Func<T> threadLocalInitFunc)
        {
            return new ThreadLocalExampe<T>(threadLocalInitFunc);
        }
    }
}
