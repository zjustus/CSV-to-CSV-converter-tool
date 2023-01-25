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

            JsonElement columnJsonObjects = defJsonObject.GetProperty("Columns");
            foreach (JsonElement i in columnJsonObjects.EnumerateArray())
            {
                ColumnDef x = new(
                    i.TryGetProperty("InputName", out JsonElement InputValue) ? InputValue.GetString() : null,
                    i.TryGetProperty("OutputName", out JsonElement Outvalue) ? Outvalue.GetString() : null,
                    i.TryGetProperty("Value", out JsonElement theValue) ? theValue : null,
                    i.TryGetProperty("Transformations", out JsonElement theTransformations) ? theTransformations.EnumerateArray().ToList() : null
                );

                columnObjects.Add(x);
            }

            defObject = new(
                columnObjects,
                defJsonObject.TryGetProperty("DefTitle", out JsonElement theDefTitle) ? theDefTitle.ToString() : "Def File",
                defJsonObject.TryGetProperty("Delimiter", out JsonElement theDelimiter) ? theDelimiter.ToString() : ",",
                defJsonObject.TryGetProperty("Marks", out JsonElement theMarks) ? theMarks.ToString() : "",
                defJsonObject.TryGetProperty("HasHeaders", out JsonElement theHasHeaders) && theHasHeaders.GetBoolean(),
                defJsonObject.TryGetProperty("OutputDelimiter", out JsonElement theOutputDelimiter) ? theOutputDelimiter.ToString() : ",",
                defJsonObject.TryGetProperty("OutputMarks", out JsonElement theOutputMarks) ? theOutputMarks.ToString() : "",
                defJsonObject.TryGetProperty("OutputHasHeaders", out JsonElement theOutputHasHeaders) && theOutputHasHeaders.GetBoolean()
            );

            // check if loaded file is empty
            if (columnObjects.Count == 0) 
                throw new Exception("JSON columns array is empty");

            return defObject;
        }
    }
}
