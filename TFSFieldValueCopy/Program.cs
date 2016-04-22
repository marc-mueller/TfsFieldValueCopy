using Fclp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSFieldValueCopy
{
    class Program
    {
        static void Main(string[] args)
        {

            string tfsCollectionUri = string.Empty;
            string[] tfsProjectNames = new string[0];
            string[] tfsWorkItemTypes = new string[0];
            string[] fromFields = new string[0];
            string toField = string.Empty;
            bool deleteFromFieldValue = false;

            var parser = new FluentCommandLineParser();
            parser.Setup<string>('c', "collection").Callback(p => tfsCollectionUri = p).Required();
            parser.Setup<string>('p', "projectNames").Callback(p => tfsProjectNames = p.Split(',')).Required();
            parser.Setup<string>('w', "workItemTypes").Callback(p => tfsWorkItemTypes = p.Split(',')).Required();
            parser.Setup<string>('f', "fromFields").Callback(p => fromFields = p.Split(',')).Required();
            parser.Setup<string>('t', "toField").Callback(p => toField = p).Required();
            parser.Setup<bool>('d', "deleteFromFieldValue").Callback(p => deleteFromFieldValue = p);

            parser.SetupHelp("?", "help").Callback(text => Console.WriteLine(text));

            parser.Parse(args);


            Console.WriteLine("TFS Team Project Collection Uri: {0}", tfsCollectionUri);
            Console.WriteLine("Projects: {0}", string.Join(", ", tfsProjectNames.Select(s => "'" + s + "'")));
            Console.WriteLine("WITs: {0}", string.Join(", ", tfsWorkItemTypes.Select(s => "'" + s + "'")));
            Console.WriteLine("From Field: {0}", string.Join(", ",fromFields));
            Console.WriteLine("To Field: {0}", toField);

            Console.WriteLine("Continue?");
            var confirm = Console.ReadLine();
            if (confirm.ToLowerInvariant().StartsWith("y"))
            {
                if (Uri.IsWellFormedUriString(tfsCollectionUri, UriKind.Absolute))
                {
                    FieldCopy.CopyFieldValues(new Uri(tfsCollectionUri), tfsProjectNames, tfsWorkItemTypes, fromFields, toField, deleteFromFieldValue);

                    // sample for providing a history value matching lambda function to get the right history value (newest will be checked first
                    //FieldCopy.CopyFieldValues(new Uri(tfsCollectionUri), tfsProjectNames, tfsWorkItemTypes, fromField, toField, false, val => val != null && long.Parse(val.ToString()) <= 1000);
                }
                else
                {
                    Console.WriteLine("Team Project Collection Uri is not well-formed!");
                }
            }
            else
            {
                Console.WriteLine("Cancelled!");
            }
            Console.WriteLine("<ENTER> to exit?");
            Console.ReadLine();

        }
    }
}
