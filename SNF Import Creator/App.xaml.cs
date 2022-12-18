using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Diagnostics; //Debugging
using System.IO; // File Handling 
using System.Text.Json; // JSON serialiser

namespace SNF_Import_Creator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Trace.WriteLine("Hi Mom");
            /**
             * TODO: check if a dict file exists in current directory
             * If file or files exist, 
             *  pick first one and load it in
             *  start Main Window
             * Else launch error window
             * 
            **/

            // Get all json files
            string currentDir = Directory.GetCurrentDirectory();
            string[] jsonFiles = Directory.GetFiles(currentDir, "*.def.json");
           Trace.WriteLine("Current dir: " + currentDir);

            // If there are no json files
            if(jsonFiles.Length == 0)
            {
                // make a stock json file
                string[] exampleJson =
                {
                    "[",
                    "    {",
                    "        \"inputName\": \"Notes\",",
                    "        \"mode\": \"rename\",",
                    "        \"outputName\": \"Cool Notes\"",
                    "    },",
                    "    {",
                    "        \"inputName\": \"new Giver\",",
                    "        \"mode\": \"remove\"",
                    "    },",
                    "    {",
                    "        \"outputName\": \"OrgCode\",",
                    "        \"mode\": \"create\",",
                    "        \"value\": 1",
                    "    },",
                    "    {",
                    "        \"inputName\":\"Donation Fund\",",
                    "        \"mode\":\"transform\",",
                    "        \"outputName\": \"out1\",",
                    "        \"value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"}",
                    "        ]",
                    "    },",
                    "    {",
                    "        \"inputName\":\"Donation Fund\",",
                    "        \"mode\":\"transform\",",
                    "        \"outputName\": \"out2\",",
                    "        \"value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"}",
                    "        ]",
                    "    }",
                    "]"
                };
                File.WriteAllLines(currentDir + "/example.def.json", exampleJson);

                ErrorWindow errorWin = new ErrorWindow("No Def file found!\n An example has been created");
                errorWin.Show();
                return;
            }

            // Load the JSON, check for bad values
            columnDef[]? columnObjects;
            try { 
                var jsonText = File.ReadAllText(jsonFiles[0]);
                columnObjects = JsonSerializer.Deserialize<columnDef[]>(jsonText);
            }
            catch(Exception except)
            {
                Trace.WriteLine(except);
                ErrorWindow errorWin = new ErrorWindow("JSON is not formatted correctly");
                errorWin.Show();
                return;
            }

            // check if loaded file is empty
            if(columnObjects == null || columnObjects.Length == 0 )
            {
                ErrorWindow errorWin = new ErrorWindow("JSON array is empty");
                errorWin.Show();
                return;
            }

            // check if each column object contains any of the required fields
            foreach(columnDef i in columnObjects)
            {
                if(i.inputName == null && i.outputName == null)
                {
                    ErrorWindow errorWin = new ErrorWindow("At Least one column definition does not contain an inputName or outputName");
                    errorWin.Show();
                    return;
                }
                // Preemptive! - this code can grab values or arrays from the value string. 
                //if (new[] {"Number", "String"}.Contains(i.value.ValueKind.ToString()))
                //    Trace.WriteLine(i.value);
                //else Trace.WriteLine(i.value.ValueKind);
            }

            // save Json to application memory
            Current.Properties.Add("columnDefs", columnObjects);

            // Start the main application
            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}
