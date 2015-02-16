using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSFieldValueCopy
{
    public static class FieldCopy
    {
        public static void CopyFieldValues(string tfsName, string[] tfsProjectNames, string[] tfsWorkItemTypes, string fromField, string toField, bool deleteFromValue = false)
        {
            using (TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(tfsName))
            {
                WorkItemStore wit = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
                WorkItemCollection result = wit.Query(String.Format("SELECT [System.Id], [{0}], [{1}] FROM WorkItems WHERE [System.TeamProject] IN ({2}) AND [System.WorkItemType] IN ({3})", fromField, toField, string.Join(", ", tfsProjectNames.Select(s => "'" + s + "'")), string.Join(", ", tfsWorkItemTypes.Select(s => "'" + s + "'"))));
                List<WorkItem> affectedWorkItems = new List<WorkItem>();
                foreach (WorkItem wi in result)
                {
                    try
                    {
                        var fromValue = wi[fromField];
                        if (fromValue != null)
                        {
                            wi.Open();
                            var orig = wi[toField];
                            wi[toField] = fromValue;

                            if (deleteFromValue)
                            {
                                wi[fromField] = null;
                            }

                            affectedWorkItems.Add(wi);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not copy field value for item {0}: {1}", wi.Id, ex.Message);
                    }
                }

                if (affectedWorkItems.Count > 0)
                {
                    wit.BatchSave(affectedWorkItems.ToArray());
                    Console.WriteLine("Items updated: " + affectedWorkItems.Count);
                }
                else
                {
                    Console.WriteLine("Nothing to do.");
                }
            }
        }
    }
}
