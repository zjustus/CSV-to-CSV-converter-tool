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
            List<ColumnDef> columnObjects = new();
            try { 
                var jsonText = File.ReadAllText(jsonFiles[0]);

                JsonElement columnJsonObjects = JsonSerializer.Deserialize<JsonElement>(jsonText);

                foreach(JsonElement i in columnJsonObjects.EnumerateArray())
                {
                    ColumnDef x = new(
                        i.TryGetProperty("InputName", out JsonElement InputValue) ? InputValue.GetString() : null,
                        i.TryGetProperty("OutputName", out JsonElement Outvalue) ? Outvalue.GetString() : null,
                        i.TryGetProperty("Value", out JsonElement theValue)? theValue : null,
                        i.TryGetProperty("Transformations", out JsonElement theTransformations)? theTransformations.EnumerateArray().ToList() : null
                    );

                    columnObjects.Add(x);
                }
            }
            catch(Exception except)
            {
                Trace.WriteLine(except);
                ErrorWindow errorWin = new ErrorWindow(except.Message);
                errorWin.Show();
                return;
            }

            // check if loaded file is empty
            if(columnObjects.Count == 0 )
            {
                ErrorWindow errorWin = new ErrorWindow("JSON array is empty");
                errorWin.Show();
                return;
            }

            // save Json to application memory
            Current.Properties.Add("columnDefs", columnObjects);

            // Start the main application
            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}
