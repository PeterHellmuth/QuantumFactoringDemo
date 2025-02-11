using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace QuantumFactoringDemo.Pages;

public class FactoringDataPoint
{
    public int Bits { get; set; }          // X-axis: Input size in bits
    public int Qubits { get; set; }        // Alternative X-axis for quantum
    public double SimulatedQuantumTime { get; set; }
    public double ClassicalTime { get; set; }
    public double TheoreticalQuantumTime { get; set; }
    public double TheoreticalClassicalTime { get; set; }
}


public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;

            // Precompute theoretical values
        foreach (var bits in new[] { 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 })
        {
            DataPoints.Add(new FactoringDataPoint
            {
                Bits = bits,
                Qubits = 2 * bits,
                TheoreticalQuantumTime = Math.Pow(bits, 3),
                TheoreticalClassicalTime = Math.Exp(1.9 * Math.Pow(bits, 1/3.0))
            });
        }
    }

    public string? QuantumResult { get; set; }
    public double QuantumTime { get; set; }
    public string? ClassicalResult { get; set; }
    public double ClassicalTime { get; set; }

    public List<FactoringDataPoint> DataPoints { get; set; } = new();
    public int Number { get; set; }

    public List<(string Label, double SimulatedQuantumTime, double RealQuantumTime, double ClassicalTime)> FactoringResults { get; set; } = new();

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

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    QuantumResult = "The number is prime and cannot be factored.";
                    QuantumTime = stopwatch.Elapsed.TotalSeconds;
                }
                else
                {
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
                    }
                    else if (result.TryGetProperty("error", out var error))
                    {
                        QuantumResult = error.GetString();
                        QuantumTime = stopwatch.Elapsed.TotalSeconds;
                    }
                    else
                    {
                        _logger.LogError("Invalid response from Python API: {Response}", result);
                        throw new Exception("Invalid response from Python API");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            QuantumResult = "An error occurred while processing the request.";
        }

        // Classical factorization
        var (factors, time, isPrime) = ClassicalFactor.Factor(number);
        ClassicalTime = time;
        if (isPrime)
        {
            ClassicalResult = "The number is prime and cannot be factored.";
        }
        else
        {
            ClassicalResult = $"({string.Join(", ", factors)})";
        }

        // Add the results to the list for charting
        FactoringResults.Add((number.ToString(), QuantumTime, QuantumTime, ClassicalTime));

        var bits = (int)Math.Log2(number) + 1;
        DataPoints.Add(new FactoringDataPoint
        {
            Bits = bits,
            Qubits = 2 * bits,  // Shor's algorithm requires ~2n qubits for n-bit number
            SimulatedQuantumTime = QuantumTime,
            ClassicalTime = ClassicalTime,
            TheoreticalQuantumTime = Math.Pow(bits, 3),       // O((log N)^3)
            TheoreticalClassicalTime = Math.Exp(1.9 * Math.Pow(bits, 1/3.0)) // GNFS
        });
    }
}