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
                    "\"defTitle\": \"Cool Conversion\",",
                    "\"delimiter\": \",\",",
                    "\"marks\": \"\\\"\",",
                    "\"hasHeaders\": true,",
                    "\"outputDelimiter\": \",\",",
                    "\"outputMarks\": \"\\\"\",",
                    "\"outputHasHeaders\": false,",
                    "\"columns\": [",
                    "    {",
                    "        \"inputName\": \"Notes\",",
                    "        \"outputName\": \"Cool Notes\"",
                    "    },",
                    "    {",
                    "        \"inputName\": \"new Giver\"",
                    "    },",
                    "    {",
                    "        \"outputName\": \"OrgCode\",",
                    "        \"value\": 1",
                    "    },",
                    "    {",
                    "        \"inputName\":\"Donation Fund\",",
                    "        \"outputName\": \"out1\",",
                    "        \"value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"}",
                    "        ]",
                    "    },",
                    "    {",
                    "        \"inputName\":\"Donation Fund\",",
                    "        \"outputName\": \"out2\",",
                    "        \"value\":[",
                    "            { \"if\": \"Tithes and Offerings\", \"then\": \"oneTwoThree\"},",
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"},",
                    "            { \"else\": \"Missions\" }",
                    "        ]",
                    "    },",
                    "    {",
                    "           \"inputName\": \"Amount\",",
                    "           \"transformations\": [",
                    "               { \"method\": \"math\", \"function\": \" * 100\"},",
                    "               { \"method\": \"regClip\", \"function\": \"^[^.]*\"}",
                    "           ],",
                    "           \"padding\":{ \"side\": \"left\", \"char\":\"0\", \"length\":9 }",
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
                CsvDef? defObject = JsonProcessor.processJSON(jsonFiles[0]);
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
