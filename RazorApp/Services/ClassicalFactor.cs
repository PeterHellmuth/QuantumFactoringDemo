using System.Diagnostics;

public static class ClassicalFactor
{
    public static (List<int> factors, double time) Factor(int number)
    {
        var stopwatch = Stopwatch.StartNew();
        var factors = new List<int>();

        // Check for number of 2s that divide number
        while (number % 2 == 0)
        {
            factors.Add(2);
            number /= 2;
        }

        // Number must be odd at this point so we can skip one element (i.e., i += 2)
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            // While i divides number, add i and divide number
            while (number % i == 0)
            {
                factors.Add(i);
                number /= i;
            }
        }

        // This condition is to handle the case when number is a prime number
        // greater than 2
        if (number > 2)
        {
            factors.Add(number);
        }

        stopwatch.Stop();
        return (factors, stopwatch.Elapsed.TotalSeconds);
    }
}