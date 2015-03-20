using NUnit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CTA.NUnitAddin
{
    public class TestMethodExtension : NUnitTestMethod
    {
        private ScreenCapture _screenCapture;
        /// <summary>
        /// Arguments to be used in invoking the method
        /// </summary>
        internal object[] arguments;

        /// <summary>
        /// The expected result of the method return value
        /// </summary>
        internal object expectedResult;

        /// <summary>
        /// Indicates whether expectedResult was set - thereby allowing null as a value
        /// </summary>
        internal bool hasExpectedResult;

        public TestMethodExtension(MethodInfo method, ScreenCapture screenCapture)
            : base(method)
        {
            this._screenCapture = screenCapture;
        }

        protected override void RecordException(Exception exception, TestResult testResult, FailureSite failureSite)
        {
            this._screenCapture.TakeAndSave(this._screenCapture.GetEtapScreenshotPath(this.TestName.FullName));
            base.RecordException(exception, testResult, failureSite);
        }

        protected override object RunTestMethod()
        {
            object fixture = this.Method.IsStatic ? null : this.Fixture;
            object result =  Reflect.InvokeMethod(this.Method, fixture, this.arguments);
            if (this.hasExpectedResult)
            {
                NUnitFramework.Assert.AreEqual(expectedResult, result);
            }
            return result;
        }

    }


}
