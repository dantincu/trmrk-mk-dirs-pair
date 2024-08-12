using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// Component that parses the raw string arguments passed in by the user and retrieves an object
    /// containing the normalized argument values used by the program component.
    /// </summary>
    internal class ProgramArgsRetriever
    {
        /// <summary>
        /// The console message printer component.
        /// </summary>
        private readonly IConsoleMsgPrinter consoleMsgPrinter;

        /// <summary>
        /// The program config retriever (the component that retrieves the normalized config values).
        /// </summary>
        private readonly ProgramConfigRetriever cfgRetriever;

        /// <summary>
        /// An object containing the normalized config values.
        /// </summary>
        private readonly ProgramConfig config;

        /// <summary>
        /// The only constructor of the component which initializes the config object.
        /// </summary>
        public ProgramArgsRetriever(
            IConsoleMsgPrinter consoleMsgPrinter,
            ProgramConfigRetriever cfgRetriever)
        {
            this.consoleMsgPrinter = consoleMsgPrinter ?? throw new ArgumentNullException(
                nameof(consoleMsgPrinter));

            this.cfgRetriever = cfgRetriever ?? throw new ArgumentNullException(
                nameof(cfgRetriever));

            config = cfgRetriever.Config.Value;
        }

        /// <summary>
        /// The main method of the component that parses the raw string arguments passed in by the user and retrieves an object
        /// containing the normalized argument values used by the program component
        /// </summary>
        /// <param name="args">The raw string arguments passed in by the user</param>
        /// <returns>An object containing the normalized argument values used by the program component</returns>
        /// <exception cref="ArgumentNullException">Gets thrown when the short folder name is not provided by the user
        /// or is an empty or al white spaces string.</exception>
        /// <exception cref="InvalidOperationException">Gets thrown when the user passed in an invalid combination of arguments and flags,
        /// that is when the user doesn't specify a non-empty value for the title (in which case the note files
        /// pair of folders will be created without any markdown file inside it) but also provided the flag specifying
        /// that the markdown file that is only created for note item pair of folders should be open)</exception>
        public ProgramArgs GetProgramArgs(
            string[] args)
        {
            var pgArgs = new ProgramArgs
            {
                PrintHelp = args.Length == 0 || args.First() == config.PrintHelpMessage
            };

            if (!pgArgs.PrintHelp)
            {
                var nextArgs = args.ToList();

                SeekFlag(nextArgs, config.WorkDirCmdArgName, (flagValue, idx) =>
                {
                    pgArgs.WorkDir = flagValue;

                    if (!Path.IsPathRooted(pgArgs.WorkDir))
                    {
                        pgArgs.WorkDir = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            pgArgs.WorkDir);
                    }
                }, () =>
                {
                    pgArgs.WorkDir = Directory.GetCurrentDirectory();
                });

                SeekFlag(nextArgs, config.DumpConfigFileCmdArgName, (flagValue, idx) =>
                {
                    pgArgs.DumpConfigFile = true;
                    pgArgs.DumpConfigFileName = flagValue;
                },
                () =>
                {
                    SeekFlag(nextArgs, config.UpdateFullDirNameCmdArgName, (flagValue, idx) =>
                    {
                        OnUpdateFullDirName(pgArgs, nextArgs);
                    },
                    () =>
                    {
                        SeekFlag(nextArgs, config.UpdateDirNameIdxesCmdArgName, (flagValue, idx) =>
                        {
                            OnUpdateDirNameIdxes(pgArgs, nextArgs, flagValue);
                        },
                        () =>
                        {
                            SeekFlag(nextArgs, config.CreateDirsPairNoteBookCmdArgName, (flagValue, idx) =>
                            {
                                pgArgs.CreateDirsPairNoteBookPath = OnCreateNoteBook(
                                    pgArgs, nextArgs, flagValue, true);
                            },
                            () =>
                            {
                                SeekFlag(nextArgs, config.CreateBasicNoteBookCmdArgName, (flagValue, idx) =>
                                {
                                    pgArgs.CreateBasicNoteBookPath = OnCreateNoteBook(
                                        pgArgs, nextArgs, flagValue, false);
                                },
                                () =>
                                {
                                    SeekFlag(nextArgs, config.OpenMdFileCmdArgName, (flagValue, idx) =>
                                    {
                                        pgArgs.OpenMdFile = true;
                                    });

                                    OnCreateDirsPair(pgArgs, nextArgs);
                                });
                            });
                        });
                    });
                });
            }

            return pgArgs;
        }

        /// <summary>
        /// Normalizes the full folder name part starting from the title passed in by the user, sanitizing it
        /// by replacing the <c>/</c> character with the <c>%</c> character and all the file system entry name
        /// invalid characters with the space character. Then, if the length of the result string is greater than
        /// the value specified by the <see cref="ProgramConfig.MaxDirNameLength" /> config property,
        /// the string is trimmed by removing the exceeding characters after the maximum allowed number of characters
        /// has been exceeded starting from the start of the string.
        /// </summary>
        /// <param name="fullDirNamePart">The initial value of the full folder name part that is the
        /// title provided by the user that has been just trimmed of starting and trailling white spaces.</param>
        /// <returns>The normalized full folder name part that can be used as such when appending it to the
        /// short folder name and join string or when creating the name of the markdown file.</returns>
        public string NormalizeFullDirNamePart(
            string fullDirNamePart)
        {
            fullDirNamePart = fullDirNamePart.Replace('/', '%').Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries).JoinStr(" ").Trim();

            if (fullDirNamePart.Length > config.MaxDirNameLength)
            {
                fullDirNamePart = fullDirNamePart.Substring(
                    0, config.MaxDirNameLength);

                fullDirNamePart = fullDirNamePart.TrimEnd();
            }

            if (fullDirNamePart.Last() == '.')
            {
                fullDirNamePart += "%";
            }

            return fullDirNamePart;
        }

        /// <summary>
        /// Normalizes the provided title.
        /// </summary>
        /// <param name="title">The provided title</param>
        /// <returns>The normalized title</returns>
        public string NormalizeTitle(
            string? title)
        {
            title = title?.Trim().Nullify();

            if (title != null)
            {
                if (title.StartsWith(":"))
                {
                    title = title.Substring(1);
                }

                title = string.Join("|", title.Split("||").Select(
                    part =>
                    {
                        foreach (var kvp in config.TitleMacros)
                        {
                            part = part.Replace(
                                kvp.Key, kvp.Value);
                        }

                        return part;
                    }));
            }

            return title;
        }

        /// <summary>
        /// Prints the help message to the command prompt for the user.
        /// </summary>
        /// <param name="config">An object containing the config values.</param>
        public void PrintHelp(ProgramConfig config)
        {
            var x = consoleMsgPrinter.GetDefaultExpressionValues();

            Func<string, string, string> optsHead = (optsStr, sffxStr) =>
            {
                var retStr = string.Concat(
                    $"{{{x.DarkCyan}}}{optsStr}{{{x.DarkGray}}}{sffxStr}");

                return retStr;
            };

            var m = new
            {
                ThisTool = ToMsgTuple($"{{{x.Cyan}}}", "this tool", x.Splitter),
                NoArguments = ToMsgTuple($"{{{x.Cyan}}}", "no arguments", x.Splitter),
                Arguments = ToMsgTuple($"{{{x.Cyan}}}", "arguments", x.Splitter),
                Argument = ToMsgTuple($"{{{x.Cyan}}}", "argument", x.Splitter),
                Prints = ToMsgTuple($"{{{x.Cyan}}}", "prints", x.Splitter),
                PrintingThisHelpMessage = ToMsgTuple($"{{{x.White}}}", "printing this help message", x.Splitter),

                TakeNotes = ToMsgTuple($"{{{x.Cyan}}}", "take notes", x.Splitter),
                PairOfFolders = ToMsgTuple($"{{{x.Cyan}}}", "pair of folders", x.Splitter),
                FoldersPair = ToMsgTuple($"{{{x.Cyan}}}", "folders pair", x.Splitter),
                OneFolder = ToMsgTuple($"{{{x.Cyan}}}", "one folder", x.Splitter),
                Note = ToMsgTuple($"{{{x.Cyan}}}", "note", x.Splitter),
                New = ToMsgTuple($"{{{x.Cyan}}}", "new", x.Splitter),
                Newly = ToMsgTuple($"{{{x.Cyan}}}", "newly", x.Splitter),

                Creating = ToMsgTuple($"{{{x.Cyan}}}", "creating", x.Splitter),
                Created = ToMsgTuple($"{{{x.Cyan}}}", "created", x.Splitter),
                Optional = ToMsgTuple($"{{{x.Cyan}}}", "optional", x.Splitter),

                Renaming = ToMsgTuple($"{{{x.Cyan}}}", "renaming", x.Splitter),
                Renamed = ToMsgTuple($"{{{x.Cyan}}}", "renamed", x.Splitter),
                Instead = ToMsgTuple($"{{{x.Cyan}}}", "instead", x.Splitter),
                Updates = ToMsgTuple($"{{{x.Cyan}}}", "updates", x.Splitter),
                Updating = ToMsgTuple($"{{{x.Cyan}}}", "updating", x.Splitter),
                Mandatory = ToMsgTuple($"{{{x.Cyan}}}", "mandatory", x.Splitter),
                Not = ToMsgTuple($"{{{x.Cyan}}}", "not", x.Splitter),
                But = ToMsgTuple($"{{{x.Cyan}}}", "but", x.Splitter),

                One = ToMsgTuple($"{{{x.Cyan}}}", "one", x.Splitter),
                TheSame = ToMsgTuple($"{{{x.Cyan}}}", "the same", x.Splitter),
                Above = ToMsgTuple($"{{{x.Cyan}}}", "above", x.Splitter),
                Followed = ToMsgTuple($"{{{x.Cyan}}}", "followed", x.Splitter),
                And = ToMsgTuple($"{{{x.Cyan}}}", "and", x.Splitter),
                Next = ToMsgTuple($"{{{x.Cyan}}}", "next", x.Splitter),
                First = ToMsgTuple($"{{{x.Cyan}}}", "first", x.Splitter),
                Second = ToMsgTuple($"{{{x.Cyan}}}", "second", x.Splitter),
                Third = ToMsgTuple($"{{{x.Cyan}}}", "third", x.Splitter),

                Starting = ToMsgTuple($"{{{x.Cyan}}}", "starting", x.Splitter),
                With = ToMsgTuple($"{{{x.Cyan}}}", "with", x.Splitter),
                Current = ToMsgTuple($"{{{x.Cyan}}}", "current", x.Splitter),
                Change = ToMsgTuple($"{{{x.Cyan}}}", "change", x.Splitter),
                OneSpecified = ToMsgTuple($"{{{x.Cyan}}}", "one specified", x.Splitter),
                NextOption = ToMsgTuple($"{{{x.Cyan}}}", "next option", x.Splitter),
                Open = ToMsgTuple($"{{{x.Cyan}}}", "open", x.Splitter),
                Dump = ToMsgTuple($"{{{x.Cyan}}}", "dump", x.Splitter),
                Extracted = ToMsgTuple($"{{{x.Cyan}}}", "extracted", x.Splitter),
                MarkdownFileEditor = ToMsgTuple($"{{{x.Cyan}}}", "markdown file editor", x.Splitter),
                EmptyOrAllWhiteSpaces = ToMsgTuple($"{{{x.Cyan}}}", "empty or all-white-spaces", x.Splitter),

                Short = ToMsgTuple($"{{{x.Cyan}}}", "short", x.Splitter),
                Full = ToMsgTuple($"{{{x.Cyan}}}", "full", x.Splitter),
                Name = ToMsgTuple($"{{{x.Cyan}}}", "name", x.Splitter),
                Folder = ToMsgTuple($"{{{x.Cyan}}}", "folder", x.Splitter),
                File = ToMsgTuple($"{{{x.Cyan}}}", "file", x.Splitter),
                Title = ToMsgTuple($"{{{x.Cyan}}}", "title", x.Splitter),
                NoteItem = ToMsgTuple($"{{{x.Cyan}}}", "note item", x.Splitter),
                NoteFiles = ToMsgTuple($"{{{x.Cyan}}}", "note files", x.Splitter),
                JoinString = ToMsgTuple($"{{{x.Cyan}}}", "join string", x.Splitter),
                MarkdownFile = ToMsgTuple($"{{{x.Cyan}}}", "markdown file", x.Splitter),
                WorkingDirectory = ToMsgTuple($"{{{x.Cyan}}}", "working directory", x.Splitter)
            };

            var joinString = $"{{{x.White}-{x.Black}}}{config.FullDirNameJoinStr}";

            string[] linesArr = [
                $"{{{x.Blue}}}Welcome to the Turmerik MkFsDirsPair note taking tool{{{x.NewLine}}}",

                string.Join(" ", $"{m.ThisTool.U}{{{x.Splitter}}} helps you {m.TakeNotes.L} by {m.Creating.L}",
                    $"or {m.Renaming.L} a {m.PairOfFolders.L} starting {m.With.L} {m.TheSame.L}",
                    $"{m.Short.L} {m.Name.L} you provided {m.Above.L}, {m.Followed.L} by",
                    $"a short {m.JoinString.L} {m.And.L} the {m.Name.L} you would usually give to",
                    $"{m.OneFolder.L} representing a {m.Note.L}{{{x.NewLine}}}"),

                string.Join(" ", $"Running {m.ThisTool.L} with {m.NoArguments.L} has no effect other than",
                    $"{m.PrintingThisHelpMessage.L}{{{x.NewLine}}}"),

                $"Here is a list of argument options {m.ThisTool.L} supports:{{{x.NewLine}}}{{{x.NewLine}}}",

                string.Join(" ",
                    optsHead(config.UpdateFullDirNameCmdArgName, ""),
                    $"{m.Instead.U} of {m.Creating.L} a {m.New.L} {m.PairOfFolders.L}, it {m.Updates.L}",
                    $"the {m.MarkdownFile.L} and the {m.Full.L} {m.Name.L} {m.Folder.L} for the {m.Current.L}",
                    $"{m.WorkingDirectory.L} (or the {m.OneSpecified.L} with the {m.NextOption.L}){{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ",
                    optsHead(config.WorkDirCmdArgName, "<dir_path>"),
                    $"{m.Change.U} the {m.WorkingDirectory.L} where the {m.PairOfFolders.L} will be",
                    $"{m.Created.L} (or {m.Renamed.L}){{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ",
                    optsHead(config.OpenMdFileCmdArgName, ""),
                    $"{m.Open.U} the {m.Newly.L} {m.Created.L} {m.MarkdownFile.L} after the",
                    $"{m.PairOfFolders.L} has been {m.Created.L} with the OS default",
                    $"{m.MarkdownFileEditor.L}{{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ",
                    optsHead(config.DumpConfigFileCmdArgName + ":", "?<file_name>"),
                    $"{m.Dump.U} the current config values to a {m.File.L}{{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ",
                    optsHead(config.PrintHelpMessage, ""),
                    $"{m.Prints.U} this help message{{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ", $"And here is what the main arguments you pass to",
                    $"{m.ThisTool.L} should be:{{{x.NewLine}}}"),

                string.Join(" ", $"If you're {m.Creating.L} a {m.New.L} {m.PairOfFolders.L},",
                    $"the {m.First.L} {m.Argument.L} is {m.Mandatory.L} and should be the",
                    $"{m.Short.L} {m.Folder.L} {m.Name.L} and the {m.Second.L} {m.One.L}",
                    $"should be the {m.Note.L} {m.Title.L}. If you don't specify a {m.Second.L}",
                    $"{m.Argument.L} or specify an {m.EmptyOrAllWhiteSpaces.L} string as a {m.Title.L},",
                    $"a {m.PairOfFolders.L} for {m.NoteFiles.L} will be {m.Created.L} {m.Instead.L} of a",
                    $"{m.NoteItem.L} {m.FoldersPair.L}. In both cases, the {m.Third.L} {m.Argument.L}",
                    $"is {m.Optional.L} and represents the {m.JoinString.L} for the {m.Full.L}",
                    $"{m.Folder.L} {m.Name.L}. If you don't provide a {m.Third.L} {m.Argument.L},",
                    $"the following string will be used as {m.JoinString.L}: {joinString}{{{x.Splitter}}}{{{x.NewLine}}}"),

                string.Join(" ", $"If, on the other hand, you're {m.Not.L} {m.Creating.L} a {m.New.L}",
                    $"{m.PairOfFolders.L}, {m.But.L} {m.Updating.L} the {m.MarkdownFile.L} and the",
                    $"{m.Full.L} {m.Name.L} {m.Folder.L} for the {m.WorkingDirectory.L}, all",
                    $"further {m.Arguments.L} are {m.Optional.L}. You can still provide between 1 and 2",
                    $"{m.Arguments.L}. If you do, the {m.First.L} {m.One.L} will be used as the",
                    $"{m.New.L} {m.Title.L} for the {m.Current.L} {m.Note.L} to bre {m.Renamed.L} to.",
                    $"If you don't provide the {m.First.L} {m.Argument.L} or pass in an",
                    $"{m.EmptyOrAllWhiteSpaces.L} string, the {m.Title.L} will be {m.Extracted.L}",
                    $"from the {m.MarkdownFile.L} residing in the {m.WorkingDirectory.L}.",
                    $"Lastly, if you provide the {m.Second.L} {m.Argument.L}, it will be used for the",
                    $"{m.JoinString.L}. Otherwise, the {m.JoinString.L} takes the same default value as for when",
                    $"{m.Creating.L} a {m.New.L} {m.PairOfFolders.L}: {joinString}{{{x.Splitter}}}{{{x.NewLine}}}{{{x.NewLine}}}"),

                    $"{{{x.Blue}}}You can find the source code for this tool at the following url:",
                    $"{{{x.DarkGreen}}}{ProgramComponent.REPO_URL}{{{x.Splitter}}}{{{x.NewLine}}}{{{x.NewLine}}}"];

            consoleMsgPrinter.Print(linesArr, null, x);
        }

        private string OnCreateNoteBook(
            ProgramArgs pgArgs,
            List<string> nextArgs,
            string flagValue,
            bool isDirsPairNoteBook)
        {
            string commonDirPath = flagValue.NormalizePath();

            SeekFlag(nextArgs, config.NoteBookSrcPathCmdArgName, (flagValue, idx) =>
            {
                pgArgs.NoteBookSrcPath = flagValue.NormalizePath(
                    commonDirPath);
            });

            SeekFlag(nextArgs, config.NoteBookDestnPathCmdArgName, (flagValue, idx) =>
            {
                pgArgs.NoteBookDestnPath = flagValue.NormalizePath(
                    commonDirPath);
            });

            return commonDirPath;
        }

        /// <summary>
        /// Handles the command line arg that matches the update full dir name option.
        /// </summary>
        /// <param name="pgArgs">The program args parsed so far</param>
        /// <param name="nextArgs">The list of command line args that have not yet been parsed</param>
        private void OnUpdateFullDirName(
            ProgramArgs pgArgs,
            List<string> nextArgs)
        {
            pgArgs.UpdateFullDirName = true;

            if (nextArgs.Any())
            {
                pgArgs.Title = NormalizeTitle(
                    nextArgs[0]);

                pgArgs.MdTitleStr = HttpUtility.HtmlEncode(
                    pgArgs.Title);

                nextArgs.RemoveAt(0);

                if (nextArgs.Any())
                {
                    pgArgs.JoinStr = nextArgs[0];
                    nextArgs.RemoveAt(0);
                }
                else
                {
                    pgArgs.JoinStr = config.FullDirNameJoinStr;
                }
            }
            else
            {
                pgArgs.JoinStr = config.FullDirNameJoinStr;
            }
        }

        /// <summary>
        /// Handles the command line arg that matches the update dir name indexes option.
        /// </summary>
        /// <param name="pgArgs">The program args parsed so far</param>
        /// <param name="nextArgs">The list of command line args that have not yet been parsed</param>
        /// <param name="flagValue">The substring of the matching command line arg that starts after the flag name prefix</param>
        private void OnUpdateDirNameIdxes(
            ProgramArgs pgArgs,
            List<string> nextArgs,
            string flagValue)
        {
            pgArgs.UpdateDirNameIdxes = flagValue.Split('|').Select(
                part =>
                {
                    var rangesPartsArr = part.Split(
                        '-', StringSplitOptions.RemoveEmptyEntries);

                    var retArr = rangesPartsArr.Select(
                        rangesPart =>
                        {
                            var subPartsArr = rangesPart.Split("..");

                            var retObj = new ProgramArgs.EntryNamesRange
                            {
                                StartStr = subPartsArr[0],
                                IsRange = subPartsArr.Length > 1,
                                IsSwap = part.Contains("--")
                            };

                            if (retObj.IsRange)
                            {
                                retObj.EndStr = subPartsArr[1].Nullify();
                            }

                            return retObj;
                        }).ToArray();

                    if (retArr.Length != 2)
                    {
                        throw new ArgumentException(
                            "Invalid value for dir name idxes map");
                    }

                    return Tuple.Create(
                        retArr[0],
                        retArr[1]);
                }).ToArray();

            pgArgs.UpdateDirNameIdxes = pgArgs.UpdateDirNameIdxes.SelectMany(
                rangeObj => rangeObj.Item1.IsSwap ? rangeObj.ToArr(
                    Tuple.Create(rangeObj.Item2,
                        rangeObj.Item1)) : rangeObj.ToArr()).ToArray();
        }

        /// <summary>
        /// Handles the case when the pair of folders will be created.
        /// </summary>
        /// <param name="pgArgs">The program args parsed so far</param>
        /// <param name="nextArgs">The list of command line args that have not yet been parsed</param>
        private void OnCreateDirsPair(
            ProgramArgs pgArgs,
            List<string> nextArgs)
        {
            pgArgs.ShortDirName = nextArgs[0].Trim(
                ).Nullify() ?? throw new ArgumentNullException(
                    nameof(pgArgs.ShortDirName));

            nextArgs.RemoveAt(0);

            if (nextArgs.Any())
            {
                pgArgs.Title = NormalizeTitle(
                    nextArgs[0]);

                pgArgs.MdTitleStr = HttpUtility.HtmlEncode(
                    pgArgs.Title);

                nextArgs.RemoveAt(0);
            }

            pgArgs.FullDirNamePart = pgArgs.Title;
            pgArgs.CreatePairForNoteFiles = pgArgs.FullDirNamePart == null;

            if (pgArgs.CreatePairForNoteFiles)
            {
                pgArgs.FullDirNamePart = config.NoteFilesFullDirNamePart;
            }
            else
            {
                pgArgs.FullDirNamePart = NormalizeFullDirNamePart(
                    pgArgs.FullDirNamePart!);
            }

            if (!pgArgs.CreatePairForNoteFiles)
            {
                pgArgs.MdFileName = string.Concat(
                    config.NoteFileNamePfx,
                    pgArgs.FullDirNamePart,
                    config.NoteFileName,
                    config.MdFileNameExtension);
            }
            else if (pgArgs.OpenMdFile)
            {
                throw new InvalidOperationException(
                    $"Would not create a markdown file if creating a note files dirs pair");
            }

            pgArgs.JoinStr = nextArgs.FirstOrDefault(
                ) ?? config.FullDirNameJoinStr;

            pgArgs.FullDirName = string.Join(
                pgArgs.JoinStr, pgArgs.ShortDirName,
                pgArgs.FullDirNamePart);
        }

        /// <summary>
        /// Searches the list of command line args for a one starting with the provided flag prefix.
        /// </summary>
        /// <param name="nextArgs">The list of command line args that have not yet been parsed</param>
        /// <param name="flagName">The flag name</param>
        /// <param name="flagValueCallback">The callback to be called upon finding a matching command line arg</param>
        /// <param name="defaultCallback">The callback to be called when no matching command line arg has been found</param>
        /// <returns>The substring of the matching command line arg that starts after the flag name prefix if
        /// a match has been found, or the <c>null</c> value if no such match has been found.</returns>
        private string? SeekFlag(
            List<string> nextArgs,
            string flagName,
            Action<string, int> flagValueCallback,
            Action defaultCallback = null)
        {
            var boolFlagNamePrefix = $":{flagName}";
            var flagNamePrefix = $"{boolFlagNamePrefix}:";

            var kvp = nextArgs.FirstKvp(
                arg => arg == boolFlagNamePrefix || arg.StartsWith(
                    flagNamePrefix));

            string? flagValue = null;

            if (kvp.Key >= 0)
            {
                if (kvp.Value != boolFlagNamePrefix)
                {
                    flagValue = kvp.Value.Substring(
                        flagNamePrefix.Length);
                }

                nextArgs.RemoveAt(kvp.Key);

                flagValueCallback(
                    flagValue,
                    kvp.Key);
            }
            else
            {
                defaultCallback?.Invoke();
            }

            return flagValue;
        }

        /// <summary>
        /// Flattens the provided 2 dimensional array into a 1 dimensional array.
        /// </summary>
        /// <param name="src">The input 2 dimensional array</param>
        /// <returns>The flattened 1 dimensional array</returns>
        private object[] Flatten(
            params object[][] src) => src.SelectMany(
                o => o).ToArray();

        /// <summary>
        /// Converts the provided arguments to a console string message tuple.
        /// </summary>
        /// <param name="prefix">The provided prefix</param>
        /// <param name="text">The provided text</param>
        /// <param name="splitter">The provided splitter</param>
        /// <returns>An instance of type <see cref="ConsoleStrMsgTuple"/> created from the provided arguments.</returns>
        private ConsoleStrMsgTuple ToMsgTuple(
            string prefix,
            string text,
            string splitter) => ConsoleStrMsgTuple.New(
                prefix, text, splitter);
    }
}
