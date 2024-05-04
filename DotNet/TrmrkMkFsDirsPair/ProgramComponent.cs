using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            else
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
        }

        /// <summary>
        /// Updates the full folder name and markdown file name according to the title that was either
        /// provided by the user or will be extracted from the markdown document.
        /// </summary>
        /// <param name="pgArgs">The program args parsed from the user provided arguments and normalized with the config values.</param>
        /// <param name="config">The config object containing the normalized config values.</param>
        /// <param name="workDirPath">The work dir path that has either been provided by the user or
        /// assigned the value of <see cref="Directory.GetCurrentDirectory()" />.</param>
        public void UpdateFullDirName(
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

            string newNoteFileName = $"{pgArgs.FullDirNamePart}{config.NoteFileName}.md";
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
    }
}
