using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp.Model
{
   public class ClientLogItem
    {
        public string title { get; set; }
        public string action { get; set; }
        public string heading1 { get; set; }
        public string heading2 { get; set; }
        public string heading3 { get; set; }
        public string heading4 { get; set; }
        public string heading5 { get; set; }
        public string requirementType { get; set; }
        public string itemURL { get; set; }
        public string listURL { get; set; }
        public string itemRESTURL { get; set; }
    }
}
