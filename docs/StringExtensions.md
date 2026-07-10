# StringExtensions
The `StringExtensions` class provides a set of static methods for extending the functionality of the `string` type in C#. These methods offer various string manipulation and validation capabilities, making it easier to work with strings in a .NET application. The class includes methods for generating SHA-256 hashes, creating slugs, truncating strings, validating IP addresses, and matching patterns.

## API
* `public static string ToSha256Hash(string input)`: Generates a SHA-256 hash from the input string. The method takes a string as input and returns the corresponding SHA-256 hash as a string. It does not throw any exceptions.
* `public static string ToSlug(string input)`: Converts the input string into a slug. The method takes a string as input and returns the slug as a string. It does not throw any exceptions.
* `public static string Truncate(string input, int length)`: Truncates the input string to the specified length. The method takes a string and an integer as input and returns the truncated string. It throws an `ArgumentException` if the length is less than 0.
* `public static bool IsValidIpAddress(string ip_address)`: Checks if the input string is a valid IP address. The method takes a string as input and returns a boolean indicating whether the IP address is valid. It does not throw any exceptions.
* `public static bool MatchesPattern(string input, string pattern)`: Checks if the input string matches the specified pattern. The method takes two strings as input and returns a boolean indicating whether the input matches the pattern. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `StringExtensions` class:
```csharp
// Example 1: Generating a SHA-256 hash and creating a slug
string originalString = "Hello, World!";
string sha256Hash = StringExtensions.ToSha256Hash(originalString);
string slug = StringExtensions.ToSlug(originalString);
Console.WriteLine($"SHA-256 Hash: {sha256Hash}");
Console.WriteLine($"Slug: {slug}");

// Example 2: Truncating a string and validating an IP address
string longString = "This is a very long string that needs to be truncated.";
string truncatedString = StringExtensions.Truncate(longString, 20);
string ipAddress = "192.168.1.1";
bool isValidIp = StringExtensions.IsValidIpAddress(ipAddress);
Console.WriteLine($"Truncated String: {truncatedString}");
Console.WriteLine($"Is IP Address Valid: {isValidIp}");
```

## Notes
When using the `StringExtensions` class, note that the `ToSha256Hash` method is case-sensitive and will produce different hashes for the same string with different casing. The `ToSlug` method will remove non-alphanumeric characters and convert the string to lowercase. The `Truncate` method will throw an exception if the specified length is less than 0. The `IsValidIpAddress` method will return false for IPv6 addresses. The `MatchesPattern` method uses regular expressions to match the pattern, so be cautious when using user-inputted patterns to avoid potential security vulnerabilities. The `StringExtensions` class is thread-safe, as all methods are static and do not rely on any instance state.
