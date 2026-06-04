using RDPCrystalRestService.Options;
using RDPCrystalRestService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<AppRuntimeOptions>(builder.Configuration.GetSection(AppRuntimeOptions.SectionName));
builder.Services.Configure<IrisOptions>(builder.Configuration.GetSection(IrisOptions.SectionName));
builder.Services.Configure<CredentialsOptions>(builder.Configuration.GetSection(CredentialsOptions.SectionName));

builder.Services.AddScoped<RdpValidateService>();
builder.Services.AddScoped<IrisDbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
