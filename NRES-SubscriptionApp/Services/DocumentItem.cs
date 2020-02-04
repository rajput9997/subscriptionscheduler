using System;
using System.Net;
using Microsoft.SharePoint.Client;

namespace NRES_SubscriptionApp.Services
{
    public class DocumentItem
    {
        /// <summary>
        /// Get the requirement inventory.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqID"></param>
        /// <returns></returns>
        public static string GetDocumentInventory(ClientContext context, ListItem oListItem)
        {
            context.Load(oListItem);
            context.ExecuteQuery();
            return oListItem["inventory"]?.ToString();
        }

        public static void DocumentInventoryUpdate(ClientContext context, int documentID, SubscriptionItem subscriptionItem, NetworkCredential networkCredential)
        {
            try
            {
                List olistDocs = context.Web.Lists.GetByTitle(CommonVariables.DocumentListTitle);
                ListItem oListItem = olistDocs.GetItemById(documentID);
                string inventorycoll = GetDocumentInventory(context, oListItem);

                if (!inventorycoll.Contains("-" + subscriptionItem.InvID + "-"))
                {
                    inventorycoll += "-" + subscriptionItem.InvID + "-";
                }

                oListItem["inventory"] = inventorycoll;
                oListItem.Update();
                context.Load(oListItem);
                context.ExecuteQuery();
                subscriptionItem.IsSuccess = 1;
            }
            catch (Exception ex)
            {
                Errorlogs.Log(context, new ErrorLogItem
                {
                    ErrorMessage = ex.Message,
                    MethodName = "DocumentItem.DocumentInventoryUpdate",
                    StackTrace = ex.StackTrace,
                    SubscriptionID = subscriptionItem.ID
                });
            }
        }
    }
}
