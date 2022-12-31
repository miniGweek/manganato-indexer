using Serilog.Events;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostBuilderContext, loggerConfiguration) =>
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File("ParseMangaNato.log");
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;
        
        var mangaDbConnectionString = config.GetSection("ConnectionStrings:MangaDatabase").Value;
        services.AddGenericRepository<ManganatoDbContext>();
        services.AddDbContext<ManganatoDbContext>(
            options =>
            options.UseSqlServer(mangaDbConnectionString));
        services.AddScoped<ParseMangaNato>();

    })
    .Build();

using var scope = host.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<ParseMangaNato>();
await service.Index();