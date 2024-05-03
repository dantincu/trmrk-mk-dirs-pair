using System;
using System.Collections.Generic;
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
        public ProgramArgsRetriever()
        {
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
            Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var line in new string[]
                {
                    string.Join(" ", "This tool helps you create (or rename) a pair of folders,",
                    "one with a short name of your choosing",
                    "and one starting with the same short name you provided above, followed by a short join string and",
                    "the name you would usually give to one folder where you want to add nested files and folders."),
                    "",
                    "Running this tool with no arguments has no effect other than printing this help message.",
                    "",
                    "Here is a list of arguments this tool supports:",
                    "",
                    string.Join(" - ", config.UpdateFullDirNameCmdArgName,
                        "                Instead of creating a new pair of folders, update the markdown file and the full name",
                        "folder for the current working directory (or the one specified with the next option)"),
                    "",
                    string.Join(" - ", config.WorkDirCmdArgName + "<file_name>",
                        "   Change the work folder where the pair of folders will be created (or renamed)"),
                    "",
                    string.Join(" - ", config.OpenMdFileCmdArgName,
                        "                Open the newly created markdown file after the pair of folders has been created",
                        "(only when creating pair of folders fo note item)"),
                    "",
                    string.Join(" - ", config.DumpConfigFileCmdArgName + ":<?file_name>",
                        "Dump the current config values to a file"),
                    "",
                    "And here is what the main arguments you pass to this tool should be:",
                    "",
                    string.Join(" ", "If you're creating a new pair of folders, the first argument is mandatory and",
                        "should be the short dir name and the second one should be the note title. If you don't specify",
                        "a second argument or specify an empty or all white-space string as a title,",
                        "a pair of folders for note files will be created instead of a note item folders pair.",
                        "In both cases, the third argument is optional and represents for full dir name join string.",
                        "If you don't provide the third argument, the following will be used for the join string:",
                        $"{config.FullDirNameJoinStr}"),
                    "",
                    string.Join(" ", "If, on the other hand, you're not creating a new pair of folders, but",
                        "update the markdown file and the full name folder for the working directory,",
                        "all further arguments are optional. You can still provide between 1 and 2 arguments.",
                        "If you do, the first one will be used as the new title for the current note to be renamed to.",
                        "If you don't provide this first argument or pass in an empty or all-white-spaces string,",
                        "The title will be extracted from the markdown document residing in the working directory.",
                        "Lastly, if you provide the second argument, it will be used for the join string.",
                        "Otherwise, the join string takes the same default value as for when creating new pair of folders."),
                })
            {
                Console.WriteLine(line);
            }
        }
    }
}
