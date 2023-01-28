using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SNF_Import_Creator
{
    internal class jsonProceess
    {
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
    }
}
