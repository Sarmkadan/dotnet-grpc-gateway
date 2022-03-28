# ConfigurationUtility

Utility class providing strongly-typed access and validation for configuration values in .NET applications, particularly for gRPC Gateway services.

## API

### `public static T GetConfigValue<T>(IConfiguration config, string key)`
Retrieves and converts a configuration value to the specified type `T`.
- **Parameters**:
  - `config`: The `IConfiguration` instance to read from.
  - `key`: The configuration key to retrieve.
- **Returns**: The deserialized value of type `T`.
- **Throws**: `InvalidCastException` if the value cannot be converted to `T`.
- **Remarks**: Supports primitive types, enums, and complex types via JSON deserialization.

### `public static bool GetBoolValue(IConfiguration config, string key)`
Retrieves a boolean configuration value.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `key`: The configuration key.
- **Returns**: `true` or `false` based on the configuration value.
- **Throws**: `InvalidCastException` if the value is not a valid boolean.

### `public static int GetIntValue(IConfiguration config, string key)`
Retrieves an integer configuration value.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `key`: The configuration key.
- **Returns**: The parsed integer value.
- **Throws**: `InvalidCastException` if the value is not a valid integer.

### `public static TimeSpan GetTimeSpanValue(IConfiguration config, string key)`
Retrieves a `TimeSpan` configuration value.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `key`: The configuration key.
- **Returns**: The parsed `TimeSpan`.
- **Throws**: `FormatException` if the value is not a valid `TimeSpan` string.
- **Remarks**: Accepts standard `TimeSpan` format strings (e.g., `"00:05:00"`).

### `public static T? GetSection<T>(IConfiguration config, string sectionName) where T : class, new()`
Retrieves and binds a configuration section to a new instance of type `T`.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `sectionName`: The name of the configuration section.
- **Returns**: A new instance of `T` populated from the section, or `null` if the section is missing.
- **Remarks**: Uses `IConfiguration.GetSection` and `Bind` internally.

### `public static bool ValidateRequiredKey(IConfiguration config, string key)`
Validates that a required configuration key exists and is not empty or whitespace.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `key`: The configuration key to validate.
- **Returns**: `true` if the key exists and has a non-empty value; otherwise, `false`.
- **Remarks**: Trims whitespace before checking for emptiness.

### `public static IEnumerable<string> GetKeysMatchingPattern(IConfiguration config, string pattern)`
Retrieves all configuration keys matching a specified pattern.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
  - `pattern`: A key pattern (e.g., `"Logging:*"`).
- **Returns**: An enumerable of matching keys.
- **Remarks**: Pattern matching is provider-specific (e.g., colon-delimited for JSON).

### `public static IEnumerable<string> GetAllKeys(IConfiguration config)`
Retrieves all configuration keys in the provided `IConfiguration`.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
- **Returns**: An enumerable of all keys.
- **Remarks**: Includes keys from all configuration providers and sections.

### `public static Dictionary<string, string?> MergeConfigurations(IEnumerable<IConfiguration> configurations)`
Merges multiple `IConfiguration` instances into a single dictionary.
- **Parameters**:
  - `configurations`: An enumerable of `IConfiguration` instances.
- **Returns**: A dictionary mapping keys to their last-seen values across all configurations.
- **Remarks**: Later configurations override earlier ones for the same key.

### `public static bool IsDevelopment(IConfiguration config)`
Determines if the application is running in the Development environment.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
- **Returns**: `true` if the `ASPNETCORENVIRONMENT` or `DOTNET_ENVIRONMENT` is `"Development"`; otherwise, `false`.

### `public static bool IsProduction(IConfiguration config)`
Determines if the application is running in the Production environment.
- **Parameters**:
  - `config`: The `IConfiguration` instance.
- **Returns**: `true` if the `ASPNETCORENVIRONMENT` or `DOTNET_ENVIRONMENT` is `"Production"`; otherwise, `false`.

## Usage

### Example 1: Reading and Validating Configuration
