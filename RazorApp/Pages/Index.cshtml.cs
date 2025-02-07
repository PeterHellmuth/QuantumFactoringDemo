using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace QuantumFactoringDemo.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public string? QuantumResult { get; set; }
    public double QuantumTime { get; set; }
    public double QuantumExtrapolatedTime { get; set; }
    public string? ClassicalResult { get; set; }
    public double ClassicalTime { get; set; }
    public double ClassicalExtrapolatedTime { get; set; }
    public int Number { get; set; }

    public async Task OnPostAsync(int number)
    {
        try
        {
            _logger.LogInformation("Starting API call to Python service with number: {Number}", number);

            // Call Python API
            using (var client = new HttpClient())
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await client.PostAsJsonAsync("http://quantum-python:5000/shor-factor", new { number });
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to call Python API. Status code: {StatusCode}", response.StatusCode);
                    throw new Exception("Failed to call Python API");
                }
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                _logger.LogInformation("Received response from Python API: {Response}", result);

                if (result.TryGetProperty("factors", out var qFactors) && qFactors.GetArrayLength() >= 2)
                {
                    QuantumResult = $"({string.Join(", ", qFactors.EnumerateArray().Select(f => f.GetInt32()))})";
                    QuantumTime = stopwatch.Elapsed.TotalSeconds;

                    // Extrapolate time for cracking RSA (e.g., factoring a 2048-bit number)
                    int rsaBits = 2048;
                    int currentBits = (int)Math.Log2(number) + 1;
                    QuantumExtrapolatedTime = QuantumTime * (rsaBits / (double)currentBits);
                }
                else
                {
                    _logger.LogError("Invalid response from Python API: {Response}", result);
                    throw new Exception("Invalid response from Python API");
                }
            }

            // Classical
            var (factors, time) = ClassicalFactor.Factor(number);
            ClassicalResult = factors != null ? string.Join(", ", factors) : "None";
            ClassicalTime = time;

            // Extrapolate time for cracking RSA (e.g., factoring a 2048-bit number)
            int rsaBitsClassical = 2048;
            int currentBitsClassical = (int)Math.Log2(number) + 1;
            ClassicalExtrapolatedTime = ClassicalTime * (rsaBitsClassical / (double)currentBitsClassical);

            _logger.LogInformation("Classical factorization result: {Factors}, time: {Time}", ClassicalResult, time);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            throw;
        }
    }
}