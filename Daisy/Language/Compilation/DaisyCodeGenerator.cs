
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ancestry.Daisy.Language.AST;
using Ancestry.Daisy.Statements;
using Ancestry.Daisy.Language;

namespace Ancestry.Daisy.Compilation
{
    public class DaisyCodeGenerator
    {
        public static string Generate<T>(DaisyAst ast, StatementSet statements)
        {
            var sb = new StringBuilder();
            sb.Append(
@"using Ancestry.Daisy;
using Ancestry.Daisy.Compilation;
using Ancestry.Daisy.Program;

namespace Thing
{
    public class Program
    {
        public static IDaisyExecution Prg(");
            sb.Append(typeof(T).FullName);
            sb.Append(@" scope, ContextBundle context)
{           
            var attachments = new ContextBundle(); 
            var result = ");

            Walk(sb, ast.Root, 0);

            sb.Append(@";
            return new DaisyExecution(null, DaisyMode.Release)
            {
                Attachments = attachments,
                Outcome = result
            };
        }
    }
}");
            return sb.ToString();
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
                throw new DaisyCompilationException("Cannot compile ast  -- invalid node type");
            }
            
        }
       
    }
}

