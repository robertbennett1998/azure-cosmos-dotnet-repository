using InMemoryWebTier;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

CreateHostBuilder(args).Build().Run();
