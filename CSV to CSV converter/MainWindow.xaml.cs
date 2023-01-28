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
            CsvDef? csvDef = (CsvDef?)Application.Current.Properties["csvDef"];
			if (csvDef != null) label_defTitle.Content = csvDef.DefTitle;
        }

		private void FileDrop(object sender, DragEventArgs e)
		{

			// get all dropped files and return if empty
			string[] dropedFiles = Array.Empty<string>();
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				dropedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
				//Trace.WriteLine(dropedFiles[0]);
			}
			if (dropedFiles.Length == 0) return;
			foreach(string file in dropedFiles)
			{   
				// if file is a CSV
				if(Regex.IsMatch(file, @"\b\.csv"))
				{

					
                    CsvDef? csvDef = (CsvDef?)Application.Current.Properties["csvDef"];
                    if (csvDef == null)
					{
                        ErrorWindow errorWin = new("No Def file has been Loaded. \n Please Load a Def File");
                        errorWin.Show();
                        return;
					}

                    List<ColumnDef>? columnObjects = csvDef.Columns;
                    List<Dictionary<string, string>> csv = csvDef.CSVProcess(file);
                    List <Dictionary<string, object>> output = new();


					foreach(Dictionary<string, string> row in csv)
					{
						Dictionary<string, object> outColumn = new();
						foreach(ColumnDef columnDef in columnObjects)
						{
							string? value;

							// if input is null, skip input logic
							if (string.IsNullOrEmpty(columnDef.InputName)) value = columnDef.Value.ToString();
							else
							{
								try
								{
									value = row[columnDef.InputName];

								}
								catch(Exception ex){
                                    ErrorWindow errorWin = new(ex.Message);
                                    errorWin.Show();
                                    return;
                                }
								// This section applies transformations on the given input and produces a transformed value
								foreach (JsonElement tf in columnDef.Transformations)
								{
									try
									{
										if (tf.ValueKind == JsonValueKind.Object &&
											tf.TryGetProperty("method", out JsonElement method) &&
											tf.TryGetProperty("function", out JsonElement function))
										{
											// A transformation for a mathmatical expression, (<operator> <value>)
											if (method.GetString() == "math" && !string.IsNullOrEmpty(value))
											{
												string expression = value + function.GetString();
												System.Data.DataTable table = new();
                                                value = table.Compute(expression, "").ToString();
                                            }

											// appends the input with the given text
											else if (method.GetString() == "append")
											{
												value += function.ToString();
											}

											// prepends the input with the given text
											else if (method.GetString() == "prepend")
											{
												value = function.ToString() + value;

											}

											// Matches a regex string and returns only what matches
											else if (method.GetString() == "regClip")
											{

												MatchCollection matches = Regex.Matches((string)value, function.ToString());
												value = "";
												foreach (Match match in matches.Cast<Match>())
												{
													value += match.Value;
												}
											}
										}
									}
									catch(Exception ex) {
                                        ErrorWindow errorWin = new(ex.Message);
                                        errorWin.Show();
                                        return;
                                    }
								}


								// This section applies value if then logic to the transformed input and produces final value
								if (columnDef.Value.ValueKind == JsonValueKind.Array)
								{
									bool matchFound = false;
									foreach (JsonElement statement in columnDef.Value.EnumerateArray())
									{
										if (
											statement.ValueKind == JsonValueKind.Object && 
											statement.TryGetProperty("if", out JsonElement ifValue) &&
											statement.TryGetProperty("then", out JsonElement thenValue)
										){
											if(
												ifValue.ValueKind == JsonValueKind.String &&
												ifValue.ToString() == value.ToString()
											)
											{
												value = thenValue.ToString();
												matchFound = true;
												break;
											}
										}
										else if(
											statement.ValueKind == JsonValueKind.Object &&
                                            statement.TryGetProperty("else", out JsonElement elseValue)
										)
										{
                                            value = elseValue.ToString();
                                            matchFound = true;
                                            break;
                                        }
									}
									if (!matchFound) value = "";
								}
                                else if (columnDef.Value.ValueKind == JsonValueKind.Object)
                                {
                                    ErrorWindow errorWin = new("Error!\nThe value column can not be an object");
                                    errorWin.Show();
                                    return;
                                }
                                else if(columnDef.Value.ValueKind != JsonValueKind.Undefined)
								{
									value = columnDef.Value.ToString();
								}

								// This section applys padding logic to the final value
								if (columnDef.Padding != null)
								{
									JsonElement thePadding = columnDef.Padding.Value;
									
									// calculate remaining space, throw a fuss if value is too large
									int remaining = thePadding.GetProperty("length").GetInt32() - value.Length;
									if(remaining < 0)
									{
										ErrorWindow errorWindow = new("WARNING!\nThe value excedes padding space");
										errorWindow.Show();
									}

									// construct padding space
									string padding = "";
									for (int i = 0; i < remaining; i++) padding += thePadding.GetProperty("char").ToString();

									// append or prepend padding space
									if (thePadding.GetProperty("side").ToString() == "left")
										value = padding + value;
									else value += padding;
								}

							}

							// This merges or appends to the final CSV
							if(outColumn.ContainsKey(columnDef.OutputName)) outColumn[columnDef.OutputName] = (string)outColumn[columnDef.OutputName] + (string)value;
							else outColumn.Add(columnDef.OutputName, value);

						}
						output.Add(outColumn);
					}

                    string ccsvOut = csvDef.ListToCSV(output);
					SaveFileDialog saveDialog = new();
					saveDialog.FileName = Regex.Match(file, @"(?<=\\)[^\\]*$").Value;
					saveDialog.DefaultExt = "csv";
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
					bool isValid = saveDialog.ShowDialog() ?? false;

					if (isValid) {
						File.WriteAllText(saveDialog.FileName, ccsvOut);
					}
				}

				// if file is a def.JSON
				else if(Regex.IsMatch(file, @"\b\.def\.json"))
				{
                    try
                    {
                        CsvDef? defObject = jsonProceess.processJSON(file);
						Application.Current.Properties["csvDef"] = defObject;
						if (defObject != null) label_defTitle.Content = defObject.DefTitle;
                    }
                    catch (Exception ex)
                    {
                        ErrorWindow errorWin = new(ex.Message);
                        errorWin.Show();
                        return;
                    }

				}
			}
		}
    }
}
