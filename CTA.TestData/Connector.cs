using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTA.TestData
{
    public class Connector
    {
        public static ITestCase GetTestCase(bool nullIfNotAvailable = false)
        {
            try
            {
                return TinyIoC.TinyIoCContainer.Current.Resolve<CTA.TestData.ITestCase>();
            }
            catch (Exception)
            {
                return nullIfNotAvailable ? null : new DefaultTestCase();
            }
        }
    }
}
