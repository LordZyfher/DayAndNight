
public struct CycleUnitConvert
{
    public static float ConvertUnit(TimeScale timeScale, float unit = 1)
    {
        switch (timeScale)
        {
            case TimeScale.Second:
                return unit;

            case TimeScale.Minute:
                return Minute(unit);

            case TimeScale.Hour:
                return Hour(unit);

            case TimeScale.Day:
                return Day(unit);

            default: return unit;
        }
    }

    public static float Minute(float unit = 1)
    {
        return unit * 60;
    }

    public static float Hour(float unit = 1)
    {
        return Minute(unit) * 60;
    }

    public static float Day(float unit = 1, int hoursInDay = 24)
    {
        return Hour(unit) * hoursInDay;
    }
}
