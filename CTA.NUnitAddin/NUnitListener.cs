using CTA.NUnitAddin;
using NUnit.Core;
using NUnit.Core.Extensibility;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;

namespace CTA.NUnitAddin
{
    /// <summary>
    /// The NUnitListener class implements the EventListener interface from NUnit http://www.nunit.org/index.php?p=eventListeners&r=2.6.4.
    /// This provides hooks for each test run/suite/case start and finish events.
    /// As for CTA the most important part is to send back a test status on each TestFinished
    /// as defined in the "CTA for Test Framework integrator" document https://thehub.thomsonreuters.com/docs/DOC-664536
    /// </summary>
    [NUnitAddinAttribute(Type = ExtensionType.Core, Name = "CTA Reporter NUnit Addin", Description = "An addin for reporting NUnit test results to CTA")]
    public class NUnitListener : IAddin, EventListener, CTA.TestData.ITestCase
    {
        private string testCaseID;
        private Hashtable testCaseData;
        private string testCaseDescription = "";
        private string testCaseCapability = "";
        private ScreenCapture screenCapture = new ScreenCapture();

        private static string capabilityLabel = "Capability";

		private static string UncapText(Match m)
        {
            // Get the matched string. 
            string x = m.Groups[1].ToString();
            if ((x.Length > 0) && char.IsUpper(x[0]))
            {
                // Capitalize it. 
                x = char.ToLower(x[0]) + x.Substring(1, x.Length - 1);
            }

            return x;
        }

        private readonly static MatchEvaluator MatchEvalUncapText = new MatchEvaluator(UncapText);

        /// <summary>
        /// Beautify a string into a text.
        /// </summary>
        /// <param name="inputText">The input text token to transform. It can be in the form:
        /// <list type="bullet">
        /// <item><description>"AStringLikeTHISOne"</description></item>
        /// <item><description>"a_string_like_THIS_one"</description></item>
        /// <item><description>"A_String_Like_THIS_One"</description></item>
        /// </list>
        /// </param>
        /// <returns>All the previous input string are transformed as "a string like THIS one".</returns>
        private static string BeautifyToken(string inputText)
        {
            var tokens = inputText.Split('.');
            for (var i = 0; i < tokens.Length; i++)
            {
                var text = tokens[i];
                if (text.IndexOf('_') > 0)
                {
                    text = text.Replace('_', ' ');
                }
                else
                {
                    text = Regex.Replace(
                        Regex.Replace(
                            text,
                            @"(\P{Ll})(\P{Ll}\p{Ll})",
                            "$1 $2"
                            ),
                        @"(\p{Ll})(\P{Ll})",
                        "$1 $2"
                        );
                }
                tokens[i] = Regex.Replace(text, @"\b(\P{Ll}\p{Ll}|\P{Ll}\b)", MatchEvalUncapText);
            }
            return string.Join(".", tokens);
        }

        /// <summary>
        /// Beautify a method or class name into a text.
        /// </summary>
        /// <param name="inputText">The input text to transform. It can be in the form:
        /// <list type="bullet">
        /// <item><description>"AStringLikeTHISOne("ParameterAreNotChanged",123).WithSomeMoreDetails(99)"</description></item>
        /// <item><description>"a_string_like_THIS_one("ParameterAreNotChanged",123).with_some_more_details(99)"</description></item>
        /// <item><description>"A_String_Like_THIS_One("ParameterAreNotChanged",123).With_Some_More_Details(99)"</description></item>
        /// </list>
        /// </param>
        /// <returns>All the previous input string are transformed as "a string like THIS one("ParameterAreNotChanged",123).with some more details(99)".</returns>
        public static string Beautify(string inputText)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection mc = Regex.Matches(inputText, @"\(.*?\)");
            int offset = 0;
            foreach (var m in mc)
            {
                int i = ((Match)m).Index;
                int l = ((Match)m).Length;
                sb.Append(BeautifyToken(inputText.Substring(offset, i - offset)));
                sb.Append(inputText.Substring(i, l));
                offset += i + l;
            }
            if (offset < inputText.Length)
            {
                sb.Append(BeautifyToken(inputText.Substring(offset)));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Remove all parameters from a character string. Like in "DoSomething("WithThisValue",123).AndMoreWith(this)" that will be changed in "DoSomething.AndMoreWith".
        /// </summary>
        /// <param name="text">The input string to modify like "DoSomething("WithThisValue",123).AndMoreWith(this)".</param>
        /// <returns>The modified string like "DoSomething.AndMoreWith".</returns>
        public static string StripParameters(string text)
        {
            return Regex.Replace(text, @"\(.*?\)", "", RegexOptions.Multiline);
        }

        public bool Install(IExtensionHost host)
        {
            CTATestCaseBuilder builder = new CTATestCaseBuilder(host, screenCapture);
            host.GetExtensionPoint("TestCaseBuilders").Install(builder);

            IExtensionPoint listeners = host.GetExtensionPoint("EventListeners");
            if (listeners == null)
                return false;

            listeners.Install(this);
            return true;
        }

        public void RunStarted(string name, int testCount)
        {
            TinyIoC.TinyIoCContainer.Current.Register<CTA.TestData.ITestCase>(this);
        }

        public void RunFinished(Exception exception)
        { }

        public void RunFinished(TestResult result)
        { }

        public void SuiteFinished(TestResult result)
        { }

        public void SuiteStarted(TestName testName)
        { }

        public void TestFinished(TestResult result)
        {

            TestCase testCase = BuildTestCase(result);           
            
            if (result.Executed)
            {
                if (result.IsSuccess)
                {
                    testCase.Pass(result.Message);
                    Console.WriteLine("The test case has passed.");
                }
                else if (result.IsError)
                {
                    testCase.Inconclusive(result.Message);
                    Console.WriteLine("The test case is inconclusive.");
                }
                else
                {
                    testCase.Fail(result.Message);
                    Console.WriteLine("The test case has failed.");
                }

                testCase.SendToEtap(new Service.DefaultEtapService().Instance.RestProxy);
                Console.WriteLine("Successfully sent status to CTA.");
        }

        }

        public void TestOutput(TestOutput testOutput)
        { }

        public void TestStarted(TestName testName)
        {
            testCaseID = testName.FullName;
            testCaseData = new Hashtable();
        }

        public void AddTestData(string dataName, string dataValue)
        {
            testCaseData[dataName] = dataValue;
        }

        public void UnhandledException(Exception exception)
        { }

        private TestCase BuildTestCase(TestResult result)
        {

            TestCase testCase = new TestCase(testCaseID);

            string description = result.Description;
            if (string.IsNullOrEmpty(description))
            {
                description = Beautify(StripParameters(result.Name));
            }
            testCase.Description = description;


            if (result.Test.Properties.Contains(capabilityLabel))
            {
                testCase.Capability = result.Test.Properties[capabilityLabel].ToString();
            }

            testCase.SetCategories(result.Test.Categories);
			testCase.SetProperties(result.Test.Properties);
            testCase.AddProperties(testCaseData);

            return testCase;
        }
    }
}
