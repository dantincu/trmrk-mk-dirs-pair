using System.Runtime.CompilerServices;
using TrmrkMkFsDirsPair;

UtilsH.ExecuteProgram(() =>
{
    var pgArgsRetriever = new ProgramArgsRetriever();
    var pgArgs = pgArgsRetriever.GetProgramArgs(args);

    if (pgArgs.DumpConfigFile)
    {
        var cfgRetriever = ProgramConfigRetriever.Instance.Value;
        cfgRetriever.DumpConfig();
    }
    else
    {
        new ProgramComponent().Run(pgArgs);
    }
});