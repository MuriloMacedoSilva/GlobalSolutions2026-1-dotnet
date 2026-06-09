using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceAgro.DotNetApi.Services
{
    public class NasaSpaceService
    {
        private readonly HttpClient _httpClient;

        public NasaSpaceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JsonElement> BuscarPrevisaoAgroAsync(
            double latitude,
            double longitude
        )
        {
            string latStr = latitude.ToString("F4", CultureInfo.InvariantCulture);
            string lonStr = longitude.ToString("F4", CultureInfo.InvariantCulture);

            string url =
                $"https://power.larc.nasa.gov/api/temporal/climatology/point" +
                $"?parameters=T2M,RH2M" +
                $"&community=AG" +
                $"&longitude={lonStr}" +
                $"&latitude={latStr}" +
                $"&format=JSON";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "SpaceAgroApi/1.0");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string erroCorpo = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"NASA retornou status {response.StatusCode}. Detalhe: {erroCorpo}"
                    );
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonString);

                return doc.RootElement.Clone();
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao buscar dados da NASA: {ex.Message}");
            }
        }

        public object GerarInsightsClimaticos(
            JsonElement dadosNasa,
            string cultura
        )
        {
            var meses = new[]
            {
                "JAN", "FEB", "MAR", "APR", "MAY", "JUN",
                "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
            };

            var nomesMeses = new Dictionary<string, string>
            {
                { "JAN", "Janeiro" },
                { "FEB", "Fevereiro" },
                { "MAR", "Março" },
                { "APR", "Abril" },
                { "MAY", "Maio" },
                { "JUN", "Junho" },
                { "JUL", "Julho" },
                { "AUG", "Agosto" },
                { "SEP", "Setembro" },
                { "OCT", "Outubro" },
                { "NOV", "Novembro" },
                { "DEC", "Dezembro" }
            };

            var parametros = dadosNasa
                .GetProperty("properties")
                .GetProperty("parameter");

            var temperaturas = parametros.GetProperty("T2M");
            var umidades = parametros.GetProperty("RH2M");

            double temperaturaMediaAnual =
                Math.Round(temperaturas.GetProperty("ANN").GetDouble(), 2);

            double umidadeMediaAnual =
                Math.Round(umidades.GetProperty("ANN").GetDouble(), 2);

            string mesMaisQuente = meses
                .OrderByDescending(m => temperaturas.GetProperty(m).GetDouble())
                .First();

            string mesMaisFrio = meses
                .OrderBy(m => temperaturas.GetProperty(m).GetDouble())
                .First();

            string mesMaisSeco = meses
                .OrderBy(m => umidades.GetProperty(m).GetDouble())
                .First();

            string mesMaisUmido = meses
                .OrderByDescending(m => umidades.GetProperty(m).GetDouble())
                .First();

            double tempMax =
                Math.Round(temperaturas.GetProperty(mesMaisQuente).GetDouble(), 2);

            double tempMin =
                Math.Round(temperaturas.GetProperty(mesMaisFrio).GetDouble(), 2);

            double umidadeMin =
                Math.Round(umidades.GetProperty(mesMaisSeco).GetDouble(), 2);

            double umidadeMax =
                Math.Round(umidades.GetProperty(mesMaisUmido).GetDouble(), 2);

            double amplitudeTermica =
                Math.Round(tempMax - tempMin, 2);

            string riscoClimatico;

            if (umidadeMin < 45 || tempMax > 32)
            {
                riscoClimatico = "ALTO";
            }
            else if (umidadeMin < 60 || tempMax > 28)
            {
                riscoClimatico = "MÉDIO";
            }
            else
            {
                riscoClimatico = "BAIXO";
            }

            string janelaCritica =
                $"O período mais crítico tende a ser {nomesMeses[mesMaisSeco]}, " +
                $"quando a umidade média cai para {umidadeMin:F2}%.";

            string analise =
                $"Para a cultura {cultura}, a região apresenta temperatura média anual de " +
                $"{temperaturaMediaAnual:F2}°C e umidade média anual de {umidadeMediaAnual:F2}%. " +
                $"{janelaCritica} Recomenda-se reforçar o monitoramento hídrico nesse período.";

            return new
            {
                temperaturaMediaAnual,
                umidadeMediaAnual,

                mesMaisQuente = nomesMeses[mesMaisQuente],
                temperaturaMesMaisQuente = tempMax,

                mesMaisFrio = nomesMeses[mesMaisFrio],
                temperaturaMesMaisFrio = tempMin,

                mesMaisSeco = nomesMeses[mesMaisSeco],
                umidadeMesMaisSeco = umidadeMin,

                mesMaisUmido = nomesMeses[mesMaisUmido],
                umidadeMesMaisUmido = umidadeMax,

                amplitudeTermica,
                riscoClimatico,
                janelaCritica,
                analise
            };
        }
    }
}