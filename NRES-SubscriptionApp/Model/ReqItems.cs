using System.Collections.Generic;

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
