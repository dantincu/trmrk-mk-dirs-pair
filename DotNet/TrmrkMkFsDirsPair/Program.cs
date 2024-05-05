using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using TrmrkMkFsDirsPair;

var services = new ServiceCollection();
services.AddSingleton<IConsoleMsgPrinter, ConsoleMsgPrinter>();
services.AddSingleton<IExpressionTextParser, ExpressionTextParser>();
services.AddSingleton<ProgramConfigRetriever>();
services.AddSingleton<ProgramArgsRetriever>();
services.AddSingleton<ConsoleMsgPrinter>();
services.AddSingleton<ProgramComponent>();

var svcProv = services.BuildServiceProvider();

UtilsH.ExecuteProgram(() =>
{
    var pgArgsRetriever = svcProv.GetRequiredService<ProgramArgsRetriever>();
    var pgArgs = pgArgsRetriever.GetProgramArgs(args);
    var cfgRetriever = svcProv.GetRequiredService<ProgramConfigRetriever>();

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

        var pgComponent = svcProv.GetRequiredService<ProgramComponent>();
        pgComponent.Run(pgArgs);
    }
});