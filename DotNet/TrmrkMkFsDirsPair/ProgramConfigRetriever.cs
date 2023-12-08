using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TrmrkMkFsDirsPair
{
    internal class ProgramConfigRetriever
    {
        const string CONFIG_FILE_NAME = "trmrk-config.json";
        const string KEEP = ".keep";
        const string NOTE = "[note]";
        const string NOTE_FILES = "[note-files]";
        const string JOIN_STR = "-";
        const string OPEN_MD_FILE = ":o";
        const string MD_FILE_CONTENTS_TEMPLATE = "# {0}  \n\n";
        const string KEEP_FILE_CONTENTS_TEMPLATE = "";
        const int MAX_DIR_NAME_LEN = 100;

        private ProgramConfigRetriever()
        {
            Config = GetConfig();
        }

        public static Lazy<ProgramConfigRetriever> Instance { get; } = new Lazy<ProgramConfigRetriever>(
            () => new ProgramConfigRetriever());

        public static string ConfigFilePath { get; } = Path.Combine(
            UtilsH.ExecutingAssemmblyPath, CONFIG_FILE_NAME);

        public ProgramConfig Config { get; }

        public void DumpConfig(
            ProgramConfig config = null)
        {
            config ??= Config;
            string dumpConfigFileName = GetDumpConfigFileName();

            if (File.Exists(dumpConfigFileName))
            {
                throw new InvalidOperationException(
                    $"File with name {dumpConfigFileName} already exists");
            }

            string json = JsonSerializer.Serialize(
                config, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(dumpConfigFileName, json);
        }

        private ProgramConfig GetConfig()
        {
            ProgramConfig programConfig;

            if (File.Exists(ConfigFilePath))
            {
                programConfig = GetConfigCore();
            }
            else
            {
                programConfig = new ProgramConfig();
            }

            NormalizeConfig(programConfig);
            return programConfig;
        }

        private void NormalizeConfig(
            ProgramConfig config)
        {
            config.KeepFileName ??= KEEP;
            config.NoteFileName ??= NOTE;
            config.NoteFilesFullDirNamePart ??= NOTE_FILES;
            config.FullDirNameJoinStr ??= JOIN_STR;
            config.OpenMdFileCmdArgName ??= OPEN_MD_FILE;
            config.MaxDirNameLength = config.MaxDirNameLength.Nullify() ?? MAX_DIR_NAME_LEN;
            config.KeepFileContentsTemplate ??= KEEP_FILE_CONTENTS_TEMPLATE;
            config.MdFileContentsTemplate ??= MD_FILE_CONTENTS_TEMPLATE;
        }

        private ProgramConfig GetConfigCore()
        {
            string json = File.ReadAllText(ConfigFilePath);
            var programConfig = JsonSerializer.Deserialize<ProgramConfig>(json);

            return programConfig;
        }

        private string GetDumpConfigFileName()
        {
            var now = DateTime.UtcNow;
            string tmStmp = now.ToString("yyyy-MM-dd_HH-mm-ss.FFFFFFFK");

            string dumpConfigFileName = Path.GetFileNameWithoutExtension(CONFIG_FILE_NAME);
            string dumpConfigFileExtn = Path.GetExtension(CONFIG_FILE_NAME);

            dumpConfigFileName = $"{dumpConfigFileName}-{tmStmp}{dumpConfigFileExtn}";

            Console.WriteLine($"Dumped configuration to file {dumpConfigFileName}");
            return dumpConfigFileName;
        }
    }
}
