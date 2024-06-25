using System.Diagnostics;

namespace SynthriderzMapUpdateTool
{
    public static class TimeMeasurement
    {
        public static Stopwatch Timer { get; private set; } = new();

        public static void Start()
        {
            Timer = new Stopwatch();
            Timer.Start();
        }

        public static string ElapsedMilliseconds()
        {
            Timer.Stop();
            return $"~ took {Timer.ElapsedMilliseconds} ms";
        }
    }
}
