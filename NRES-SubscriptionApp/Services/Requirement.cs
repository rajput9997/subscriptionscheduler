using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp.Services
{
    public class Requirement
    {
        public const string RequirementListTitle = "Requirements";

        /// <summary>
        /// Get the requirement inventory.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqID"></param>
        /// <returns></returns>
        public string GetRequirementInventory(ClientContext context, int reqID)
        {
            ListItem listItem = context.Web.Lists.GetByTitle(RequirementListTitle).GetItemById(reqID);
            context.Load(listItem);
            context.ExecuteQuery();
            return listItem["inventory"]?.ToString();
        }

        public static void RequirementInventoryUpdate(ClientContext context, ReqItemcollection reqItemcollection)
        {
            context.RequestTimeout = -1;
        }
    }
}
