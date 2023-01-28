﻿using System;
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
        public JsonElement? Padding { get; }

        public ColumnDef(
            string? InputName = null,
            string? OutputName = null,
            JsonElement? Value = null,
            List<JsonElement>? Transformations = null,
            JsonElement? Padding = null
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
                    else if(x.TryGetProperty("else", out JsonElement prop_else))
                    {
                        if (prop_else.ValueKind == JsonValueKind.Object || prop_else.ValueKind == JsonValueKind.Array)
                            throw new ArgumentException("Value array objecct else property must not be an object or array");
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

            // If Padding exists, make sure its object has the correct properties
            if( Padding != null)
            {
                if (Padding.Value.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("Padding must be a JSON Object");
                if (Padding.Value.TryGetProperty("side", out JsonElement side) && !side.ValueEquals("left") && !side.ValueEquals("right"))
                    throw new ArgumentException("Padding must contain a side property with a value of left or right");
                if (Padding.Value.TryGetProperty("char", out JsonElement pChar) && pChar.ValueKind != JsonValueKind.String)
                    throw new ArgumentException("Padding must contain a char property with a charecter value");
                if (Padding.Value.TryGetProperty("length", out JsonElement pLength) && pLength.ValueKind != JsonValueKind.Number)
                    throw new ArgumentException("Padding must contain a length property with a number value");
            }

            this.InputName = InputName ?? "";
            this.OutputName = OutputName ?? "";
            this.Value = (JsonElement)Value;
            this.Transformations = Transformations;
            this.Padding = Padding;
        }

        public override string ToString()
        {
            return ($"Input: {this.InputName} \n Output: {this.OutputName}");
        }
    }

    internal class CsvDef
    {
        // TODO: include quick save?
        public string DefTitle { get; }
        public string Delimiter {get;}
        public string Marks {get;}
        public bool HasHeaders {get;}
        public string OutputDelimiter {get;}
        public string OutputMarks {get;}
        public bool OutputHasHeaders {get;}
        public List<ColumnDef> Columns { get; }

        public CsvDef(
            List<ColumnDef> Columns,
            string DefTitle,
            string Delimiter,
            string Marks,
            bool HasHeaders,
            string OutputDelimiter,
            string OutputMarks,
            bool OutputHasHeaders
        ) {
            this.Columns = Columns;
            this.DefTitle= DefTitle;
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
                    // BUGFIX: did not account for commas in input fields
                    //string[] values = line != null ? line.Split(",") : Array.Empty<string>();
                    //string[] values;
                    List<string> values= new();
                    //char quoteChar = '"';
                    char quoteChar = this.Marks[0];
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
                            else if(c == quoteChar && previous.HasValue && previous == '"')
                            {
                                inQuotes = !inQuotes;
                                buffer += c;
                            }
                            // quote catch
                            else if (c == quoteChar)
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
                        if (HasHeaders)
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

        // TODO: Throw in some paramaters
        // TODO: change from object to something else...
        public string ListToCSV(List<Dictionary<string, object>> csv)
        {
            string Output = "";

            if (this.OutputHasHeaders)
            {
                string line = "";
                Dictionary<string, object> row = csv[0];
                foreach(KeyValuePair<string, object> kvp in row)
                {
                    line += OutputMarks + kvp.Key + OutputMarks + OutputDelimiter;
                }
                Output += line.TrimEnd(',') + "\n";
            }

            foreach (Dictionary<string, object> row in csv)
            {
                string line = "";
                foreach (KeyValuePair<string, object> kvp in row)
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