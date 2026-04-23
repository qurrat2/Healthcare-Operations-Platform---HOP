namespace Healthcare.Infrastructure.Services;

internal abstract class NotImplementedServiceBase
{
    protected static Exception NotReady(string capability) =>
        new NotImplementedException($"{capability} is scaffolded but not implemented yet.");
}
