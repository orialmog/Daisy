using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Program
{
    public interface IDaisyProgram<T>
    {
        IDaisyExecution Execute(T scope);
        IDaisyExecution Execute(T scope, ContextBundle context);
    }
}
