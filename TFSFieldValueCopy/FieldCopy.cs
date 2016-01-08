using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFSFieldValueCopy
{
    public static class FieldCopy
    {
        public static void CopyFieldValues(Uri teamProjectCollectionUri, string[] tfsProjectNames, string[] tfsWorkItemTypes, string fromField, string toField, bool deleteFromValue = false, Func<object, bool> isHistoryValueMatching = null)
        {
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(teamProjectCollectionUri))
            {
                WorkItemStore wit = tfs.GetService<WorkItemStore>();
                WorkItemCollection result = wit.Query(String.Format("SELECT [System.Id], [{0}], [{1}] FROM WorkItems WHERE [System.TeamProject] IN ({2}) AND [System.WorkItemType] IN ({3})", fromField, toField, string.Join(", ", tfsProjectNames.Select(s => "'" + s + "'")), string.Join(", ", tfsWorkItemTypes.Select(s => "'" + s + "'"))));
                List<WorkItem> affectedWorkItems = new List<WorkItem>();
                foreach (WorkItem wi in result)
                {
                    try
                    {
                        object fromValue = null;
                        if (isHistoryValueMatching != null)
                        {
                            foreach(Revision revision in wi.Revisions.OfType<Revision>().OrderByDescending(o => o.Index))
                            {
                                var historyValue = revision.Fields[fromField].Value;
                                if (isHistoryValueMatching(historyValue))
                                {
                                    fromValue = historyValue;
                                    break;
                                }
                            }
                        }
                        else {
                            fromValue = wi[fromField];
                        }

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
                    var errors = wit.BatchSave(affectedWorkItems.ToArray());
                    Console.WriteLine("Items updated: " + affectedWorkItems.Count);
                    Console.WriteLine("Items errors: " + errors.Length);
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Error within work item {error.WorkItem.Id}: {error.Exception.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Nothing to do.");
                }
            }
        }
    }
}
