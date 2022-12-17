using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics; //Used for debugging
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

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
            
            Trace.WriteLine("File Found");
            var jsonText = File.ReadAllText(jsonFiles[0]);
            var jsonObject = JsonSerializer.Deserialize<object>(jsonText);

            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}
