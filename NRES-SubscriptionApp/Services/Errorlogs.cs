using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRES_SubscriptionApp.Services
{
   public class Errorlogs
    {
        public static void Log(ClientContext context, ErrorLogItem errorLogItem)
        {
            List olistErrorLog = context.Web.Lists.GetByTitle(CommonVariables.ErrorLogListTitle);
            ListItemCreationInformation listItemCreationInformation = new ListItemCreationInformation();
            var listItem = olistErrorLog.AddItem(listItemCreationInformation);
            listItem["Title"] = errorLogItem.MethodName;
            listItem["SubscriptionID"] = errorLogItem.SubscriptionID;
            listItem["Errormessage"] = errorLogItem.ErrorMessage;
            listItem["StackTrace"] = errorLogItem.StackTrace;
            listItem.Update();
            context.ExecuteQuery();
        }
    }
}
