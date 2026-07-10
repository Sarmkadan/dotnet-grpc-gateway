# StringUtility

Utility class providing common string manipulation and validation methods for .NET applications, particularly in gRPC gateway scenarios where consistent string handling is required.

## API

### `string Truncate(string input, int maxLength)`

Truncates the input string to the specified maximum length. If the input is `null`, returns `null`. If the input is shorter than `maxLength`, returns the original string.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum length of the resulting string. Must be non-negative.
- **Returns**
  - The truncated string, or the original string if it is shorter than `maxLength`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `maxLength` is negative.

---

### `string NormalizeWhitespace(string input)`

Replaces any sequence of whitespace characters (spaces, tabs, newlines) with a single space and trims leading and trailing whitespace.

- **Parameters**
  - `input`: The string to normalize.
- **Returns**
  - A new string with normalized whitespace, or `null` if the input is `null`.

---

### `string ToSlug(string input)`

Converts a string into a URL-friendly slug by removing diacritics, converting to lowercase, replacing spaces with hyphens, and removing non-alphanumeric characters (except hyphens and underscores).

- **Parameters**
  - `input`: The string to convert.
- **Returns**
  - A URL-friendly slug, or `null` if the input is `null`.

---

### `string MaskSensitiveData(string input, int keepLeft, int keepRight)`

Masks sensitive data by replacing the middle portion of the string with asterisks. The first `keepLeft` and last `keepRight` characters are preserved.

- **Parameters**
  - `input`: The string to mask.
  - `keepLeft`: Number of characters to keep from the start.
  - `keepRight`: Number of characters to keep from the end.
- **Returns**
  - A masked string, or `null` if the input is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `keepLeft` or `keepRight` is negative.
  - Throws `ArgumentException` if `keepLeft + keepRight` exceeds the length of the input.

---

### `bool MatchesWildcardPattern(string input, string pattern)`

Determines whether the input string matches the given wildcard pattern (e.g., `*.txt`, `data-??`). Supports `*` (zero or more characters) and `?` (exactly one character).

- **Parameters**
  - `input`: The string to test.
  - `pattern`: The wildcard pattern to match against.
- **Returns**
  - `true` if the input matches the pattern; otherwise, `false`. Returns `false` if either input or pattern is `null`.

---

### `string ToPascalCase(string input)`

Converts a string to PascalCase by splitting on word boundaries (spaces, hyphens, underscores) and capitalizing the first letter of each word.

- **Parameters**
  - `input`: The string to convert.
- **Returns**
  - A PascalCase string, or `null` if the input is `null`.

---

### `string ToKebabCase(string input)`

Converts a string to kebab-case by replacing spaces and underscores with hyphens and converting to lowercase.

- **Parameters**
  - `input`: The string to convert.
- **Returns**
  - A kebab-case string, or `null` if the input is `null`.

---
### `bool IsValidEmail(string email)`

Validates whether the input string is a well-formed email address using a basic regex pattern.

- **Parameters**
  - `email`: The email address to validate.
- **Returns**
  - `true` if the email is valid; otherwise, `false`. Returns `false` if the input is `null`.

---
### `bool IsAlphanumeric(string input)`

Determines whether the input string contains only alphanumeric characters (letters and digits).

- **Parameters**
  - `input`: The string to test.
- **Returns**
  - `true` if the string is non-null and contains only alphanumeric characters; otherwise, `false`.

## Usage
