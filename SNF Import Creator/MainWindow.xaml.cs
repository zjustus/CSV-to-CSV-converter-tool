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

		private void fileDrop(object sender, DragEventArgs e)
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

					List<Dictionary<string, object>> csv = CSVProcess(file);
					ColumnDef[] columnObjects = (ColumnDef[])Application.Current.Properties["columnDefs"];

					List <Dictionary<string, object>> output = new();


					foreach(Dictionary<string, object> row in csv)
					{
						Dictionary<string, object> outColumn = new();
						foreach(ColumnDef columnDef in columnObjects)
						{
							object value;

							if(columnDef.InputName != null && columnDef.OutputName == null) columnDef.OutputName = columnDef.InputName;
							if (columnDef.InputName == null) value = columnDef.Value.ToString();
							else
							{

								value = row[columnDef.InputName];
								// This section applies transformations on the given input and produces a transformed value
								if(columnDef.Transformations.ValueKind == JsonValueKind.Array)
								{
									foreach (JsonElement tf in columnDef.Transformations.EnumerateArray())
									{
										if( tf.ValueKind == JsonValueKind.Object &&
											tf.TryGetProperty("method", out JsonElement method) && 
											tf.TryGetProperty("function", out JsonElement function))
										{
											// A transformation for datatypes
											if(method.GetString() == "convert")
											{
												Trace.WriteLine("Conversion Logic should be applied");
											}
											// A transformation for a mathmatical expression, (<operator> <value>)
											else if (method.GetString() == "math" && value is double)
											{
												//string mathFunction = function.GetString();
												//double = double.parse(value.ToString());
												//value = mathFunction.ParseLamda<>
											}
											// A transformation to append string to end of data
											else if(method.GetString() == "append")
											{
												if(value is not string)
												{
                                                    ErrorWindow errorWin = new ErrorWindow("Error!\nTrying to apped to a value that is not a string");
                                                    errorWin.Show();
                                                    return;
												}
                                                value += function.ToString();

											}
											else if(method.GetString() == "prepend") 
											{ 
												if(value is not string)
												{
                                                    ErrorWindow errorWin = new ErrorWindow("Error!\nTrying to apped to a value that is not a string");
                                                    errorWin.Show();
                                                    return;
                                                }

												value = function.ToString() + value;

                                            }
											

											// A transformation to prepend string to start of data
											// A transformation to trim a string based on a regular expression
										}
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
									}
									if (!matchFound) value = "";
								}
								else
								{
									//Trace.WriteLine(columnDef.outputName + ": " + columnDef.value.ValueKind);

								}

								// This is debug code
								//value = "0";

							}
							if(outColumn.ContainsKey(columnDef.OutputName)) outColumn[columnDef.OutputName] = (string)outColumn[columnDef.OutputName] + (string)value;
							else outColumn.Add(columnDef.OutputName, value);

						}
						output.Add(outColumn);
						//foreach(string column in row.Keys)
						//{
						//    Trace.WriteLine(column);
						//}
					}

					ListToCSV(output);


					/* TODO: process the CSV into some kind of array of named arrays?
					 * create output file
					 * foreach row in csv
					 *      for each column in DEF file
					 *          look for matching column in CSV
					 *              perform processing as directed in CSV
					 *              create or APPEND to output column
					 * if output column headers is not checked, remove column headers from output file
					 * if quick save is not enabled prompt user where to save new file
					 */
				}

				// if file is a def.JSON
				else if(Regex.IsMatch(file, @"\b\.def\.json"))
				{
					// TODO: process json and update def with new file
					Trace.WriteLine("File is def.json");
				}
			}

		}

		// Returns a list of rows of a csv file. 
		private List<Dictionary<string, object>> CSVProcess(string fileName)
		{
			//step 1. read the CSV
			List<Dictionary<string, object>> records = new();
			using (StreamReader reader = new(fileName))
			{
				// TODO: make this dynamic
				bool fileHasHeaders = true;

				// logic to read or create file headers
				List<string> headers = new();
				bool firstCase = true;
				while (!reader.EndOfStream)
				{
					string[] values = reader.ReadLine().Split(",");
					for(int i = 0; i < values.Length; i++)
					{
						if (values[i].StartsWith('"') && values[i].EndsWith('"')) values[i] = values[i].Substring(1, values[i].Length - 2);
					}

					// generates headers
					if (firstCase)
					{
						firstCase = false;
						if (fileHasHeaders)
						{
							headers = values.ToList();
							continue;
						}
						else
						{
							for (int i = 0; i < values.Length; i++) headers.Add(i.ToString());
						}
					}

					// fill the list
					Dictionary<string, object> record = new Dictionary<string, object>();
					for (int i = 0; i < headers.Count; i++)
					{
						record[headers[i]] = values[i];
					}
					records.Add(record);
				}
			}
			return records;
		}

		private void ListToCSV(List<Dictionary<string, object>> csv)
		{
			StreamWriter sw = new StreamWriter("output.csv");
			foreach (Dictionary<string, object> column in csv)
			{
				string line = "";
				foreach (KeyValuePair<string, object> kvp in column)
				{
					line += kvp.Value + ",";
				}
				sw.WriteLine(line.TrimEnd(','));
			}
			sw.Close();
		}
	}
}
