using System;

namespace CTA.NUnitAddin
{
   public class Result
    {
        public static string PASS = "OK";
        public static string FAIL = "Failed";
        public static string UNDEFINED = "Undefined";
        public static string INCONCLUSIVE = "Inconclusive";

        private string status = String.Empty;
        private string message = String.Empty;

        public string Status { get { return status; } }
        public string Message { get { return message; } }

        public Result(string status, string message = "")
        {
            this.status = status;
            this.message = message;
        }
    }
}
