# StringUtilityTests

The `StringUtilityTests` class is a unit test suite that validates the behavior of the `StringUtility` static utility class. Each test method exercises a specific string transformation or matching operation, ensuring correct handling of typical inputs, edge cases, and boundary conditions. The tests are designed to run under a standard test framework (e.g., xUnit or NUnit) and assert expected outcomes using assertion methods.

## API

All test methods are `public`, return `void`, and accept no parameters. They throw an assertion exception when the actual result does not match the expected value.

- **`Truncate_ValueExceedsMaxLength_ReturnsEllipsisSuffix`**  
  Verifies that when the input string is longer than the specified maximum length, the result is truncated and an ellipsis (`...`) is appended.  
  *Throws*: `AssertionException` if the truncated string does not end with `...` or exceeds the max length.

- **`Truncate_NullInput_ReturnsEmptyString`**  
  Confirms that passing `null` as the input string returns `string.Empty`.  
  *Throws*: `AssertionException` if the result is not an empty string.

- **`Truncate_ValueShorterThanMaxLength_ReturnsOriginal`**  
  Ensures that when the input string length is less than the maximum length, the original string is returned unchanged.  
  *Throws*: `AssertionException` if the result differs from the input.

- **`MaskSensitiveData_ValueLongerThanShowChars_MasksRemainder`**  
  Tests that sensitive data (e.g., a credit card number) is partially masked: the first `showChars` characters are visible, and the rest are replaced with asterisks (`*`).  
  *Throws*: `AssertionException` if the masked output does not match the expected pattern.

- **`MatchesWildcardPattern_PatternWithStar_MatchesBroadly`**  
  Validates that a wildcard pattern containing `*` matches any sequence of characters (including zero characters) at the position of the star.  
  *Throws*: `AssertionException` if the match result is incorrect.

- **`ToKebabCase_PascalCaseInput_InsertsHyphensBeforeUppercase`**  
  Checks that a PascalCase string is converted to kebab-case by inserting hyphens before each uppercase letter (except the first) and converting all letters to lowercase.  
  *Throws*: `AssertionException` if the output is not the expected kebab-case string.

- **`ToSlug_StringWithSpecialChars_ReturnsAlphanumericHyphenated`**  
  Verifies that a string containing special characters (e.g., spaces, punctuation) is converted to a URL-friendly slug: only alphanumeric characters and hyphens remain, with consecutive hyphens collapsed.  
  *Throws*: `AssertionException` if the slug does not match the expected format.

## Usage

The following examples demonstrate how the `StringUtility` methods tested by `StringUtilityTests` can be used in real-world scenarios.

### Example 1: Truncating and Masking User Data

```csharp
using static StringUtility;

public class UserProfileService
{
    public string GetDisplayName(string fullName, int maxLength)
    {
        // Truncate long names for display
        return Truncate(fullName, maxLength);
    }

    public string GetMaskedEmail(string email)
    {
        // Show only the first 3 characters, mask the rest
        return MaskSensitiveData(email, showChars: 3);
    }
}
```

### Example 2: URL Slug Generation and Route Matching

```csharp
using static StringUtility;

public class BlogController
{
    public string GenerateSlug(string title)
    {
        // Convert "Hello World! How Are You?" -> "hello-world-how-are-you"
        return ToSlug(title);
    }

    public bool IsCategoryMatch(string category, string pattern)
    {
        // Check if category matches a wildcard pattern like "tech*"
        return MatchesWildcardPattern(category, pattern);
    }
}
```

## Notes

- **Edge Cases**  
  - `Truncate` with a `null` input returns `string.Empty`; with an empty string it returns the empty string.  
  - `MaskSensitiveData` when the input length is less than or equal to `showChars` returns the original string unmasked.  
  - `MatchesWildcardPattern` supports multiple `*` characters and patterns that start or end with `*`.  
  - `ToKebabCase` handles single‑character input and strings with consecutive uppercase letters (e.g., `"XMLParser"` becomes `"xml-parser"`).  
  - `ToSlug` removes all non‑alphanumeric characters except hyphens, collapses multiple hyphens into one, and trims leading/trailing hyphens.

- **Thread‑Safety**  
  All `StringUtility` methods are static, stateless, and operate only on their input parameters. They are inherently thread‑safe and can be called concurrently from multiple threads without synchronization. The `StringUtilityTests` class itself is not designed for parallel test execution, but individual tests are independent and can be run in any order.
