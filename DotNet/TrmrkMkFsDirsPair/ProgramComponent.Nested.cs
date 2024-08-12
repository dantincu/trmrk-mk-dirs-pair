using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// The program's main component that does the core part of the program's execution.
    /// </summary>
    internal partial class ProgramComponent
    {
        /// <summary>
        /// Stores the dir names for renaming action.
        /// </summary>
        private class DirNamesTuple
        {
            /// <summary>
            /// Gets or sets the short dir name.
            /// </summary>
            public string ShortDirName { get; init; }

            /// <summary>
            /// Gets or sets the temp short dir name.
            /// </summary>
            public string TempShortDirName { get; init; }

            /// <summary>
            /// Gets or sets the new short dir name.
            /// </summary>
            public string NewShortDirName { get; init; }

            /// <summary>
            /// Gets or sets the full dir name.
            /// </summary>
            public string FullDirName { get; init; }

            /// <summary>
            /// Gets or sets the temp full dir name.
            /// </summary>
            public string TempFullDirName { get; init; }

            /// <summary>
            /// Gets or sets the new full dir name.
            /// </summary>
            public string NewFullDirName { get; init; }

            /// <summary>
            /// Gets or sets the short dir path.
            /// </summary>
            public string ShortDirPath { get; init; }

            /// <summary>
            /// Gets or sets the temp short dir path.
            /// </summary>
            public string TempShortDirPath { get; init; }

            /// <summary>
            /// Gets or sets the new short dir path.
            /// </summary>
            public string NewShortDirPath { get; init; }

            /// <summary>
            /// Gets or sets the full dir path.
            /// </summary>
            public string FullDirPath { get; init; }

            /// <summary>
            /// Gets or sets the temp full dir path.
            /// </summary>
            public string TempFullDirPath { get; init; }

            /// <summary>
            /// Gets or sets the new full dir path.
            /// </summary>
            public string NewFullDirPath { get; init; }
        }

        private class NoteBookFsEntry
        {
            /// <summary>
            /// The public constructor used to create an instance from scratch.
            /// </summary>
            public NoteBookFsEntry()
            {
            }

            /// <summary>
            /// The cloning constructor used to create an instance by cloning an existing instance.
            /// </summary>
            /// <param name="src">The source instance.</param>
            public NoteBookFsEntry(NoteBookFsEntry src)
            {
                FullPath = src.FullPath;
                FsEntryName = src.FsEntryName;
                IdxStr = src.IdxStr;
                FullNamePart = src.FullNamePart;
                IsFolder = src.IsFolder;
                IsBasicNoteItemFullNameMdFile = src.IsBasicNoteItemFullNameMdFile;
                IsNoteItemShortNameDir = src.IsNoteItemShortNameDir;
                IsBasicNoteFilesFolder = src.IsBasicNoteFilesFolder;
                IsDirPairsNoteItemFullNameDir = src.IsDirPairsNoteItemFullNameDir;
                MatchingFullNameEntry = src.MatchingFullNameEntry;
            }

            /// <summary>
            /// Gets or sets the full entry path;
            /// </summary>
            public string FullPath { get; set; }

            /// <summary>
            /// Gets or sets the file system entry name.
            /// </summary>
            public string FsEntryName { get; init; }

            /// <summary>
            /// Gets or sets the entry name index string.
            /// </summary>
            public string? IdxStr { get; init; }

            /// <summary>
            /// Gets or sets the full name part.
            /// </summary>
            public string? FullNamePart { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a folder entry.
            /// </summary>
            public bool? IsFolder { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a note book short name folder.
            /// </summary>
            public bool? IsNoteItemShortNameDir { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is 
            /// </summary>
            public bool? IsBasicNoteItemFullNameMdFile { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a basic note book note files folder.
            /// </summary>
            public bool? IsBasicNoteFilesFolder { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a dir pairs note item markdown file.
            /// </summary>
            public bool? IsDirPairsNoteItemMdFile { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a dir pairs note item full name folder.
            /// </summary>
            public bool? IsDirPairsNoteItemFullNameDir { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a dir pairs note files short name folder.
            /// </summary>
            public bool? IsDirPairsNoteFilesShortNameDir { get; init; }

            /// <summary>
            /// Gets or sets a boolean flag indicating whether this entry is a dir pairs note files full name folder.
            /// </summary>
            public bool? IsDirPairsNoteFilesFullNameDir { get; init; }

            /// <summary>
            /// Gets or sets the matching full name entry.
            /// </summary>
            public NoteBookFsEntry MatchingFullNameEntry { get; init; }

            /// <summary>
            /// Gets or sets the note title.
            /// </summary>
            public string? NoteTitle { get; init; }

            /// <summary>
            /// Gets or sets the markdown title string.
            /// </summary>
            public string? MdTitleStr { get; init; }
        }
    }
}
