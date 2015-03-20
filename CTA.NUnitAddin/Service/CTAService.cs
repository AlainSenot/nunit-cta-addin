using CTA.NUnitAddin.Rest;
using System;

namespace CTA.NUnitAddin.Service
{
    public class CTAService
    {
        private IProxy restProxy;

        public static string CtaExecutionDir
        {
            get
            {
                return Environment.GetEnvironmentVariable("CTA_EXECUTION_DIR") ?? Environment.GetEnvironmentVariable("ETAP_EXECUTION_DIR");
            }       
        }

        public IProxy RestProxy { get { return restProxy; } }

        public CTAService(IProxy proxy)
        {
            restProxy = proxy;
        }

        public CTAService(string url)
        {
            restProxy = new RestProxy(url);
        }
    }
}
