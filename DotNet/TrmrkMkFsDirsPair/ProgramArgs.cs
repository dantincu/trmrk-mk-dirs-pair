using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// Contains arguments and flags passed in by the user and normalized with default values from config.
    /// </summary>
    internal class ProgramArgs
    {
        /// <summary>
        /// Gets or sets the work dir path, where the dirs pair will be created or be renamed, if the case
        /// (optional, as it's assigned the return value of <c>Directory.GetCurrentDirectory()</c> by default).
        /// It can be provided by the user by specifying the flag according to <see cref="ProgramConfig.WorkDirCmdArgName" />
        /// config property.
        /// </summary>
        public string WorkDir { get; set; }

        /// <summary>
        /// Gets or sets the short folder name passed in by the user (it should be a 3 digits number like 999, 998... or 199, 198...
        /// This is mandatory and must be the first argument when creating a new note dir, as this app
        /// doesn't parse the existing entry names in the current folder to extract indexes and retrieve the next index.
        /// </summary>
        public string ShortDirName { get; set; }

        /// <summary>
        /// Gets or sets the note title passed in by the user.
        /// This is mandatory and must be the second argument when creating a new note dir.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the part of the full folder name that is appended after the short folder name and the join str.
        /// It is not directly provided by the user, but either calculated by sanitizing and trimming the title provided by the user
        /// (when creating a note dirs pair), or the value of <see cref="ProgramConfig.NoteFilesFullDirNamePart" /> config property
        /// (when creating the note files dirs pair).
        /// </summary>
        public string FullDirNamePart { get; set; }

        /// <summary>
        /// Gets or sets the string appended after the short folder name and before that full folder name part when
        /// creating the full folder name.
        /// It can be provided by the user or it will receive the default value of <see cref="ProgramConfig.FullDirNameJoinStr" />
        /// config property.
        /// </summary>
        public string JoinStr { get; set; }

        /// <summary>
        /// Gets or sets the full folder name that will be created along side the short folder name as part of the folders pair corresponding
        /// to the created note item.
        /// </summary>
        public string FullDirName { get; set; }

        /// <summary>
        /// Gets or sets the suffix that is appended after the sanitized and trimmed note title and before the .md extension when creating
        /// the markdown file name.
        /// </summary>
        public string MdFileName { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the folders pair created will represent an actual note item
        /// or the pairs of folders where the note file attachments will be added. It is set to true if the user passes an
        /// empty or all whitespaces string for the note title, in which case not markdown will be created.
        /// </summary>
        public bool CreatePairForNoteFiles { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the newly create markdown file will be open with
        /// the default program after the folders pair has been created. It is set to true if the user passes in
        /// the optional flag according to <see cref="ProgramConfig.OpenMdFileCmdArgName" /> config property.
        /// </summary>
        public bool OpenMdFile { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the program should print the help message instead of
        /// making any changes to any of the files on the disk.
        /// </summary>
        public bool PrintHelp { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the program should dump the existing config file
        /// (after it has been normalized with constant values hardcoded in the
        /// <see cref="ProgramConfigRetriever" /> component) into a new text file in (or relative to) the working directory.
        /// NOTE: in this case only, the working directory is always the value returned by <c>Directory.GetCurrentDirectory()</c>,
        /// regardles of whether the user passed the work dir flag according to <see cref="ProgramConfig.WorkDirCmdArgName" />
        /// config property.
        /// </summary>
        public bool DumpConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the file name where the current config values should be dumped to.
        /// </summary>
        public string DumpConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether instead of creating a new pair of folders,
        /// the program should just rename the markdown file and the full name folder for the note item represented
        /// by the pair of folders whose short name folder is the work dir (current directory or the directory
        /// specified by the user according to <see cref="ProgramArgs.WorkDir" /> property.
        /// If the user also specifies a title as the first argument, that title is used to generate the full folder
        /// name part, the name of the markdown file and also the contents of the markdown file line that contains
        /// the document title. Otherwise, the markdown file is parsed to extract the actual title, and that string
        /// is instead used to generate the full folder name part to rename the markdown file and the full name folder.
        /// In this case the contents of the markdown document is left unchanged. In both cases, the <c>.keep</c> file
        /// that resides in the full name folder also gets updated with the new title.
        /// </summary>
        public bool UpdateFullDirName { get; set; }

        public Tuple<EntryNamesRange, EntryNamesRange>[] UpdateDirNameIdxes { get; set; }

        public class EntryNamesRange
        {
            public string StartStr { get; set; }
            public bool IsRange { get; set; }
            public bool IsSwap { get; set; }
            public string? EndStr { get; set; }
        }
    }
}
