using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA.NUnitAddin.Service
{
   public class DefaultEtapService
    {
        private static CTAService instance = null;

        public CTAService Instance
        {
            get
            {
                if (instance == null)
                    instance = new CTAService("http://localhost:3000");
                return instance;
            }
        }
    }
}
