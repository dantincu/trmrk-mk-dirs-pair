using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            config = cfgRetriever.Config;
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

                var kvp = nextArgs.FirstKvp(
                    arg => arg.StartsWith(
                        config.WorkDirCmdArgName));

                if (kvp.Key >= 0)
                {
                    pgArgs.WorkDir = kvp.Value.Split(':')[2];
                    nextArgs.RemoveAt(kvp.Key);
                }
                else
                {
                    pgArgs.WorkDir = Directory.GetCurrentDirectory();
                }

                kvp = nextArgs.FirstKvp(
                    arg => arg == config.DumpConfigFileCmdArgName);

                if (kvp.Key >= 0)
                {
                    pgArgs.DumpConfigFile = true;
                    nextArgs.RemoveAt(kvp.Key);

                    pgArgs.DumpConfigFileName = kvp.Value.Split(
                        ':', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault()!;
                }

                kvp = nextArgs.FirstKvp(
                    arg => arg == config.UpdateFullDirNameCmdArgName);

                if (kvp.Key >= 0)
                {
                    pgArgs.UpdateFullDirName = true;
                    nextArgs.RemoveAt(kvp.Key);

                    if (nextArgs.Any())
                    {
                        pgArgs.Title = nextArgs[0].Trim().Nullify()!;
                        nextArgs.RemoveAt(0);

                        if (nextArgs.Any())
                        {
                            pgArgs.JoinStr = nextArgs[0];
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
                else
                {
                    kvp = nextArgs.FirstKvp(
                        arg => arg == config.OpenMdFileCmdArgName);

                    if (kvp.Key >= 0)
                    {
                        pgArgs.OpenMdFile = true;
                        nextArgs.RemoveAt(kvp.Key);
                    }

                    pgArgs.ShortDirName = nextArgs[0].Trim(
                        ).Nullify() ?? throw new ArgumentNullException(
                            nameof(pgArgs.ShortDirName));

                    pgArgs.Title = nextArgs[1].Trim().Nullify()!;
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

                    nextArgs = nextArgs[2..];

                    if (!pgArgs.CreatePairForNoteFiles)
                    {
                        pgArgs.MdFileName = string.Concat(
                            config.NoteFileNamePfx,
                            pgArgs.FullDirNamePart,
                            config.NoteFileName,
                            ".md");
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
            if (fullDirNamePart.StartsWith(":"))
            {
                fullDirNamePart = fullDirNamePart.Substring(1);
            }

            fullDirNamePart = fullDirNamePart.Replace('/', '%').Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries).JoinStr(" ").Trim();

            if (fullDirNamePart.Length > config.MaxDirNameLength)
            {
                fullDirNamePart = fullDirNamePart.Substring(
                    0, config.MaxDirNameLength);
            }

            return fullDirNamePart;
        }

        /// <summary>
        /// Prints the help message to the command prompt for the user.
        /// </summary>
        /// <param name="config">An object containing the config values.</param>
        public void PrintHelp1(ProgramConfig config)
        {
            var resetColor = new ConsoleColor?[] { };
            int optsPaddingLen = 20;

            string optsPadd = new string(
                Enumerable.Range(0, optsPaddingLen).Select(
                    i => '-').ToArray());

            Func<int, string> optsPadding = (strLen) => string.Concat(
                " ", optsPadd.Substring(strLen), " ");

            Func<string, string, object[]> optsHead = (optsStr, sffxStr) => [
                ConsoleColor.DarkCyan,
                optsStr,
                resetColor,
                sffxStr,
                ConsoleColor.DarkGray,
                optsPadding(optsStr.Length + sffxStr.Length)];

            Func<ConsoleColor?, string, object[]> MSG_PART = (color, text) => [
                color ?? (object)resetColor, text];

            Func<string, object[]> MSG = (text) => MSG_PART(null, text);

            Func<ConsoleColor?, string, ConsoleMsgTuple> msgTpl = (
                color, text) => new ConsoleMsgTuple
                {
                    L = MSG_PART(color, text),
                    U = MSG_PART(color, text.CapitalizeFirstLetter())
                };

            var _S = MSG(" ");

            var _this_tool = msgTpl(ConsoleColor.DarkCyan, "this tool");
            var _no_arguments = msgTpl(ConsoleColor.DarkCyan, "no arguments");
            var _arguments = msgTpl(ConsoleColor.DarkCyan, "arguments");
            var _argument = msgTpl(ConsoleColor.DarkCyan, "argument");
            var _printing_this_help_message = msgTpl(ConsoleColor.DarkCyan, "printing this help message");

            var _take_notes = msgTpl(ConsoleColor.Blue, "take notes");
            var _pair_of_folders = msgTpl(ConsoleColor.Blue, "pair of folders");
            var _folders_pair = msgTpl(ConsoleColor.Blue, "folders pair");
            var _one_folder = msgTpl(ConsoleColor.Blue, "one folder");
            var _note = msgTpl(ConsoleColor.Blue, "note");
            var _new = msgTpl(ConsoleColor.Blue, "new");
            var _newly = msgTpl(ConsoleColor.Blue, "newly");

            var _creating = msgTpl(ConsoleColor.Green, "creating");
            var _created = msgTpl(ConsoleColor.Green, "created");
            var _optional = msgTpl(ConsoleColor.Green, "optional");

            var _renaming = msgTpl(ConsoleColor.Magenta, "renaming");
            var _renamed = msgTpl(ConsoleColor.Magenta, "renamed");
            var _instead = msgTpl(ConsoleColor.Magenta, "instead");
            var _updates = msgTpl(ConsoleColor.Magenta, "updates");
            var _updating = msgTpl(ConsoleColor.Magenta, "updating");
            var _mandatory = msgTpl(ConsoleColor.Magenta, "mandatory");
            var _not = msgTpl(ConsoleColor.Magenta, "not");
            var _but = msgTpl(ConsoleColor.Magenta, "but");

            var _one = msgTpl(ConsoleColor.Cyan, "one");
            var _the_same = msgTpl(ConsoleColor.Cyan, "the same");
            var _above = msgTpl(ConsoleColor.Cyan, "above");
            var _followed = msgTpl(ConsoleColor.Cyan, "followed");
            var _and = msgTpl(ConsoleColor.Cyan, "and");
            var _next = msgTpl(ConsoleColor.Cyan, "next");
            var _first = msgTpl(ConsoleColor.Cyan, "first");
            var _second = msgTpl(ConsoleColor.Cyan, "second");
            var _third = msgTpl(ConsoleColor.Cyan, "third");

            var _starting = msgTpl(ConsoleColor.Yellow, "starting");
            var _with = msgTpl(ConsoleColor.Yellow, "with");
            var _current = msgTpl(ConsoleColor.Yellow, "current");
            var _change = msgTpl(ConsoleColor.Yellow, "change");
            var _one_specified = msgTpl(ConsoleColor.Yellow, "one specified");
            var _next_option = msgTpl(ConsoleColor.Yellow, "next option");
            var _open = msgTpl(ConsoleColor.Yellow, "open");
            var _dump = msgTpl(ConsoleColor.Yellow, "dump");
            var _extracted = msgTpl(ConsoleColor.Yellow, "extracted");
            var _markdown_file_editor = msgTpl(ConsoleColor.Yellow, "markdown file editor");
            var _empty_or_all_white_spaces = msgTpl(ConsoleColor.Yellow, "empty or all-white-spaces");

            var _short = msgTpl(ConsoleColor.DarkYellow, "short");
            var _full = msgTpl(ConsoleColor.DarkYellow, "full");
            var _name = msgTpl(ConsoleColor.DarkYellow, "name");
            var _folder = msgTpl(ConsoleColor.DarkYellow, "folder");
            var _file = msgTpl(ConsoleColor.DarkYellow, "file");
            var _title = msgTpl(ConsoleColor.DarkYellow, "title");
            var _note_item = msgTpl(ConsoleColor.DarkYellow, "note item");
            var _note_files = msgTpl(ConsoleColor.DarkYellow, "note files");
            var _join_string = msgTpl(ConsoleColor.DarkYellow, "join string");
            var _markdown_file = msgTpl(ConsoleColor.DarkYellow, "markdown file");
            var _working_directory = msgTpl(ConsoleColor.DarkYellow, "working directory");

            consoleMsgPrinter.Print([
                [ConsoleColor.DarkCyan, 1, "Welcome to the Turmerik MkFsDirsPair note taking tool!", 2],
                [ 1, ..Flatten(_this_tool.U, MSG(" helps you "), _take_notes.L, MSG(" by "),
                    _creating.L, MSG(" (or "), _renaming.L, MSG(") a "), _pair_of_folders.L, MSG(", "),
                    _one.L, _S, _with.L, MSG(" a "), _short.L, _S, _name.L, MSG(" of your choosing and "),
                    _one.L, _S, _starting.L, _S, _with.L, _S, _the_same.L, _S, _short.L, _S, _name.L,
                    MSG(" you provided "), _above.L, MSG(", "), _followed.L, MSG(" by a short "), _join_string.L, _S,
                    _and.L, MSG(" the "), _name.L, MSG(" you would usually give to "), _one_folder.L,
                    MSG(" representing a "), _note.L, MSG("")), 2],
                [ 1, ..Flatten(MSG("Running "), _this_tool.L,
                    MSG(" with "), _no_arguments.L, MSG(" has no effect other than "), _printing_this_help_message.L, MSG(".")), 2],
                [ 1, ..Flatten(MSG("Here is a list of argument options "),
                    _this_tool.L, MSG(" supports:")), 1],
                [ 1, ..Flatten(optsHead(config.UpdateFullDirNameCmdArgName, ""),
                    _instead.U, MSG(" of "), _creating.L, MSG(" a "), _new.L, _S, _pair_of_folders.L,
                    MSG(", it "), _updates.L, MSG(" the "), _markdown_file.L,
                    MSG(" and the "), _full.L, _S, _name.L, _S, _folder.L,
                    MSG(" for the "), _current.L, _S, _working_directory.L,
                    MSG(" (or the "), _one_specified.L,
                    MSG(" with the "), _next_option.L,
                    MSG(")")), 1 ],
                [ 1, ..Flatten(optsHead(config.WorkDirCmdArgName, "<file_name>"),
                    _change.U, MSG(" the "), _working_directory.L, MSG(" where the "),
                    _pair_of_folders.L, MSG(" will be "), _created.L, MSG(" (or "), _renamed.L, MSG(")")), 1 ],
                [ 1, ..Flatten(optsHead(config.OpenMdFileCmdArgName, ""),
                    _open.U, MSG(" the "), _newly.L, _S, _created.L, _S, _markdown_file.L, MSG(" after the "),
                    _pair_of_folders.L, MSG(" has been "), _created.L, MSG(" with the OS default "),
                    _markdown_file_editor.L), 1 ],
                [ 1, ..Flatten(optsHead(config.DumpConfigFileCmdArgName, ":<?file_name>"),
                    _dump.U, MSG(" the current config values to a "), _file.L), 2 ],
                [ 1, ..Flatten(MSG("And here is what the main arguments you pass to "),
                    _this_tool.L, MSG(" should be:")), 1],
                [ 1, ..Flatten(MSG("If you're "), _creating.L, MSG(" a "), _new.L, _S, _pair_of_folders.L, MSG(", "),
                    MSG("the "), _first.L, _S, _argument.L, MSG(" is "), _mandatory.L, MSG(" and should be the "),
                    _short.L, _S, _folder.L, _S, _name.L, MSG(" and the "), _second.L, _S, _one.L,
                    MSG(" should be the "), _note.L, _S, _title.L, MSG(". If you don't specify a "),
                    _second.L, _S, _argument.L, MSG(" or specify an "), _empty_or_all_white_spaces.L,
                    MSG(" string as a "), _title.L, MSG(", a "), _pair_of_folders.L, MSG(" for "),
                    _note_files.L, MSG(" will be "), _created.L, _S, _instead.L, MSG(" of a "),
                    _note_item.L, _S, _folders_pair.L, MSG(". In both cases, the "), _third.L, _S,
                    _argument.L, MSG(" is "), _optional.L, MSG(" and represents the "), _join_string.L,
                    MSG(" for the "), _full.L, _S, _folder.L, _S, _name.L, MSG(". If you don't provide a "),
                    _third.L, _S, _argument.L, MSG(", the following string will be used as "),
                    _join_string.L, MSG(": "), [new ConsoleColor?[]
                        {
                            ConsoleColor.White,
                            ConsoleColor.Black
                        },
                        config.FullDirNameJoinStr], [ resetColor, ""]), 1 ],
                [ 1, ..Flatten(MSG("If, on the other hand, you're "), _not.L, _S, _creating.L,
                    MSG(" a "), _new.L, _S, _pair_of_folders.L, MSG(", "), _but.L, _S, _updating.L,
                    MSG(" the "), _markdown_file.L, MSG(" and the "), _full.L, _S, _name.L, _S, _folder.L,
                    MSG(" for the "), _working_directory.L, MSG(" all further "), _arguments.L,
                    MSG(" are "), _optional.L, MSG(". You can still provide between 1 and 2 "), _arguments.L,
                    MSG(". If you do, the "), _first.L, _S, _one.L, MSG(" will be used for as the "), _new.L,
                    _S, _title.L, MSG(" for the "), _current.L, _S, _note.L, MSG(" to be "),
                    _renamed.L, MSG(" to. If you don't provide the "), _first.L, _S, _argument.L,
                    MSG(" or pass in an "), _empty_or_all_white_spaces.L, MSG(" string, the "),
                    _title.L, MSG(" will be "), _extracted.L, MSG(" from the "), _markdown_file.L,
                    MSG(" residing in the "), _working_directory.L, MSG(". Lastly, if you provide the "),
                    _second.L, _S, _argument.L, MSG(", it will be used for the "), _join_string.L,
                    MSG(". Otherwise, the "), _join_string.L, MSG(" takes the same default value as for when "),
                    _creating.L, MSG(" a "), _new.L, _S, _pair_of_folders.L, MSG(": "), [new ConsoleColor?[]
                        {
                            ConsoleColor.White,
                            ConsoleColor.Black
                        },
                        config.FullDirNameJoinStr], [ resetColor, ""]), 2 ],
                [ 1, ConsoleColor.DarkCyan, "You can find the source code for this tool at the following url:" ],
                [ ConsoleColor.DarkGreen, ProgramComponent.REPO_URL, resetColor, 2 ]
            ]);

            // Console.WriteLine("Happy note taking!");
        }

        /// <summary>
        /// Prints the help message to the command prompt for the user.
        /// </summary>
        /// <param name="config">An object containing the config values.</param>
        public void PrintHelp2(ProgramConfig config)
        {
            var x = consoleMsgPrinter.GetDefaultExpressionValues();
            int optsPaddingLen = 20;

            string optsPadd = new string(
                Enumerable.Range(0, optsPaddingLen).Select(
                    i => '*').ToArray());

            Func<int, string> optsPadding = (
                strLen) => optsPadd.Substring(strLen);

            Func<string, string, string> optsHead = (optsStr, sffxStr) =>
            {
                var optsPaddingStr = optsPadding(optsStr.Length + sffxStr.Length);
                var retStr = $"{{{x.DarkCyan}}}{optsStr}{{{x.Splitter}}}{sffxStr}{{{x.DarkGray}}} {optsPaddingStr}";

                return retStr;
            };

            var m = new
            {
                ThisTool = ToMsgTuple($"{{{x.DarkCyan}}}", "this tool", x.Splitter),
                NoArguments = ToMsgTuple($"{{{x.DarkCyan}}}", "no arguments", x.Splitter),
                Arguments = ToMsgTuple($"{{{x.DarkCyan}}}", "arguments", x.Splitter),
                Argument = ToMsgTuple($"{{{x.DarkCyan}}}", "argument", x.Splitter),
                Prints = ToMsgTuple($"{{{x.DarkCyan}}}", "prints", x.Splitter),
                PrintingThisHelpMessage = ToMsgTuple($"{{{x.DarkCyan}}}", "printing this help message", x.Splitter),

                TakeNotes = ToMsgTuple($"{{{x.Blue}}}", "take notes", x.Splitter),
                PairOfFolders = ToMsgTuple($"{{{x.Blue}}}", "pair of folders", x.Splitter),
                FoldersPair = ToMsgTuple($"{{{x.Blue}}}", "folders pair", x.Splitter),
                OneFolder = ToMsgTuple($"{{{x.Blue}}}", "one folder", x.Splitter),
                Note = ToMsgTuple($"{{{x.Blue}}}", "note", x.Splitter),
                New = ToMsgTuple($"{{{x.Blue}}}", "new", x.Splitter),
                Newly = ToMsgTuple($"{{{x.Blue}}}", "newly", x.Splitter),

                Creating = ToMsgTuple($"{{{x.Green}}}", "creating", x.Splitter),
                Created = ToMsgTuple($"{{{x.Green}}}", "created", x.Splitter),
                Optional = ToMsgTuple($"{{{x.Green}}}", "optional", x.Splitter),

                Renaming = ToMsgTuple($"{{{x.Magenta}}}", "renaming", x.Splitter),
                Renamed = ToMsgTuple($"{{{x.Magenta}}}", "renamed", x.Splitter),
                Instead = ToMsgTuple($"{{{x.Magenta}}}", "instead", x.Splitter),
                Updates = ToMsgTuple($"{{{x.Magenta}}}", "updates", x.Splitter),
                Updating = ToMsgTuple($"{{{x.Magenta}}}", "updating", x.Splitter),
                Mandatory = ToMsgTuple($"{{{x.Magenta}}}", "mandatory", x.Splitter),
                Not = ToMsgTuple($"{{{x.Magenta}}}", "not", x.Splitter),
                But = ToMsgTuple($"{{{x.Magenta}}}", "but", x.Splitter),

                One = ToMsgTuple($"{{{x.Cyan}}}", "one", x.Splitter),
                TheSame = ToMsgTuple($"{{{x.Cyan}}}", "the same", x.Splitter),
                Above = ToMsgTuple($"{{{x.Cyan}}}", "above", x.Splitter),
                Followed = ToMsgTuple($"{{{x.Cyan}}}", "followed", x.Splitter),
                And = ToMsgTuple($"{{{x.Cyan}}}", "and", x.Splitter),
                Next = ToMsgTuple($"{{{x.Cyan}}}", "next", x.Splitter),
                First = ToMsgTuple($"{{{x.Cyan}}}", "first", x.Splitter),
                Second = ToMsgTuple($"{{{x.Cyan}}}", "second", x.Splitter),
                Third = ToMsgTuple($"{{{x.Cyan}}}", "third", x.Splitter),

                Starting = ToMsgTuple($"{{{x.Yellow}}}", "starting", x.Splitter),
                With = ToMsgTuple($"{{{x.Yellow}}}", "with", x.Splitter),
                Current = ToMsgTuple($"{{{x.Yellow}}}", "current", x.Splitter),
                Change = ToMsgTuple($"{{{x.Yellow}}}", "change", x.Splitter),
                OneSpecified = ToMsgTuple($"{{{x.Yellow}}}", "one specified", x.Splitter),
                NextOption = ToMsgTuple($"{{{x.Yellow}}}", "next option", x.Splitter),
                Open = ToMsgTuple($"{{{x.Yellow}}}", "open", x.Splitter),
                Dump = ToMsgTuple($"{{{x.Yellow}}}", "dump", x.Splitter),
                Extracted = ToMsgTuple($"{{{x.Yellow}}}", "extracted", x.Splitter),
                MarkdownFileEditor = ToMsgTuple($"{{{x.Yellow}}}", "markdown file editor", x.Splitter),
                EmptyOrAllWhiteSpaces = ToMsgTuple($"{{{x.Yellow}}}", "empty or all-white-spaces", x.Splitter),

                Short = ToMsgTuple($"{{{x.DarkYellow}}}", "short", x.Splitter),
                Full = ToMsgTuple($"{{{x.DarkYellow}}}", "full", x.Splitter),
                Name = ToMsgTuple($"{{{x.DarkYellow}}}", "name", x.Splitter),
                Folder = ToMsgTuple($"{{{x.DarkYellow}}}", "folder", x.Splitter),
                File = ToMsgTuple($"{{{x.DarkYellow}}}", "file", x.Splitter),
                Title = ToMsgTuple($"{{{x.DarkYellow}}}", "title", x.Splitter),
                NoteItem = ToMsgTuple($"{{{x.DarkYellow}}}", "note item", x.Splitter),
                NoteFiles = ToMsgTuple($"{{{x.DarkYellow}}}", "note files", x.Splitter),
                JoinString = ToMsgTuple($"{{{x.DarkYellow}}}", "join string", x.Splitter),
                MarkdownFile = ToMsgTuple($"{{{x.DarkYellow}}}", "markdown file", x.Splitter),
                WorkingDirectory = ToMsgTuple($"{{{x.DarkYellow}}}", "working directory", x.Splitter)
            };

            var joinString = $"{{{x.White}-{x.Black}}}{config.FullDirNameJoinStr}";

            string[] linesArr = [
                $"{{{x.DarkCyan}}}Welcome to the Turmerik MkFsDirsPair note taking tool{{{x.NewLine}}}",

                string.Join(" ", $"{m.ThisTool.U}{{{x.Splitter}}} helps you {m.TakeNotes.L} by {m.Creating.L}",
                    $"or {m.Renaming.L} a {m.PairOfFolders.L} starting {m.With.L} {m.TheSame.L}",
                    $"{m.Short.L} {m.Name.L} you provided {m.Above.L}, {m.Followed.L} by",
                    $"a short {m.JoinString.L} {m.And.L} the {m.Name.L} you would usually give to",
                    $"{m.OneFolder.L} representing a {m.Note.L}{{{x.NewLine}}}"),

                string.Join(" ", $"Running {m.ThisTool.L} with {m.NoArguments.L} has no effect other than",
                    $"{m.PrintingThisHelpMessage.L}{{{x.NewLine}}}"),

                $"Here is a list of argument options {m.ThisTool.L} supports:{{{x.NewLine}}}{{{x.NewLine}}}",

                string.Join(" ", optsHead(config.UpdateFullDirNameCmdArgName, ""),
                    $"{m.Instead.U} of {m.Creating.L} a {m.New.L} {m.PairOfFolders.L}, it {m.Updates.L}",
                    $"the {m.MarkdownFile.L} and the {m.Full.L} {m.Name.L} {m.Folder.L} for the {m.Current.L}",
                    $"{m.WorkingDirectory.L} (or the {m.OneSpecified.L} with the {m.NextOption.L}){{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ", optsHead(config.WorkDirCmdArgName, "<file_name>"),
                    $"{m.Change.U} the {m.WorkingDirectory.L} where the {m.PairOfFolders.L} will be",
                    $"{m.Created.L} (or {m.Renamed.L}){{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ", optsHead(config.OpenMdFileCmdArgName, ""),
                    $"{m.Open.U} the {m.Newly.L} {m.Created.L} {m.MarkdownFile.L} after the",
                    $"{m.PairOfFolders.L} has been {m.Created.L} with the OS default",
                    $"{m.MarkdownFileEditor.L}{{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ", optsHead(config.DumpConfigFileCmdArgName, ":<?file_name>"),
                    $"{m.Dump.U} the current config values to a {m.File.L}{{{x.NewLine}}}{{{x.NewLine}}}"),

                string.Join(" ", optsHead(config.PrintHelpMessage, ""),
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

                    $"{{{x.DarkCyan}}}You can find the source code for this tool at the following url:",
                    $"{{{x.DarkGreen}}}{ProgramComponent.REPO_URL}{{{x.Splitter}}}{{{x.NewLine}}}{{{x.NewLine}}}"];

            consoleMsgPrinter.Print(linesArr, null, x);
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

        private object[] Flatten(
            params object[][] src) => src.SelectMany(
                o => o).ToArray();

        private ConsoleStrMsgTuple ToMsgTuple(
            string prefix,
            string text,
            string splitter) => ConsoleStrMsgTuple.New(
                prefix, text, splitter);
    }
}
