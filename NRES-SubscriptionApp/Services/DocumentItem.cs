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

        public static void DocumentInventoryUpdate(ClientContext context, int documentID, SubscriptionItem subscriptionItem, NetworkCredential networkCredential, bool isRemoveCall)
        {
            try
            {
                List olistDocs = context.Web.Lists.GetByTitle(CommonVariables.DocumentListTitle);
                ListItem oListItem = olistDocs.GetItemById(documentID);
                string inventorycoll = GetDocumentInventory(context, oListItem);

                if (!string.IsNullOrWhiteSpace(inventorycoll))
                {
                    if (!inventorycoll.Contains("-" + subscriptionItem.InvID + "-"))
                    {
                        inventorycoll += "-" + subscriptionItem.InvID + "-";
                    }

                    oListItem["inventory"] = inventorycoll;
                }
                else
                {
                    oListItem["inventory"] = "-" + subscriptionItem.InvID + "-";
                }
                oListItem["_ModerationStatus"] = 0;
                oListItem.Update();
                context.Load(oListItem);
                context.ExecuteQuery();
                if (isRemoveCall)
                {
                    RemoveInventoryIdsDocuments(context, oListItem, documentID, olistDocs, "-" + subscriptionItem.InvID + "-", subscriptionItem);
                }

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

        private static void RemoveInventoryIdsDocuments(ClientContext context, ListItem oDocListItem, int documentID, List olistDocs, string invID, SubscriptionItem subscriptionItem)
        {
            try
            {
                List olistReq = context.Web.Lists.GetByTitle(CommonVariables.RequirementListTitle);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><Query><Where><And><Eq><FieldRef Name='ParentId' LookupId='TRUE'/><Value Type='Lookup'>" + documentID + "</Value></Eq><Contains><FieldRef Name='inventory' /><Value Type='Text'>" + invID + "</Value></Contains></And></Where></Query><RowLimit>10000</RowLimit></View>";

                ListItemCollection oReqlistItems = olistReq.GetItems(camlQuery);
                context.Load(oReqlistItems, items => items.Include(
                item => item["ID"]));

                context.ExecuteQuery();
                if (oReqlistItems.Count == 0)
                {
                    oDocListItem["inventory"] = oDocListItem["inventory"]?.ToString().Replace(invID, "");
                    oDocListItem["_ModerationStatus"] = 0;
                    oDocListItem.Update();
                    context.Load(oDocListItem);
                    context.ExecuteQuery();
                }
            }
            catch(Exception ex)
            {
                Errorlogs.Log(context, new ErrorLogItem
                {
                    ErrorMessage = ex.Message,
                    MethodName = "DocumentItem.RemoveInventoryIdsDocuments",
                    StackTrace = ex.StackTrace,
                    SubscriptionID = subscriptionItem.ID
                });
            }
        }
    }
}
