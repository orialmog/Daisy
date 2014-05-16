using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;

namespace Ancestry.Daisy
{
    using Ancestry.Daisy.Language;
    using Ancestry.Daisy.Linking;
    using Ancestry.Daisy.Program;
    using Ancestry.Daisy.Statements;
    using Microsoft.CSharp;
    using System.IO;
    using System.Reflection;
    using System.CodeDom.Compiler;
    using Ancestry.Daisy.Compilation;
    using Ancestry.Daisy.Language.Compilation;

    public class DaisyCompiler
    {
        public static IDaisyProgram<T> Compile<T>(string code, StatementSet statements, DaisyMode mode = DaisyMode.Debug)
        {
            var ast = DaisyParser.Parse(code);
            var linker = new DaisyLinker(ast, statements, typeof(T));
            linker.Link();
            if(mode == DaisyMode.Debug)
                return new DaisyProgram<T>(ast);

            var cached = DaisyProgramCache.Get<T>(code);
            if (cached != null)
                return cached;

            var csharpcode = DaisyCodeGenerator.Generate<T>(ast, statements);
            var compiled = Compile(csharpcode, statements);
            var dlg = compiled.DelegateForCallMethod("Prg", new[] { typeof(T), typeof(ContextBundle) });
            var prog = new DaisyCompiledProgram<T>((scope, context) => (IDaisyExecution)dlg(null, new object[] { scope, context }));

            DaisyProgramCache.Stash(code, prog);
            return prog;
        }
        private static Type Compile(string code, StatementSet set)
        {
            var csp = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            var libPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().EscapedCodeBase).Substring(6);
            var parameters = new CompilerParameters(new[] {
                    "mscorlib.dll",
                    "System.Core.dll",
                    "Ancestry.Daisy.dll",
                    "System.dll"
                }.Concat(set.Statements
                        .OfType<ReflectionStatementDefinition>()
                        .SelectMany(i => new[]{
                                Path.GetFileName(i.ControllerType.Assembly.EscapedCodeBase),
                                Path.GetFileName(i.ScopeType.Assembly.EscapedCodeBase)
                            }))
                    .Distinct()
                    .ToArray())
            {
                GenerateInMemory = true,
                CompilerOptions = string.Format("/lib:{0}", libPath)
            };
            var results = csp.CompileAssemblyFromSource(parameters, code);
            if (results.Errors.HasErrors)
            {
                throw new AggregateException(results.Errors.OfType<CompilerError>().Select(x => new Exception(x.ToString())));
            }
            return results.CompiledAssembly.GetType("Thing.Program");
        }
    }

    public enum DaisyMode
    {
        Debug,
        Release
    }
}
