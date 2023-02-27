﻿using Microsoft.Win32;
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

            // Loop Through All Droped Files
            foreach (string file in dropedFiles)
			{   
				// if file is a CSV
				if(Regex.IsMatch(file, @"\b\.csv"))
				{
					currentBatch++; //TODO: Dont increment current batch if set to merge!

                    //List<ColumnDef>? columnObjects = csvDef.Columns;
                    List<Dictionary<string, string>> csv = CsvTools.stringParse(file);

					// Determinee Origin of CSV File
					bool isElvanto = csv[0].ContainsKey("Receipt Name");
                    bool isTithly = csv[0].ContainsKey("Currency Code");
                    bool isPushpay = csv[0].ContainsKey("Listing");

					int rowCount = 0;
                    foreach (Dictionary<string, string> row in csv)
					{
						
						if (isElvanto) {
							// skip the blank rows
							if (rowCount == 0 || rowCount + 1 == csv.Count()) continue;
							
							TitheAccount x = defObject.GetFundDetails("elvanto", "Account / Sub Account");

							// Processing

							DateTime transactionDate = DateTime.ParseExact(row["Transaction Date"], "yyyy-MM-dd HH-mm:ss", null);
							int fiscalMonth = ((transactionDate.Month + 5) % 12)+1;

							double amountExact = Convert.ToDouble(row["Amount"]);
							int amount = Convert.ToInt32(amountExact*-100);

                            // Mapping
                            Dictionary<string, object> outColumn = new()
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

                            standardFormat.Add(outColumn);
                        }
						else if (isTithly) { }
						else if(isPushpay) { }

                        rowCount++;
					}

					// Old Save Code
					//string ccsvOut = csvDef.ListToCSV(output);
					//SaveFileDialog saveDialog = new();
					//saveDialog.FileName = Regex.Match(file, @"(?<=\\)[^\\]*$").Value;
					//saveDialog.DefaultExt = "csv";
					//saveDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
					//bool isValid = saveDialog.ShowDialog() ?? false;

					//if (isValid) {
					//	File.WriteAllText(saveDialog.FileName, ccsvOut);
					//}
				}
			}

			// TODO: Add Department totals here

			// TODO: Save Output File Here

		}
    }
}