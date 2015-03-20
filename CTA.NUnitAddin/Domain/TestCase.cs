using CTA.NUnitAddin.Rest;
using RestSharp;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;
using System;

namespace CTA.NUnitAddin
{
    public class TestCase
    {
        private string id;
        private string description;        
        private string categories;
        private string capability;
        private Hashtable properties = null;
        private Result result;


        public TestCase(string id)
        {
            this.id = id;
            result = new Result(Result.UNDEFINED);
        }    


        public void Pass(string message)
        {
            result = new Result(Result.PASS, message);
        }

        public void Fail(string message)
        {
            result = new Result(Result.FAIL, message);
        }

        public void Inconclusive(string message)
        {
            result = new Result(Result.INCONCLUSIVE, message);
        }

        public string DescriptionPrefix
        {
            get;
            set;
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }       

        public string Capability
        {
            get
            {
                return capability;
            }     
            set
            {
                capability = value;
            }
        }

        

        public void SetCategories(IList categoriesList)
        {
            if (categoriesList != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var s in categoriesList)
                {
                    if (sb.Length > 0)
                        sb.Append(',');
                    sb.Append(s);
                }
                categories = sb.ToString();
            }
        }

        public string Categories
        {
            get
            {
                return categories;
            }
        }

        public void SetProperties(IDictionary props)
        {
            properties = new Hashtable();
            foreach (DictionaryEntry de in props)
            {
                string key = de.Key.ToString();
                if (!key.StartsWith("_"))
                {
                    properties[key] = de.Value;
                }
            }
        }

        public void AddProperties(IDictionary props)
        {
            if (properties == null)
                properties = new Hashtable();
            foreach (DictionaryEntry de in props)
            {
                string key = de.Key.ToString();
                if (!key.StartsWith("_"))
                {
                    properties[key] = de.Value;
                }
            }
        }

        public IDictionary Properties
        {
            get
            {
                return properties;
            }

        }

        public void SendToEtap(IProxy proxy)
        {
            RestRequest request = new RestRequest("testframework/testStatus");
            request.AddParameter("id", GetShortTestCaseId(id));
            if (!string.IsNullOrWhiteSpace(description))
            {
                if (!string.IsNullOrWhiteSpace(DescriptionPrefix))
                    request.AddParameter("description", DescriptionPrefix + description);
                else
                    request.AddParameter("description", description);
            }
            request.AddParameter("status", result.Status);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                request.AddParameter("message", result.Message);
            }
            if (!string.IsNullOrWhiteSpace(categories))
            {
                request.AddParameter("categories", categories);
            }
            if (!string.IsNullOrWhiteSpace(capability))
            {
                request.AddParameter("capability", capability);
            }
            if (properties != null)
            {
                foreach (DictionaryEntry de in properties)
                {
                    string key = de.Key.ToString();
                    if (!key.Equals("Capability", StringComparison.OrdinalIgnoreCase))
                    {
                        request.AddParameter(key, de.Value);
                    }
                }
            }
            proxy.Execute<NullResponse>(request);
        }


        //TO-DO: Remove this method, which is a temp hack added to get the screenshot file name under 255 characters.
        private string GetShortTestCaseId(string TestCaseId)
        {
            if (TestCaseId.Trim().StartsWith("Eikon.MonitoringTests.Tests."))
            {
                return Regex.Replace(TestCaseId, @"Eikon.MonitoringTests.Tests.", "");
            }
            else if (TestCaseId.Trim().StartsWith("Eikon.OPSConfidenceTests.Tests."))
            {
                return Regex.Replace(TestCaseId, @"Eikon.OPSConfidenceTests.Tests.", "");
            }

            return TestCaseId;

        }
    }
}
