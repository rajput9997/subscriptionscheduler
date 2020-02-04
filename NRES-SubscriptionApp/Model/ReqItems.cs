using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp
{
    public class ReqItemcollection
    {
        public List<reqitem> reqitems { get; set; }
    }

    public class reqitem
    {
        public string reqID { get; set; }
        public bool Isselected { get; set; }
        public string invID { get; set; }
    }
}
