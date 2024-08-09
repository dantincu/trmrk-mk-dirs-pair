using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static TrmrkMkFsDirsPair.ProgramArgs;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// The program's main component that does the core part of the program's execution.
    /// </summary>
    internal class ProgramComponent
    {
        public const string REPO_URL = "https://github.com/dantincu/trmrk-mk-dirs-pair";

        /// <summary>
        /// The program args retriever component.
        /// </summary>
        private readonly ProgramArgsRetriever pgArgsRetriever;

        /// <summary>
        /// The only constructor containing the component dependencies.
        /// </summary>
        /// <param name="pgArgsRetriever">The program args retriever component</param>
        /// <exception cref="ArgumentNullException">Gets thrown when the value for <see cref="pgArgsRetriever" />
        /// is <c>null</c></exception>
        public ProgramComponent(
            ProgramArgsRetriever pgArgsRetriever)
        {
            this.pgArgsRetriever = pgArgsRetriever ?? throw new ArgumentNullException(
                nameof(pgArgsRetriever));
        }

        /// <summary>
        /// The component's main method that does the core part of the program's execution.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments
        /// and normalized with the config values.</param>
        public void Run(ProgramArgs pgArgs)
        {
            var config = ProgramConfigRetriever.Instance.Value.Config;
            string workDirPath = Path.GetFullPath(pgArgs.WorkDir);

            if (pgArgs.UpdateFullDirName)
            {
                UpdateFullDirName(pgArgs, config, workDirPath);
            }
            else if (pgArgs.UpdateDirNameIdxes != null)
            {
                UpdateDirNameIdxes(pgArgs, config, workDirPath);
            }
            else
            {
                CreateDirsPair(pgArgs, config, workDirPath);
            }
        }

        /// <summary>
        /// Updates the full folder name and markdown file name according to the title that was either
        /// provided by the user or will be extracted from the markdown document.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments and normalized with the config values.</param>
        /// <param name="config">The config object containing the normalized config values.</param>
        /// <param name="workDirPath">The work dir path that has either been provided by the user or
        /// assigned the value of <see cref="Directory.GetCurrentDirectory()" />.</param>
        private void UpdateFullDirName(
            ProgramArgs pgArgs,
            ProgramConfig config,
            string workDirPath)
        {
            var mdFileName = Directory.GetFiles(
                workDirPath).Select(
                    file => Path.GetFileName(file)).Single(
                    file => Path.GetExtension(file) == ".md");

            string mdFilePath = Path.Combine(
                workDirPath, mdFileName);

            if (pgArgs.Title == null)
            {
                pgArgs.Title = File.ReadAllLines(mdFilePath).First(
                    line => line.StartsWith("# ")).Substring("# ".Length).Trim();
            }
            else
            {
                bool foundTitle = false;

                var textLines = File.ReadAllLines(
                    mdFilePath).Select(
                        (line, idx) =>
                        {
                            if (!foundTitle)
                            {
                                if (foundTitle = line.StartsWith("# "))
                                {
                                    line = $"# {pgArgs.Title}  ";
                                }
                            }

                            return line;
                        }).ToArray();

                File.WriteAllLines(mdFilePath, textLines);
            }

            pgArgs.FullDirNamePart ??= pgArgsRetriever.NormalizeFullDirNamePart(
                pgArgs.Title);

            string baseDirName = Path.GetFileName(workDirPath);

            string parentDirPath = Path.GetDirectoryName(
                workDirPath)!;

            string baseDirsPairPfx = string.Concat(
                baseDirName, pgArgs.JoinStr);

            string baseFullDirName = Directory.GetDirectories(
                parentDirPath).Select(
                    dir => Path.GetFileName(dir)).Single(
                    dir => dir != baseDirName && dir.StartsWith(baseDirsPairPfx));

            string newBaseFullDirName = string.Join(
                pgArgs.JoinStr, baseDirName, pgArgs.FullDirNamePart);

            string newNoteFileName = string.Concat(
                config.NoteFileNamePfx,
                pgArgs.FullDirNamePart,
                config.NoteFileName,
                ".md");

            string newNoteFilePath = Path.Combine(workDirPath, newNoteFileName);

            string baseFullDirPath = Path.Combine(
                    parentDirPath, baseFullDirName);

            string newBaseFullDirPath = Path.Combine(
                parentDirPath, newBaseFullDirName);

            if (newNoteFileName != mdFileName)
            {
                File.Move(mdFilePath, newNoteFilePath);
            }

            if (newBaseFullDirName != baseFullDirName)
            {
                Directory.Move(baseFullDirPath, newBaseFullDirPath);
            }

            WriteKeepFile(config, pgArgs, newBaseFullDirPath);
        }

        /// <summary>
        /// Updates the folder name indexes according to the specified options.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments and normalized with the config values.</param>
        /// <param name="config">The config object containing the normalized config values.</param>
        /// <param name="workDirPath">The work dir path that has either been provided by the user or
        /// assigned the value of <see cref="Directory.GetCurrentDirectory()" />.</param>
        private void UpdateDirNameIdxes(
            ProgramArgs pgArgs,
            ProgramConfig config,
            string workDirPath)
        {
            var entryNameRangesArr = pgArgs.UpdateDirNameIdxes;
            bool sortOrderIsAscending = pgArgs.SortOrderIsAscending ?? true;

            var entryNamesArr = Directory.GetDirectories(
                workDirPath).Select(
                    dirPath => Path.GetFileName(
                        dirPath)).With(entriesNmrbl =>
                        {
                            if (sortOrderIsAscending)
                            {
                                entriesNmrbl = entriesNmrbl.OrderBy(
                                    dirPath => dirPath);
                            }
                            else
                            {
                                entriesNmrbl = entriesNmrbl.OrderByDescending(
                                    dirPath => dirPath);
                            }

                            string[] entriesArr = entriesNmrbl.ToArray();
                            return entriesArr;
                        });

            var entryNamesMap = entryNamesArr.Where(
                entryName => !entryName.Contains(
                    config.FullDirNameJoinStr)).ToDictionary(
                entryName => entryName,
                entryName => entryNamesArr.Where(
                    fullDirName => fullDirName.Contains(
                    config.FullDirNameJoinStr) && fullDirName.StartsWith(
                        entryName)).ToArray()).Where(
                kvp => kvp.Value.Length == 1).ToDictionary(
                    kvp => kvp.Key, kvp => kvp.Value.Single());

            var entryNamesMapMxes = entryNameRangesArr.ToDictionary(
                entryNameRange => entryNameRange,
                entryNameRange =>
                {
                    string[] matchingShortDirNamesArr;

                    if (entryNameRange.Item1.IsRange)
                    {
                        matchingShortDirNamesArr = entryNamesMap.Keys.SkipWhile(
                            dirName => dirName.CompareTo(
                                entryNameRange.Item1.StartStr) < 0).TakeWhile(
                            dirName => entryNameRange.Item1.EndStr == null || dirName.CompareTo(
                                entryNameRange.Item1.EndStr) <= 0).ToArray();
                    }
                    else
                    {
                        matchingShortDirNamesArr = [entryNameRange.Item1.StartStr];
                    }

                    string newShortDirName = entryNameRange.Item2.StartStr;

                    var dirNamesMap = matchingShortDirNamesArr.ToDictionary(
                        matchingShortDirName => matchingShortDirName,
                        matchingShortDirName =>
                        {
                            string shortDirName = matchingShortDirName;
                            string tempShortDirName = config.FullDirNameJoinStr + shortDirName;

                            var fullDirName = entryNamesMap[matchingShortDirName];
                            var tempFullDirName = config.FullDirNameJoinStr + fullDirName;

                            var fullDirNamePart = fullDirName.Substring(
                                matchingShortDirName.Length + config.FullDirNameJoinStr.Length);

                            string newFullDirName = string.Join(
                                config.FullDirNameJoinStr,
                                newShortDirName,
                                fullDirNamePart);

                            var retTuple = new DirNamesTuple
                            {
                                ShortDirName = shortDirName,
                                TempShortDirName = tempShortDirName,
                                NewShortDirName = newShortDirName,
                                FullDirName = fullDirName,
                                TempFullDirName = tempFullDirName,
                                NewFullDirName = newFullDirName,
                                ShortDirPath = Path.Combine(
                                    workDirPath, shortDirName),
                                TempShortDirPath = Path.Combine(
                                    workDirPath, tempShortDirName),
                                NewShortDirPath = Path.Combine(
                                    workDirPath, newShortDirName),
                                FullDirPath = Path.Combine(
                                    workDirPath, fullDirName),
                                TempFullDirPath = Path.Combine(
                                    workDirPath, tempFullDirName),
                                NewFullDirPath = Path.Combine(
                                    workDirPath, newFullDirName)
                            };

                            newShortDirName = IncrementDirName(
                                newShortDirName,
                                sortOrderIsAscending);

                            return retTuple;
                        });

                    return dirNamesMap;
                });

            var targetedShortDirNames = entryNamesMapMxes.Values.SelectMany(
                map => map.Keys).ToArray();

            foreach (var mxKvp in entryNamesMapMxes)
            {
                foreach (var kvp in mxKvp.Value)
                {
                    Directory.Move(
                        kvp.Value.ShortDirPath,
                        kvp.Value.TempShortDirPath);

                    Directory.Move(
                        kvp.Value.FullDirPath,
                        kvp.Value.TempFullDirPath);
                }
            }

            foreach (var mxKvp in entryNamesMapMxes)
            {
                foreach (var kvp in mxKvp.Value)
                {
                    Directory.Move(
                        kvp.Value.TempShortDirPath,
                        kvp.Value.NewShortDirPath);

                    Directory.Move(
                        kvp.Value.TempFullDirPath,
                        kvp.Value.NewFullDirPath);
                }
            }
        }

        /// <summary>
        /// Creates the pair of folders according to the specified options.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments and normalized with the config values.</param>
        /// <param name="config">The config object containing the normalized config values.</param>
        /// <param name="workDirPath">The work dir path that has either been provided by the user or
        /// assigned the value of <see cref="Directory.GetCurrentDirectory()" />.</param>
        private void CreateDirsPair(
            ProgramArgs pgArgs,
            ProgramConfig config,
            string workDirPath)
        {
            string shortDirPath = GetDirPathAndThrowIfDirAlreadyExists(
                workDirPath, pgArgs.ShortDirName);

            string fullDirPath = GetDirPathAndThrowIfDirAlreadyExists(
                workDirPath, pgArgs.FullDirName);

            Directory.CreateDirectory(shortDirPath);
            Directory.CreateDirectory(fullDirPath);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Short dir name: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(pgArgs.ShortDirName);
            Console.ResetColor();

            WriteKeepFile(config, pgArgs, fullDirPath);

            if (!pgArgs.CreatePairForNoteFiles)
            {
                string mdFilePath = Path.Combine(
                    shortDirPath,
                    pgArgs.MdFileName);

                string mdContents = string.Format(
                    config.MdFileContentsTemplate,
                    pgArgs.Title);

                File.WriteAllText(mdFilePath, mdContents);

                if (pgArgs.OpenMdFile)
                {
                    UtilsH.OpenWithDefaultProgramIfNotNull(mdFilePath);
                }
            }
        }

        /// <summary>
        /// Combines the provided base folder path and folder name and,
        /// if the resulted path points to an existing directory, it throws an exception.
        /// </summary>
        /// <param name="baseDirPath">The parent folder path.</param>
        /// <param name="dirName">The folder name</param>
        /// <returns>The path resulted by combining the base folder path and folder name.</returns>
        /// <exception cref="InvalidOperationException">Gets thrown when the resulted path points to
        /// an existing directory.</exception>
        private string GetDirPathAndThrowIfDirAlreadyExists(
            string baseDirPath,
            string dirName)
        {
            string dirPath = Path.Combine(
                baseDirPath, dirName);

            if (Directory.Exists(dirPath))
            {
                throw new InvalidOperationException(
                    $"Folder with name {dirName} already exists");
            }

            return dirPath;
        }

        /// <summary>
        /// Returns the text contents of the <c>.keep</c> file name that will reside in the full name folder.
        /// </summary>
        /// <param name="config">An object containing the normalized config values.</param>
        /// <param name="pgArgs">An object containing the program args parsed from the user provided arguments and normalized
        /// with the config values.</param>
        /// <returns>The text contents of the <c>.keep</c> file that will reside in the full name folder.</returns>
        private string GetKeepFileContents(
            ProgramConfig config,
            ProgramArgs pgArgs)
        {
            string keepFileContents;

            if (config.KeepFileContainsNoteJson == true)
            {
                keepFileContents = JsonSerializer.Serialize(new NoteItem
                {
                    Title = pgArgs.Title
                },
                typeof(NoteItem),
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
            }
            else
            {
                keepFileContents = string.Format(
                    config.KeepFileContentsTemplate,
                    pgArgs.Title);
            }

            return keepFileContents;
        }

        /// <summary>
        /// Creates or overwrites the <c>.keep</c> file that will reside in the full name folder.
        /// </summary>
        /// <param name="config">An object containing the normalized config values.</param>
        /// <param name="pgArgs">An object containing the program args parsed from the user provided arguments and normalized
        /// with the config values.</param>
        /// <param name="fullDirPath">The full name folder path.</param>
        private void WriteKeepFile(
            ProgramConfig config,
            ProgramArgs pgArgs,
            string fullDirPath)
        {
            string keepFilePath = Path.Combine(
                fullDirPath,
                config.KeepFileName);

            string keepFileContents = GetKeepFileContents(
                config, pgArgs);

            File.WriteAllText(keepFilePath, keepFileContents);
        }

        /// <summary>
        /// Increments the number found in a folder name.
        /// </summary>
        /// <param name="dirName">The existing dir name</param>
        /// <returns>The name of a new dir name having had its number incremented by 1</returns>
        private string IncrementDirName(
            string dirName,
            bool sortOrderisAscending)
        {
            string digits = new string(
                dirName.SkipWhile(
                    c => !char.IsDigit(c)).ToArray());

            int dirNamePfxLen = dirName.Length - digits.Length;

            string dirNamePfx = dirName.Substring(
                dirNamePfxLen);

            int number = int.Parse(digits);

            if (sortOrderisAscending)
            {
                number++;
            }
            else
            {
                number--;
            }

            string newDigits = number.ToString();
            int pfxLen = digits.Length - newDigits.Length;

            if (pfxLen > 0)
            {
                string pfx = digits.Substring(pfxLen);
                newDigits = pfx + newDigits;
            }

            string newDirName = dirNamePfx + newDigits;
            return newDigits;
        }

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
    }
}
