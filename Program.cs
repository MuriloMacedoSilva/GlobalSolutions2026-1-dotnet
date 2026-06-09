using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceAgro.DotNetApi.Data;
using SpaceAgro.DotNetApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// CONFIGURAÇÃO DE SERVIÇOS (DI)
// =========================================================================

// Gerador de metadados OpenAPI via SwaggerGen (Compatível com .NET 8)
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

// Suporte a CORS para o Aplicativo Mobile React Native conseguir consumir a API
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// String de Conexão Inteligente para o Banco da FIAP
var connectionString = Environment.GetEnvironmentVariable("ORACLE_CONN_STRING") 
                       ?? builder.Configuration.GetConnectionString("OracleConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));

// Registra o HttpClient e o Serviço de Integração com a NASA
builder.Services.AddHttpClient<NasaSpaceService>();

var app = builder.Build();

// =========================================================================
// PIPELINE DE MIDDLEWARES HTTP
// =========================================================================

// Ativa o JSON de rotas e renderiza a interface do SCALAR no ambiente de Dev
if (app.Environment.IsDevelopment())
{
    // 1. PRIMEIRO: Inicializa o gerador de rotas do Swagger para expor o JSON de metadados
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    // 2. SEGUNDO: Ativa a interface do Scalar apontando para o arquivo criado acima de forma simplificada e limpa
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SpaceAgro API - Documentação Executiva");
        options.WithTheme(ScalarTheme.DeepSpace);
        options.WithOpenApiRoutePattern("/openapi/v1.json"); // Alimenta o Scalar com o JSON gerado pelo Swagger
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
    });
}

app.UseCors("AllowAll");

// =========================================================================
// ENDPOINTS (MINIMAL API)
// =========================================================================

// Endpoint 1: Consulta direta de dados brutos da NASA por Lat/Lon
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


// Endpoint 2: Diagnóstico Cruzado (Une dados de Satélite com dados do Solo do ESP32)
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