﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Tests.Component.Controllers
{
    using Ancestry.Daisy.Statements;
    using Ancestry.Daisy.Tests.Component.Domain;

    public class TransactionController : StatementController<Transaction>
    {
        [Matches("Timestamp before (\\d+) years? ago")]
        public bool TimestampBeforeYearsAgo(int yearsAgo)
        {
            return DateTime.Now.AddYears(-yearsAgo) > Scope.Timestamp;
        }

        [Matches("Amount is greater than (\\d+)")]
        public bool AmountIsGreaterThan(int value)
        {
            return Scope.Amount > value;
        }
    }
}
