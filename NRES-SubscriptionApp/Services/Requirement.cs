using System;
using System.Net;
using System.Threading;
using Microsoft.SharePoint.Client;

namespace NRES_SubscriptionApp.Services
{
    public class Requirement
    {
        public const int noOfAttempts  = 3;

        /// <summary>
        /// Get the requirement inventory.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqID"></param>
        /// <returns></returns>
        public static string GetRequirementInventory(ClientContext context, ListItem oListItem)
        {
            context.Load(oListItem);
            context.ExecuteQuery();
            return oListItem["inventory"]?.ToString();
        }

        public static void RequirementInventoryUpdate(ClientContext context, ReqItemcollection reqItemcollection, SubscriptionItem subscriptionItem, NetworkCredential networkCredential)
        {
            try
            {
                context.Credentials = networkCredential;
                List olistReq = context.Web.Lists.GetByTitle(CommonVariables.RequirementListTitle);
                var count = reqItemcollection.reqitems.Count;
                var maxChunkSize = 100;
                var shouldRun = true;
                var start = 0;
                var chunkSize = count < maxChunkSize ? count : maxChunkSize;
                var end = start + chunkSize < count ? start + chunkSize : count;
                context.RequestTimeout = -1;
                while (shouldRun)
                {
                    shouldRun = end == count ? false : true;
                    for (int i = start; i < end; i++)
                    {
                        ListItem oListItem = olistReq.GetItemById(reqItemcollection.reqitems[i].reqID);
                        oListItem["inventory"] = reqItemcollection.reqitems[i].invID;
                        oListItem.Update();
                        context.Load(oListItem);
                    }

                    context.ExecuteQuery();
                    start = end;
                    end = start + chunkSize < count ? start + chunkSize : count;
                }
                subscriptionItem.IsSuccess = 1;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Version conflict"))
                {
                    for (int i = 0; i < noOfAttempts; i++)
                    {
                        Thread.Sleep(1000);
                        Requirement.RequirementInventoryUpdate(context, reqItemcollection, subscriptionItem, networkCredential);
                        if(subscriptionItem.IsSuccess == 1)
                        {
                            break;
                        }
                    }
                }
                Errorlogs.Log(context, new ErrorLogItem
                {
                    ErrorMessage = ex.Message,
                    MethodName = "Requirement.RequirementInventoryUpdate",
                    StackTrace = ex.StackTrace,
                    SubscriptionID = subscriptionItem.ID
                });
            }
        }
    }
}
