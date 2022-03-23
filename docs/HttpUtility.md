# HttpUtility

`HttpUtility` is a static utility class that provides helper methods for common HTTP-related operations, including content type detection, status code classification, and header manipulation for gRPC-Web compatibility.

## API

### `public static string GetAcceptedContentType`

Returns the preferred content type from an `Accept` header value, or `null` if no acceptable content type is found.

- **Parameters**: `acceptHeader` – The `Accept` header value to parse.
- **Return value**: The highest-priority accepted content type, or `null` if none match.
- **Throws**: `ArgumentNullException` if `acceptHeader` is `null`.

---

### `public static string BuildAuthorizationHeader`

Constructs an `Authorization` header value from a bearer token.

- **Parameters**: `token` – The bearer token to include in the header.
- **Return value**: A formatted `Authorization` header string (e.g., `"Bearer <token>"`).
- **Throws**: `ArgumentNullException` if `token` is `null`.

---

### `public static string? ExtractBearerToken`

Extracts the bearer token from an `Authorization` header value.

- **Parameters**: `authorizationHeader` – The `Authorization` header value to parse.
- **Return value**: The extracted token (without the `"Bearer "` prefix), or `null` if the header is malformed or missing.
- **Throws**: `ArgumentNullException` if `authorizationHeader` is `null`.

---

### `public static HttpRequestHeaders AddGrpcWebHeaders`

Adds gRPC-Web-specific headers to an `HttpRequestHeaders` collection.

- **Parameters**:
  - `headers` – The `HttpRequestHeaders` to modify.
  - `contentType` – The content type to set (e.g., `"application/grpc-web"`).
- **Return value**: The modified `HttpRequestHeaders` for method chaining.
- **Throws**:
  - `ArgumentNullException` if `headers` or `contentType` is `null`.
  - `InvalidOperationException` if `headers` is read-only.

---

### `public static bool IsSuccessStatusCode`

Determines whether an HTTP status code is in the 2xx range.

- **Parameters**: `statusCode` – The HTTP status code to evaluate.
- **Return value**: `true` if the code is between 200 and 299 (inclusive); otherwise, `false`.

---

### `public static bool IsClientError`

Determines whether an HTTP status code is in the 4xx range.

- **Parameters**: `statusCode` – The HTTP status code to evaluate.
- **Return value**: `true` if the code is between 400 and 499 (inclusive); otherwise, `false`.

---

### `public static bool IsServerError`

Determines whether an HTTP status code is in the 5xx range.

- **Parameters**: `statusCode` – The HTTP status code to evaluate.
- **Return value**: `true` if the code is between 500 and 599 (inclusive); otherwise, `false`.

---
### `public static string GetStatusCodeCategory`

Maps an HTTP status code to its category (e.g., `"Success"`, `"ClientError"`).

- **Parameters**: `statusCode` – The HTTP status code to categorize.
- **Return value**: A string representing the category (e.g., `"Success"`, `"ClientError"`, `"ServerError"`, or `"Other"`).
- **Throws**: `ArgumentOutOfRangeException` if `statusCode` is negative.

---
### `public static bool IsJsonContentType`

Checks if a content type string represents JSON.

- **Parameters**: `contentType` – The content type to evaluate (e.g., `"application/json"`).
- **Return value**: `true` if the content type matches JSON (case-insensitive); otherwise, `false`.
- **Throws**: `ArgumentNullException` if `contentType` is `null`.

---
### `public static bool IsXmlContentType`

Checks if a content type string represents XML.

- **Parameters**: `contentType` – The content type to evaluate (e.g., `"application/xml"`).
- **Return value**: `true` if the content type matches XML (case-insensitive); otherwise, `false`.
- **Throws**: `ArgumentNullException` if `contentType` is `null`.

---
### `public static bool IsFormContentType`

Checks if a content type string represents an HTML form (`application/x-www-form-urlencoded` or `multipart/form-data`).

- **Parameters**: `contentType` – The content type to evaluate.
- **Return value**: `true` if the content type matches a form type (case-insensitive); otherwise, `false`.
- **Throws**: `ArgumentNullException` if `contentType` is `null`.

## Usage

### Example 1: Extracting and Validating a Bearer Token
