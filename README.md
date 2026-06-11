# 🛰️ SpaceAgro API — Painel Agroclimatológico

**Microsserviço de inteligência geoespacial integrado ao Oracle e dados de satélite da NASA**

> **FIAP - Global Solution 2026**  
> **Curso:** Análise e Desenvolvimento de Sistemas | Turma: 2TDSA  
> **Organização:** Space Agro

---

## 📋 Sumário

- [Sobre o Projeto](#sobre-o-projeto)
- [Contexto da Solução](#contexto-da-solução)
- [Arquitetura](#arquitetura)
  - [Visão Geral do Sistema](#visão-geral-do-sistema)
  - [Pipeline de Middleware](#pipeline-de-middleware)
  - [Injeção de Dependências](#injeção-de-dependências)
- [Stack Tecnológica](#stack-tecnológica)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Endpoints](#endpoints)
  - [GET /api/climaespacial/previsao](#get-apiclimaespacialprevisao)
  - [GET /api/climaespacial/diagnostico/{talhaoId}](#get-apiclimaespacialdiagnosticotalhaoid)
- [Modelos de Dados](#modelos-de-dados)
  - [Talhao (TB_TALHAO)](#talhao-tb_talhao)
  - [LeituraSensor (TB_LEITURA_SENSOR)](#leiturasensor-tb_leitura_sensor)
- [Fluxo do Diagnóstico](#fluxo-do-diagnóstico)
- [Algoritmo de Risco Climático](#algoritmo-de-risco-climático)
- [Integração NASA POWER](#integração-nasa-power)
- [Documentação Interativa](#documentação-interativa)
- [Como Rodar](#como-rodar)
  - [Pré-requisitos](#pré-requisitos)
  - [Passos](#passos)
  - [Configuração do Banco Oracle](#configuração-do-banco-oracle)
  - [Perfis de Execução](#perfis-de-execução)
- [Testes](#testes)
  - [Via Swagger UI](#via-swagger-ui)
  - [Via Scalar UI](#via-scalar-ui)
  - [Via curl](#via-curl)
  - [Via VS Code REST Client (.http)](#via-vs-code-rest-client-http)
  - [Exemplos de Testes](#exemplos-de-testes)
- [Scripts Úteis](#scripts-úteis)
- [Integrantes do Grupo](#integrantes-do-grupo)
- [Links](#links)
- [Licença](#licença)

---

## Sobre o Projeto

A **SpaceAgro API** é um microsserviço **.NET 8 Minimal API** que fornece inteligência geoespacial e agroclimatológica para a plataforma **AeroNet Agro**. Ela é responsável por:

1. **Consultar dados climáticos orbitais** da NASA (POWER API) para qualquer coordenada geográfica
2. **Gerar diagnósticos cruzados** combinando dados macroclimáticos (satélite) com microclimáticos (sensores IoT no solo)
3. **Produzir insights agronômicos** como risco climático, meses críticos, amplitude térmica e análise textual personalizada por cultura

A API consome dados do **Oracle FIAP** como banco de dados principal e é consumida pelo aplicativo mobile **AeroNet Agro** (React Native/Expo).

---

## Contexto da Solução

O agronegócio brasileiro enfrenta desafios crescentes relacionados às mudanças climáticas. A SpaceAgro API foi projetada para:

- **Orquestrar** dados de diferentes fontes (NASA + IoT + banco relacional) em um único endpoint de diagnóstico
- **Processar** dados brutos da NASA em insights acionáveis para o produtor rural
- **Classificar** riscos climáticos com base em thresholds agronômicos científicos
- **Recomendar** práticas de manejo baseadas em dados reais de satélite e solo

---

## Arquitetura

### Visão Geral do Sistema

```
┌────────────────────────────────────────────────────────────────┐
│                     AeroNet Agro App                           │
│                  (React Native / Expo)                         │
└────────────────────────┬───────────────────────────────────────┘
                         │ HTTP (fetch)
                         ▼
┌────────────────────────────────────────────────────────────────┐
│                    SpaceAgro API (.NET 8)                       │
│                     Minimal API — Porta 5081                    │
│                                                                │
│  ┌────────────────────────────────────────────────────────┐   │
│  │  Endpoints                                              │   │
│  │  GET /api/climaespacial/previsao?lat=&lon=             │   │
│  │  GET /api/climaespacial/diagnostico/{talhaoId}         │   │
│  └────────────────────────┬───────────────────────────────┘   │
│                           │                                    │
│  ┌──────────────────┐    │    ┌────────────────────────┐     │
│  │  NasaSpaceService │────┼────▶  NASA POWER API        │     │
│  │  (HttpClient)     │    │    │  (climatologia orbital) │     │
│  └──────────────────┘    │    └────────────────────────┘     │
│                          │                                    │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  AppDbContext (Oracle via EF Core 8)                    │  │
│  │  ┌─────────────┐  ┌───────────────────────────┐       │  │
│  │  │ TB_TALHAO   │  │ TB_LEITURA_SENSOR (IoT)   │       │  │
│  │  └─────────────┘  └───────────────────────────┘       │  │
│  └────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
```

### Pipeline de Middleware

```
Requisição HTTP
  ↓
UseCors("AllowAll") ← Política liberada para consumo mobile
  ↓
Roteamento (MapGet)
  ↓
Execução do Endpoint
  ├── GET /previsao → NasaSpaceService.BuscarPrevisaoAgroAsync()
  └── GET /diagnostico/{id} → FindAsync(Talhao) → NASA → Insights → Sensor IoT
  ↓
Resposta HTTP (JSON)
```

### Injeção de Dependências

| Serviço | Lifetime | Registro | Finalidade |
|---|---|---|---|
| `AppDbContext` | Scoped | `AddDbContext<AppDbContext>()` | Acesso Oracle via EF Core |
| `NasaSpaceService` | Transient | `AddHttpClient<NasaSpaceService>()` | Integração NASA POWER |
| `HttpClient` | Gerenciado | Via `AddHttpClient` | Chamadas HTTP externas |

---

## Stack Tecnológica

| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 8.0 | Runtime principal |
| ASP.NET Core | 8.0 | Framework web (Minimal API) |
| Oracle.EntityFrameworkCore | 8.21.240 | Provider Oracle para EF Core |
| Microsoft.AspNetCore.OpenApi | 8.0.* | Metadados OpenAPI para Minimal APIs |
| Swashbuckle.AspNetCore | 6.6.2 | Geração de documentação Swagger |
| Scalar.AspNetCore | 1.2.* | UI moderna para OpenAPI (alternativa ao Swagger) |
| Newtonsoft.Json | 13.0.4 | Serialização JSON |
| Microsoft.EntityFrameworkCore.Design | 8.0.11 | Ferramentas CLI para migrations |

---

## Estrutura de Pastas

```
SpaceAgro.DotNetApi/
├── Program.cs                              # Configuração e endpoints Minimal API (148 linhas)
├── SpaceAgro.DotNetApi.csproj              # Projeto .NET 8 (Oracle, Swagger, Scalar, OpenApi)
├── appsettings.json                        # Config produção (ConnectionStrings Oracle + Logging)
├── appsettings.Development.json            # Config desenvolvimento (apenas Logging)
├── SpaceAgro.DotNetApi.http                # Arquivo de teste HTTP (VS Code REST Client)
├── Properties/
│   └── launchSettings.json                 # Perfis: http (5081), https (7048), IIS Express
├── Data/
│   └── AppDbContext.cs                     # DbContext (DbSet<Talhao>, DbSet<LeituraSensor>)
├── Models/
│   ├── Talhao.cs                           # Entidade TB_TALHAO (7 propriedades)
│   └── LeituraSensor.cs                    # Entidade TB_LEITURA_SENSOR (6 propriedades)
├── Services/
│   └── NasaSpaceService.cs                 # Integração NASA POWER + Geração de insights (182 linhas)
└── Migrations/
    ├── 20260605135702_InitialCreate.cs      # Migration inicial (criação das tabelas)
    ├── 20260605135702_InitialCreate.Designer.cs
    └── AppDbContextModelSnapshot.cs         # Snapshot do modelo EF Core
```

---

## Endpoints

# Agora a api também roda no render : 
https://globalsolutions2026-1-dotnet.onrender.com

### GET /api/climaespacial/previsao

Consulta dados brutos da NASA POWER para coordenadas geográficas específicas.

**Parâmetros (Query String):**

| Nome | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `lat` | `double` | Sim | Latitude (ex: `-23.5505`) |
| `lon` | `double` | Sim | Longitude (ex: `-46.6333`) |

**Requisição de exemplo:**
```
GET http://localhost:5081/api/climaespacial/previsao?lat=-23.5505&lon=-46.6333
```

**Resposta (200 OK):** JSON raw retornado pela NASA POWER contendo:
- `properties.parameter.T2M` — Temperaturas médias mensais + anual (ANN)
- `properties.parameter.RH2M` — Umidade relativa média mensal + anual (ANN)

**Resposta (400 Bad Request):**
```
"A latitude e longitude são obrigatórias."
```

---

### GET /api/climaespacial/diagnostico/{talhaoId}

Endpoint principal. Retorna diagnóstico cruzado completo combinando dados de satélite (NASA), leituras de solo (IoT) e insights processados.

**Parâmetros (Route):**

| Nome | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `talhaoId` | `int` | Sim | ID do talhão no banco Oracle |

**Requisição de exemplo:**
```
GET http://localhost:5081/api/climaespacial/diagnostico/1
```

**Resposta (200 OK):**

```json
{
  "talhaoNome": "string",
  "cultura": "string",
  "coordenadas": {
    "lat": -23.55,
    "lon": -46.63
  },
  "dadosMacro_Nasa": {
    "properties": {
      "parameter": {
        "T2M": { "JAN": 25.4, "FEB": 25.3, ..., "ANN": 24.1 },
        "RH2M": { "JAN": 82.1, "FEB": 81.5, ..., "ANN": 78.3 }
      }
    }
  },
  "insightsClimaticos": {
    "temperaturaMediaAnual": 24.1,
    "umidadeMediaAnual": 78.3,
    "mesMaisQuente": "Janeiro",
    "temperaturaMesMaisQuente": 28.9,
    "mesMaisFrio": "Julho",
    "temperaturaMesMaisFrio": 18.3,
    "mesMaisSeco": "Agosto",
    "umidadeMesMaisSeco": 65.1,
    "mesMaisUmido": "Janeiro",
    "umidadeMesMaisUmido": 85.4,
    "amplitudeTermica": 10.6,
    "riscoClimatico": "BAIXO",
    "janelaCritica": "O período mais crítico tende a ser Agosto, quando a umidade média cai para 65.10%.",
    "analise": "Para a cultura Soja, a região apresenta temperatura média anual de 24.10°C e umidade média anual de 78.30%. O período mais crítico tende a ser Agosto... Recomenda-se reforçar o monitoramento hídrico nesse período."
  },
  "dadosMicro_SoloAtual": {
    "temperaturaSolo": 26.5,
    "umidadeSolo": 72.3,
    "ultimaAtualizacao": "2026-06-09T10:30:00"
  },
  "recomendacaoSistema": "Diagnóstico cruzado executado com dados NASA POWER e telemetria local do solo."
}
```

> **Nota:** O campo `dadosMicro_SoloAtual` será `null` se não houver nenhuma leitura de sensor IoT cadastrada para o dispositivo vinculado ao talhão. Nesse caso, a `recomendacaoSistema` indicará que o diagnóstico foi baseado apenas em dados macroclimáticos.

**Resposta (404 Not Found):**
```
"Talhão não encontrado."
```

---

## Modelos de Dados

### Talhao (TB_TALHAO)

Representa um talhão (lote de plantio) de um produtor rural.

**Arquivo:** `Models/Talhao.cs`

| Propriedade | Coluna Oracle | Tipo C# | Tamanho | Descrição |
|---|---|---|---|---|
| `Id` | `ID_TALHAO` | `int` | NUMBER | Chave primária (Identity) |
| `Nome` | `NOME_TALHAO` | `string` | NVARCHAR2 | Nome do talhão |
| `Cultura` | `CULTURA` | `string` | NVARCHAR2 | Cultura plantada (ex: Soja, Milho, Café) |
| `AreaHectares` | `AREA_HECTARES` | `double` | BINARY_DOUBLE | Área em hectares |
| `Latitude` | `LATITUDE` | `double` | BINARY_DOUBLE | Latitude geográfica |
| `Longitude` | `LONGITUDE` | `double` | BINARY_DOUBLE | Longitude geográfica |
| `IdProdutor` | `ID_PRODUTOR` | `int` | NUMBER | ID do produtor proprietário |

### LeituraSensor (TB_LEITURA_SENSOR)

Representa uma leitura de sensor IoT (ESP32) instalado no talhão.

**Arquivo:** `Models/LeituraSensor.cs`

| Propriedade | Coluna Oracle | Tipo C# | Tamanho | Descrição |
|---|---|---|---|---|
| `Id` | `ID_LEITURA` | `int` | NUMBER | Chave primária (Identity) |
| `Temperatura` | `TEMPERATURA` | `double` | BINARY_DOUBLE | Temperatura do solo (°C) |
| `UmidadeAr` | `UMIDADE_AR` | `double` | BINARY_DOUBLE | Umidade relativa do ar (%) |
| `UmidadeSolo` | `UMIDADE_SOLO` | `double` | BINARY_DOUBLE | Umidade do solo (%) |
| `DataHora` | `DATA_HORA` | `DateTime` | TIMESTAMP | Data/hora da leitura |
| `IdDispositivo` | `ID_DISPOSITIVO` | `int` | NUMBER | ID do dispositivo IoT (relacionado ao TalhaoId) |

---

## Fluxo do Diagnóstico

O fluxo completo do endpoint de diagnóstico (`GET /api/climaespacial/diagnostico/{talhaoId}`) segue estas etapas:

```
1. Requisição chega com talhaoId
       │
2. Busca Talhao no Oracle (EF Core FindAsync)
       │
       ├── Não encontrado → 404 Not Found
       │
       └── Encontrado →
              │
3. Chama NASA POWER com Lat/Lon do talhão
       │
4. Gera insights climáticos (risco, análise, meses críticos)
       │
5. Busca última leitura IoT do talhão (ORDER BY DataHora DESC)
       │
6. Monta objeto anônimo de resposta:
   ┌─────────────────────────────────────┐
   │ TalhaoNome, Cultura, Coordenadas    │
   │ DadosMacro_Nasa (raw NASA)          │
   │ InsightsClimaticos (processado)     │
   │ DadosMicro_SoloAtual (IoT ou null)  │
   │ RecomendacaoSistema (texto)         │
   └─────────────────────────────────────┘
       │
7. Retorna 200 OK com JSON
```

---

## Algoritmo de Risco Climático

O método `GerarInsightsClimaticos()` no `NasaSpaceService` classifica o risco climático com base nos seguintes thresholds:

```
SE (umidadeMínimaMensal < 45%) OU (temperaturaMáximaMensal > 32°C)
  → RISCO ALTO

SENÃO SE (umidadeMínimaMensal < 60%) OU (temperaturaMáximaMensal > 28°C)
  → RISCO MÉDIO

SENÃO
  → RISCO BAIXO
```

**Critérios técnicos:**

| Risco | Umidade Mínima | Temperatura Máxima | Ação Recomendada |
|---|---|---|---|
| 🔴 ALTO | < 45% | > 32°C | Monitoramento intensivo, irrigação emergencial |
| 🟡 MÉDIO | < 60% | > 28°C | Reforçar monitoramento hídrico |
| 🟢 BAIXO | ≥ 60% | ≤ 28°C | Condições favoráveis, manejo padrão |

Além do risco, o algoritmo também identifica:
- **Mês mais quente** (maior temperatura média mensal)
- **Mês mais frio** (menor temperatura média mensal)
- **Mês mais seco** (menor umidade relativa mensal)
- **Mês mais úmido** (maior umidade relativa mensal)
- **Amplitude térmica anual** (diferença entre mês mais quente e mais frio)
- **Janela crítica** (descrição textual do período mais preocupante)
- **Análise personalizada** para a cultura do talhão

---

## Integração NASA POWER

A API consome o **POWER Project** (Prediction Of Worldwide Energy Resources) da NASA, especificamente o endpoint de climatologia temporal para agricultura.

### Endpoint Consumido

```
GET https://power.larc.nasa.gov/api/temporal/climatology/point
  ?parameters=T2M,RH2M
  &community=AG
  &longitude={lon}
  &latitude={lat}
  &format=JSON
```

### Parâmetros Solicitados à NASA

| Parâmetro | Unidade | Significado |
|---|---|---|
| `T2M` | °C | Temperature at 2 Meters |
| `RH2M` | % | Relative Humidity at 2 Meters |

### Comunidade

| Código | Significado |
|---|---|
| `AG` | Agriculture — parâmetros e médias relevantes para o setor agrícola |

### Tratamento de Erros

O `NasaSpaceService` inclui tratamento robusto de erros:
- Validação de status code HTTP da NASA
- Captura e relançamento de exceções com mensagens descritivas
- User-Agent personalizado (`SpaceAgroApi/1.0`) para identificação nas requisições

---

## Documentação Interativa

A API expõe **duas interfaces** de documentação (disponíveis apenas em modo Development):

### Swagger UI
```
http://localhost:5081/swagger
```
Interface Swagger padrão com capacidade de testar os endpoints diretamente pela interface web.

### Scalar UI (Recomendada)
```
http://localhost:5081/scalar/v1
```
Interface moderna com tema **DeepSpace**, ideal para apresentações e demonstrações. Configurada com:
- Título: "SpaceAgro API - Documentação Executiva"
- Tema: DeepSpace (fundo escuro)
- Cliente padrão: JavaScript / Fetch
- Rota OpenAPI: `/openapi/v1.json`

---

## Como Rodar

### Pré-requisitos

| Ferramenta | Versão | Instalação |
|---|---|---|
| .NET SDK | 8.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Oracle Database | — | Acesso ao Oracle FIAP (`oracle.fiap.com.br`) |
| Git | Qualquer | Para clonar o repositório |

### Passos

```bash
# 1. Clone o repositório
git clone https://github.com/MuriloMacedoSilva/<repo>
cd SpaceAgro.DotNetApi

# 2. Restaure as dependências
dotnet restore

# 3. Configure a connection string (opcional — já existe em appsettings.json)
#    A ordem de precedência é:
#    1º: Variável de ambiente ORACLE_CONN_STRING
#    2º: appsettings.json → ConnectionStrings:OracleConnection
export ORACLE_CONN_STRING="User Id=rm566462;Password=030407;Data Source=oracle.fiap.com.br:1521/orcl;"

# 4. Aplique as migrations ao banco Oracle
dotnet ef database update

# 5. Execute a API
dotnet run --launch-profile http
```

A API estará disponível em:
- **Base URL:** `http://localhost:5081`
- **Swagger UI:** `http://localhost:5081/swagger`
- **Scalar UI:** `http://localhost:5081/scalar/v1`
- **OpenAPI JSON:** `http://localhost:5081/openapi/v1.json`

### Configuração do Banco Oracle

O banco Oracle da FIAP é acessado com as seguintes credenciais:

| Propriedade | Valor |
|---|---|
| Host | `oracle.fiap.com.br` |
| Porta | `1521` |
| Service Name | `orcl` |
| Usuário | `rm566462` |
| Senha | `030407` |
| Provider | Oracle.EntityFrameworkCore 8.21.240 |

**Connection String:**
```
User Id=rm566462;Password=030407;Data Source=oracle.fiap.com.br:1521/orcl;
```

A connection string pode ser configurada de duas formas:

1. **Arquivo `appsettings.json`** (fallback):
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=rm566462;Password=030407;Data Source=oracle.fiap.com.br:1521/orcl;"
  }
}
```

2. **Variável de ambiente** (prioridade máxima):
```bash
export ORACLE_CONN_STRING="User Id=rm566462;Password=030407;Data Source=oracle.fiap.com.br:1521/orcl;"
```

### Perfis de Execução

| Perfil | URL | Porta | SSL |
|---|---|---|---|
| `http` | `http://localhost:5081` | 5081 | Não |
| `https` | `https://localhost:7048` + `http://localhost:5081` | 7048 / 5081 | Sim |
| `IIS Express` | `http://localhost:52839` | 52839 | Não |

Para executar com um perfil específico:

```bash
dotnet run --launch-profile http
dotnet run --launch-profile https
```

---

## Testes

Esta seção contém instruções e exemplos para testar todos os endpoints da API.

> **Pré-requisito:** A API deve estar rodando (`dotnet run --launch-profile http`).

---

### Via Swagger UI

A interface Swagger permite testar os endpoints visualmente pelo navegador.

```
http://localhost:5081/swagger
```

**Passos:**
1. Abra `http://localhost:5081/swagger` no navegador
2. Localize o endpoint desejado na lista
3. Clique no endpoint para expandir
4. Preencha os parâmetros obrigatórios
5. Clique em **Try it out** e depois em **Execute**
6. Visualize a resposta (status code, headers, body)

---

### Via Scalar UI

Interface mais moderna e limpa, com tema escuro DeepSpace.

```
http://localhost:5081/scalar/v1
```

**Passos:**
1. Abra `http://localhost:5081/scalar/v1` no navegador
2. Navegue pelos endpoints listados à esquerda
3. Preencha os parâmetros no formulário interativo
4. Clique em **Send** para executar a requisição
5. Visualize a resposta formatada

---

### Via curl

Teste diretamente do terminal utilizando `curl`.

**Previsão (dados brutos NASA):**
```bash
curl -X GET "http://localhost:5081/api/climaespacial/previsao?lat=-23.5505&lon=-46.6333" \
  -H "Accept: application/json"
```

**Diagnóstico completo (requer talhão cadastrado no Oracle):**
```bash
curl -X GET "http://localhost:5081/api/climaespacial/diagnostico/1" \
  -H "Accept: application/json"
```

---

### Via VS Code REST Client (.http)

O projeto já inclui o arquivo `SpaceAgro.DotNetApi.http` para teste com a extensão **REST Client** do VS Code.

**Passos:**
1. Instale a extensão [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) no VS Code
2. Abra o arquivo `SpaceAgro.DotNetApi.http`
3. Clique em **Send Request** acima da requisição desejada

**Conteúdo sugerido para o arquivo:**

```http
### Previsão NASA por coordenadas
GET http://localhost:5081/api/climaespacial/previsao?lat=-23.5505&lon=-46.6333
Accept: application/json

### Diagnóstico completo do talhão 1
GET http://localhost:5081/api/climaespacial/diagnostico/1
Accept: application/json

### Diagnóstico com talhão inexistente (deve retornar 404)
GET http://localhost:5081/api/climaespacial/diagnostico/999
Accept: application/json

### Previsão sem parâmetros (deve retornar 400)
GET http://localhost:5081/api/climaespacial/previsao?lat=0&lon=0
Accept: application/json
```

---

### Exemplos de Testes

#### Cenário 1: Previsão com coordenadas válidas

**Requisição:**
```bash
curl -s "http://localhost:5081/api/climaespacial/previsao?lat=-15.7801&lon=-47.9292" \
  -H "Accept: application/json" | head -c 500
```

**Resposta esperada (200 OK):**
```json
{
  "type": "FeatureCollection",
  "features": [{
    "geometry": {},
    "properties": {
      "parameter": {
        "T2M": {
          "ANN": 24.12,
          "JAN": 26.8,
          "FEB": 26.9,
          "MAR": 26.3,
          ...
        },
        "RH2M": {
          "ANN": 78.3,
          "JAN": 82.1,
          ...
        }
      }
    }
  }]
}
```

#### Cenário 2: Diagnóstico de talhão existente

**Requisição:**
```bash
curl -s "http://localhost:5081/api/climaespacial/diagnostico/1" \
  -H "Accept: application/json" | python3 -m json.tool
```

**Resposta esperada (200 OK):**
```json
{
  "talhaoNome": "Nome do Talhão",
  "cultura": "Soja",
  "coordenadas": {
    "lat": -23.55,
    "lon": -46.63
  },
  "dadosMacro_Nasa": { ... },
  "insightsClimaticos": {
    "temperaturaMediaAnual": 24.1,
    "umidadeMediaAnual": 78.3,
    "mesMaisQuente": "Janeiro",
    "riscoClimatico": "BAIXO",
    "analise": "Para a cultura Soja, a região apresenta..."
  },
  "dadosMicro_SoloAtual": {
    "temperaturaSolo": 26.5,
    "umidadeSolo": 72.3,
    "ultimaAtualizacao": "2026-06-09T10:30:00"
  },
  "recomendacaoSistema": "Diagnóstico cruzado executado com dados NASA POWER e telemetria local do solo."
}
```

#### Cenário 3: Talhão inexistente (erro 404)

**Requisição:**
```bash
curl -s "http://localhost:5081/api/climaespacial/diagnostico/999" \
  -H "Accept: application/json"
```

**Resposta esperada (404 Not Found):**
```
"Talhão não encontrado."
```

#### Cenário 4: Parâmetros inválidos (erro 400)

**Requisição:**
```bash
curl -s "http://localhost:5081/api/climaespacial/previsao?lat=0&lon=0" \
  -H "Accept: application/json"
```

**Resposta esperada (400 Bad Request):**
```
"A latitude e longitude são obrigatórias."
```

#### Cenário 5: Diagnóstico sem sensor IoT

Quando não há leituras de sensor cadastradas para o talhão, o campo `dadosMicro_SoloAtual` retorna `null`:

```json
{
  "talhaoNome": "Nome do Talhão",
  "dadosMicro_SoloAtual": null,
  "recomendacaoSistema": "Diagnóstico baseado apenas em dados macroclimáticos da NASA. Nenhuma leitura recente do sensor IoT foi encontrada."
}
```

---

## Scripts Úteis

| Comando | Descrição |
|---|---|
| `dotnet build` | Compila o projeto |
| `dotnet run` | Executa a API (perfil padrão) |
| `dotnet run --launch-profile http` | Executa na porta 5081 |
| `dotnet ef migrations add <nome>` | Cria uma nova migration |
| `dotnet ef database update` | Aplica migrations pendentes ao banco |
| `dotnet ef migrations remove` | Remove a última migration |
| `dotnet ef database drop` | Remove o banco de dados |

---

## Integrantes do Grupo

| Nome | RM | Papel |
|---|---|---|
| Murilo Macedo Silva | RM 566462 | Full Stack / Infraestrutura |
| Lucas Lopes | RM 563544 | Backend / Integração NASA |
| Thiago Sposito Pedro Gomez | RM 562606 | Frontend Mobile |
| Vitor Madrigrano | RM 564191 | Database / Oracle |

---

## Links

- 📺 **Vídeo YouTube:** [Video.com.br](Video.com.br)
- 📂 **Repositório GitHub:** [https://github.com/MuriloMacedoSilva/GlobalSolutions2026-1-dotnet.git]()
- 🏫 **FIAP:** [https://www.fiap.com.br](https://www.fiap.com.br)
- 🛰️ **NASA POWER:** [https://power.larc.nasa.gov](https://power.larc.nasa.gov)
- 📖 **Scalar:** [http://localhost:5081/scalar/v1#tag/spaceagrodotnetapi]()
