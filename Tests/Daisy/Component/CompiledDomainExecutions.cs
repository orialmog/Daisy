﻿using System;
using Ancestry.Daisy.Language.Walks;

namespace Ancestry.Daisy.Tests.Daisy.Component
{
    using Ancestry.Daisy.Statements;
    using Ancestry.Daisy.Tests.Daisy.Component.Controllers;
    using Ancestry.Daisy.Tests.Daisy.Component.Domain;

    using NUnit.Framework;
    using Ancestry.Daisy.Program;
    using System.Diagnostics;
    using Ancestry.Daisy.Language.Compilation;

    [TestFixture, Category("Component")]
    public class CompiledDomainExecutions
    {
        private StatementSet statements;

        [SetUp]
        public void Setup()
        {
            statements = new StatementSet().FromAssemblyOf(typeof(UserController));
        }

        private TestCaseData[] itExecutesStatments =
            {
                new TestCaseData(Statements.UserHasNoRecentTransactions, TestData.Ben, false)
                .Returns(false)
                .SetName("Has no recent transaction"),
                new TestCaseData(Statements.UserHasUnusedMoneyMarket, TestData.Ben, false)
                .Returns(true)
                .SetName("Has unused money market account"),
                new TestCaseData(Statements.UserHasNonCheckingWithABalance, TestData.Ben, false)
                .Returns(true)
                .SetName("Has non checking account with a balance"),
                new TestCaseData(Statements.AttachmentsInChildfulGroup, TestData.Ben, true)
                .Returns(true)
                .SetName("Attaches with group that has children"),
                new TestCaseData(Statements.AttachmentsInChildlessGroup, TestData.Ben, true)
                .Returns(true)
                .SetName("Attaches with group without children"),
            };

        [TestCaseSource("itExecutesStatments")]
        public bool ItExecutesStatements(string code, User data, bool hasAttachments)
        {
            var execution = DaisyCompiler.Compile<User>(code, statements, DaisyMode.Release).Execute(data, new ContextBundle());
            return execution.Outcome && (hasAttachments 
                ? execution.Attachments.Count > 0
                : execution.Attachments.Count == 0);
        }
        [Test]
        public void ItCachesCompiledPrograms()
        {
            Assert.IsNull(DaisyProgramCache.Get<User>(Statements.UserIsOverdrawnOnChecking));
            var foo = DaisyCompiler.Compile<User>(Statements.UserIsOverdrawnOnChecking, 
                statements,
                DaisyMode.Debug);
            Assert.IsNull(DaisyProgramCache.Get<User>(Statements.UserIsOverdrawnOnChecking));
            var prog = DaisyCompiler.Compile<User>(Statements.UserIsOverdrawnOnChecking, 
                statements,
                DaisyMode.Release);
            Assert.IsNotNull(DaisyProgramCache.Get<User>(Statements.UserIsOverdrawnOnChecking));
            Assert.AreNotEqual(foo, prog);
            var prog2 = DaisyCompiler.Compile<User>(Statements.UserIsOverdrawnOnChecking,
                statements,
                DaisyMode.Release);
            Assert.AreEqual(prog, prog2);
        }
    }
}
