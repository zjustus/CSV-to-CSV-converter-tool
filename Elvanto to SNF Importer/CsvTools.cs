using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace SNF_Import_Creator
{

    internal class CsvTools
    {

        // Returns a list of rows of a csv file. 
        public static List<Dictionary<string, string>> stringParse(string fileName)
        {
            //step 1. read the CSV
            List<Dictionary<string, string>> records = new();
            using (StreamReader reader = new(fileName))
            {

                // logic to read or create file headers
                List<string> headers = new();
                bool firstCase = true;
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    // BUGFIX: did not account for commas in input fields
                    //string[] values = line != null ? line.Split(",") : Array.Empty<string>();
                    //string[] values;
                    List<string> values= new();
                    //char quoteChar = '"';
                    if (line != null)
                    {
                        string buffer = "";
                        bool inQuotes = false;
                        char? previous = null;
                        foreach (char c in line)
                        {
                            // next column
                            if (c == ',' && !inQuotes)
                            {
                                values.Add(buffer);
                                buffer = "";
                            }
                            // escape catch
                            else if(c == '"' && previous.HasValue && previous == '"')
                            {
                                inQuotes = !inQuotes;
                                buffer += c;
                            }
                            // quote catch
                            else if (c == '"')
                            {
                                inQuotes = !inQuotes;
                            }
                            // all other values
                            else
                            {
                                buffer += c;
                            }
                            previous= c;
                        }
                        // Last Catch
                        if(buffer != "") values.Add(buffer);
                    }

                    // might have been made redundant
                    //for (int i = 0; i < values.Count; i++)
                    //{
                    //    if (values[i].StartsWith('"') && values[i].EndsWith('"')) values[i] = values[i].Substring(1, values[i].Length - 2);
                    //}

                    // generates headers
                    if (firstCase)
                    {
                        firstCase = false;
                        //if (HasHeaders)
                        if (true)
                        {
                            headers = values;
                            continue;
                        }
                        else
                        {
                            for (int i = 0; i < values.Count; i++) headers.Add(i.ToString());
                        }
                    }

                    // fill the list
                    Dictionary<string, string> record = new();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        record[headers[i]] = values[i];
                    }
                    records.Add(record);
                }
            }
            return records;
        }

        public static string ListToCSV(List<Dictionary<string, object>> csv)
        {
            string Output = "";

            foreach (Dictionary<string, object> row in csv)
            {
                string line = "";
                foreach (KeyValuePair<string, object> kvp in row)
                {
                    line += '"' + kvp.Value.ToString() + "\",";
                }
                Output += line.TrimEnd(',') + "\n";
            }

            return Output;

            // Old code
            //StreamWriter sw = new(outputName);
            //foreach (Dictionary<string, object> column in csv)
            //{
            //    string line = "";
            //    foreach (KeyValuePair<string, object> kvp in column)
            //    {
            //        line += OutputMarks + kvp.Value + OutputMarks + ",";
            //    }
            //    sw.WriteLine(line.TrimEnd(','));
            //}
            //sw.Close();
        }
    }
}
