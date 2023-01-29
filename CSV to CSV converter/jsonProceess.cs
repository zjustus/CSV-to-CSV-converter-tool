using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SNF_Import_Creator
{
    internal class JsonProcessor
    {

        //private CsvDef csvDef;
        //List<ColumnDef>? columnObjects;
        //public JsonProcessor(CsvDef csvDef)
        //{
        //    this.csvDef = csvDef;
        //    this.columnObjects = csvDef.Columns;
        //}
        // TODO: add another init class that takes file string


        public static CsvDef? processJSON(string jsonFile)
        {
            CsvDef defObject;
            List<ColumnDef> columnObjects = new();
            var jsonText = File.ReadAllText(jsonFile);

            JsonElement defJsonObject = JsonSerializer.Deserialize<JsonElement>(jsonText);

            JsonElement columnJsonObjects = defJsonObject.GetProperty("columns");
            foreach (JsonElement i in columnJsonObjects.EnumerateArray())
            {
                ColumnDef x = new(
                    i.TryGetProperty("inputName", out JsonElement InputValue) ? InputValue.GetString() : null,
                    i.TryGetProperty("outputName", out JsonElement Outvalue) ? Outvalue.GetString() : null,
                    i.TryGetProperty("value", out JsonElement theValue) ? theValue : null,
                    i.TryGetProperty("transformations", out JsonElement theTransformations) ? theTransformations.EnumerateArray().ToList() : null,
                    i.TryGetProperty("padding", out JsonElement thePadding) ? thePadding : null
                );

                columnObjects.Add(x);
            }

            defObject = new(
                columnObjects,
                defJsonObject.TryGetProperty("defTitle", out JsonElement theDefTitle) ? theDefTitle.ToString() : "Def File",
                defJsonObject.TryGetProperty("delimiter", out JsonElement theDelimiter) ? theDelimiter.ToString() : ",",
                defJsonObject.TryGetProperty("marks", out JsonElement theMarks) ? theMarks.ToString() : "",
                defJsonObject.TryGetProperty("hasHeaders", out JsonElement theHasHeaders) && theHasHeaders.GetBoolean(),
                defJsonObject.TryGetProperty("outputDelimiter", out JsonElement theOutputDelimiter) ? theOutputDelimiter.ToString() : ",",
                defJsonObject.TryGetProperty("outputMarks", out JsonElement theOutputMarks) ? theOutputMarks.ToString() : "",
                defJsonObject.TryGetProperty("outputHasHeaders", out JsonElement theOutputHasHeaders) && theOutputHasHeaders.GetBoolean()
            );

            // check if loaded file is empty
            if (columnObjects.Count == 0) 
                throw new Exception("JSON columns array is empty");

            return defObject;
        }

        // This function needs to be wrapped in a try catch
        public static string Transform(ColumnDef columnDef, string value)
        {
            foreach (JsonElement tf in columnDef.Transformations)
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
                        value = table.Compute(expression, "").ToString() ?? "";
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
                        MatchCollection matches = Regex.Matches(value, function.ToString());
                        value = "";
                        foreach (Match match in matches.Cast<Match>())
                        {
                            value += match.Value;
                        }
                    }
                }
            }
            return value;
        }

        public static string IfThenProcess(ColumnDef columnDef, string value)
        {
            if (columnDef.Value.ValueKind == JsonValueKind.Array)
            {
                bool matchFound = false;
                foreach (JsonElement statement in columnDef.Value.EnumerateArray())
                {
                    if (
                        statement.ValueKind == JsonValueKind.Object &&
                        statement.TryGetProperty("if", out JsonElement ifValue) &&
                        statement.TryGetProperty("then", out JsonElement thenValue)
                    )
                    {
                        if (
                            ifValue.ValueKind == JsonValueKind.String &&
                            ifValue.ToString() == value.ToString()
                        )
                        {
                            value = thenValue.ToString();
                            matchFound = true;
                            break;
                        }
                    }
                    else if (
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
                throw new Exception("Error!\nThe value column can not be an object");

            else if (columnDef.Value.ValueKind != JsonValueKind.Undefined)
                value = columnDef.Value.ToString();

            return value;
        }

        public static string Padding(ColumnDef columnDef, string value)
        {
            if (columnDef.Padding != null)
            {
                JsonElement thePadding = columnDef.Padding.Value;

                // calculate remaining space, throw a fuss if value is too large
                int remaining = thePadding.GetProperty("length").GetInt32() - value.Length;
                if (remaining < 0)
                    throw new Exception("Error!\nThe value excedes padding space");

                // construct padding space
                string padding = "";
                for (int i = 0; i < remaining; i++) padding += thePadding.GetProperty("char").ToString();

                // append or prepend padding space
                if (thePadding.GetProperty("side").ToString() == "left")
                    value = padding + value;
                else value += padding;
            }

            return value;
        }
    }
}
