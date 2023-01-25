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
                    "{",
                    "\"DefTitle\": \"Cool Conversion\",",
                    "\"Delimiter\": \",\",",
                    "\"Marks\": \"\\\"\",",
                    "\"HasHeaders\": true,",
                    "\"OutputDelimiter\": \",\",",
                    "\"OutputMarks\": \"\\\"\",",
                    "\"OutputHasHeaders\": false,",
                    "\"Columns\": [",
                    "    {",
                    "        \"InputName\": \"Notes\",",
                    "        \"OutputName\": \"Cool Notes\"",
                    "    },",
                    "    {",
                    "        \"InputName\": \"new Giver\"",
                    "    },",
                    "    {",
                    "        \"OutputName\": \"OrgCode\",",
                    "        \"Value\": 1",
                    "    },",
                    "    {",
                    "        \"InputName\":\"Donation Fund\",",
                    "        \"OutputName\": \"out1\",",
                    "        \"Value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"}",
                    "        ]",
                    "    },",
                    "    {",
                    "        \"InputName\":\"Donation Fund\",",
                    "        \"OutputName\": \"out2\",",
                    "        \"Value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"},",
                    "            { \"else\": \"Missions\" }",
                    "        ]",
                    "    },",
                    "    {",
                    "           \"InputName\": \"Amount\",",
                    "           \"Transformations\": [",
                    "               { \"method\": \"math\", \"function\": \" * 100\"},",
                    "               { \"method\": \"regClip\", \"function\": \"^[^.]*\"}",
                    "           ]",
                    "     }",
                    "]",
                    "}"
                };
                File.WriteAllLines(currentDir + "/example.def.json", exampleJson);

                ErrorWindow errorWin = new("No Def file found!\n An example has been created");
                errorWin.Show();
                return;
            }

            // Load the JSON, check for bad values
            var jsonText = File.ReadAllText(jsonFiles[0]);
            // save Json to application memory

            try
            {
                CsvDef? defObject = jsonProceess.processJSON(jsonFiles[0]);
                Current.Properties.Add("csvDef", defObject);
            }
            catch (Exception ex)
            {
                ErrorWindow errorWin = new(ex.Message);
                errorWin.Show();
                return;
            }

            // Start the main application
            MainWindow main = new();
            main.Show();
        }
    }
}
