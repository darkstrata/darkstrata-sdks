using System.Diagnostics;
using DarkStrata.CredentialCheck;

// Get API key from environment variable
var apiKey = Environment.GetEnvironmentVariable("DARKSTRATA_API_KEY")
    ?? throw new InvalidOperationException("DARKSTRATA_API_KEY environment variable is required");

// Create client
using var client = new DarkStrataCredentialCheck(new ClientOptions
{
    ApiKey = apiKey
});

Console.WriteLine("DarkStrata Credential Check - Batch Check Example");
Console.WriteLine("==================================================\n");

// Define credentials to check
var credentials = new Credential[]
{
    new("user1@example.com", "password123"),
    new("user2@example.com", "securepass456"),
    new("user3@example.com", "mypassword789"),
    new("user4@example.com", "letmein2024"),
    new("user5@example.com", "admin123"),
};

Console.WriteLine($"Checking {credentials.Length} credentials...\n");

try
{
    var stopwatch = Stopwatch.StartNew();

    // 1. Hash every credential locally. Only the hashes are handed to the client;
    //    the plaintext emails and passwords never leave this process.
    var hashes = credentials.Select(c => CryptoUtils.HashCredential(c.Email, c.Password)).ToList();

    // 2. Check the hashes in one batch. The SDK groups them by 5-6 character
    //    prefix, fetches each prefix once, and compares the full hashes locally.
    //    results[i] corresponds to hashes[i] (and so to credentials[i]).
    var results = await client.CheckHashBatchAsync(hashes);

    stopwatch.Stop();

    Console.WriteLine("Results:");
    Console.WriteLine("--------");

    var compromisedCount = 0;
    for (var i = 0; i < results.Count; i++)
    {
        var status = results[i].Found ? "[COMPROMISED]" : "[OK]";
        Console.WriteLine($"  {status} {credentials[i].Email}");

        if (results[i].Found)
        {
            compromisedCount++;
        }
    }

    Console.WriteLine($"\nSummary:");
    Console.WriteLine($"  Total checked: {results.Count}");
    Console.WriteLine($"  Compromised: {compromisedCount}");
    Console.WriteLine($"  Safe: {results.Count - compromisedCount}");
    Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");

    // Show efficiency - credentials are grouped by prefix
    var uniquePrefixes = results
        .Select(r => r.Metadata.Prefix)
        .Distinct()
        .Count();

    Console.WriteLine($"\nAPI Efficiency:");
    Console.WriteLine($"  Credentials: {results.Count}");
    Console.WriteLine($"  API Calls: {uniquePrefixes} (grouped by prefix)");
}
catch (AuthenticationException)
{
    Console.WriteLine("Error: Invalid API key");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
