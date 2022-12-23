using System;
using System.Collections.Generic;
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
        public string InputName { get; set; }
        public string OutputName { get; set; }
        public JsonElement? Value { get; set; }
        public List<JsonElement>? Transformations { get; set; }

        public ColumnDef(
            string InputName = "",
            string OutputName = "",
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
            if(string.IsNullOrEmpty(InputName) && Value == null) throw new ArgumentNullException("InputName and Value can not both be empty");

            // If value exists, make sure its not an object
            if (Value != null && Value.Value.ValueKind == JsonValueKind.Object) 
                throw new ArgumentException("Value cannot be an Object. It must be a value or array of objects");

            // If value exists and is an array, make sure it has the correct properties.
            if (Value != null && Value.Value.ValueKind == JsonValueKind.Array)
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

            this.InputName = InputName;
            this.OutputName = OutputName;
            this.Value = Value;
            this.Transformations = Transformations;
        }

        public override string ToString()
        {
            return ($"Input: {this.InputName} \n Output: {this.OutputName}");
        }
    }

    internal class csvDef
    {
        // TODO: include quick save?
        public string Delimiter;
        public string TextMarks;
        public string? Marks;
        public bool HasHeaders;
        public string OutputDelimiter;
        public string OutputTextMarks;
        public string? OutputMarks;
        public bool OutputHasHeaders;
        public List<ColumnDef> Columns;

        public csvDef(
            List<ColumnDef> Columns,
            string Delimiter = ",",
            string TextMarks = "'",
            string? Marks = null,
            bool HasHeaders = true,
            string OutputDelimiter = "'",
            string OutputTextMarks = "'",
            string? OutputMarks = null,
            bool OutputHasHeaders = true
        ) {
            this.Columns = Columns;
            this.Delimiter= Delimiter;
            this.TextMarks= TextMarks;
            this.Marks= Marks;
            this.HasHeaders= HasHeaders;
            this.OutputDelimiter= OutputDelimiter;
            this.OutputTextMarks= OutputTextMarks;
            this.Marks = OutputMarks;
            this.OutputHasHeaders= OutputHasHeaders;            
        }
    }
}
