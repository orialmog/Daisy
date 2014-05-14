using Ancestry.Daisy.Language;
using Ancestry.Daisy.Linking;
using Ancestry.Daisy.Program;
using Ancestry.Daisy.Statements;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using Ancestry.Daisy.Language.AST;

namespace Ancestry.Daisy
{
    public class DaisyCodeCompiler
    {
        public static Func<T, ContextBundle, DaisyCompiledExecution> Compile<T>(string code, StatementSet statements)
        {
            var ast = DaisyParser.Parse(code);
            var linker = new DaisyLinker(ast, statements, typeof(T));
            linker.Link();
            return Compile<T>(ast, statements);
        }

        public static Func<T, ContextBundle, DaisyCompiledExecution> Compile<T>(DaisyAst ast, StatementSet statements)
        {
            var sb = new StringBuilder();
            sb.Append(
@"using Ancestry.Daisy;
using Ancestry.Daisy.Program;

namespace Thing
{
    public class Program
    {
        public static DaisyCompiledExecution Prg(");
            sb.Append(typeof(T).FullName);
            sb.Append(@" scope, ContextBundle context)
{           
            var attachments = new ContextBundle(); 
            var result = ");

            Walk(sb, ast.Root, 0);

            sb.Append(@";
            return new DaisyCompiledExecution
            {
                Attachments = attachments,
                Outcome = result
            };
        }
    }
}");
            var compiled = Compile(sb.ToString(), statements);
            var dlg = compiled.DelegateForCallMethod("Prg", new[] { typeof(T), typeof(ContextBundle)});
            return (scope, context) => (DaisyCompiledExecution)dlg(null, new object[] { scope, context });
                       
        }
        private static void Walk(StringBuilder sb, IDaisyAstNode node, int groupDepth)
        {
            if(node is AndOperatorNode)
            {
                var casted = (AndOperatorNode)node;
                sb.Append("((");
                Walk(sb, casted.Left, groupDepth);
                sb.Append(") && (");
                Walk(sb, casted.Right, groupDepth);
                sb.Append("))");
            }
            else if (node is OrOperatorNode)
            {
                var casted = (OrOperatorNode)node;
                sb.Append("((");
                Walk(sb, casted.Left, groupDepth);
                sb.Append(") || (");
                Walk(sb, casted.Right, groupDepth);
                sb.Append("))");
            }
            else if (node is NotOperatorNode)
            {
                var casted = (NotOperatorNode)node;
                sb.Append("(!(");
                Walk(sb, casted.Inner, groupDepth);
                sb.Append("))");
            }
            else if (node is GroupOperatorNode)
            {
                var casted = (GroupOperatorNode)node;
                if (casted.LinkedStatement == null)
                    throw new Exception("anonymous group operators not allowed");
                var statement = casted.LinkedStatement;
                if (!(statement is ReflectionStatementDefinition.ReflectionLinkedStatement))
                    throw new Exception("only reflected statement definitions allowed");
                var castedStatement = (ReflectionStatementDefinition.ReflectionLinkedStatement)statement;

                sb.Append("new ");
                sb.Append(((ReflectionStatementDefinition)castedStatement.Definition).ControllerType.FullName);
                sb.Append(@"{
			Attachments = attachments,
			Context = context,
			Scope = scope");
                foreach(var d in Enumerable.Range(0,groupDepth))
                    sb.Append("a");
                sb.Append(@"
}.");

                groupDepth++;

                sb.Append(castedStatement.Definition.Name);
                sb.Append("(scope");
                foreach(var d in Enumerable.Range(0,groupDepth))
                    sb.Append("a");
                sb.Append(@" => 
");
                Walk(sb, casted.Root, groupDepth);

                sb.Append(")");
            }
            else if(node is StatementNode)
            {
                var casted = (StatementNode)node;
                var statement = casted.LinkedStatement;
                if (!(statement is ReflectionStatementDefinition.ReflectionLinkedStatement))
                    throw new Exception("only reflected statement definitions allowed");
                var castedStatement = (ReflectionStatementDefinition.ReflectionLinkedStatement)statement;
                sb.Append("new ");
                sb.Append(((ReflectionStatementDefinition)castedStatement.Definition).ControllerType.FullName);
                sb.Append(@"{
			Attachments = attachments,
			Context = context,
			Scope = scope");
                foreach(var d in Enumerable.Range(0,groupDepth))
                    sb.Append("a");
                sb.Append(@"
}.");
                sb.Append(castedStatement.Definition.Name);
                sb.Append("(");

                var hasProceedFunc = castedStatement.MappedParameters.Length == 1
                    && castedStatement.MappedParameters.Where(i => new[] { typeof(string), typeof(int) }.Contains(i.GetType())).Count() == 0;
                if(hasProceedFunc)
                {
                    sb.Append("foobidylols => true");
                }
                else
                {
                    sb.Append(string.Join(", ", castedStatement.MappedParameters
                        .Where(i => new[] { typeof(string), typeof(int) }.Contains(i.GetType()))
                        .Select(i => i.GetType() == typeof(string) ? string.Join(i.ToString(), "\"", "\"") : i.ToString())));
                }
                sb.Append(")");
            }
            else
            {
                throw new Exception("invalid ast node type");
            }
            
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

    public class DaisyCompiledExecution
    {
        public bool Outcome { get; set; }
        public ContextBundle Attachments { get; set; }
    }
}

