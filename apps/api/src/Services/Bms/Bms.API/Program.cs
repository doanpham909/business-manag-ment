using Bms.API.Extensions;
using Bms.Application;
using Bms.Infrastructure;
using Bms.Persistence;
using Bms.Presentation;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.HealthChecks;
using Serilog;

// Logger tạm để ghi được lỗi xảy ra trong lúc khởi động (trước khi host build xong)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Composition root — mỗi tầng tự đăng ký DI của mình
    builder.Services
        .AddApplication()
        .AddPipelineBehaviors()
        .AddInfrastructure()
        .AddPersistence(builder.Configuration)
        .AddPresentation()
        .AddGlobalExceptionHandler()
        .AddApiCors(builder.Configuration)
        .AddApiDocumentation()
        .AddApiJsonDefaults();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSecurityHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseSerilogRequestLogging(options =>
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms");

    app.UseCors(CorsExtensions.PolicyName);

    // Phase 0 chưa có authentication; khi thêm JWT: app.UseAuthentication(); app.UseAuthorization();
    app.MapAppHealthEndpoint("/health");
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException là hành vi bình thường khi chạy `dotnet ef` — không log Fatal
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
