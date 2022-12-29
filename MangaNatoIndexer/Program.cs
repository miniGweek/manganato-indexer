using MangaNatoIndexer;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostBuilderContext, loggerConfiguration) =>
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("ParseMangaNato.log");
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ParseMangaNato>();
    })
    .Build();

await host.Services.GetRequiredService<ParseMangaNato>().Index();