# StringUtilityTestsExtensions
The `StringUtilityTestsExtensions` class provides a set of static methods for performing various string operations, including repetition, character classification, line counting, and whitespace removal. These methods can be used to simplify string manipulation tasks and improve code readability.

## API
* `public static string Repeat`: Repeats a string a specified number of times. Parameters: the input string and the number of repetitions. Return value: the repeated string. Throws: `ArgumentNullException` if the input string is null.
* `public static bool IsAlphabetic`: Checks if a string contains only alphabetic characters. Parameters: the input string. Return value: true if the string is alphabetic, false otherwise. Throws: `ArgumentNullException` if the input string is null.
* `public static bool IsNumeric`: Checks if a string contains only numeric characters. Parameters: the input string. Return value: true if the string is numeric, false otherwise. Throws: `ArgumentNullException` if the input string is null.
* `public static int CountLines`: Counts the number of lines in a string. Parameters: the input string. Return value: the number of lines. Throws: `ArgumentNullException` if the input string is null.
* `public static string RemoveWhitespace`: Removes all whitespace characters from a string. Parameters: the input string. Return value: the string without whitespace. Throws: `ArgumentNullException` if the input string is null.

## Usage
```csharp
// Example 1: Repeat a string and check if it's alphabetic
string repeatedString = StringUtilityTestsExtensions.Repeat("abc", 3);
bool isAlphabetic = StringUtilityTestsExtensions.IsAlphabetic(repeatedString);
Console.WriteLine($"Repeated string: {repeatedString}, Is alphabetic: {isAlphabetic}");

// Example 2: Count lines and remove whitespace from a string
string originalString = "Hello\nWorld!";
int lineCount = StringUtilityTestsExtensions.CountLines(originalString);
string noWhitespaceString = StringUtilityTestsExtensions.RemoveWhitespace(originalString);
Console.WriteLine($"Original string: {originalString}, Line count: {lineCount}, No whitespace: {noWhitespaceString}");
```

## Notes
The `StringUtilityTestsExtensions` class provides thread-safe methods, as they only operate on input strings and do not maintain any internal state. However, the methods may throw exceptions if the input strings are null, so callers should ensure that the input strings are not null before invoking these methods. Additionally, the `Repeat` method may throw an `OutOfMemoryException` if the repeated string exceeds the maximum allowed length. The `IsAlphabetic` and `IsNumeric` methods consider only the ASCII character set, so they may not work correctly for non-ASCII characters. The `CountLines` method counts each newline character (`\n`) as a separate line, and the `RemoveWhitespace` method removes all whitespace characters, including spaces, tabs, and newline characters.
