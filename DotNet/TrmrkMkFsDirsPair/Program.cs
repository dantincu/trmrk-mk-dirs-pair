using System.Runtime.CompilerServices;
using TrmrkMkFsDirsPair;

UtilsH.ExecuteProgram(() =>
{
    var pgArgsRetriever = new ProgramArgsRetriever();
    var pgArgs = pgArgsRetriever.GetProgramArgs(args);
    var cfgRetriever = ProgramConfigRetriever.Instance.Value;

    if (pgArgs.PrintHelp)
    {
        pgArgsRetriever.PrintHelp(cfgRetriever.Config);
    }
    else
    {
        if (pgArgs.DumpConfigFile)
        {
            cfgRetriever.DumpConfig(pgArgs.DumpConfigFileName);
        }

        new ProgramComponent(pgArgsRetriever).Run(pgArgs);
    }
});