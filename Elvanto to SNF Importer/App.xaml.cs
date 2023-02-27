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
            string[] jsonFiles = Directory.GetFiles(currentDir, "SNF_ImportTool.json");
           Trace.WriteLine("Current dir: " + currentDir);

            // If there are no json files
            if(jsonFiles.Length == 0)
            {
                // make a stock json file
                string[] exampleJson =
                {
                    "\"inputFunds\":[",
                    "    {",
                    "                \"co\": \"0001\",",
                    "        \"fund\": \"00200\",",
                    "        \"department\":\"000\",",
                    "        \"account\":\"001041020\"",
                    "    }",
                    "],",
                    "\"elvanto\":[",
                    "    {",
                    "                \"name\": \"Tithes & Offerings - Deductible\",",
                    "        \"co\": \"0001\",",
                    "        \"fund\": \"00000\",",
                    "        \"department\":\"000\",",
                    "        \"account\":\"001040500\"",
                    "    },",
                    "]",
                };
                File.WriteAllLines(currentDir + "SNF_ImportTool.json", exampleJson);

                ErrorWindow errorWin = new("No SNF Import JSON file found!\n An example has been created");
                errorWin.Show();
                return;
            }

            // Load the JSON, check for bad values
            //var jsonText = File.ReadAllText(jsonFiles[0]);
            // save Json to application memory

            try
            {
                FundDict? defObject = JsonProcessor.ProcessJSON(jsonFiles[0]);
                Current.Properties.Add("FundDict", defObject);
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
