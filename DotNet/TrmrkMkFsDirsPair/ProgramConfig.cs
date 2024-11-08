﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// DTO containing config values that are deserialized from the text contents that have been read from the config file.
    /// </summary>
    public class ProgramConfig
    {
        /// <summary>
        /// Gets or sets the markdown file name extension.
        /// </summary>
        public string MdFileNameExtension { get; set; }

        /// <summary>
        /// Gets or sets the name of the <c>.keep</c> file (the default being just that: "<c>.keep</c>")
        /// that will reside in the full name folder..
        /// </summary>
        public string KeepFileName { get; set; }

        /// <summary>
        /// Gets or sets the suffix that is appended to the name of the markdown file created for the note item.
        /// </summary>
        public string NoteFileName { get; set; }

        /// <summary>
        /// Gets or sets the prefix that is prepended to the name of the markdown file created for the note item.
        /// </summary>
        public string NoteFileNamePfx { get; set; }

        /// <summary>
        /// Gets or sets the full dir name part for the pair of folders created for the note files.
        /// </summary>
        public string NoteFilesFullDirNamePart { get; set; }

        /// <summary>
        /// Gets or sets the string appended after the short dir name and before the full dir name part when creating the full dir name.
        /// </summary>
        public string FullDirNameJoinStr { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating that the current config values should be dumped to 
        /// a file in (or relative to) the current directory.
        /// </summary>
        public string DumpConfigFileCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating that instead of the normal program execution,
        /// the help message should be printed to the console.
        /// </summary>
        public string PrintHelpMessage { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating the work dir path.
        /// </summary>
        public string WorkDirCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating whether the newly created markdown file
        /// should be open in the default program after the pair of folders has been created.
        /// </summary>
        public string OpenMdFileCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating that instead of a note item, a pair of folders
        /// for the note files should be created.
        /// </summary>
        public string UpdateFullDirNameCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments flag indicating that instead of creating a pair of folders,
        /// the idxes of folder pairs in the current folder should be updated.
        /// </summary>
        public string UpdateDirNameIdxesCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments option indicating that a folders pair note book should be created.
        /// </summary>
        public string CreateDirsPairNoteBookCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments option indicating that a basic note book should be created.
        /// </summary>
        public string CreateBasicNoteBookCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments option indicating the source folder used to create the note book.
        /// </summary>
        public string NoteBookSrcPathCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program arguments option indicating the destination folder used to create the note book.
        /// </summary>
        public string NoteBookDestnPathCmdArgName { get; set; }

        /// <summary>
        /// Gets or sets the prefix prepended to the names of all note item folders.
        /// </summary>
        public string NoteItemDirNamesPfx { get; set; }

        /// <summary>
        /// Gets or sets the prefix prepended to the names of all note internal (like the note files) folders.
        /// </summary>
        public string NoteInternalDirNamesPfx { get; set; }

        /// <summary>
        /// Gets or sets the default note files short dir name index string.
        /// </summary>
        public string DefaultNoteFilesShortDirNameIdxStr { get; set; }

        /// <summary>
        /// Gets or sets the basic note book note files directory name.
        /// </summary>
        public string BasicNoteBookNoteFilesDirName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters the full dir name part should contain.
        /// </summary>
        public int MaxDirNameLength { get; set; }

        /// <summary>
        /// Gets or sets the <c>.keep</c> file contents template. It is used when the value for
        /// <see cref="KeepFileContainsNoteJson"/> property is different from <c>true</c> (or is not provided).
        /// In such case, it will be provided as the first argument to the
        /// <see cref="string.Format(string, object?)" /> method call when generating the file contents template,
        /// the second one being the title of the note provided by the user.
        /// </summary>
        public string KeepFileContentsTemplate { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the <c>.keep</c> file should contain a json
        /// representation of an object containing the note title.
        /// </summary>
        public bool? KeepFileContainsNoteJson { get; set; }

        /// <summary>
        /// Gets or sets the markdown file contents template. It will be provided as the first argument to the
        /// <see cref="string.Format(string, object?)" /> method call when creating the markdown file for a newly created
        /// note item, the second one being the title of the note provided by the user.
        /// </summary>
        public string MdFileContentsTemplate { get; set; }

        /// <summary>
        /// Gets or sets the title macros.
        /// </summary>
        public Dictionary<string, string> TitleMacros { get; set; }

        /// <summary>
        /// Gets or sets the title macros file paths array.
        /// </summary>
        public string[]? TitleMacrosFilePathsArr { get; set; }
    }
}
