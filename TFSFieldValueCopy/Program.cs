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
            if (args.Length == 5)
            {
                var tfsServer = args[0];
                var tfsProjectNames = args[1].Split(',');
                var tfsWorkItemTypes = args[2].Split(',');
                var fromField = args[3];
                var toField = args[4];

                Console.WriteLine("TFS Server: {0}", tfsServer);
                Console.WriteLine("Projects: {0}", string.Join(", ", tfsProjectNames.Select(s => "'"+s+"'")));
                Console.WriteLine("WITs: {0}", string.Join(", ", tfsWorkItemTypes.Select(s => "'" + s + "'")));
                Console.WriteLine("From Field: {0}", fromField);
                Console.WriteLine("To Field: {0}", toField);

                Console.WriteLine("Continue?");
                var confirm = Console.ReadLine();
                if (confirm.ToLowerInvariant().StartsWith("y"))
                {
                    FieldCopy.CopyFieldValues(tfsServer, tfsProjectNames, tfsWorkItemTypes, fromField, toField, false);
                }
                else
                {
                    Console.WriteLine("Cancelled!");
                }
                Console.WriteLine("<ENTER> to exit?");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Wrong input");
            }
        }
    }
}
