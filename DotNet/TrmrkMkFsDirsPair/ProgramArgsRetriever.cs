﻿using Microsoft.VisualBasic.FileIO;
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
        private readonly ConsoleMsgPrinter consoleMsgPrinter;

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
            ConsoleMsgPrinter consoleMsgPrinter)
        {
            this.consoleMsgPrinter = consoleMsgPrinter ?? throw new ArgumentNullException(
                nameof(consoleMsgPrinter));

            cfgRetriever = ProgramConfigRetriever.Instance.Value;
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
                PrintHelp = args.Length == 0
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
                        pgArgs.MdFileName = $"{pgArgs.FullDirNamePart}{config.NoteFileName}.md";
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
        /// <param name="config"></param>
        public void PrintHelp(ProgramConfig config)
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

            Func<ConsoleColor?, string, MsgTuple> msgTpl = (
                color, text) => new MsgTuple
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
                [ 1, ConsoleColor.DarkCyan, "Happy note taking!", resetColor, 2],
            ]);
        }

        private object[] Flatten(params object[][] src) => src.SelectMany(o => o).ToArray();
    }
}
