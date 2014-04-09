﻿using System;
using System.Linq;
using Ancestry.Daisy.Linking;
using Ancestry.Daisy.Tests.Daisy.Component.Controllers;
using Ancestry.Daisy.Tests.Daisy.Component.Domain;
using Moq;

namespace Ancestry.Daisy.Tests.Daisy.Component
{
    using NUnit.Framework;

    using Ancestry.Daisy.Statements;

    [TestFixture]
    public class EndToEndDaisyTest
    {

        public StatementSet set = new StatementSet().FromAssemblyOf<UserController>();

        [Test]
        public void ItPreservesAttachments()
        {
            var code = @"Set Attachment";

            var program = DaisyCompiler.Compile<int>(code, new StatementSet().FromController(typeof(TestStatementController)));
            var result = program.Execute(4);
            Assert.AreEqual(5, result.Attachments["Test"]);
        }

        [Test]
        public void ItDoesNotAllowNongroupingStatementsToBeUsedAsGroups()
        {
            var code = @"Is greater than 2
  Is greater than 10
";
            var exp = Assert.Throws<FailedLinkException>(() =>
                DaisyCompiler.Compile<int>(code, new StatementSet().FromController(typeof(TestStatementController))));
            Assert.AreEqual(1, exp.Errors.Count);
            var err = exp.Errors.First();
            Assert.IsInstanceOf<NoLinksPermittedError>(err);
        }

        [Test]
        public void ItMonitorsPerformance()
        {
            var program = DaisyCompiler.Compile<User>(Statements.UserHasUnusedAccount, set);
            var result = program.Execute(TestData.Ben);

            Console.WriteLine(result.DebugInfo.DebugView);

            Assert.IsTrue(result.Outcome);
            Assert.IsNotNull(result.DebugInfo.Measurments);
            Assert.GreaterOrEqual(result.DebugInfo.Measurments.OpsExecuted,7);
            Assert.GreaterOrEqual(result.DebugInfo.Measurments.TotalStatementsExecuted,4);
            Assert.Greater(result.DebugInfo.Measurments.OpsExecuted,result.DebugInfo.Measurments.TotalStatementsExecuted);
            Assert.GreaterOrEqual(result.DebugInfo.Measurments.ExecutionTime,0);
            Assert.Less(result.DebugInfo.Measurments.ExecutionTime,10);
        }

        public class TestStatementController : StatementController<int>
        {
            public bool SetAttachment()
            {
                this.Attachments["Test"] = 5;
                return true;
            }

            [Matches("Is greater than (\\d+)")]
            public bool IsGreaterThan(int val)
            {
                return Scope > val;
            }
        }
    }
    
}
