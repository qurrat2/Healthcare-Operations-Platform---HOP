if (args.Length == 0)
{
    Console.WriteLine("Provide one or more username=password pairs.");
    Console.WriteLine("Example: dotnet run --project tools/PasswordHashGenerator/PasswordHashGenerator.csproj -- admin=ChangeMe123");
    return;
}

foreach (var argument in args)
{
    var separatorIndex = argument.IndexOf('=');
    if (separatorIndex <= 0 || separatorIndex == argument.Length - 1)
    {
        Console.WriteLine($"Skipping invalid input: {argument}");
        continue;
    }

    var username = argument[..separatorIndex];
    var password = argument[(separatorIndex + 1)..];
    Console.WriteLine($"{username}|{BCrypt.Net.BCrypt.HashPassword(password)}");
}
