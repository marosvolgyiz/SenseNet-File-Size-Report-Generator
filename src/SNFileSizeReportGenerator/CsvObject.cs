using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    public class CsvObject
    {
      
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string NodeTypeName { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string Extension { get; set; }
        public int Versions { get; set; }
        public long SumSize { get; set; }
    }
}
