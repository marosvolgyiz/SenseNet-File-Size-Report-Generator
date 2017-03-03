using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    public class DataObject
    {
        public int VersionId { get; set; }
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string NodeTypeName { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
    }
}
