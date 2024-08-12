using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using TrmrkMkFsDirsPair;

var services = new ServiceCollection();
services.AddScoped<IConsoleMsgPrinter, ConsoleMsgPrinter>();
services.AddScoped<IExpressionTextParser, ExpressionTextParser>();
services.AddScoped<ProgramConfigRetriever>();
services.AddScoped<ProgramArgsRetriever>();
services.AddScoped<ConsoleMsgPrinter>();
services.AddScoped<ProgramComponent>();

var svcProv = services.BuildServiceProvider();

UtilsH.ExecuteProgram(() =>
{
    var pgComponent = svcProv.GetRequiredService<ProgramComponent>();
    pgComponent.Run(args);
});