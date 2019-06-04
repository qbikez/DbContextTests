using NUnit.Framework;
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
    static class AssertThat
    {
        public static void AreEqual<T>(T expected, T actual, AssertOutcome outcome = AssertOutcome.Fail)
        {
            try
            {
                Assert.AreEqual(expected, actual);
            }
            catch (AssertionException ex)
            {
                switch(outcome)
                {
                    case AssertOutcome.Inconclusive:
                        Assert.Inconclusive(ex.Message);
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
