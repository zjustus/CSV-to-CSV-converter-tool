using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SNF_Import_Creator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
        }

		private void FileDrop(object sender, DragEventArgs e)
		{
			// Load the Def Object
            FundDict? defObject = (FundDict?)Application.Current.Properties["FundDict"];
            if (defObject == null)
            {
                ErrorWindow errorWin = new("No Def file has been Loaded. \n Please Load a Def File");
                errorWin.Show();
                return;
            }

            // get all dropped files and return if empty
            string[] dropedFiles = Array.Empty<string>();
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				dropedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
				//Trace.WriteLine(dropedFiles[0]);
			}
			if (dropedFiles.Length == 0) return;

			//Batch Counters
			int currentBatch = -1;
            List<Dictionary<string, object>> standardFormat = new();
            List<Dictionary<string, int>> inputAccountsTotals = new(); //Department:total
			List<DateTime> despositeDates = new();

            // Loop Through All Droped Files
            foreach (string file in dropedFiles)
			{   
				// if file is a CSV
				if(Regex.IsMatch(file, @"\b\.csv"))
				{
					currentBatch++; //TODO: Dont increment current batch if set to merge!
					inputAccountsTotals.Add(new());
					DateTime depositeDate = new();
                    //List<ColumnDef>? columnObjects = csvDef.Columns;
                    List<Dictionary<string, string>> csv = CsvTools.stringParse(file);

					// Determinee Origin of CSV File
					bool isElvanto = csv[0].ContainsKey("Receipt Name");
                    bool isTithly = csv[0].ContainsKey("Currency Code");
                    bool isPushpay = csv[0].ContainsKey("Listing");

					int rowCount = 0;
                    foreach (Dictionary<string, string> row in csv)
					{
						Dictionary<string, object> outColumn = new();

                        if (isElvanto) {
							// skip the blank rows
							if (rowCount == 0 || rowCount + 1 == csv.Count()) continue;
							
							TitheAccount x = defObject.GetFundDetails("elvanto", "Account / Sub Account");

							// Processing

							DateTime transactionDate = DateTime.ParseExact(row["Transaction Date"], "yyyy-MM-dd HH-mm:ss", null);
							int fiscalMonth = ((transactionDate.Month + 5) % 12)+1;
							depositeDate = DateTime.ParseExact(row["Deposit Date"], "yyyy-MM-dd HH-mm:ss", null);


                            double amountExact = Convert.ToDouble(row["Amount"]);
							int amount = Convert.ToInt32(amountExact*-100);

                            // Mapping
                            outColumn = new()
                            {
                                { "Unused1", "00000" },
                                { "CO", x.CoNumber },
                                { "Fund", x.FundNumber },
                                { "Accounting Period", fiscalMonth.ToString("00")},
                                { "Journal Type", "CN" },
                                { "Journal Number", currentBatch },
                                { "Unused2", "000" },
                                { "Date", transactionDate.ToString("MMddyy") },
                                { "Description1", row["memo"] ?? "" },
                                { "Description2", "" },
                                { "Department", x.DepartmentNumber },
                                { "Account", x.AccountNumber },
                                { "Amount", amount },
                                { "Project", "" }
                            };
                        }
						else if (isTithly) { } // TODO: Add Tithly Mapping
						else if(isPushpay) { } // TODO: Add PushPay Mapping

						// This section keeps track of Input totals per department per batch
                        standardFormat.Add(outColumn);
						// If department exists, apped total
                        if(inputAccountsTotals[currentBatch].ContainsKey((string)outColumn["Department"])){
							inputAccountsTotals[currentBatch][(string)outColumn["Department"]] -= (int)outColumn["Amount"];
                        }
						// If department does not, create the department with the total
						else{
							inputAccountsTotals[currentBatch].Add((string)outColumn["Department"], -(int)outColumn["Amount"]);
                        }

						// This section stores deoposite data per batch
						if (despositeDates.Count == currentBatch) despositeDates.Add(depositeDate);

                        rowCount++;
					}
				}
			}

			// Add Department totals to the list
            for(int batchNumber = 0; batchNumber < inputAccountsTotals.Count; batchNumber++) {
                DateTime depositeDate = despositeDates[batchNumber];
                int fiscalMonth = ((depositeDate.Month + 5) % 12) + 1;
                foreach (KeyValuePair<string, int> department in inputAccountsTotals[batchNumber]){
                    Account x = defObject.GetAccountDetails(department.Key);
                    standardFormat.Add(new()
                            {
                                { "Unused1", "00000" },
                                { "CO", x.CoNumber },
                                { "Fund", x.FundNumber },
                                { "Accounting Period", fiscalMonth.ToString("00")},
                                { "Journal Type", "CN" },
                                { "Journal Number", batchNumber },
                                { "Unused2", "000" },
                                { "Date", depositeDate.ToString("MMddyy") },
                                { "Description1", "" },
                                { "Description2", "" },
                                { "Department", department.Key },
                                { "Account", x.AccountNumber },
                                { "Amount", department.Value },
                                { "Project", "" }
                            }
                    );
                }
			}


			// TODO: Create final output dict from standard format

			// Save Output File
			string csvOut = CsvTools.ListToCSV(standardFormat);
			SaveFileDialog saveDialog = new();
			saveDialog.FileName = "SNF_Import_.txt";
			saveDialog.DefaultExt = "txt";
            saveDialog.Filter = "Text files (*.txt)|*.txt";
			bool isValid = saveDialog.ShowDialog() ?? false;

			if(isValid)
			{
				File.WriteAllText(saveDialog.FileName, csvOut);
			}

        }
    }
}
