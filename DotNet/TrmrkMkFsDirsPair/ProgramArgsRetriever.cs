using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    internal class ProgramArgsRetriever
    {
        private readonly ProgramConfigRetriever cfgRetriever;
        private readonly ProgramConfig config;

        public ProgramArgsRetriever()
        {
            cfgRetriever = ProgramConfigRetriever.Instance.Value;
            config = cfgRetriever.Config;
        }

        public ProgramArgs GetProgramArgs(
            string[] args)
        {
            var pgArgs = new ProgramArgs
            {
                DumpConfigFile = args.Length == 0
            };

            if (!pgArgs.DumpConfigFile)
            {
                pgArgs.ShortDirName = args[0].Trim().Nullify() ?? throw new ArgumentNullException(
                    nameof(pgArgs.ShortDirName));

                pgArgs.Title = args[1].Trim().Nullify();
                pgArgs.FullDirNamePart = pgArgs.Title;
                pgArgs.CreatePairForNoteFiles = pgArgs.FullDirNamePart == null;

                if (pgArgs.CreatePairForNoteFiles)
                {
                    pgArgs.FullDirNamePart = config.NoteFilesFullDirNamePart;
                }
                else
                {
                    pgArgs.FullDirNamePart = NormalizeFullDirNamePart(
                        pgArgs.FullDirNamePart);
                }

                var nextArgs = args[2..];

                if (pgArgs.OpenMdFile = nextArgs.Contains(
                    config.OpenMdFileCmdArgName))
                {
                    nextArgs = nextArgs.Except(
                        [config.OpenMdFileCmdArgName]).ToArray();
                }

                if (!pgArgs.CreatePairForNoteFiles)
                {
                    pgArgs.MdFileName = $"{pgArgs.FullDirNamePart}{config.NoteFileName}.md";
                }
                else if (pgArgs.OpenMdFile)
                {
                    throw new InvalidOperationException(
                        $"Would not create a markdown file if creating a {config.NoteFilesFullDirNamePart} dirs pair");
                }

                pgArgs.JoinStr = nextArgs.FirstOrDefault() ?? config.FullDirNameJoinStr;

                pgArgs.FullDirName = string.Join(
                    pgArgs.JoinStr, pgArgs.ShortDirName,
                    pgArgs.FullDirNamePart);
            }

            return pgArgs;
        }

        private string NormalizeFullDirNamePart(
            string fullDirNamePart)
        {
            fullDirNamePart = fullDirNamePart.Replace('/', '%').Split(
                Path.GetInvalidFileNameChars()).JoinStr(" ");

            if (fullDirNamePart.Length > config.MaxDirNameLength)
            {
                fullDirNamePart = fullDirNamePart.Substring(
                    0, config.MaxDirNameLength);
            }

            return fullDirNamePart;
        }
    }
}
