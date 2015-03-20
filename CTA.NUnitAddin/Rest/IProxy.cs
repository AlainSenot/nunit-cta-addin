using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA.NUnitAddin.Rest
{
    public interface IProxy
    {
        T Execute<T>(RestSharp.RestRequest request) where T : new();
        T Execute<T>(RestSharp.RestRequest request, RestSharp.RestClient client) where T : new();
    }
}
