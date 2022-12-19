using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SNF_Import_Creator
{
    internal class ColumnDef
    {
        public string? InputName { get; set; }
        public string? OutputName { get; set; }
        public JsonElement Value { get; set; }

        public JsonElement Transformations { get; set; }

        public override string ToString()
        {
            return ($"Input: {this.InputName} \n Output: {this.OutputName}");
        }
    }
}
