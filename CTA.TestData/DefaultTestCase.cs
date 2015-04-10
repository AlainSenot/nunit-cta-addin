using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTA.TestData
{
    public sealed class DefaultTestCase : ITestCase
    {
        public bool AddTestData(string dataName, string dataValue)
        {
            Console.Error.WriteLine("Warning: cannot connect to CTA.NUnitAddin, your data are not recorded.\nPlease check your \"addin\" directory in your NUnit installation.");
            return false;
        }
    }
}
