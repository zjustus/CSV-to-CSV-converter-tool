using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SNF_Import_Creator
{
    internal class columnDef
    {
        public string inputName { get; set; }
        public string outputName { get; set; }
        public JsonElement value { get; set; }

        public override string ToString()
        {
            return ($"Input: {this.inputName} \n Output: {this.outputName}");
        }
    }
}
