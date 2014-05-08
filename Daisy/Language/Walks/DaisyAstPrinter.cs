namespace Ancestry.Daisy.Language.Walks
{
    using System;
    using System.Text;

    using Ancestry.Daisy.Language.AST;

    public class DaisyAstPrinter : AstTreeWalker<IDaisyAstNode>
    {
        public DaisyAstPrinter(IDaisyAstNode node) : base(node) { }

        private StringBuilder sb = new StringBuilder();
        internal int indent = 0;

        public static String Print(IDaisyAstNode root)
        {
            return new DaisyAstPrinter(root).Print();
        }

        public string Print()
        {
            Walk();
            return sb.ToString().Trim();
        }

        private bool VisitingNot = false;

        protected override bool PreVisit(IAndOperatorNode<IDaisyAstNode> node)
        {
            sb.Append("\n");
            Pad(sb, indent); sb.Append("(AND");
            indent++;
            return true;
        }

        protected override void PostVisit(IAndOperatorNode<IDaisyAstNode> node)
        {
            indent--;
            sb.Append(")");
        }

        protected override bool PreVisit(IOrOperatorNode<IDaisyAstNode> node)
        {
            sb.Append("\n");
            Pad(sb, indent); sb.Append("(OR");
            indent++;
            return true;
        }

        protected override void PostVisit(IOrOperatorNode<IDaisyAstNode> node)
        {
            indent--;
            sb.Append(")");
        }

        protected override bool PreVisit(INotOperatorNode<IDaisyAstNode> node)
        {
            VisitingNot = true;
            sb.Append("\n");
            Pad(sb, indent); sb.Append("(NOT ");
            indent++;
            return true;
        }

        protected override void PostVisit(INotOperatorNode<IDaisyAstNode> node)
        {
            VisitingNot = false;
            indent--;
            sb.Append(")");
        }

        protected override void Visit(IStatementNode node)
        {
            if(!VisitingNot)
            {
                sb.Append("\n");
                Pad(sb, indent);
            }
            sb.Append(node.Text);
        }

        protected override bool PreVisit(IGroupOperatorNode<IDaisyAstNode> node)
        {
            sb.Append("\n");
            Pad(sb, indent);
            sb.Append("(GROUP");
            if(node.Text != null)
            {
                sb.Append(" ");
                sb.Append(node.Text);
            }
                
            indent++;
            return true;
        }

        protected override void PostVisit(IGroupOperatorNode<IDaisyAstNode> node)
        {
            indent--;
            sb.Append(")");
        }

        private static void Pad(StringBuilder sb, int pads)
        {
            for(int i=0; i<pads; ++i)
            {
                sb.Append("    ");
            }
        }
    }
}
