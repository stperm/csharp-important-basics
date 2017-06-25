using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threading
{
    public class ThreadStaticExample
    {
        [ThreadStatic]
        public static int ThreadStaticField;
    }
}