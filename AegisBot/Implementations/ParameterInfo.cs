using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBot.Implementations
{
    public class ParameterInfo
    {
        public string ParameterName { get; set; }
        public int ParameterIndex { get; set; }
        public string ParameterValue { get; set; }
        public bool IsRequired { get; set; }
    }
}
