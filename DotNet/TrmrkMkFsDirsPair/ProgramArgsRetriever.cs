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
                var nextArgs = args.ToArray();

                if ((pgArgs.WorkDir = nextArgs.FirstOrDefault(
                    arg => arg.StartsWith(
                        config.WorkDir))?.Split(':')[2]!) != null)
                {
                    nextArgs = nextArgs.Except(
                        nextArgs.Where(
                            arg => arg.StartsWith(
                        config.WorkDir))).ToArray();
                }
                else
                {
                    pgArgs.WorkDir = Directory.GetCurrentDirectory();
                }

                if (pgArgs.UpdateFullDirName = nextArgs.Contains(
                    config.UpdateFullDirName))
                {
                    nextArgs = nextArgs.Except(
                        [config.UpdateFullDirName]).ToArray();

                    if (nextArgs.Length > 0)
                    {
                        pgArgs.Title = nextArgs[0].Trim().Nullify();
                        nextArgs = nextArgs[1..];

                        if (nextArgs.Length > 0)
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
                    if (pgArgs.OpenMdFile = nextArgs.Contains(
                    config.OpenMdFileCmdArgName))
                    {
                        nextArgs = nextArgs.Except(
                            [config.OpenMdFileCmdArgName]).ToArray();
                    }

                    pgArgs.ShortDirName = nextArgs[0].Trim(
                        ).Nullify() ?? throw new ArgumentNullException(
                            nameof(pgArgs.ShortDirName));

                    pgArgs.Title = nextArgs[1].Trim().Nullify();
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

                    nextArgs = nextArgs[2..];

                    if (!pgArgs.CreatePairForNoteFiles)
                    {
                        pgArgs.MdFileName = $"{pgArgs.FullDirNamePart}{config.NoteFileName}.md";
                    }
                    else if (pgArgs.OpenMdFile)
                    {
                        throw new InvalidOperationException(
                            $"Would not create a markdown file if creating a {config.NoteFilesFullDirNamePart} dirs pair");
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

        public string NormalizeFullDirNamePart(
            string fullDirNamePart)
        {
            if (fullDirNamePart.StartsWith(":"))
            {
                fullDirNamePart = fullDirNamePart.Substring(1);
            }

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
