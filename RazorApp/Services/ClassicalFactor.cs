public static class ClassicalFactor
{
    public static (int? factor, double time) Factor(int number)
    {
        var start = DateTime.Now;
        if (number % 2 == 0) return (2, (DateTime.Now - start).TotalSeconds);
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0)
                return (i, (DateTime.Now - start).TotalSeconds);
        }
        return (null, (DateTime.Now - start).TotalSeconds);
    }
}