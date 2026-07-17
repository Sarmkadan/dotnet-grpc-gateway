# DateTimeUtilityValidation
The `DateTimeUtilityValidation` class provides a set of static methods for validating dates and times, ensuring they meet specific criteria such as being within a valid range or falling within business hours. These methods can be used to enforce date and time constraints in various applications, helping to prevent errors and inconsistencies.

## API
The `DateTimeUtilityValidation` class offers several static methods for validating dates and times:
- `Validate`: Overloaded methods that validate a date and time against various criteria, returning a list of error messages if the date and time are invalid.
- `ValidateForBusinessHours`: Validates a date and time to ensure it falls within business hours, returning a list of error messages if it does not.
- `IsValid`: Overloaded methods that check if a date and time meet specific validation criteria, returning a boolean indicating whether the date and time are valid.
- `EnsureValid`: Overloaded methods that validate a date and time and throw an exception if the date and time are invalid.
- `EnsureValidForBusinessHours`: Validates a date and time to ensure it falls within business hours and throws an exception if it does not.

## Usage
Here are two examples of using the `DateTimeUtilityValidation` class:
```csharp
// Example 1: Validate a date and time
var dateTime = new DateTime(2022, 12, 25, 12, 0, 0);
var errors = DateTimeUtilityValidation.Validate(dateTime);
if (errors.Count > 0)
{
    Console.WriteLine("Invalid date and time:");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
else
{
    Console.WriteLine("Date and time are valid.");
}

// Example 2: Ensure a date and time are valid for business hours
var businessHoursDateTime = new DateTime(2022, 12, 26, 9, 0, 0);
try
{
    DateTimeUtilityValidation.EnsureValidForBusinessHours(businessHoursDateTime);
    Console.WriteLine("Date and time are valid for business hours.");
}
catch (Exception ex)
{
    Console.WriteLine("Invalid date and time for business hours: " + ex.Message);
}
```

## Notes
When using the `DateTimeUtilityValidation` class, consider the following:
- The `Validate` and `IsValid` methods do not throw exceptions, instead returning error messages or a boolean indicating validity.
- The `EnsureValid` methods throw exceptions if the date and time are invalid, making them suitable for use in critical validation scenarios.
- The `ValidateForBusinessHours` and `EnsureValidForBusinessHours` methods are specifically designed for validating dates and times against business hours criteria.
- The class is designed to be thread-safe, as all methods are static and do not rely on instance state. However, the validity of dates and times may depend on the system's cultural and timezone settings, which can affect the results of validation.
