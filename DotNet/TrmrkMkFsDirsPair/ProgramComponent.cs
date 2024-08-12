using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static TrmrkMkFsDirsPair.ProgramArgs;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// The program's main component that does the core part of the program's execution.
    /// </summary>
    internal partial class ProgramComponent
    {
        public const string REPO_URL = "https://github.com/dantincu/trmrk-mk-dirs-pair";

        /// <summary>
        /// The program args retriever component.
        /// </summary>
        private readonly ProgramArgsRetriever pgArgsRetriever;

        /// <summary>
        /// The program config retriever component.
        /// </summary>
        private readonly ProgramConfigRetriever pgCfgRetriever;

        /// <summary>
        /// The only constructor containing the component dependencies.
        /// </summary>
        /// <param name="pgArgsRetriever">The program args retriever component</param>
        /// <exception cref="ArgumentNullException">Gets thrown when the value for <see cref="pgArgsRetriever" />
        /// is <c>null</c></exception>
        public ProgramComponent(
            ProgramArgsRetriever pgArgsRetriever,
            ProgramConfigRetriever pgCfgRetriever)
        {
            this.pgArgsRetriever = pgArgsRetriever ?? throw new ArgumentNullException(
                nameof(pgArgsRetriever));

            this.pgCfgRetriever = pgCfgRetriever ?? throw new ArgumentNullException(
                nameof(pgCfgRetriever));
        }

        /// <summary>
        /// The component's main method that runs the program.
        /// </summary>
        /// <param name="args">The raw command line args</param>
        public void Run(
            string[] args)
        {
            var pgArgs = pgArgsRetriever.GetProgramArgs(args);
            Run(pgArgs);
        }

        /// <summary>
        /// Runs the core part of the program.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments
        /// and normalized with the config values.</param>
        public void Run(
            ProgramArgs pgArgs)
        {
            var config = pgCfgRetriever.Config.Value;

            if (pgArgs.PrintHelp)
            {
                pgArgsRetriever.PrintHelp(config);
            }
            else
            {
                if (pgArgs.DumpConfigFile)
                {
                    pgCfgRetriever.DumpConfig(
                        pgArgs.DumpConfigFileName);
                }
                else
                {
                    string workDirPath = Path.GetFullPath(pgArgs.WorkDir);

                    if (pgArgs.UpdateFullDirName)
                    {
                        UpdateFullDirName(pgArgs, config, workDirPath);
                    }
                    else if (pgArgs.UpdateDirNameIdxes != null)
                    {
                        UpdateDirNameIdxes(pgArgs, config, workDirPath);
                    }
                    else if (pgArgs.CreateDirsPairNoteBookPath != null)
                    {
                        CreateNoteBook(pgArgs, config, true);
                    }
                    else if (pgArgs.CreateBasicNoteBookPath != null)
                    {
                        CreateNoteBook(pgArgs, config, false);
                    }
                    else
                    {
                        CreateDirsPair(pgArgs, config, workDirPath);
                    }
                }
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
                    file => Path.GetExtension(file) == config.MdFileNameExtension);

            string mdFilePath = Path.Combine(
                workDirPath, mdFileName);

            if (pgArgs.Title == null)
            {
                pgArgs.Title = GetMdTitle(
                    mdFilePath,
                    out string mdTitleStr);

                pgArgs.MdTitleStr ??= mdTitleStr;
            }
            else if (pgArgs.MdTitleStr == null)
            {
                pgArgs.MdTitleStr = HttpUtility.HtmlDecode(pgArgs.Title);
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
                config.MdFileNameExtension);

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
        /// Retrieves the title from the provided markdown file.
        /// </summary>
        /// <param name="mdFilePath">The provided markdown file path</param>
        /// <param name="mdTitleStr">The markdown title string.</param>
        /// <returns>The title extracted from the provided markdown file.</returns>
        private string GetMdTitle(
            string mdFilePath,
            out string mdTitleStr)
        {
            mdTitleStr = File.ReadAllLines(mdFilePath).First(
                line => line.StartsWith("# ")).Substring("# ".Length).Trim();

            string title = HttpUtility.HtmlDecode(mdTitleStr);
            return title;
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

            var entryNamesArr = Directory.GetDirectories(
                workDirPath).Select(
                    dirPath => Path.GetFileName(
                        dirPath)).OrderBy(
                dirPath => dirPath).ToArray();

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
                                newShortDirName, true);

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
            Console.WriteLine(pgArgs.ShortDirName);
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
                    pgArgs.MdTitleStr);
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
    }
}
