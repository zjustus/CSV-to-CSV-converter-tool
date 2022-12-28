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
                    "\"Delimiter\": \",\",",
                    "\"Marks\": \"\\\"\",",
                    "\"HasHeaders\": true,",
                    "\"OutputDelimiter\": \",\",",
                    "\"OutputMarks\": \"\\\"\",",
                    "\"OutputHasHeaders\": false,",
                    "\"Columns\": [",
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
                    "            { \"if\": \"General Fund\", \"then\": \"oneTwoThree\"}",
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
            CsvDef defObject;
            List<ColumnDef> columnObjects = new();
            try { 
                var jsonText = File.ReadAllText(jsonFiles[0]);

                JsonElement defJsonObject = JsonSerializer.Deserialize<JsonElement>(jsonText);

                JsonElement columnJsonObjects = defJsonObject.GetProperty("Columns");
                foreach (JsonElement i in columnJsonObjects.EnumerateArray())
                {
                    ColumnDef x = new(
                        i.TryGetProperty("InputName", out JsonElement InputValue) ? InputValue.GetString() : null,
                        i.TryGetProperty("OutputName", out JsonElement Outvalue) ? Outvalue.GetString() : null,
                        i.TryGetProperty("Value", out JsonElement theValue)? theValue : null,
                        i.TryGetProperty("Transformations", out JsonElement theTransformations)? theTransformations.EnumerateArray().ToList() : null
                    );

                    columnObjects.Add(x);
                }

                defObject = new(
                    columnObjects,
                    defJsonObject.TryGetProperty("Delimiter", out JsonElement theDelimiter) ? theDelimiter.ToString() : ",",
                    defJsonObject.TryGetProperty("Marks", out JsonElement theMarks) ? theMarks.ToString() : "",
                    defJsonObject.TryGetProperty("HasHeaders", out JsonElement theHasHeaders) && theHasHeaders.GetBoolean(),
                    defJsonObject.TryGetProperty("OutputDelimiter", out JsonElement theOutputDelimiter) ? theOutputDelimiter.ToString() : ",",
                    defJsonObject.TryGetProperty("OutputMarks", out JsonElement theOutputMarks ) ? theOutputMarks.ToString() : "",
                    defJsonObject.TryGetProperty("OutputHasHeaders", out JsonElement theOutputHasHeaders) && theOutputHasHeaders.GetBoolean()
                );


            }
            catch(Exception except)
            {
                Trace.WriteLine(except);
                ErrorWindow errorWin = new(except.Message);
                errorWin.Show();
                return;
            }

            // check if loaded file is empty
            if(columnObjects.Count == 0 )
            {
                ErrorWindow errorWin = new("JSON array is empty");
                errorWin.Show();
                return;
            }

            // save Json to application memory
            //Current.Properties.Add("columnDefs", columnObjects);
            Current.Properties.Add("csvDef", defObject);

            // Start the main application
            MainWindow main = new();
            main.Show();
        }
    }
}
