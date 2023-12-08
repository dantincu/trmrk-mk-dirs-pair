using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    internal class ProgramComponent
    {
        public void Run(ProgramArgs pgArgs)
        {
            var config = ProgramConfigRetriever.Instance.Value.Config;
            string baseDirPath = Directory.GetCurrentDirectory();

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

            string keepFilePath = Path.Combine(
                fullDirPath,
                config.KeepFileName);

            string keepFileContents = string.Format(
                config.KeepFileContentsTemplate,
                pgArgs.Title);

            File.WriteAllText(keepFilePath, keepFileContents);

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

        private string GetDirPathAndThrowIfDirAlreadyExists(
            string baseDirPath,
            string dirName)
        {
            string dirPath = Path.Combine(
                baseDirPath, dirName);

            if (Directory.Exists(dirName))
            {
                throw new InvalidOperationException(
                    $"Folder with name {dirName} already exists");
            }

            return dirPath;
        }
    }
}
