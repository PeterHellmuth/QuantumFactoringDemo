using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuantumFactoringDemo.Pages;

public class IndexModel : PageModel
{
    public string? QuantumResult { get; set; }
    public double QuantumTime { get; set; }
    public int? ClassicalResult { get; set; }
    public double ClassicalTime { get; set; }
    public int Number { get; set; }

    public async Task OnPostAsync(int number)
    {
        // Call Python API
        using (var client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync("http://quantum-python:5000/shor-factor", new { number });
            var result = await response.Content.ReadFromJsonAsync<dynamic>() ?? throw new Exception("Failed to call Python API");
            QuantumResult = result.result.ToString();
            QuantumTime = result.time;
        }

        // Classical
        var (factor, time) = ClassicalFactor.Factor(number);
        ClassicalResult = factor;
        ClassicalTime = time;
    }
}