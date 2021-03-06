﻿using Microsoft.SharePoint.Client;

namespace NRES_SubscriptionApp
{
    public class SubscriptionItem
    {
        public int ID { get; set; }
        public int DocumentID { get; set; }
        public string DocumentTitle { get; set; }
        public string InvID { get; set; }
        public bool IsSubscribeDone { get; set; }
        public string Jurisdiction { get; set; }
        public string DocumentAuthor { get; set; }
        public string Notes { get; set; }
        public string WebApplicationURL { get; set; }
        public int IsSuccess { get; set; }
        public FieldUserValue CreatedBy { get; set; }
    }
}
