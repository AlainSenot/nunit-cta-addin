using log4net;
using RestSharp;

namespace CTA.NUnitAddin.Rest
{
    class RestProxy : IProxy
    {
        private string baseUrl;
        private ILog Logger { get { return LogManager.GetLogger(this.GetType()); } }

        public RestProxy(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public T Execute<T>(RestRequest request, RestClient client) where T : new()
        {
            T responseData = default(T);

            if ((baseUrl != null) && (RequestIsValid(request)))
            {
                LogRequestDetailsForDebugging(request);

                IRestResponse<T> response = client.Execute<T>(request);

                if (!HttpError(response))
                {
                    responseData = response.Data;
                    Logger.Debug(response.Content);
                }
            }

            return responseData;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            RestClient restClient = new RestClient(baseUrl);
            return Execute<T>(request, restClient);
        }

        private bool HttpError(IRestResponse response)
        {
            bool errorDetected = (response.StatusCode != System.Net.HttpStatusCode.OK);

            if (errorDetected)
                Logger.ErrorFormat("There was a problem calling the CTA API. [HTTP Status: '{0}' - '{1}']", response.StatusCode, response.StatusDescription);

            return errorDetected;
        }

        private bool RequestIsValid(IRestRequest request)
        {
            bool isValid = false;

            if (request != null)
            {
                isValid = true;
                request.Parameters.ForEach(param =>
                {
                    isValid &= (param.Name != null) && (param.Value != null);
                });
            }

            return isValid;
        }

        private void LogRequestDetailsForDebugging(RestRequest request)
        {
            Logger.DebugFormat("Calling API resource '{0}'", request.Resource);

            if (request.Parameters.Count > 0)
            {
                Logger.DebugFormat("\twith parameters:");

                request.Parameters.ForEach(param =>
                {
                    Logger.DebugFormat("['{0}'] ('{1}') = '{2}'", param.Name, param.Type, param.Value);
                });
            }
        }
    }
}
