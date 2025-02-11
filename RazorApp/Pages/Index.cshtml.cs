using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace QuantumFactoringDemo.Pages;
public class FactoringDataPoint
{
    public int Number { get; set; }        // The actual number being factored
    public int Bits { get; set; }          // X-axis: Input size in bits
    public int Qubits { get; set; }        // Alternative X-axis for quantum
    public double SimulatedQuantumTime { get; set; }
    public double ClassicalTime { get; set; }
    public double TheoreticalQuantumTime { get; set; }
    public double TheoreticalClassicalTime { get; set; }
}

public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
}

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public string? QuantumResult { get; set; }
    public double QuantumTime { get; set; }
    public string? ClassicalResult { get; set; }
    public double ClassicalTime { get; set; }

    [BindProperty]
    public List<FactoringDataPoint> DataPoints { get; set; } = new();
    public int Number { get; set; }

    public List<(string Label, double SimulatedQuantumTime, double RealQuantumTime, double ClassicalTime)> FactoringResults { get; set; } = new();

    public void OnGet()
    {
        // Load from session
        DataPoints = HttpContext.Session.GetObject<List<FactoringDataPoint>>("DataPoints") ?? new();
    }

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

                    var pFactors = result.GetProperty("factors").EnumerateArray().Select(x => x.GetInt32()).ToArray();
                    QuantumResult = $"({string.Join(", ", pFactors)})";
                    QuantumTime = stopwatch.Elapsed.TotalSeconds;
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
        DataPoints.Add(new FactoringDataPoint {
            Bits = bits,
            SimulatedQuantumTime = QuantumTime,
            ClassicalTime = ClassicalTime
        });

        HttpContext.Session.SetObject("DataPoints", DataPoints);
    }
}