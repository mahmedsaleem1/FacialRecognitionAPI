namespace FacialRecognitionAPI.Configuration;

public class AttendanceSettings
{
    public const string SectionName = "Attendance";

    /// <summary>
    /// Expected workday start time. Arrivals after this are marked "Late".
    /// </summary>
    public TimeOnly WorkdayStart { get; set; } = new(9, 0);

    /// <summary>
    /// Grace period in minutes after WorkdayStart before marking as late.
    /// </summary>
    public int GraceMinutes { get; set; } = 15;

    /// <summary>
    /// Check-in after this many minutes past WorkdayStart is a HalfDay.
    /// </summary>
    public int HalfDayThresholdMinutes { get; set; } = 120;

    /// <summary>
    /// Expected workday end time (for auto check-out if needed).
    /// </summary>
    public TimeOnly WorkdayEnd { get; set; } = new(17, 0);
}
