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


    internal class ColumnDef
    {
        public string InputName { get; }
        public string OutputName { get; }
        public JsonElement Value { get; }
        public List<JsonElement> Transformations { get; }

        public ColumnDef(
            string? InputName = null,
            string? OutputName = null,
            JsonElement? Value = null,
            List<JsonElement>? Transformations = null
        ) {

            // make sure either Input or output contains a value
            if (string.IsNullOrEmpty(OutputName) && string.IsNullOrEmpty(InputName)) 
                throw new ArgumentNullException("InputName and OutputName can not both be empty");
            
            // If input is valid, but output is not, make output input
            if (string.IsNullOrEmpty(OutputName) && !string.IsNullOrEmpty(InputName)) 
                OutputName = InputName;

            // If input is not valid and value is missing cause a problem
            Value ??= new JsonElement();
            if (string.IsNullOrEmpty(InputName) && Value.Value.ValueKind == JsonValueKind.Null) 
                throw new ArgumentNullException("InputName and Value can not both be empty");

            // If value exists, make sure its not an object
            if (Value.Value.ValueKind == JsonValueKind.Object) 
                throw new ArgumentException("Value cannot be an Object. It must be a value or array of objects");

            // If value exists and is an array, make sure it has the correct properties.
            if (Value.Value.ValueKind == JsonValueKind.Array)
            {
                foreach(JsonElement x in Value.Value.EnumerateArray())
                {
                    if (x.ValueKind != JsonValueKind.Object) throw new ArgumentException("a Value of arrays must only contain objects");

                    if (
                        x.TryGetProperty("if", out JsonElement prop_if) &&
                        x.TryGetProperty("then", out JsonElement prop_then)
                    ){
                        if (prop_if.ValueKind == JsonValueKind.Object || prop_if.ValueKind == JsonValueKind.Array)
                            throw new ArgumentException("value array object if property must not be an object or array");
                        if (prop_then.ValueKind == JsonValueKind.Object || prop_then.ValueKind == JsonValueKind.Array)
                            throw new ArgumentException("value array object then property must not be an object or array");
                    } 
                    else throw new ArgumentException("a Value of arrays must contain the property If and Then");
                }
            }

            // If Transformations exist, make sure its objects has the correct properties
            if(Transformations != null) {
                foreach(JsonElement x in Transformations)
                {
                    if (
                        x.TryGetProperty("method", out JsonElement method) &&
                        x.TryGetProperty("function", out JsonElement function)
                    ){
                        if(method.ValueKind == JsonValueKind.Object || method.ValueKind == JsonValueKind.Array)
                            throw new ArgumentException("transformations array object method property must not be an object or array");
                        if (function.ValueKind == JsonValueKind.Object || function.ValueKind == JsonValueKind.Array)
                            throw new ArgumentException("transformations array object function property must not be an object or array");
                    }
                    else throw new ArgumentException("Transformations array objects must contain the properties method and function");
                }
            }
            else
            {
                Transformations = new List<JsonElement> { };
            }

            this.InputName = InputName ?? "";
            this.OutputName = OutputName ?? "";
            this.Value = (JsonElement)Value;
            this.Transformations = Transformations;
        }

        public override string ToString()
        {
            return ($"Input: {this.InputName} \n Output: {this.OutputName}");
        }
    }

    internal class CsvDef
    {
        // TODO: include quick save?
        public string Delimiter {get;}
        public string TextMarks {get;}
        public string Marks {get;}
        public bool HasHeaders {get;}
        public string OutputDelimiter {get;}
        public string OutputTextMarks {get;}
        public string OutputMarks {get;}
        public bool OutputHasHeaders {get;}
        public List<ColumnDef> Columns { get; }

        public CsvDef(
            List<ColumnDef> Columns,
            string Delimiter,
            string Marks,
            bool HasHeaders,
            string OutputDelimiter,
            string OutputMarks,
            bool OutputHasHeaders
        ) {
            this.Columns = Columns;
            this.Delimiter = Delimiter;
            this.Marks = Marks;
            this.HasHeaders = HasHeaders;
            this.OutputDelimiter = OutputDelimiter;
            this.OutputMarks = OutputMarks;
            this.OutputHasHeaders = OutputHasHeaders;            
        }

        // Returns a list of rows of a csv file. 
        public List<Dictionary<string, string>> CSVProcess(string fileName)
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
                    string[] values = line != null ? line.Split(",") : Array.Empty<string>();
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].StartsWith('"') && values[i].EndsWith('"')) values[i] = values[i].Substring(1, values[i].Length - 2);
                    }

                    // generates headers
                    if (firstCase)
                    {
                        firstCase = false;
                        if (HasHeaders)
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

        // TODO: Throw in some paramaters
        // TODO: change from object to something else...
        public string ListToCSV(List<Dictionary<string, object>> csv)
        {
            string Output = "";
            foreach (Dictionary<string, object> column in csv)
            {
                string line = "";
                foreach (KeyValuePair<string, object> kvp in column)
                {
                    line += OutputMarks + kvp.Value + OutputMarks + OutputDelimiter;
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
