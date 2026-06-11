using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceAgro.DotNetApi.Data;
using SpaceAgro.DotNetApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SpaceAgro API - Painel Agroclimatológico",
        Version = "v1",
        Description = "Microsserviço de inteligência geoespacial integrado ao banco Oracle e dados de satélite da NASA."
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddHttpClient<NasaSpaceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SpaceAgro API - Documentação Executiva");
        options.WithTheme(ScalarTheme.DeepSpace);
        options.WithOpenApiRoutePattern("/openapi/v1.json");
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
    });
}

app.UseCors("AllowAll");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapGet("/api/climaespacial/previsao", async (
    [FromQuery] double lat, 
    [FromQuery] double lon, 
    NasaSpaceService nasaService) =>
{
    if (lat == 0 || lon == 0) return Results.BadRequest("A latitude e longitude são obrigatórias.");
    
    var dadosNasa = await nasaService.BuscarPrevisaoAgroAsync(lat, lon);
    return Results.Ok(dadosNasa);
})
.WithName("GetPrevisaoSatelite")
.WithOpenApi();


app.MapGet("/api/climaespacial/diagnostico/{talhaoId:int}", async (
        int talhaoId,
        AppDbContext context,
        NasaSpaceService nasaService) =>
    {
        var talhao = await context.Talhoes.FindAsync(talhaoId);

        if (talhao == null)
        {
            return Results.NotFound("Talhão não encontrado.");
        }

        var dadosNasa = await nasaService.BuscarPrevisaoAgroAsync(
            talhao.Latitude,
            talhao.Longitude
        );

        var insightsClimaticos =
            nasaService.GerarInsightsClimaticos(
                dadosNasa,
                talhao.Cultura
            );

        var ultimaLeitura = await context.LeiturasSensores
            .Where(l => l.IdDispositivo == talhaoId)
            .OrderByDescending(l => l.DataHora)
            .FirstOrDefaultAsync();

        var diagnostico = new
        {
            TalhaoNome = talhao.Nome,
            Cultura = talhao.Cultura,

            Coordenadas = new
            {
                lat = talhao.Latitude,
                lon = talhao.Longitude
            },

            DadosMacro_Nasa = dadosNasa,

            InsightsClimaticos = insightsClimaticos,

            DadosMicro_SoloAtual = ultimaLeitura != null ? (object)new
            {
                temperaturaSolo = ultimaLeitura.Temperatura,
                umidadeSolo = ultimaLeitura.UmidadeSolo,
                ultimaAtualizacao = ultimaLeitura.DataHora
            } : null,

            RecomendacaoSistema =
                ultimaLeitura == null
                    ? "Diagnóstico baseado apenas em dados macroclimáticos da NASA. Nenhuma leitura recente do sensor IoT foi encontrada."
                    : "Diagnóstico cruzado executado com dados NASA POWER e telemetria local do solo."
        };

        return Results.Ok(diagnostico);
    })
    .WithName("GetDiagnosticoCompleto")
    .WithOpenApi();

app.Run();