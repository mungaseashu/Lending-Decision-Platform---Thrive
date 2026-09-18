# Test Coverage Report

The repository enforces stringent testing requirements yielding **88 Passing xUnit Tests** across the solution.

## 1. Domain Tests (81 Tests)
The core business engine is subjected to rigorous permutation testing:
- **Boundary Precision:** Testing LTVs exactly falling on 59.999%, 60.00%, and 60.001% to ensure fractional precision behaves as mathematically expected.
- **Rule Independence:** Unit testing GeneralLimitsRule, SmallLoanRule, and LargeLoanRule independently using Moq frameworks.
- **Decision Engine Aggregation:** Verifying that a loan failing multiple distinct rules accumulates all failure strings concurrently instead of short-circuiting, providing maximum feedback to the end user.
- **Simulator Algebra:** The DecisionSimulator logic is tested against backwards calculations to prove it accurately recommends mathematically viable alternatives to declined applicants.

## 2. Infrastructure & Repository Tests (3 Tests)
- Uses Microsoft.EntityFrameworkCore.InMemory (SQLite in-memory mode) to validate database schemas.
- Ensures the ValueConverter reliably serializes complex LoanDecision aggregate graphs into JSON structures and hydrates them accurately on reads.
- Validates that streaming operations like AsAsyncEnumerable() effectively compute ledger sums.

## 3. Application & Service Tests (1 Test)
- Validates DTO mapping constraints, ensuring EF objects successfully translate into frontend-compatible Data Transfer Objects.

## 4. API Integration Tests (4 Tests)
- Utilizes WebApplicationFactory to spin up a sandboxed runtime of the Kestrel server in-memory.
- Sends actual HTTP POST and GET requests through the routing and middleware pipelines.
- Verifies HTTP 201 Created outputs.
- Verifies HTTP 400 Bad Request triggers when DTO validation constraints are breached.
