using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using NRES_SubscriptionApp.Model;

namespace NRES_SubscriptionApp.Services
{
    public class ClientLogFolder
    {
        public const int noOfAttempts = 3;

        // credentials - we used into program cs file.
        public const string AccountName = @"rsaparco";
        public const string Password = @"NRES";

        public static bool IsFolderCreated { get; set; }

        public static bool CreateInventoryIDFolder(ClientContext rootcontext, List olistClientLog, string inventoryID)
        {
            try
            {
                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();

                itemCreateInfo.UnderlyingObjectType = FileSystemObjectType.Folder;
                itemCreateInfo.LeafName = inventoryID;

                ListItem newItem = olistClientLog.AddItem(itemCreateInfo);
                newItem["Title"] = inventoryID;
                newItem.Update();
                rootcontext.ExecuteQuery();
                IsFolderCreated = true;
            }
            catch (Exception ex)
            {
                IsFolderCreated = true;
            }
            return IsFolderCreated;
        }

        private static async Task<ClientLogItem> ReadDataCollection(NetworkCredential networkCredential, string siteUrl, int reqId, int docId)
        {
            string apiUrl = siteUrl + "/_api/web/lists/getByTitle('" + CommonVariables.RequirementListTitle + "')/items(" + reqId + ")";
            HttpWebRequest endpointRequest = (HttpWebRequest)HttpWebRequest.Create(apiUrl);

            endpointRequest.Method = "GET";
            endpointRequest.Accept = "application/json;odata=verbose";
            endpointRequest.Credentials = networkCredential;
            HttpWebResponse endpointResponse = (HttpWebResponse)endpointRequest.GetResponse();
            try
            {
                using (WebResponse webResponse = endpointRequest.GetResponse())
                {
                    using (Stream webStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            JObject jobj = JObject.Parse(response);
                            responseReader.Close();
                            return new ClientLogItem
                            {
                                title = jobj.First.First["Title"]?.ToString(),
                                heading1 = jobj.First.First["heading1"]?.ToString(),
                                heading2 = jobj.First.First["heading2"]?.ToString(),
                                heading3 = jobj.First.First["heading3"]?.ToString(),
                                heading4 = jobj.First.First["heading4"]?.ToString(),
                                heading5 = jobj.First.First["heading5"]?.ToString(),
                                requirementType = jobj.First.First["RequirementType0"]?.ToString(),
                                listURL = siteUrl + "/lists/Requirements",
                                itemRESTURL = siteUrl + "/_api/web/lists/GetByTitle('" + CommonVariables.RequirementListTitle + "')/items(" + reqId + ")",
                                itemURL = siteUrl + "/SitePages/ViewRequirement.aspx?rid=" + reqId + "&listName=" + CommonVariables.DocumentListTitle + "&did=" + docId + "&isdlg=1&isReq=false"
                        };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new ClientLogItem();
                // Console.Out.WriteLine(e.Message); Console.ReadLine();
            }
        }

        public static async Task ClientLogFolderCreation(ClientContext parentContext, ReqItemcollection reqItemcollection, SubscriptionItem subscriptionItem)
        {
            string LogApplicationURL = System.Configuration.ConfigurationManager.AppSettings["LogApplicationURL"];

            NetworkCredential networkCredential = new NetworkCredential(AccountName, Password, "NRES");
            using (ClientContext rootcontext = new ClientContext(LogApplicationURL))
            {
                rootcontext.Credentials = networkCredential;
                List list = rootcontext.Web.Lists.GetByTitle(CommonVariables.ClientLogListTitle);
                if (!IsFolderCreated)
                {
                    CreateInventoryIDFolder(rootcontext, list, subscriptionItem.InvID);
                }
                await UpdateClientLogItems(parentContext, rootcontext, reqItemcollection, networkCredential, subscriptionItem);
            }
        }

        public static async Task UpdateClientLogItems(ClientContext parentContext, ClientContext rootcontext, ReqItemcollection reqItemcollection, NetworkCredential networkCredential, SubscriptionItem subscriptionItem)
        {
            try
            {
                string folder_RelativeUrl = "/Lists/ClientLogs/" + subscriptionItem.InvID;

                List olistClientLog = rootcontext.Web.Lists.GetByTitle(CommonVariables.ClientLogListTitle);
                var count = reqItemcollection.reqitems.Count;
                var maxChunkSize = 100;
                var shouldRun = true;
                var start = 0; 
                var chunkSize = count < maxChunkSize ? count : maxChunkSize;
                var end = start + chunkSize < count ? start + chunkSize : count;
                rootcontext.RequestTimeout = -1;
                while (shouldRun)
                {
                    shouldRun = end == count ? false : true;
                    for (int i = start; i < end; i++)
                    {
                        string reqID = reqItemcollection.reqitems[i].reqID;
                        ClientLogItem clientLogItem = await ReadDataCollection(networkCredential, subscriptionItem.WebApplicationURL, Convert.ToInt32(reqID), subscriptionItem.DocumentID);

                        ListItemCreationInformation listItemCreationInformation = new ListItemCreationInformation();
                        listItemCreationInformation.FolderUrl = folder_RelativeUrl;

                        var listItem = olistClientLog.AddItem(listItemCreationInformation);
                        listItem["Title"] = "Log entry";
                        listItem["Action"] = reqItemcollection.reqitems[i].Isselected ? "Add" : "Remove";
                        listItem["Heading1"] = clientLogItem.heading1;
                        listItem["Heading2"] = clientLogItem.heading2;
                        listItem["Heading3"] = clientLogItem.heading3;
                        listItem["Header4"] = clientLogItem.heading4;
                        listItem["Header5"] = clientLogItem.heading5;
                        listItem["RequirementType"] = clientLogItem.requirementType;
                        listItem["ListURL"] = clientLogItem.listURL;
                        listItem["ItemID"] = reqID;
                        listItem["ItemURL"] = clientLogItem.itemURL;
                        listItem["RESTItemProperties"] = clientLogItem.itemRESTURL;
                        listItem["Client"] = subscriptionItem.InvID;
                        listItem["Note"] = reqItemcollection.reqitems[i].Isselected ? "" : subscriptionItem.Notes;
                        listItem["Jurisdiction"] = subscriptionItem.Jurisdiction;
                        listItem["Author0"] = subscriptionItem.DocumentAuthor;

                        listItem.Update();
                        rootcontext.Load(listItem);

                    }

                    rootcontext.ExecuteQuery();
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
                        await UpdateClientLogItems(parentContext, rootcontext, reqItemcollection, networkCredential, subscriptionItem);
                        if (subscriptionItem.IsSuccess == 1)
                        {
                            break;
                        }
                    }
                }

                Errorlogs.Log(parentContext, new ErrorLogItem
                {
                    ErrorMessage = ex.Message,
                    MethodName = "ClientLogFolder.UpdateClientLogItems",
                    StackTrace = ex.StackTrace,
                    SubscriptionID = subscriptionItem.ID
                });
            }
        }
    }
}
