namespace Ancestry.Daisy.Tests.Daisy.Unit.Language.WalksTests
{
    using Ancestry.Daisy.Language;
    using Ancestry.Daisy.Language.Walks;

    using NUnit.Framework;

    [TestFixture,Category("Unit")]
    public class DaisyAstPrinterTests
    {
        [TestCase("a\r\nAND b", Result = 
@"(AND
    a
    b)")]
        [TestCase("a\r\nOR b", Result = 
@"(OR
    a
    b)")]
        [TestCase("NOT a", Result = "(NOT a)")]
        public string ItPrintsPrograms(string code)
        {
            var ast = DaisyParser.Parse(code);
            var p = new DaisyAstPrinter(ast.Root);
            var back = p.Print();
            Assert.AreEqual(0, p.indent);
            return back;
        }
    }
}
