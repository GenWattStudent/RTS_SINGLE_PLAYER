
namespace Utils
{
    public static class Extensions
    {
        public static float GetValueFromPercent(this int value, float max)
        {
            return value * max / 100;
        }

        public static float GetValueFromPercent(this float value, float max)
        {
            return value * max / 100;
        }
    }
}

