using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Program
{
    public class DaisyCompiledProgram<T> : IDaisyProgram<T>
    {
        public DaisyCompiledProgram(Func<T, ContextBundle, IDaisyExecution> entryPoint)
        {
            this.EntryPoint = entryPoint;
        }
        private Func<T, ContextBundle, IDaisyExecution> EntryPoint;
        public IDaisyExecution Execute(T scope)
        {
            return this.EntryPoint(scope, new ContextBundle());
        }

        public IDaisyExecution Execute(T scope, ContextBundle context)
        {
            return this.EntryPoint(scope, context);
        }
    }
}
