using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp.Services
{
   public class Errorlogs
    {
        

        public static void Log(ClientContext context, ErrorLogItem errorLogItem)
        {
            string LogApplicationURL = System.Configuration.ConfigurationManager.AppSettings["LogApplicationURL"];
            NetworkCredential networkCredential = new NetworkCredential(CommonVariables.AccountName, CommonVariables.Password, "NRES");
            using (ClientContext rootcontext = new ClientContext(LogApplicationURL))
            {
                List olistErrorLog = rootcontext.Web.Lists.GetByTitle(CommonVariables.ErrorLogListTitle);
                ListItemCreationInformation listItemCreationInformation = new ListItemCreationInformation();
                var listItem = olistErrorLog.AddItem(listItemCreationInformation);
                listItem["Title"] = errorLogItem.MethodName;
                listItem["SubscriptionID"] = errorLogItem.SubscriptionID;
                listItem["Errormessage"] = errorLogItem.ErrorMessage;
                listItem["StackTrace"] = errorLogItem.StackTrace;
                listItem.Update();
                rootcontext.ExecuteQuery();
            }
        }
    }
}
