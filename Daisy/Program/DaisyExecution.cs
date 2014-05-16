using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Program
{
    using Ancestry.Daisy.Language;

    public interface IDaisyExecution
    {
        bool Outcome { get; }
        ExecutionDebugInfo DebugInfo { get; }
        ContextBundle Attachments { get; }
    }

    public class DaisyExecution : IDaisyExecution
    {
        public bool Outcome { get; set; }

        public ExecutionDebugInfo DebugInfo { get; set; }

        public ContextBundle Attachments { get; set; }

        public DaisyExecution(DaisyAst ast, DaisyMode mode)
        {
            Attachments = new ContextBundle();
            DebugInfo = new ExecutionDebugInfo(ast,mode);
        }
    }
}
