namespace Healthcare.Domain.Constants;

public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Doctor = "DOCTOR";
    public const string Receptionist = "RECEPTIONIST";

    public static readonly string[] All = [Admin, Doctor, Receptionist];
}
