namespace Ancestry.Daisy.Tests.Daisy.Unit.Language
{
    using System;

    using Ancestry.Daisy.Language;
    using Ancestry.Daisy.Language.Walks;
    using Ancestry.Daisy.Utils;

    using NUnit.Framework;

    [TestFixture,Category("Unit")]
    public class DaisyParserTest
    {
        [TestCase("a\nAND b",
@"(AND
    a
    b)",TestName = "It parses ands")]
        [TestCase("a\nOR b",
@"(OR
    a
    b)",TestName = "It parses ors")]
        [TestCase("a\nOR NOT b",
@"(OR
    a
    (NOT b))",TestName = "It parses nots")]
        [TestCase("a\nNOT b",
@"(AND
    a
    (NOT b))",TestName = "It parses nots after implicit ands")]
        [TestCase("a\nb",
@"(AND
    a
    b)",TestName = "It parses implicit ands")]
        [TestCase("a\nAND b\nAND c",
@"(AND
    (AND
        a
        b)
    c)",TestName = "It parses chained statements")]
        [TestCase(
@"a
AND b
AND c
OR NOT d
AND NOT e",
@"(AND
    (OR
        (AND
            (AND
                a
                b)
            c)
        (NOT d))
    (NOT e))",
            TestName = "It parses deeply chained statements")]
        [TestCase(
@"a
AND
  b
  OR c",
@"(AND
    a
    (GROUP
        (OR
            b
            c)))",
            TestName = "It parses anonymous groups")]
        [TestCase(
@"a
AND d
  b
  OR c",
@"(AND
    a
    (GROUP d
        (OR
            b
            c)))",
            TestName = "It parses named groups")]
        [TestCase(
@"a
  b",
@"(GROUP a
    b)",
            TestName = "It parses groups")]
        [TestCase( @"
a
  b
    c
    d",
@"(GROUP a
    (GROUP b
        (AND
            c
            d)))",
            TestName = "It parses groups in groups")]
        [TestCase(
@"a
OR b
OR
  c
    ca
    cb
    cc
  d
  NOT e
AND f",

@"(AND
    (OR
        (OR
            a
            b)
        (GROUP
            (AND
                (AND
                    (GROUP c
                        (AND
                            (AND
                                ca
                                cb)
                            cc))
                    d)
                (NOT e))))
    f)", 
       TestName="New AST")]
        public void ItParsesLanguages(string code, string expectedTree)
        {
            var llstream = new LookAheadStream<Token>(new Lexer(code.ToStream()).Lex());
            var parser = new DaisyParser(llstream);
            var tree = parser.Parse();
            Assert.IsNotNull(tree);
            var actualTree = DaisyAstPrinter.Print(tree.Root);
            if(expectedTree != actualTree)
            {
                Console.WriteLine(expectedTree);
                Console.WriteLine("----------------");
                Console.WriteLine(actualTree);
            }
            Assert.AreEqual(expectedTree, actualTree);
        }

        [TestCase("AND a")]
        [TestCase("a\r\nAND")]
        [TestCase("a\r\nAND\r\nb")]
        [TestCase("a\r\nAND NOT\r\nb")]
        public void ItDiesOnIllegalStatements(string code)
        {
            var llstream = new LookAheadStream<Token>(new Lexer(code.ToStream()).Lex());
            var parser = new DaisyParser(llstream);
            try
            {
                parser.Parse();
            }
            catch (Exception)
            {
                //throw;
                return;
            }
            Assert.Fail("Expected " + code + " to not parse");
        }
    }
}
