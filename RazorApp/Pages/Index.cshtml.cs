using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text.Json;

namespace QuantumFactoringDemo.Pages;

public class IndexModel : PageModel
{
    public string? QuantumResult { get; private set; }
    public double QuantumTime { get; private set; }
    public string? ClassicalResult { get; private set; }
    public double ClassicalTime { get; private set; }

    public List<int> Semiprimes {get; private set;} = new List<int>();
    public void OnGet()
    {
        // Initialize empty results
        QuantumResult = null;
        ClassicalResult = null;
        QuantumTime = 0;
        ClassicalTime = 0;

        // Load semiprimes from JSON file
        var filePath = "../Data/semiprimes.json";
        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            Semiprimes = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        ViewData["Semiprimes"] = Semiprimes;
    }

    public async Task OnPostAsync(int number)
    {
        // Quantum factoring
        var quantumWatch = Stopwatch.StartNew();
        var quantumFactors = await QuantumFactor(number);
        QuantumTime = quantumWatch.Elapsed.TotalSeconds;
        QuantumResult = string.Join(", ", quantumFactors);

        // Classical factoring
        var classicalWatch = Stopwatch.StartNew();
        var classicalFactors = ClassicalFactor(number);
        ClassicalTime = classicalWatch.Elapsed.TotalSeconds;
        ClassicalResult = string.Join(", ", classicalFactors);
    }

    private async Task<int[]> QuantumFactor(int number)
    {
        // Simulated quantum factoring
        await Task.Delay(50); // Simulate quantum processing time
        if (number % 2 == 0) return new[] { 2, number / 2 };
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0) return new[] { i, number / i };
        }
        return new[] { number };
    }

    private int[] ClassicalFactor(int number)
    {
        // Simple brute force factorization
        if (number % 2 == 0) return new[] { 2, number / 2 };
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0) return new[] { i, number / i };
        }
        return new[] { number };
    }
}