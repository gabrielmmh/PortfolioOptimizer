using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortfolioFetcher;

public static class StooqApi
{
    private static readonly HttpClient _http = new();

    public static async Task DownloadAsync(
        DateTime start, DateTime end, string folder)
    {
        Directory.CreateDirectory(folder);

        string d1 = start.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        string d2 = end.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        foreach (var sym in Dow30.Concat(new[] { "^DJI" }))
        {
            string ticker = sym == "^DJI"
                ? "%5Edji"  // já escapado
                : $"{sym.ToLower()}.us";

            string url = $"https://stooq.com/q/d/l/?" +
                         $"s={ticker}&d1={d1}&d2={d2}&i=d";

            try
            {
                var csv = await _http.GetStringAsync(url);

                if (csv.StartsWith("No data", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"⚠ {sym}: Nenhum dado encontrado.");
                    continue;
                }

                string path = Path.Combine(folder, $"{sym}.csv");
                await File.WriteAllTextAsync(path, csv);
                Console.WriteLine($"✔ {sym} salvo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✖ Erro ao baixar {sym}: {ex.Message}");
            }
        }
    }

    private static readonly string[] Dow30 = {
        "AAPL","AMGN","AXP","BA","CAT","CRM","CSCO","CVX","DIS","DOW",
        "GS","HD","HON","IBM","INTC","JNJ","JPM","KO","MCD","MMM",
        "MRK","MSFT","NKE","PG","TRV","UNH","V","VZ","WBA","WMT"
    };
}
