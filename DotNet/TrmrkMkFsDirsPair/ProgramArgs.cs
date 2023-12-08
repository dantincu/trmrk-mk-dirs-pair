using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    internal class ProgramArgs
    {
        public string ShortDirName { get; set; }
        public string Title { get; set; }
        public string FullDirNamePart { get; set; }
        public string JoinStr { get; set; }
        public string FullDirName { get; set; }
        public string MdFileName { get; set; }
        public bool CreatePairForNoteFiles { get; set; }
        public bool OpenMdFile { get; set; }
        public bool DumpConfigFile { get; set; }
    }
}
