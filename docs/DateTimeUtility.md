# DateTimeUtility

Utility class providing static methods for common date and time calculations, primarily expressed in UTC. The helpers are intended for use in service logic where consistent, timezone‑agnostic date handling is required.

## API

### GetTodayStartUtc
- **Purpose**: Returns a `DateTime` representing the start (00:00:00.000) of the current day in UTC.
- **Parameters**: None.
- **Return Value**: `DateTime` with `Kind` set to `Utc`.
- **Exceptions**: None.

### GetTodayEndUtc
- **Purpose**: Returns a `DateTime` representing the end (23:59:59.999) of the current day in UTC.
- **Parameters**: None.
- **Return Value**: `DateTime` with `Kind` set to `Utc`.
- **Exceptions**: None.

### GetDayStartUtc
- **Purpose**: Returns the start of the day (00:00:00.000) for a supplied date, expressed in UTC.
- **Parameters**:
  - `date` (`DateTime`): The date to evaluate. If the value is not UTC, it is converted using `ToUniversalTime()`.
- **Return Value**: `DateTime` representing the beginning of that day in UTC.
- **Exceptions**: None.

### GetDayEndUtc
- **Purpose**: Returns the end of the day (23:59:59.999) for a supplied date, expressed in UTC.
- **Parameters**:
  - `date` (`DateTime`): The date to evaluate. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the end of that day in UTC.
- **Exceptions**: None.

### GetWeekStartUtc
- **Purpose**: Returns the start of the week (Monday 00:00:00.000) for the week containing the supplied date, in UTC.
- **Parameters**:
  - `date` (`DateTime`): The date to evaluate. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the Monday start of that week in UTC.
- **Exceptions**: None.

### GetMonthStartUtc
- **Purpose**: Returns the first moment of the month (day 1, 00:00:00.000) for the month containing the supplied date, in UTC.
- **Parameters**:
  - `date` (`DateTime`): The date to evaluate. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the start of that month in UTC.
- **Exceptions**: None.

### GetMonthEndUtc
- **Purpose**: Returns the last moment of the month (last day, 23:59:59.999) for the month containing the supplied date, in UTC.
- **Parameters**:
  - `date` (`DateTime`): The date to evaluate. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the end of that month in UTC.
- **Exceptions**: None.

### MillisecondsToHumanReadable
- **Purpose**: Converts a duration expressed in milliseconds to a human‑readable string (e.g., “1h 23m 45s”).
- **Parameters**:
  - `milliseconds` (`long`): The duration to format. Negative values produce a leading minus sign.
- **Return Value**: `string` describing the duration in days, hours, minutes, and seconds, omitting zero‑valued components.
- **Exceptions**: None.

### IsToday
- **Purpose**: Determines whether a supplied `DateTime` falls on the current UTC day.
- **Parameters**:
  - `date` (`DateTime`): The date to test. Non‑UTC values are converted to UTC.
- **Return Value**: `true` if the date’s UTC day matches today’s UTC day; otherwise `false`.
- **Exceptions**: None.

### DaysDifference
- **Purpose**: Calculates the number of whole days between two dates (exclusive of the end date).
- **Parameters**:
  - `start` (`DateTime`): The earlier date.
  - `end` (`DateTime`): The later date.
  Both values are treated as UTC; non‑UTC inputs are converted.
- **Return Value**: `int` representing the count of full days from `start` up to, but not including, `end`. Returns a negative value if `end` precedes `start`.
- **Exceptions**: None.

### IsSameDay
- **Purpose**: Checks whether two `DateTime` values represent the same calendar day in UTC.
- **Parameters**:
  - `date1` (`DateTime`): First date.
  - `date2` (`DateTime`): Second date.
  Both are converted to UTC before comparison.
- **Return Value**: `true` if both dates share the same year, month, and day in UTC; otherwise `false`.
- **Exceptions**: None.

### RoundDownToMinute
- **Purpose**: Truncates a `DateTime` to the start of its minute (seconds and milliseconds set to zero).
- **Parameters**:
  - `dt` (`DateTime`): The date‑time to round. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the rounded‑down instant in UTC.
- **Exceptions**: None.

### RoundDownToHour
- **Purpose**: Truncates a `DateTime` to the start of its hour (minutes, seconds, and milliseconds set to zero).
- **Parameters**:
  - `dt` (`DateTime`): The date‑time to round. Non‑UTC values are converted to UTC.
- **Return Value**: `DateTime` representing the rounded‑down instant in UTC.
- **Exceptions**: None.

### IsBusinessHours
- **Purpose**: Determines whether a supplied `DateTime` falls within defined business hours (09:00–17:00, Monday‑Friday) in UTC.
- **Parameters**:
  - `dt` (`DateTime`): The date‑time to evaluate. Non‑UTC values are converted to UTC.
- **Return Value**: `true` if the time is on a weekday and between 09:00:00 and 16:59:59.999 inclusive; otherwise `false`.
- **Exceptions**: None.

## Usage

```csharp
using static MyProject.DateTimeUtility;

// Determine if a timestamp is within today (UTC)
DateTime now = DateTime.UtcNow;
bool today = IsToday(now);
Console.WriteLine($"Is now today? {today}");

// Convert an elapsed time measured in milliseconds to a readable string
long elapsedMs = 3_661_000; // 1 hour, 1 minute, 1 second
string readable = MillisecondsToHumanReadable(elapsedMs);
Console.WriteLine($"Elapsed: {readable}"); // Output: "Elapsed: 1h 1m 1s"
```

```csharp
using static MyProject.DateTimeUtility;

// Find the start of the week for a given date (treated as UTC)
DateTime someDay = new DateTime(2025, 4, 15, 14, 30, 0, DateTimeKind.Utc);
DateTime weekStart = GetWeekStartUtc(someDay);
Console.WriteLine($"Week starts at: {weekStart:u}"); // 2025-04-14 00:00:00Z

// Check whether a timestamp occurs during business hours
DateTime meetingTime = new DateTime(2025, 4, 16, 10, 15, 0, DateTimeKind.Utc);
bool inHours = IsBusinessHours(meetingTime);
Console.WriteLine($"Meeting in business hours? {inHours}"); // true
```

## Notes

- All methods treat input `DateTime` values as UTC; if a value has a different `Kind`, it is implicitly converted using `ToUniversalTime()`. Callers should therefore pass UTC times when possible to avoid unexpected shifts.
- The “start” and “end” helpers return times with millisecond precision (`.000` and `.999`). They do not account for leap seconds; the underlying .NET `DateTime` type ignores them.
- `MillisecondsToHumanReadable` omits zero‑valued components (e.g., 0 days) and does not include fractions of a second.
- `IsBusinessHours` uses a fixed 09:00–17:00 window, Monday through Friday, irrespective of locale or organizational calendars. Adjustments for holidays or alternative workweeks require additional logic.
- Because the type contains only stateless static members, it is inherently thread‑safe; no locking is required when invoking any member from multiple threads.
