using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.SharePoint.Client;
using System.Net;
using Newtonsoft.Json.Linq;
using NRES_SubscriptionApp.Services;

namespace NRES_SubscriptionApp
{
    class Program
    {
        public static string WebApplicationURL = string.Empty;
        public static string ListTitle = string.Empty;
        public static string AccountName = string.Empty;
        public static string Password = string.Empty;

        static void Main(string[] args)
        {

            WebApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];
            ListTitle = System.Configuration.ConfigurationManager.AppSettings["ListTitle"];
            AccountName = System.Configuration.ConfigurationManager.AppSettings["ServiceAccountName"];
            Password = System.Configuration.ConfigurationManager.AppSettings["Password"];

            StartSubscription(WebApplicationURL, ListTitle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webApplicationURL">The web application url</param>
        /// <param name="listTitle">The list title</param>
        public static void StartSubscription(string webApplicationURL, string listTitle)
        {
            using (ClientContext context = new ClientContext(webApplicationURL))
            {
                context.Credentials = new NetworkCredential(AccountName, Password);
                List list = context.Web.Lists.GetByTitle(listTitle);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='IsSubscribeDone' /><Value Type='Boolean'>0</Value></Eq></Where></Query><RowLimit>100</RowLimit></View>";

                ListItemCollection listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.Include(
                item => item["ID"], item => item["Title"],
                item => item["Items"], item => item["DocumentID"]));

                context.ExecuteQuery();
                ReadSubscriptionItemCollection(context, listItems);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="listItems"></param>
        public static void ReadSubscriptionItemCollection(ClientContext context, ListItemCollection listItems)
        {
            foreach (var listItem in listItems)
            {
                string Itemcoll = listItem["Items"]?.ToString();
                var reqItemcoll = Newtonsoft.Json.JsonConvert.DeserializeObject<ReqItemcollection>(Itemcoll); // parse as array

                Requirement.RequirementInventoryUpdate(context, reqItemcoll);
                
            }
        }
    }
}
