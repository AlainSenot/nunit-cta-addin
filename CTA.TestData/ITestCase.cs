namespace CTA.TestData
{
    /// <summary>
    /// Minimal interface for a test implementer to provide some custom data result back to CTA.
    /// See the "CTA for Test Framework integrator" document https://thehub.thomsonreuters.com/docs/DOC-664536
    /// </summary>
    public interface ITestCase
    {
        /// <summary>
        /// Add a custom test data to the current TestCase being executed.
        /// Data value are overriden if the same key name is used several times.
        /// </summary>
        /// <param name="dataName">The name of data to add. If a data value is already defined for this name, it will be overriden.</param>
        /// <param name="dataValue">The value of the data to add</param>
        void AddTestData(string dataName, string dataValue);
    }
}
