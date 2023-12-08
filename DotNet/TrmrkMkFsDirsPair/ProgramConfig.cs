using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    public class ProgramConfig
    {
        public string KeepFileName { get; set; }
        public string NoteFileName { get; set; }
        public string NoteFilesFullDirNamePart { get; set; }
        public string FullDirNameJoinStr { get; set; }
        public string OpenMdFileCmdArgName { get; set; }
        public int MaxDirNameLength { get; set; }
        public string KeepFileContentsTemplate { get; set; }
        public string MdFileContentsTemplate { get; set; }
    }
}
