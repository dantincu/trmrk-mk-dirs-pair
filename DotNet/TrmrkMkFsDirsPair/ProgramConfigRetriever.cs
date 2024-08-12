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
    /// <summary>
    /// The component that retrieves and normalizes the config values from the config file.
    /// </summary>
    internal class ProgramConfigRetriever
    {
        /// <summary>
        /// The default markdown file name extension.
        /// </summary>
        const string MD_FILE_NAME_EXTENSION = ".md";

        /// <summary>
        /// The default name for the work dir user arguments flag.
        /// </summary>
        const string WORK_DIR = "wd";

        /// <summary>
        /// The default name for the work dir user arguments flag.
        /// </summary>
        const string DUMP_CONFIG_FILE = "dmpc";

        /// <summary>
        /// The name of the file that contains json serialized representation of the configuration values.
        /// </summary>
        const string CONFIG_FILE_NAME = "trmrk-config.json";

        /// <summary>
        /// The default value for the <c>.keep</c> file name that will be created inside the full name folder.
        /// </summary>
        const string KEEP = ".keep";

        /// <summary>
        /// The default value for the suffix appended to the markdown file name.
        /// </summary>
        const string NOTE = "[note]";

        /// <summary>
        /// The default value for full dir name part for the pair of folders created for note files.
        /// </summary>
        const string NOTE_FILES = "[note-files]";

        /// <summary>
        /// The default value for the string used for joining the short folder name with the full folder name part
        /// when creating the full folder name.
        /// </summary>
        const string JOIN_STR = "-";

        /// <summary>
        /// The default name for the user arguments flag that indicates whether the newly created markdown file
        /// should be open in the default program after the pair of folders have been created.
        /// </summary>
        const string OPEN_MD_FILE = "o";

        /// <summary>
        /// The default name for the work dir user arguments flag.
        /// </summary>
        const string UPDATE_FULL_DIR_NAME = "u";

        /// <summary>
        /// The default name for the work dir user arguments flag.
        /// </summary>
        const string UPDATE_DIR_NAME_IDXES = "ux";

        /// <summary>
        /// The default name for the print help message user arguments flag.
        /// </summary>
        const string PRINT_HELP_MESSAGE = "h";

        /// <summary>
        /// The default value for the markdown file text contents template.
        /// </summary>
        const string MD_FILE_CONTENTS_TEMPLATE = "# {0}  \n\n";

        /// <summary>
        /// The default value for the <c>.keep</c> file text contents template.
        /// </summary>
        const string KEEP_FILE_CONTENTS_TEMPLATE = "";

        /// <summary>
        /// The default name for the create folders pair note book option.
        /// </summary>
        const string CREATE_DIRS_PAIR_NOTE_BOOK = "mkdpnb";

        /// <summary>
        /// The default name for the create basic note book option.
        /// </summary>
        const string CREATE_BASIC_NOTE_BOOK = "mkbsnb";

        /// <summary>
        /// The default name for the note book source path option.
        /// </summary>
        const string NOTE_BOOK_SRC_PATH = "nbsrc";

        /// <summary>
        /// The default name for the note book destination path option.
        /// </summary>
        const string NOTE_BOOK_DESTN_PATH = "nbdstn";

        /// <summary>
        /// The default value for the note item folder names prefix.
        /// </summary>
        const string NOTE_ITEM_DIR_NAMES_PFX = "";

        /// <summary>
        /// The default value for the note internals folder names prefix.
        /// </summary>
        const string NOTE_INTERNAL_DIR_NAMES_PFX = "0";

        /// <summary>
        /// The default value for the basic note book note files directory name.
        /// </summary>
        const string BASIC_NOTE_BOOK_NOTE_FILES_DIR_NAME = "NF";

        /// <summary>
        /// The default value for the maximum number of characters allowed for the full folder name part.
        /// </summary>
        const int MAX_DIR_NAME_LEN = 100;

        /// <summary>
        /// The only constructor of the singleton component where the config is being read from the disk and normalized.
        /// </summary>
        public ProgramConfigRetriever()
        {
            Config = new Lazy<ProgramConfig>(() => GetConfig());
        }

        /// <summary>
        /// Gets the path of the config file.
        /// </summary>
        public static string ConfigFilePath { get; } = Path.Combine(
            UtilsH.ExecutingAssemmblyPath, CONFIG_FILE_NAME);

        /// <summary>
        /// Gets the object containing the normalized config values.
        /// </summary>
        public Lazy<ProgramConfig> Config { get; }

        /// <summary>
        /// Serializes the provided config object (or the existing normalized one, if the provided argument is null)
        /// to json and writes the json to a text file in the current directory.
        /// </summary>
        /// <param name="dumpConfigFileName">The name of the file where the config json should be written.</param>
        /// <param name="config">The object containing configuration values.</param>
        /// <exception cref="InvalidOperationException">Gets thrown when the provided dump config file name
        /// points to an already existing file.</exception>
        public void DumpConfig(
            string dumpConfigFileName = null,
            ProgramConfig config = null)
        {
            config ??= Config.Value;
            dumpConfigFileName ??= GetDumpConfigFileName();

            if (File.Exists(dumpConfigFileName))
            {
                throw new InvalidOperationException(
                    $"File with name {dumpConfigFileName} already exists");
            }

            Console.WriteLine($"Dumped configuration to file {dumpConfigFileName}");

            string json = JsonSerializer.Serialize(
                config, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(dumpConfigFileName, json);
        }

        /// <summary>
        /// Returns an object containing the normalized config values.
        /// </summary>
        /// <returns>An object containing the normalized config values.</returns>
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

        /// <summary>
        /// Normalizes the config values by assigning hardcoded constants to properties whose values are <c>null</c>.
        /// </summary>
        /// <param name="config"></param>
        private void NormalizeConfig(
            ProgramConfig config)
        {
            config.MdFileNameExtension ??= MD_FILE_NAME_EXTENSION;
            config.WorkDirCmdArgName ??= WORK_DIR;
            config.DumpConfigFileCmdArgName ??= DUMP_CONFIG_FILE;
            config.KeepFileName ??= KEEP;
            config.NoteFileName ??= NOTE;
            config.NoteFilesFullDirNamePart ??= NOTE_FILES;
            config.FullDirNameJoinStr ??= JOIN_STR;
            config.OpenMdFileCmdArgName ??= OPEN_MD_FILE;
            config.UpdateFullDirNameCmdArgName ??= UPDATE_FULL_DIR_NAME;
            config.UpdateDirNameIdxesCmdArgName ??= UPDATE_DIR_NAME_IDXES;
            config.PrintHelpMessage ??= PRINT_HELP_MESSAGE;
            config.CreateDirsPairNoteBookCmdArgName ??= CREATE_DIRS_PAIR_NOTE_BOOK;
            config.CreateBasicNoteBookCmdArgName ??= CREATE_BASIC_NOTE_BOOK;
            config.NoteBookSrcPathCmdArgName ??= NOTE_BOOK_SRC_PATH;
            config.NoteBookDestnPathCmdArgName ??= NOTE_BOOK_DESTN_PATH;
            config.BasicNoteBookNoteFilesDirName ??= BASIC_NOTE_BOOK_NOTE_FILES_DIR_NAME;
            config.MaxDirNameLength = config.MaxDirNameLength.Nullify() ?? MAX_DIR_NAME_LEN;
            config.NoteItemDirNamesPfx ??= NOTE_ITEM_DIR_NAMES_PFX;
            config.NoteInternalDirNamesPfx ??= NOTE_INTERNAL_DIR_NAMES_PFX;
            config.KeepFileContentsTemplate ??= KEEP_FILE_CONTENTS_TEMPLATE;
            config.KeepFileContainsNoteJson ??= false;
            config.MdFileContentsTemplate ??= MD_FILE_CONTENTS_TEMPLATE;
            config.TitleMacros ??= new Dictionary<string, string>();
        }

        /// <summary>
        /// Reads the contents of the config file, deserializes it into an object and returns that object.
        /// </summary>
        /// <returns>An object obtained by deserializing the contents of the config file.</returns>
        private ProgramConfig GetConfigCore()
        {
            string json = File.ReadAllText(ConfigFilePath);
            var programConfig = JsonSerializer.Deserialize<ProgramConfig>(json);

            return programConfig;
        }

        /// <summary>
        /// Returns the file name where the serialized configuration values should be dumped.
        /// </summary>
        /// <returns></returns>
        private string GetDumpConfigFileName()
        {
            var now = DateTime.UtcNow;
            string tmStmp = now.ToString("yyyy-MM-dd_HH-mm-ss.FFFFFFFK");

            string dumpConfigFileName = Path.GetFileNameWithoutExtension(CONFIG_FILE_NAME);
            string dumpConfigFileExtn = Path.GetExtension(CONFIG_FILE_NAME);

            dumpConfigFileName = $"{dumpConfigFileName}-{tmStmp}{dumpConfigFileExtn}";
            return dumpConfigFileName;
        }
    }
}
