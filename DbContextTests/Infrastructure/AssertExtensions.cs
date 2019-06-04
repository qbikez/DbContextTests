using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Infrastructure
{
    /// <summary>
    /// will result in an inconclusive test, not a failing one
    /// </summary>
    static class AssertExtensions
    {
        public static void AreEqual<T>(this Assert a, T expected, T actual, AssertOutcome outcome = AssertOutcome.Fail)
        {
            try
            {
                Assert.AreEqual(expected, actual);
            }
            catch (AssertFailedException ex)
            {
                switch(outcome)
                {
                    case AssertOutcome.Inconclusive:
                        Assert.Inconclusive(ex.Message, ex);
                        return;
                    case AssertOutcome.Fail:
                    default:
                        throw;
                }
            }
        }
    }

    enum AssertOutcome
    {
        Fail,
        Inconclusive
    }
}
