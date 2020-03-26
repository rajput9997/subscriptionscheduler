using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using NRES_SubscriptionApp.Services;

namespace NRES_SubscriptionApp
{
    class Program
    {
        public static string WebApplicationURL = string.Empty;
        public static string ListTitle = string.Empty;
        public static NetworkCredential networkCredential = null;

        static void Main(string[] args)
        {
            WebApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];
            ListTitle = System.Configuration.ConfigurationManager.AppSettings["ListTitle"];
            networkCredential = new NetworkCredential(CommonVariables.AccountName, CommonVariables.Password, "NRES");
            StartSubscription(WebApplicationURL, ListTitle).Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webApplicationURL">The web application url</param>
        /// <param name="listTitle">The list title</param>
        public static async Task StartSubscription(string webApplicationURL, string listTitle)
        {
            using (ClientContext context = new ClientContext(webApplicationURL))
            {
                try
                {
                    context.Credentials = new NetworkCredential(CommonVariables.AccountName, CommonVariables.Password, "NRES");
                    List list = context.Web.Lists.GetByTitle(listTitle);
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='IsSubscribeDone' /><Value Type='Boolean'>0</Value></Eq></Where></Query><RowLimit>100</RowLimit></View>";

                    ListItemCollection listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.Include(
                    item => item["ID"], item => item["Title"], item => item["InvID"],
                    item => item["IsSubscribeDone"], item => item["Items"], item => item["DocumentID"],
                    item => item["Jurisdiction"], item => item["DocumentAuthor"], item => item["Notes"],
                    item => item["Author"], item => item["SiteUrl"]));

                    context.ExecuteQuery();
                    await ReadSubscriptionItemCollection(context, listItems, webApplicationURL);
                }
                catch (Exception ex)
                {
                    Errorlogs.Log(context, new ErrorLogItem
                    {
                        ErrorMessage = ex.Message,
                        MethodName = "Program.StartSubscription",
                        StackTrace = ex.StackTrace,
                        SubscriptionID = 0
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="listItems"></param>
        public static async Task ReadSubscriptionItemCollection(ClientContext context, ListItemCollection listItems, string webApplicationURL)
        {
            string targetSiteCollectionUrl = string.Empty;
            foreach (var listItem in listItems)
            {
                try
                {
                    int subscriptionID = Convert.ToInt32(listItem["ID"]);
                    string Itemcoll = listItem["Items"]?.ToString();
                    string inventoryID = listItem["InvID"]?.ToString();
                    int DocumentID = Convert.ToInt32(listItem["DocumentID"]?.ToString());
                    targetSiteCollectionUrl = listItem["SiteUrl"]?.ToString();
                    FieldUserValue createdBy = (FieldUserValue)listItem["Author"];
                    var reqItemcoll = Newtonsoft.Json.JsonConvert.DeserializeObject<ReqItemcollection>(Itemcoll); // parse as array

                    SubscriptionItem subscriptionItem = new SubscriptionItem
                    {
                        ID = subscriptionID,
                        InvID = inventoryID,
                        DocumentAuthor = listItem["DocumentAuthor"]?.ToString(),
                        DocumentID = DocumentID,
                        Notes = listItem["Notes"]?.ToString(),
                        WebApplicationURL = webApplicationURL,
                        IsSuccess = 0,
                        Jurisdiction = listItem["Jurisdiction"]?.ToString(),
                        CreatedBy = createdBy
                    };

                    using (ClientContext targetContext = new ClientContext(targetSiteCollectionUrl))
                    {
                        targetContext.Credentials = new NetworkCredential(CommonVariables.AccountName, CommonVariables.Password, "NRES");
                        subscriptionItem.WebApplicationURL = targetSiteCollectionUrl;
                        Requirement.RequirementInventoryUpdate(targetContext, reqItemcoll, subscriptionItem, networkCredential);
                        DocumentItem.DocumentInventoryUpdate(targetContext, DocumentID, subscriptionItem, networkCredential);
                        await ClientLogFolder.ClientLogFolderCreation(targetContext, reqItemcoll, subscriptionItem);
                    }

                    listItem["IsSubscribeDone"] = subscriptionItem.IsSuccess;
                    listItem.Update();
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    Errorlogs.Log(context, new ErrorLogItem
                    {
                        ErrorMessage = ex.Message,
                        MethodName = "Program.ReadSubscriptionItemCollection",
                        StackTrace = ex.StackTrace,
                        SubscriptionID = Convert.ToInt32(listItem["ID"])
                    });
                }
            }
        }
    }
}
