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
        /// Creates a note book into the provided destination folder using the provided source folder.
        /// </summary>
        /// <param name="pgArgs">The program args.</param>
        /// <param name="config">The config object.</param>
        /// <param name="createDirPairsNoteBook">A boolean flag indicating whether the note book to be created
        /// is a folders pair note book (or a basic note book).</param>
        private void CreateNoteBook(
            ProgramArgs pgArgs,
            ProgramConfig config,
            bool createDirPairsNoteBook)
        {
            GetNoteBookEntries(pgArgs, config,
                createDirPairsNoteBook,
                out var entriesArr,
                out var basicNoteBookNoteFilesFolder,
                out var basicNoteItemMdFilesArr);

            if (createDirPairsNoteBook)
            {
                foreach (var entry in basicNoteItemMdFilesArr!)
                {
                    ConvertToDirsPairNoteItemDir(
                        pgArgs, config, entry);
                }

                if (basicNoteBookNoteFilesFolder != null)
                {
                    ConvertToDirsPairNoteFilesDir(
                        pgArgs, config, basicNoteBookNoteFilesFolder);
                }
            }
            else
            {
                foreach (var entry in entriesArr)
                {
                    ConvertToBasicNote(
                        pgArgs, config, entry);
                }
            }
        }

        /// <summary>
        /// Converts the provided basic note item to a dirs pair note item.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="basicNoteItem">The provided basic note item.</param>
        private void ConvertToDirsPairNoteItemDir(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry basicNoteItem)
        {

        }

        /// <summary>
        /// Converts the provided basic note files folder to a dirs pair note files pair of folders.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="noteFilesDir">The note files pair of folders.</param>
        private void ConvertToDirsPairNoteFilesDir(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry noteFilesDir)
        {

        }

        /// <summary>
        /// Converts the provided dirs pair note item to a basic note item.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="noteItemDir">The note item pair of folders.</param>
        private void ConvertToBasicNote(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry noteItemDir)
        {

        }

        /// <summary>
        /// Gets the source note book entries.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="createDirPairsNoteBook">A boolean value indicating whether to create a dirs pair note book or a basic note book.</param>
        /// <param name="entriesArr">The array of entries in the source location</param>
        /// <param name="basicNoteBookNoteFilesFolder">The basic note book note files folder</param>
        /// <param name="basicNoteItemMdFilesArr">The array of basic note item markdown files</param>
        private void GetNoteBookEntries(
            ProgramArgs pgArgs,
            ProgramConfig config,
            bool createDirPairsNoteBook,
            out NoteBookFsEntry[] entriesArr,
            out NoteBookFsEntry? basicNoteBookNoteFilesFolder,
            out NoteBookFsEntry[]? basicNoteItemMdFilesArr)
        {
            ValidateNoteBookArgs(
                pgArgs,
                config,
                createDirPairsNoteBook);

            var entriesArray = UtilsH.GetFileSystemEntries(
                    pgArgs.WorkDir).Select(
                fsEntry => GetNoteBookFsEntry(
                    config, fsEntry, createDirPairsNoteBook)).ToArray();

            entriesArray = entriesArray.Where(
                entry => entry.IsNoteItemShortNameDir == true).Select(
                shortNameEntry => new NoteBookFsEntry(shortNameEntry)
                {
                    MatchingFullNameEntry = entriesArray.Where(
                        candidate => EntriesMatch(
                            shortNameEntry,
                            candidate)).With(nmrbl =>
                        {
                            var count = nmrbl.Count();

                            if (count != 1)
                            {
                                OnInvalidNoteBookFsEntry(shortNameEntry,
                                    $"Short name dir with {count} matching full name dirs: {{0}}");
                            }

                            return nmrbl.Single();
                        })
                }).ToArray();

            entriesArr = entriesArray;

            basicNoteBookNoteFilesFolder = entriesArray.SingleOrDefault(
                entry => entry.IsBasicNoteFilesFolder == true);

            basicNoteItemMdFilesArr = createDirPairsNoteBook switch
            {
                true => entriesArray.Where(
                    entry => entry.IsNoteItemShortNameDir != true && entry.IsFolder != true && entriesArray.All(
                        shortNameDir => shortNameDir.MatchingFullNameEntry.With(
                            matchingFullNameEntry => matchingFullNameEntry.IsFolder == true || matchingFullNameEntry.FullNamePart != entry.FullNamePart))).ToArray(
                    ).ActWith(noteItemsArr => entriesArray.All(
                        shortNameDir => noteItemsArr.Single(
                            noteItem => noteItem.IdxStr == shortNameDir.IdxStr) != null)),
                false => null
            };
        }

        /// <summary>
        /// Determines whether the provided file system entries match as a pair of folders.
        /// </summary>
        /// <param name="shortNameEntry">The short name file system entry.</param>
        /// <param name="candidate">The match candidate for full name system entry.</param>
        /// <returns></returns>
        private bool EntriesMatch(
            NoteBookFsEntry shortNameEntry,
            NoteBookFsEntry candidate)
        {
            bool entriesMatch = candidate.IsNoteItemShortNameDir != true && candidate.IsDirPairsNoteFilesShortNameDir != true &&
                candidate.IdxStr == shortNameEntry.IdxStr &&
                candidate.IsDirPairsNoteItemFullNameDir == shortNameEntry.IsNoteItemShortNameDir && candidate.IsDirPairsNoteFilesFullNameDir == shortNameEntry.IsDirPairsNoteFilesShortNameDir;

            return entriesMatch;
        }

        /// <summary>
        /// Converts the provided file system entry to a note book file system entry.
        /// </summary>
        /// <param name="config">The config object.</param>
        /// <param name="fsEntry">The provided file system entry.</param>
        /// <param name="createDirPairsNoteBook">A boolean flag indicating whether the note book to create is a
        /// dir pairs note book or a basic note book.</param>
        /// <returns>A instance of type <see cref="NoteBookFsEntry" /> representing the note book file system entry.</returns>
        /// <exception cref="InvalidOperationException">When the entry name does not match a note book item.</exception>
        private NoteBookFsEntry GetNoteBookFsEntry(
            ProgramConfig config,
            FsEntry fsEntry,
            bool createDirPairsNoteBook)
        {
            NoteBookFsEntry? noteFsEntry = null;
            bool isFolder = fsEntry.IsFolder == true;

            bool isBasicNoteBookNoteFilesFolder = isFolder && fsEntry.Name == config.BasicNoteBookNoteFilesDirName;

            var fullDirNamePart = GetEntryNameType(
                config, fsEntry,
                out var entryNameIdxStr,
                out var isNoteItemEntryName,
                out var isDirPairsNoteItemMdFile);

            if (isNoteItemEntryName.HasValue || isDirPairsNoteItemMdFile.HasValue || isBasicNoteBookNoteFilesFolder)
            {
                bool isNoteFullNameEntry = fullDirNamePart != null && entryNameIdxStr != null;
                bool isNoteShortNameDir = fullDirNamePart == null && entryNameIdxStr != null;

                if (!isFolder && isNoteShortNameDir)
                {
                    OnInvalidNoteBookFsEntry(noteFsEntry,
                        "Not expecting file with shot name: {0}");
                }

                string? noteTitle = null;
                string? mdTitleStr = null;

                if (!isFolder)
                {
                    noteTitle = GetMdTitle(
                        fsEntry.FullPath,
                        out mdTitleStr);
                }

                noteFsEntry = new NoteBookFsEntry
                {
                    FullPath = fsEntry.FullPath,
                    FsEntryName = fsEntry.Name,
                    FullNamePart =  fullDirNamePart,
                    IdxStr = entryNameIdxStr,
                    IsFolder = isFolder,
                    IsNoteItemShortNameDir = isNoteShortNameDir && isNoteItemEntryName == true,
                    IsDirPairsNoteFilesShortNameDir = isNoteShortNameDir && isNoteItemEntryName == false,
                    IsDirPairsNoteItemMdFile = isDirPairsNoteItemMdFile,
                    IsDirPairsNoteItemFullNameDir = isFolder && isNoteFullNameEntry && isNoteItemEntryName == true,
                    IsDirPairsNoteFilesFullNameDir = isFolder && isNoteFullNameEntry && isNoteItemEntryName == false,
                    IsBasicNoteItemFullNameMdFile = !isFolder && isNoteFullNameEntry,
                    IsBasicNoteFilesFolder = isBasicNoteBookNoteFilesFolder,
                    NoteTitle = noteTitle,
                    MdTitleStr = mdTitleStr,
                };
            }
            else
            {
                OnInvalidNoteBookFsEntry(noteFsEntry,
                    "Entry name does not match a note book item: {0}");
            }

            if (createDirPairsNoteBook)
            {
                if (noteFsEntry.IsDirPairsNoteItemFullNameDir == true)
                {
                    OnInvalidNoteBookFsEntry(noteFsEntry,
                        "Not expecting a dir pairs note item full name dir in the source location when creating a dir pairs note book: {0}");
                }
                else if (noteFsEntry.IsDirPairsNoteItemMdFile == true)
                {
                    OnInvalidNoteBookFsEntry(noteFsEntry,
                        "Not expecting a dir pairs note item md file in the source location when creating a dir pairs note book: {0}");
                }
                else if (noteFsEntry.IsNoteItemShortNameDir.ToArr(
                    noteFsEntry.IsBasicNoteFilesFolder,
                    noteFsEntry.IsBasicNoteItemFullNameMdFile).All(
                    nllblVal => nllblVal != true))
                {
                    OnInvalidNoteBookFsEntry(noteFsEntry, string.Join(Environment.NewLine,
                        "When creating a dir pairs note book, an entry in the source location should be either",
                        "a short name dir, a basic note files folder or a basic note item full name md file: {0}"));
                }
            }
            else
            {
                if (isFolder)
                {
                    if (noteFsEntry.IsNoteItemShortNameDir.ToArr(
                        noteFsEntry.IsBasicNoteFilesFolder,
                        noteFsEntry.IsBasicNoteItemFullNameMdFile).Any(
                        nllblVal => nllblVal == true))
                    {
                        OnInvalidNoteBookFsEntry(noteFsEntry, string.Join(Environment.NewLine,
                            "When creating a basic note book, an entry in the source location should be neither",
                            "a short name dir, nor a basic note files folder, nor a basic note item full name md file: {0}"));
                    }
                    else if (noteFsEntry.IsDirPairsNoteItemFullNameDir.ToArr(
                        noteFsEntry.IsDirPairsNoteItemMdFile).All(
                            nllblVal => nllblVal != true))
                    {
                        OnInvalidNoteBookFsEntry(noteFsEntry, string.Join(Environment.NewLine,
                            "When creating a basic note book, an entry in the source location should be either",
                            "a short name dir or a full name dir: {0}"));
                    }
                }
                else
                {
                    OnInvalidNoteBookFsEntry(noteFsEntry,
                        "Expecting only folders in the source location when creating a basic note book: {0}");
                }
            }

            return noteFsEntry ?? throw new InvalidOperationException(
                "Something went wrong...");
        }

        private void OnInvalidNoteBookFsEntry(
            NoteBookFsEntry noteFsEntry,
            string excMsgTemplate,
            Type exceptionType = null)
        {
            exceptionType ??= typeof(InvalidOperationException);

            string excMsg = string.Format(
                excMsgTemplate,
                noteFsEntry.FullPath);

            var excp = (Exception)Activator.CreateInstance(
                exceptionType, excMsg)!;

            throw excp;
        }

        /// <summary>
        /// Parses an entry name and determines whether it matches either a note item or a note internal entry name.
        /// </summary>
        /// <param name="config">The config object.</param>
        /// <param name="entryName">The entry name.</param>
        /// <param name="entryNameIdxStr">The entry name index string.</param>
        /// <param name="isNoteItemEntryName">A boolean flag indicating whether the provided entry name matches a
        /// note item entry name.</param>
        /// <returns>An instance of type <see cref="string" /> representing the full name part of the provided entry if the provided entry
        /// is a full name entry or the null value of the provided entry is a short name entry.</returns>
        private string? GetEntryNameType(
            ProgramConfig config,
            FsEntry fsEntry,
            out string? entryNameIdxStr,
            out bool? isNoteItemEntryName,
            out bool? isDirPairsNoteItemMdFile)
        {
            string? fullNamePart;
            isDirPairsNoteItemMdFile = null;

            if (EntryNameMatches(
                config,
                fsEntry,
                config.NoteItemDirNamesPfx,
                out entryNameIdxStr,
                out fullNamePart))
            {
                isNoteItemEntryName = true;
            }
            else
            {
                if (EntryNameMatches(
                    config,
                    fsEntry,
                    config.NoteInternalDirNamesPfx,
                    out entryNameIdxStr,
                    out fullNamePart))
                {
                    isNoteItemEntryName = false;
                }
                else
                {
                    isNoteItemEntryName = null;

                    if (EntryNameMatchesDirPairsNoteItemMdFile(
                        config, fsEntry, out fullNamePart))
                    {
                        isDirPairsNoteItemMdFile = true;
                    }
                }
            }

            return fullNamePart;
        }

        /// <summary>
        /// Determines whether the provided entry name matches the provided note dir name prefix and, if it does,
        /// it extracts the entry name index string and (optionally) the full dir name part.
        /// </summary>
        /// <param name="config">The config object.</param>
        /// <param name="entryName">The entry name</param>
        /// <param name="noteDirNamesPfx">The note dir names prefix</param>
        /// <param name="entryNameIdxStr">The entry name index string.</param>
        /// <param name="fullDirNamePart">The rest of the entry name.</param>
        /// <returns>A boolean value indicating whether the provided entry name matches the provided note dir name.</returns>
        private bool EntryNameMatches(
            ProgramConfig config,
            FsEntry fsEntry,
            string noteDirNamesPfx,
            out string? entryNameIdxStr,
            out string? fullDirNamePart)
        {
            entryNameIdxStr = null;
            fullDirNamePart = null;

            bool matches = fsEntry.Name.StartsWith(
                noteDirNamesPfx) && (fsEntry.IsFolder == true || (fsEntry.Name.EndsWith(
                    config.MdFileNameExtension)));

            if (matches)
            {
                entryNameIdxStr = fsEntry.Name.Substring(
                    noteDirNamesPfx.Length);

                var joinStrIdx = entryNameIdxStr.IndexOf(
                    config.FullDirNameJoinStr);

                if (joinStrIdx >= 0)
                {
                    matches = joinStrIdx > 0;

                    fullDirNamePart = entryNameIdxStr.Substring(
                        joinStrIdx + config.FullDirNameJoinStr.Length);

                    if (fsEntry.IsFolder != true)
                    {
                        fullDirNamePart = fullDirNamePart.Substring(
                            0, fullDirNamePart.Length - config.MdFileNameExtension.Length);
                    }

                    entryNameIdxStr = entryNameIdxStr.Substring(
                        0, joinStrIdx);
                }

                matches = matches && entryNameIdxStr.All(
                    c => char.IsDigit(c));

                if (!matches)
                {
                    entryNameIdxStr = null;
                    fullDirNamePart = null;
                }
            }

            return matches;
        }

        /// <summary>
        /// Determines whether the entry name is a dir pairs note item markdown file.
        /// </summary>
        /// <param name="config">The config object.</param>
        /// <param name="fsEntry">The provided file system entry.</param>
        /// <param name="fullMdFileNamePart">The markdown file full name part</param>
        /// <returns>A boolean value indicating whether the provided entry is a dir pairs note item markdown file.</returns>
        private bool EntryNameMatchesDirPairsNoteItemMdFile(
            ProgramConfig config,
            FsEntry fsEntry,
            out string fullMdFileNamePart)
        {
            fullMdFileNamePart = null;

            bool matches = fsEntry.IsFolder != true && fsEntry.Name.StartsWith(
                config.NoteFileNamePfx) && fsEntry.Name.EndsWith(
                    config.MdFileNameExtension);

            if (matches)
            {
                fullMdFileNamePart = fsEntry.Name.Substring(
                    config.NoteFileNamePfx.Length,
                    fsEntry.Name.Length - config.NoteFileNamePfx.Length - config.MdFileNameExtension.Length);
            }

            return matches;
        }

        /// <summary>
        /// Validates the arguments passed to the <see cref="CreateNoteBook(ProgramArgs, ProgramConfig, bool)"/> method.
        /// </summary>
        /// <param name="pgArgs">The program args.</param>
        /// <param name="config">The config object.</param>
        /// <param name="createDirPairsNoteBook">A boolean flag indicating whether the note book to be created
        /// is a folders pair note book (or a basic note book).</param>
        /// <exception cref="ArgumentException">If the provided source path is the same with the destination path</exception>
        /// <exception cref="InvalidOperationException">If the destination directory is not an empty directory</exception>
        private void ValidateNoteBookArgs(
            ProgramArgs pgArgs,
            ProgramConfig config,
            bool createDirPairsNoteBook)
        {
            if (pgArgs.NoteBookSrcPath == pgArgs.NoteBookDestnPath)
            {
                throw new ArgumentException(
                    $"The source path cannot be the same with the destination path: {pgArgs.NoteBookSrcPath}");
            }
            else if (Directory.EnumerateFileSystemEntries(pgArgs.NoteBookDestnPath).Any())
            {
                throw new InvalidOperationException(
                    $"The destination directory must be empty: {pgArgs.NoteBookDestnPath}");
            }
        }
    }
}
