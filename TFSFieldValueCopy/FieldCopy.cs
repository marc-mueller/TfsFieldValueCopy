using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFSFieldValueCopy
{
    public static class FieldCopy
    {
        public static void CopyFieldValues(Uri teamProjectCollectionUri, string[] tfsProjectNames, string[] tfsWorkItemTypes, string[] fromFields, string toField, bool deleteFromValue = false, Func<object, bool> isHistoryValueMatching = null)
        {
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(teamProjectCollectionUri))
            {
                WorkItemStore wit = tfs.GetService<WorkItemStore>();
                WorkItemCollection result = wit.Query(String.Format("SELECT [System.Id], {0}, [{1}] FROM WorkItems WHERE [System.TeamProject] IN ({2}) AND [System.WorkItemType] IN ({3})", 
                    string.Join(", ",fromFields.Select(s => $"[{s}]")), 
                    toField, 
                    string.Join(", ", tfsProjectNames.Select(s => "'" + s + "'")), 
                    string.Join(", ", tfsWorkItemTypes.Select(s => "'" + s + "'"))
                    ));
                List<WorkItem> affectedWorkItems = new List<WorkItem>();
                foreach (WorkItem wi in result)
                {
                    try
                    {
                        object fromValue = null;
                        if (isHistoryValueMatching != null)
                        {
                            if(fromFields.Length != 1)
                            {
                                throw new Exception("History value matching only works with a single from field!");
                            }
                            foreach(Revision revision in wi.Revisions.OfType<Revision>().OrderByDescending(o => o.Index))
                            {
                                var historyValue = revision.Fields[fromFields[0]].Value;
                                if (isHistoryValueMatching(historyValue))
                                {
                                    fromValue = historyValue;
                                    break;
                                }
                            }
                        }
                        else {
                            if (fromFields.Length == 1)
                            {
                                fromValue = wi[fromFields[0]];
                            }
                            else
                            {
                                string separator = "";
                                if (wi.Fields[toField].FieldDefinition.FieldType == FieldType.PlainText)
                                {
                                    separator = "\r\n\r\n";
                                }
                                else if (wi.Fields[toField].FieldDefinition.FieldType == FieldType.Html)
                                {
                                    separator = "<br />";
                                }
                                else
                                {
                                    throw new Exception("Multiple fields can only be combined into a text based target field!");
                                }

                                List<string> fromValues = new List<string>();
                                foreach (var fromField in fromFields)
                                {
                                   fromValues.Add(wi[fromField].ToString());
                                }
                                fromValue = string.Join(separator, fromValues);
                            }
                        }

                        if (fromValue != null)
                        {
                            wi.Open();
                            var orig = wi[toField];
                            wi[toField] = fromValue;

                            if (deleteFromValue)
                            {
                                foreach (var fromField in fromFields.Where(f => !f.Equals(toField, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    wi[fromField] = null;
                                }
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
