namespace Healthcare.Domain.Constants;

public static class AppointmentStatuses
{
    public const string Scheduled = "SCHEDULED";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
    public const string NoShow = "NO_SHOW";

    public static readonly string[] All = [Scheduled, Completed, Cancelled, NoShow];
}
