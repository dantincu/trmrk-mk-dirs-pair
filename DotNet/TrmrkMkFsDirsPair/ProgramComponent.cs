using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    internal class ProgramComponent
    {
        private readonly ProgramArgsRetriever pgArgsRetriever;

        public ProgramComponent(
            ProgramArgsRetriever pgArgsRetriever)
        {
            this.pgArgsRetriever = pgArgsRetriever ?? throw new ArgumentNullException(
                nameof(pgArgsRetriever));
        }

        public void Run(ProgramArgs pgArgs)
        {
            var config = ProgramConfigRetriever.Instance.Value.Config;
            string baseDirPath = Path.GetFullPath(pgArgs.WorkDir);

            if (pgArgs.UpdateFullDirName)
            {
                UpdateFullDirName(pgArgs, config, baseDirPath);
            }
            else
            {
                string shortDirPath = GetDirPathAndThrowIfDirAlreadyExists(
                    baseDirPath, pgArgs.ShortDirName);

                string fullDirPath = GetDirPathAndThrowIfDirAlreadyExists(
                    baseDirPath, pgArgs.FullDirName);

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

        public void UpdateFullDirName(
            ProgramArgs pgArgs,
            ProgramConfig config,
            string baseDirPath)
        {
            var mdFileName = Directory.GetFiles(
                baseDirPath).Select(
                    file => Path.GetFileName(file)).Single(
                    file => Path.GetExtension(file) == ".md");

            string mdFilePath = Path.Combine(
                baseDirPath, mdFileName);

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

            string baseDirName = Path.GetFileName(baseDirPath);

            string parentDirPath = Path.GetDirectoryName(
                baseDirPath)!;

            string baseDirsPairPfx = string.Concat(
                baseDirName, pgArgs.JoinStr);

            string baseFullDirName = Directory.GetDirectories(
                parentDirPath).Select(
                    dir => Path.GetFileName(dir)).Single(
                    dir => dir != baseDirName && dir.StartsWith(baseDirsPairPfx));

            string newBaseFullDirName = string.Join(
                pgArgs.JoinStr, baseDirName, pgArgs.FullDirNamePart);

            string newNoteFileName = $"{pgArgs.FullDirNamePart}{config.NoteFileName}.md";
            string newNoteFilePath = Path.Combine(baseDirPath, newNoteFileName);

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
