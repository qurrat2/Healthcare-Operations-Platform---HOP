# Healthcare.Api.Tests

This project is where the backend safety net should live.

Suggested next layers:

- contract tests
  Keep simple checks around request and response DTO shape.
- service tests
  Validate business rules in services such as appointment availability and authorization.
- integration tests
  Start the API with a test host and verify real HTTP behavior.
- database-backed tests
  Exercise EF Core mappings and SQL Server behavior for the most critical flows.

Good first additions:

- login succeeds for a valid seeded user
- login fails for an invalid password
- appointment creation fails outside doctor availability
- appointment creation succeeds for a valid available slot
- doctor cannot access another doctor's prescription
