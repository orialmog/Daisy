using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Compilation
{
    public class DaisyCompilationException : Exception
    {
        public DaisyCompilationException(string message) : base(message) { }
    }
}
