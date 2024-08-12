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
                out var shortNameEntriesArr,
                out var basicNoteBookNoteFilesFolder,
                out var basicNoteItemMdFilesArr);

            var shortNameDirsList = new List<string>();

            if (createDirPairsNoteBook)
            {
                foreach (var entry in basicNoteItemMdFilesArr!)
                {
                    AddShortDirNameToListIfReq(
                        shortNameDirsList,
                        ConvertToDirsPairNoteItemDir(
                            pgArgs, config, entry));
                }

                if (basicNoteBookNoteFilesFolder != null)
                {
                    ConvertToDirsPairNoteFilesDir(
                        pgArgs, config, basicNoteBookNoteFilesFolder);
                }
            }
            else
            {
                foreach (var entry in shortNameEntriesArr)
                {
                    if (entry.IsNoteItemShortNameDir == true)
                    {
                        AddShortDirNameToListIfReq(
                            shortNameDirsList,
                            ConvertToBasicNote(
                                pgArgs, config, entry));

                        
                    }
                    else if (entry.IsDirPairsNoteFilesShortNameDir == true)
                    {
                        ConvertToBasicNoteFilesFolder(
                            pgArgs, config, entry);
                    }
                }
            }

            foreach (var shortNameDir in shortNameDirsList)
            {
                var newPgArgs = new ProgramArgs
                {
                    WorkDir = Path.Combine(pgArgs.WorkDir, shortNameDir),
                    JoinStr = pgArgs.JoinStr,
                    NoteBookDestnPath = Path.Combine(
                        pgArgs.NoteBookDestnPath,
                        shortNameDir),
                    NoteBookSrcPath = Path.Combine(
                        pgArgs.NoteBookSrcPath,
                        shortNameDir)
                };

                CreateNoteBook(
                    newPgArgs,
                    config,
                    createDirPairsNoteBook);
            }
        }

        private string? AddShortDirNameToListIfReq(
            List<string> shortNameDirsList,
            string? shortDirName)
        {
            if (shortDirName != null)
            {
                shortNameDirsList.Add(
                    shortDirName);
            }

            return shortDirName;
        }

        /// <summary>
        /// Converts the provided basic note item to a dirs pair note item.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="basicNoteItem">The provided basic note item.</param>
        /// <returns>A string value containing the short dir name.</returns>
        private string? ConvertToDirsPairNoteItemDir(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry basicNoteItem)
        {
            string shortDirName = string.Concat(
                config.NoteItemDirNamesPfx,
                basicNoteItem.IdxStr);

            string shortDirPath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                shortDirName);

            Directory.CreateDirectory(
                shortDirPath);

            string fullDirName = string.Join(
                config.FullDirNameJoinStr,
                shortDirName,
                basicNoteItem.FullNamePart);

            string fullDirPath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                fullDirName);

            Directory.CreateDirectory(
                fullDirPath);

            pgArgs.Title = GetMdTitle(
                basicNoteItem.FullPath,
                out string mdTitleStr);

            pgArgs.MdTitleStr = mdTitleStr;

            WriteKeepFile(
                config,
                pgArgs,
                fullDirPath);

            string mdFileName = string.Concat(
                config.NoteFileNamePfx,
                basicNoteItem.FullNamePart,
                config.MdFileNameExtension);

            string mdFilePath = Path.Combine(
                shortDirPath,
                mdFileName);

            File.Copy(
                basicNoteItem.FullPath,
                mdFilePath);

            if (basicNoteItem.MatchingShortNameEntry != null)
            {
                return shortDirName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the provided basic note files folder to a dirs pair note files pair of folders.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="noteFilesDir">The note files pair of folders.</param>
        /// <returns>A string value containing the short dir name.</returns>
        private string ConvertToDirsPairNoteFilesDir(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry noteFilesDir)
        {
            string shortDirName = string.Concat(
                config.NoteInternalDirNamesPfx,
                config.DefaultNoteFilesShortDirNameIdxStr);

            string shortDirPath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                shortDirName);

            CreateDirsPair(new ProgramArgs
            {
                CreatePairForNoteFiles = true,
                ShortDirName = shortDirName,
                FullDirNamePart = config.NoteFilesFullDirNamePart,
                FullDirName = string.Join(
                pgArgs.JoinStr,
                shortDirName,
                config.NoteFilesFullDirNamePart)
            }, config, pgArgs.NoteBookDestnPath);

            UtilsH.CopyDirectory(
                noteFilesDir.FullPath,
                shortDirPath);

            return shortDirName;
        }

        /// <summary>
        /// Converts the provided dirs pair note item to a basic note item.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="noteItemDir">The note item pair of folders.</param>
        /// <returns>A string value containing the short dir name.</returns>
        private string ConvertToBasicNote(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry noteItemDir)
        {
            string shortDirName = string.Concat(
                config.NoteItemDirNamesPfx,
                noteItemDir.IdxStr);

            string shortDirPath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                shortDirName);

            string srcShortDirPath = Path.Combine(
                pgArgs.NoteBookSrcPath,
                shortDirName);

            var shortDirFsEntriesArr = UtilsH.GetFileSystemEntries(
                srcShortDirPath);

            bool hasContent = shortDirFsEntriesArr.Length > 1;

            if (hasContent)
            {
                Directory.CreateDirectory(
                    shortDirPath);
            }

            var srcMdFileEntry = shortDirFsEntriesArr.Single(
                file => EntryNameMatchesDirPairsNoteItemMdFile(
                    config, file, out _));

            string fullMdFileName = string.Join(
                config.FullDirNameJoinStr,
                shortDirName,
                noteItemDir.MatchingFullNameEntry.FullNamePart) + config.MdFileNameExtension;

            string fullMdFilePath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                fullMdFileName);

            File.Copy(
                srcMdFileEntry.FullPath,
                fullMdFilePath);

            if (hasContent)
            {
                return shortDirName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the provided dirs pair note item to a basic note item.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="noteFilesDir">The note item pair of folders.</param>
        /// <returns>A string value containing the short dir name.</returns>
        private string ConvertToBasicNoteFilesFolder(
            ProgramArgs pgArgs,
            ProgramConfig config,
            NoteBookFsEntry noteFilesDir)
        {
            string shortDirName = config.BasicNoteBookNoteFilesDirName;

            string shortDirPath = Path.Combine(
                pgArgs.NoteBookDestnPath,
                shortDirName);

            UtilsH.CopyDirectory(
                noteFilesDir.FullPath,
                shortDirPath);

            return shortDirName;
        }

        /// <summary>
        /// Gets the source note book entries.
        /// </summary>
        /// <param name="pgArgs">The provided program args.</param>
        /// <param name="config">The config obj.</param>
        /// <param name="createDirPairsNoteBook">A boolean value indicating whether to create a dirs pair note book or a basic note book.</param>
        /// <param name="shortNameEntriesArr">The array of entries in the source location</param>
        /// <param name="basicNoteBookNoteFilesFolder">The basic note book note files folder</param>
        /// <param name="basicNoteItemMdFilesArr">The array of basic note item markdown files</param>
        private void GetNoteBookEntries(
            ProgramArgs pgArgs,
            ProgramConfig config,
            bool createDirPairsNoteBook,
            out NoteBookFsEntry[] shortNameEntriesArr,
            out NoteBookFsEntry? basicNoteBookNoteFilesFolder,
            out NoteBookFsEntry[]? basicNoteItemMdFilesArr)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("NoteBookSrcPath: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(pgArgs.NoteBookSrcPath);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("NoteBookDestnPath: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(pgArgs.NoteBookDestnPath);

            Console.ResetColor();
            Console.WriteLine();

            ValidateNoteBookArgs(
                pgArgs,
                config,
                createDirPairsNoteBook);

            var allEntriesArr = UtilsH.GetFileSystemEntries(
                    pgArgs.NoteBookSrcPath).Select(
                fsEntry => GetNoteBookFsEntry(
                    config, fsEntry, createDirPairsNoteBook)).Where(
                fsEntry => fsEntry != null).ToArray();

            var entriesArr = allEntriesArr.Where(
                entry => entry.IsNoteItemShortNameDir == true || entry.IsDirPairsNoteFilesShortNameDir == true).Select(
                shortNameEntry => new NoteBookFsEntry(shortNameEntry)
                {
                    MatchingFullNameEntry = allEntriesArr.Where(
                        candidate => EntriesMatch(
                            shortNameEntry,
                            candidate,
                            createDirPairsNoteBook)).With(nmrbl =>
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

            shortNameEntriesArr = entriesArr;

            basicNoteBookNoteFilesFolder = allEntriesArr.SingleOrDefault(
                entry => entry.IsBasicNoteFilesFolder == true);

            basicNoteItemMdFilesArr = createDirPairsNoteBook switch
            {
                true => allEntriesArr.Where(entry => entry.IsBasicNoteItemFullNameMdFile == true).With(
                    noteItemsNmrbl => noteItemsNmrbl.Select(
                        noteItem => new NoteBookFsEntry(noteItem)
                        {
                            MatchingShortNameEntry = entriesArr.SingleOrDefault(
                                shortNameDir => shortNameDir.MatchingFullNameEntry == noteItem)
                        })).ToArray(),
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
            NoteBookFsEntry candidate,
            bool createDirPairsNoteBook)
        {
            /* bool entriesMatch = candidate.IsNoteItemShortNameDir != true && candidate.IsDirPairsNoteFilesShortNameDir != true &&
                candidate.IdxStr == shortNameEntry.IdxStr &&
                candidate.IsDirPairsNoteItemFullNameDir == shortNameEntry.IsNoteItemShortNameDir && candidate.IsDirPairsNoteFilesFullNameDir == shortNameEntry.IsDirPairsNoteFilesShortNameDir; */

            bool entriesMatch = candidate.IsNoteItemShortNameDir != true && candidate.IsDirPairsNoteFilesShortNameDir != true &&
                candidate.IdxStr == shortNameEntry.IdxStr;

            if (entriesMatch)
            {
                if (createDirPairsNoteBook)
                {
                    entriesMatch = candidate.IsBasicNoteItemFullNameMdFile == true;
                }
                else
                {
                    entriesMatch = candidate.IsDirPairsNoteItemFullNameDir == shortNameEntry.IsNoteItemShortNameDir && candidate.IsDirPairsNoteFilesFullNameDir == shortNameEntry.IsDirPairsNoteFilesShortNameDir;
                }
            }

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
                out var isNoteItemEntryName);

            if (isNoteItemEntryName.HasValue || isBasicNoteBookNoteFilesFolder)
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
                    IsDirPairsNoteItemFullNameDir = isFolder && isNoteFullNameEntry && isNoteItemEntryName == true,
                    IsDirPairsNoteFilesFullNameDir = isFolder && isNoteFullNameEntry && isNoteItemEntryName == false,
                    IsBasicNoteItemFullNameMdFile = !isFolder && isNoteFullNameEntry,
                    IsBasicNoteFilesFolder = isBasicNoteBookNoteFilesFolder,
                    NoteTitle = noteTitle,
                    MdTitleStr = mdTitleStr,
                };
            }

            return noteFsEntry;
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
            out bool? isNoteItemEntryName)
        {
            string? fullNamePart;

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
                    isNoteItemEntryName = null;
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
            else if (Directory.EnumerateFileSystemEntries(pgArgs.NoteBookDestnPath).Count() > 1)
            {
                throw new InvalidOperationException(
                    $"The destination directory must be not contain more than 1 entry: {pgArgs.NoteBookDestnPath}");
            }
        }
    }
}
