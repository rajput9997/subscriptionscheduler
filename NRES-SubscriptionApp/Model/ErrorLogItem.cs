using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp
{
    public class ErrorLogItem
    {
        public int ID { get; set; }

        public int SubscriptionID { get; set; }

        public string MethodName { get; set; }

        public string StackTrace { get; set; }

        public string ErrorMessage { get; set; }
    }
}
