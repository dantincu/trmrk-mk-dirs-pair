using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    public static class EnumsH
    {
        public static TEnum[] GetValues<TEnum>(
            ) where TEnum : struct, Enum => Enum.GetValues<TEnum>();
    }
}
