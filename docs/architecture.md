# System Architecture

The Blackfinch Lending Platform is designed using **Domain-Driven Design (DDD)** and **Clean Architecture** principles. The solution is strictly segregated into independent layers, ensuring the core business logic remains entirely agnostic of external frameworks, databases, or user interfaces.

## Backend Stack (.NET 8)

The backend is composed of four primary projects:

1. **Lending.Domain (Core Business Logic)**
   - Contains pure C# classes, records, and interfaces with **no external dependencies**.
   - Contains LoanApplication (Aggregate Root), LoanDecision (Value Object), and RuleResult.
   - The decision engine (LoanDecisionEngine) and rules (ILoanRule) live here.
   - Exact decimal mathematics are used for LTV calculations to prevent floating-point anomalies.

2. **Lending.Application (Use Cases & Services)**
   - Orchestrates domain objects to satisfy specific use cases (e.g., SubmitApplicationAsync).
   - Defines interfaces for external concerns, primarily the ILoanApplicationRepository.
   - Contains all Data Transfer Objects (DTOs) used for API communication.

3. **Lending.Infrastructure (Data & Persistence)**
   - Implements ILoanApplicationRepository using **Entity Framework Core (EF Core)**.
   - Uses **SQLite** as a localized, persistent datastore.
   - Utilizes EF Core ValueConverter configurations to serialize complex domain Value Objects (like LoanDecision) directly into JSON strings within the TEXT columns of SQLite, keeping the domain schema clean from relational mapping artifacts.
   - Uses AsAsyncEnumerable() for highly efficient, streaming memory calculations when aggregating Dashboard metrics.

4. **Lending.Api (Presentation & Routing)**
   - The ASP.NET Core Web API layer.
   - Exposes REST endpoints via thin Controllers that delegate immediately to the Application layer.
   - Configured with ExceptionHandlingMiddleware to catch domain validation violations (ArgumentOutOfRangeException) and transform them into RFC-compliant ProblemDetails responses. Unhandled 500 exceptions are obscured in production.

## Frontend Stack (React 18 + Vite)

The frontend is a strictly isolated Single Page Application (SPA) built to consume the API.

- **Vite & TypeScript:** Provides rapid HMR compilation and strict type-safety mapping directly to the C# DTOs.
- **Tailwind CSS:** Utility-first styling for responsive layout design.
- **Service Isolation:** All network fetching logic is sequestered inside src/services/api.ts. React components never invoke etch() directly.
- **Single Source of Truth:** The UI contains **zero** business logic regarding lending rules. The frontend simply maps the decision.ruleResults returned by the API into green/red UI checkmarks, ensuring the backend maintains undisputed authority.
