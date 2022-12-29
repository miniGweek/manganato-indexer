using Microsoft.EntityFrameworkCore;

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
        var config = hostContext.Configuration;
        
        var mangaDbConnectionString = config.GetSection("ConnectionStrings:MangaDatabase").Value;
        services.AddSingleton<ParseMangaNato>();
        services.AddDbContext<ManganatoContext>(
            options =>
            options.UseSqlServer(mangaDbConnectionString));
    })
    .Build();

await host.Services.GetRequiredService<ParseMangaNato>().Index();