using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    public class MsgTupleCore<T>
    {
        public T L { get; set; }
        public T U { get; set; }
    }

    public class MsgTuple : MsgTupleCore<object[]>
    {
    }

    public class StrMsgTuple : MsgTupleCore<string>
    {
    }
}
