# AI Development Log

## Phase 3: Backend Foundation

**Objective:** Scaffold the Clean Architecture solution for the Lending Decision Platform with .NET 8.

**Steps Taken (Autonomous Execution):**
1. **Environment Verification:** Checked for the `dotnet` CLI. It was missing from the standard PATH.
2. **SDK Installation:** Autonomously downloaded and installed the .NET 8 SDK locally into `.dotnet` using `dotnet-install.ps1`.
3. **Project Scaffolding:** Used the locally installed `.NET SDK` to construct the entire backend directory tree (`backend/src/` and `backend/tests/`) and created the following components:
   - `Lending.sln`
   - `Lending.Api` (Web API)
   - `Lending.Application` (Class Library)
   - `Lending.Domain` (Class Library)
   - `Lending.Infrastructure` (Class Library)
   - `Lending.Domain.Tests`, `Lending.Application.Tests`, `Lending.Api.Tests` (xUnit)
4. **Dependency Graph Configuration:** Programmatically added the projects to the solution and established strict references following clean architecture principles (e.g., API depends on Application/Infrastructure; Application depends on Domain; Domain has no dependencies).
5. **Package Management:** Installed `Microsoft.EntityFrameworkCore.Sqlite` in the Infrastructure project and `Microsoft.EntityFrameworkCore.Design` in the API project (explicitly setting version `8.0.*` to avoid framework incompatibilities).
6. **API Configuration:** Modified `Program.cs` to remove the default `WeatherForecast` boilerplate and add a `/health` endpoint. Kept Swagger and Dependency Injection enabled.
7. **Build and Test Verification:** Ran `dotnet build` (0 warnings, 0 errors) and `dotnet test` (all 3 test suites passed successfully).

**Issues Handled:**
- Detected `dotnet` was not present, corrected by running a local PowerShell install script.
- The initial `dotnet add package` grabbed EF Core version 9, which broke the build against .NET 8. I automatically detected this, removed the packages, and re-added them specifically pinned to `8.0.*`.

**Next Steps:**
Awaiting instructions for Phase 4 to begin moving the Domain logic we discussed into this active scaffolding and wiring up the API.

## Phase 4: Domain Model

**Objective:** Implement the pure Domain Model for the Lending Decision Platform in the workspace.

**Steps Taken (Autonomous Execution):**
1. **Domain Creation:** Created clean, framework-agnostic models in `Lending.Domain`:
   - `LoanStatus` (Enum)
   - `RuleResult` (Record value object)
   - `LoanDecision` (Record value object with factory methods)
   - `LoanApplication` (Entity with encapsulation and validation constraints)
2. **Validation Logic:** Secured `LoanApplication` by strictly using decimals for monetary/LTV values, and throwing exceptions on invalid creation states (e.g. credit score out of bounds, 0/negative asset amounts).
3. **Domain Tests:** Wrote robust xUnit tests in `Lending.Domain.Tests` checking valid application setup, LTV calculations, and verifying that appropriate exceptions are thrown on boundary breaches.
4. **Build and Test Verification:** Ran `dotnet build` and `dotnet test` across the solution. All projects compiled successfully with 0 warnings, and all 9 tests passed.

**Design Choices:**
- **Immutability and Encapsulation:** Used immutable `record` types for `LoanDecision` and `RuleResult` to prevent state side-effects. Enforced a private default constructor and static factory methods on the `LoanApplication` entity to preserve invariant domain rules and accommodate EF Core hydration without compromising the model.
- **Decoupled from Frameworks:** Avoided HTTP/EF Core artifacts strictly within the domain.


## Phase 5: Lending Decision Engine

**Objective:** Implement the core lending decision engine using independent rule abstractions and strict testing bounds.

**Steps Taken (Autonomous Execution):**
1. **Model Refactoring:** Extended `RuleResult` and `LoanDecision` to include calculated LTVs, rule names, and collections of reasons to satisfy the requirement for structured decision output.
2. **Rule Abstraction:** Created `ILoanRule` interface.
3. **Rule Implementations:**
   - `GeneralLimitsRule`: Handles bounds <100k and >1.5m.
   - `LargeLoanRule`: Handles loans >= 1M (LTV <= 60% AND Score >= 950).
   - `SmallLoanRule`: Handles loans < 1M utilizing strict inclusive/exclusive bounds (e.g. 0 <= LTV < 60, 60 <= LTV < 80, etc).
4. **Engine Orchestration:** Developed `LoanDecisionEngine` to iterate through all rules. It accumulates *all* failure reasons (preventing short-circuiting) so that multiple failures on a single application are all returned to the user.
5. **Comprehensive Testing:** Added `LoanDecisionEngineTests` covering exactly 100k, exactly 1.5m, multiple failures per rule, the 1M special case, and asset bounds testing.
6. **Build & Test:** Re-ran `dotnet test`. All tests passed on the first run.

**Design Choices & Assumptions:**
- **Evaluated ALL Rules:** Rather than short-circuiting on the first failure, the engine evaluates all rules to return a complete set of human-readable reasons (e.g., if a 50k loan fails the general limit, and also fails the small loan score limit, both reasons are reported).
- **Exclusive Bounds:** Enforced exactly what was assumed in Phase 1 (e.g., exactly 60% LTV falls into the 60-80 bucket for small loans).


## Phase 6: Comprehensive Business Logic Testing

**Objective:** Rigorously stress test the Domain Rules against all possible boundary conditions and invalid states without assuming correctness.

**Steps Taken (Autonomous Execution):**
1. **Test Strategy:** Built a massive, parameterized xUnit `[Theory]` suite (`ComprehensiveDecisionTests.cs`) hitting over 40 exact boundary vectors required by the specifications.
2. **Loan Amount Exhaustion:** Checked 1 under, exactly on, 1 over for 100k, 1M, and 1.5M.
3. **LTV vs. Score Exhaustion (Small Loans):** Iterated precisely around 60%, 80%, and 90% LTV, varying the score by +/- 1 point around 750, 800, and 900 respectively.
4. **High Value Exhaustion:** Tested >1M boundaries explicitly checking valid, single-failure, and dual-failure combinations.
5. **Validation Testing:** Confirmed `ArgumentOutOfRangeException` triggers precisely on out-of-bounds credit scores (0, 1000) and unphysical financial values (0 asset, negative asset, zero loan, negative loan).
6. **Matrix Generation:** Generated `docs/test-coverage.md` summarizing the test matrix.
7. **Execution:** Ran `dotnet test`. All 73 tests compiled and passed perfectly without requiring downstream implementation tweaks. This validated that my strict inclusive/exclusive decimal logic in Phase 5 was correct.


## Phase 7: Persistence

**Objective:** Implement an EF Core SQLite persistence layer to store LoanApplications with their fully evaluated LoanDecisions while keeping the domain completely independent of EF Core attributes.

**Steps Taken (Autonomous Execution):**
1. **Repository Interface:** Created `ILoanApplicationRepository` inside `Lending.Application` to ensure inversion of control.
2. **Database Context:** Configured `LendingDbContext` in `Lending.Infrastructure`.
3. **EF Core Configuration:** Used `IEntityTypeConfiguration<LoanApplication>` to map the entity. 
   - *Design Choice:* To persist the highly structured value object `LoanDecision` without polluting the domain with EF-specific shadow properties, I utilized EF Core Value Converters to serialize/deserialize `LoanDecision` into a single JSON `TEXT` column in SQLite. This natively handles the complex nested generic lists (RuleResults, Reasons).
   - *Design Choice:* `RuleResult` and `LoanDecision` required `init` setters and a `[JsonConstructor]`, successfully satisfying System.Text.Json mapping while keeping immutability strictly intact.
4. **Migrations & Startup:** Installed the EF CLI autonomously, generated the `InitialCreate` migration, and updated `Program.cs` to execute `dbContext.Database.Migrate()` at application startup.
5. **Integration Tests:** Created a new xUnit project (`Lending.Infrastructure.Tests`) evaluating in-memory SQLite mapping. 
   - Verified that a generated domain application mapping down to EF and back retains exact decision details and lists.
   - All tests passed.
6. **Runtime Verification:** Successfully booted the Web API project, validating that EF successfully instantiated `lending.db`, applied the SQL schema, and initiated the hosting environment.


## Phase 8: REST API

**Objective:** Create a thin API layer that exposes endpoints to submit applications and query dashboards, bridging HTTP calls safely down to the Application Service without leaking Domain or EF elements.

**Steps Taken (Autonomous Execution):**
1. **Data Transfer Objects (DTOs):** Built strictly typed DTOs (`CreateLoanApplicationRequest`, `LoanApplicationResponse`, `DashboardStatsResponse`, etc.) incorporating DataAnnotations for immediate HTTP 400 rejection of unphysical values.
2. **Application Layer:** Built `ILoanApplicationService` and its implementation. This service orchestrates validating the DTO, mapping it to the domain `LoanApplication`, executing the `LoanDecisionEngine`, persisting via the Repository, and mapping back to a Response DTO.
3. **Controllers:**
   - `LoanApplicationsController`: Exposes `POST /api/loan-applications`, `GET /api/loan-applications`, and `GET /api/loan-applications/{id}`.
   - `DashboardController`: Exposes `GET /api/dashboard` aggregating real-time DB data metrics.
4. **Exception Middleware:** Created `ExceptionHandlingMiddleware.cs` to gracefully catch any lower-level `ArgumentOutOfRangeException`s from the Domain and format them cleanly as standard RFC-7807 `ProblemDetails`.
5. **Integration Testing & Real API Verification:** 
   - Installed `Microsoft.AspNetCore.Mvc.Testing` and configured `ApiIntegrationTests.cs` to emulate the API host and run assertions against endpoints. All 4 tests passed.
   - Started the live Web API via PowerShell, curled actual JSON requests against it, and verified the live SQLite interactions and JSON payload responses. 
     - *Approved Response:* Generated full success tree.
     - *Declined Response:* Passed a multi-failure request generating 2 distinct business reasons.
     - *Dashboard Response:* Computed averages efficiently over the real dataset.


## Phase 9: React Frontend

**Objective:** Develop a modern React, TypeScript, and Vite frontend styled with Tailwind CSS, utilizing a scalable folder structure that natively connects to the REST API.

**Steps Taken (Autonomous Execution):**
1. **Environment Initialization:** Scaffolded the application using Vite (`react-ts` template), installed `react-router-dom`, `lucide-react`, and configured Tailwind CSS (`postcss`, `autoprefixer`).
2. **TypeScript Integrity:** Mirrored all C# DTOs seamlessly into `src/types/index.ts` to maintain 1:1 type mapping between the frontend and backend boundaries.
3. **Centralized Service:** Established `src/services/api.ts` implementing native `fetch` with clean async/await patterns, standardized error extraction, and REST operations targeting `localhost:5062`.
4. **Layout & UI Creation:**
   - Built `Layout.tsx` utilizing a responsive Sidebar and Lucide icons.
   - Built `Dashboard.tsx` aggregating the live statistics through intuitive stylized cards.
   - Built `NewApplication.tsx` containing standard HTML5 controlled inputs ensuring seamless conversion to REST JSON payloads.
   - Built `Applications.tsx` containing an elegant tabular list format showing LTV/Statuses.
   - Built `ApplicationDetails.tsx` parsing the dynamic JSON collections recursively to expose underlying rule evaluation matrices graphically.
5. **Backend CORS:** Retroactively updated `Lending.Api/Program.cs` to implement a global `AllowAll` CORS policy, preventing browser-side Origin restrictions.
6. **Compilation & Build:** 
   - Refactored all implicit React imports to resolve strictly via Vite/ESLint typescript conventions. 
   - Ran `npm run build` which executed `tsc -b && vite build`.
   - Verified that all 1,892 modules transformed and built successfully with zero syntax, routing, or type errors.


## Phase 10: New Loan Application UI

**Objective:** Develop a robust, user-friendly, and professional React component for accepting lending parameters, rendering live calculations, and presenting authoritative backend decisions inline.

**Steps Taken (Autonomous Execution):**
1. **State & Validation:** Constructed a robust controlled form managing loanAmount, assetValue, and creditScore. Implemented regex-driven numeric input constraints and rigorous inline field validation blocking non-positive amounts and bounding the credit score between 1-999.
2. **UX Enhancements:** 
   - Added GBP (£) visual prefixing.
   - Developed a reactive "Live LTV Preview" widget updating asynchronously during keystrokes.
   - Built an interactive "Evaluating..." loading overlay over the submit action.
3. **Result Presentation (Inline Architecture):** Rather than redirecting immediately to the ledger, I engineered the component to dynamically swap the form out for a detailed Decision Summary Card upon receiving the HTTP payload.
   - The card leverages Lucide iconography (CheckCircle, XCircle, AlertCircle) scaling colors structurally based on the Approved vs Declined state.
   - Iterates through the RuleResults rendering visual pass/fail graphs for every individual lending threshold.
4. **Compilation Verification:** Ran a strict 	sc -b build. Intercepted one unused import locally, rectified it, and successfully built the optimized React tree.


## Phase 11: Dashboard and Application History

**Objective:** Wire up the comprehensive platform statistics and historical application ledger to the actual backend APIs, providing a polished and scalable data-viewing experience.

**Steps Taken (Autonomous Execution):**
1. **Dashboard Refinement:**
   - Transformed Dashboard.tsx to asynchronously fetch both aggregate metrics (/api/dashboard) and the applications collection (/api/loan-applications) simultaneously using Promise.all.
   - Rendered dynamic stat cards reflecting live real-world SQLite calculations for total apps, success ratios, value written, and exact Mean LTV.
   - Introduced a "Recent Applications" sub-table, automatically slicing the chronologically sorted ledger to expose only the 5 most recent activities directly on the main dashboard.
2. **Applications Ledger Enhancements:**
   - Enhanced Applications.tsx with a multi-layered filtering system driven by useMemo.
   - Built a live Search bar evaluating against alphanumeric Application UUIDs, Loan Amounts, and Asset Values seamlessly.
   - Constructed a Decision Filter dropdown, enabling immediate parsing of Approved vs Declined histories.
   - Designed robust "Empty State" vectors, utilizing friendly Lucide iconography to differentiate between a truly empty database vs a search query returning no results.
3. **Data Formatting & Resilience:**
   - Unified formatting paradigms globally across the UI. Monetary fields strictly serialize through Intl.NumberFormat ensuring accurate GBP string representations (£XX,XXX). Dates employ localized Intl.DateTimeFormat.
   - Implemented AlertCircle API failure states intercepting network/CORS timeouts cleanly without crashing the React DOM.
   - Ran final strict 	sc -b && vite build which confirmed zero TypeScript or linting deviations across the stack.


## Phase 12: Explainable Lending Decisions

**Objective:** Implement transparent, rule-by-rule decision explanations driven entirely by the backend engine to ensure zero business logic duplication in the frontend.

**Steps Taken (Autonomous Execution):**
1. **Domain Model Evolution:**
   - Modified RuleResult.cs value object to accept and serialize a Reason explanation for successful evaluations (previously only failures carried a reason).
2. **Rule Explanations (C# Engine):**
   - Refactored GeneralLimitsRule.cs, LargeLoanRule.cs, and SmallLoanRule.cs to explicitly output descriptive success text incorporating live data (e.g., " is under £1M; large loan requirements do not apply.").
   - Preserved all complex logic thresholds within the domain layer; ensuring that the backend remains the strict authoritative source of truth.
3. **Domain Testing:**
   - Authored RuleExplanationsTests.cs using xUnit.
   - Asserted that ApprovedApplication_ReturnsSuccessfulRuleEvaluations_WithMeaningfulExplanations strictly proves that positive feedback maps identically to correct payload fields.
   - Verified that DeclinedLargeApplication_ExplainsBothLtvAndScoreFailures aggregates all relevant values and required thresholds into human-readable sentences properly.
   - Executed the full backend suite: 83 tests passed.
4. **React Frontend Propagation:**
   - Modified the JSX iterating loops in both NewApplication.tsx and ApplicationDetails.tsx which previously gated the ule.reason rendering behind a !rule.isPassed conditional.
   - The UI now prints the dynamic backend text for both Approved (green) and Declined (red) matrix nodes.
   - Built via Vite with 0 typing or mapping deviations.


## Phase 13: Optional Decision Simulator

**Objective:** Introduce an advanced informational tool designed to algorithmically compute and suggest minimum/maximum values necessary to clear specific declined barriers, without modifying core persistence.

**Steps Taken (Autonomous Execution):**
1. **Simulation Domain Implementation:** 
   - Crafted a DecisionSimulator Service internally evaluating the LoanApplication thresholds mathematically.
   - Designed a robust SimulationScenario value object to strongly type dynamic hypothetical thresholds (e.g., minimum asset values strictly required to bring LTVs below bounding margins such as < 90% or <= 60%).
   - Implemented exact arithmetic using Math.Ceiling and Math.Floor to guarantee suggested fixes mathematically push edge cases into valid tiers.
2. **Backend Architecture & Exposure:**
   - Bound DecisionSimulator strictly as a transient calculation layer devoid of repository access to enforce non-modification of application history.
   - Exposed GET /api/loan-applications/{id}/simulate locally routed through the unified Application Service boundaries.
3. **Frontend Integration:** 
   - Dynamically populated a "Decision Simulator (Hypothetical)" UI pane explicitly constrained to Status !== 'Approved' ledgers.
   - Styled using distinct Lightbulb indicators and clear visual warnings distinguishing hypotheticals from actual ledger facts.
4. **Validation:**
   - Wrote comprehensive xUnit integration scenarios DecisionSimulatorTests.cs validating every single matrix path (Credit fixes, max threshold bounds, mathematically derived LTV resolutions). All 81 domain tests successfully passed.


## Phase 14: Robustness and Security Review

**Objective:** Audit and harden the entire application (Backend + Frontend) ensuring stability, security, valid bounds, and polished user experience before final delivery.

**Steps Taken (Autonomous Execution):**
1. **Backend Validation & Hardening:**
   - **Exception Handling:** Modified ExceptionHandlingMiddleware to dynamically check _env.IsDevelopment(). Stack traces and raw error strings are now obscured with generic messages in production, preventing DB/Schema leaks.
   - **Data Transfer Objects (DTO):** Added mathematical upper bounds (1 Trillion) to LoanAmount and AssetValue within CreateLoanApplicationRequest. This guarantees SQLite decimal constraints and runtime percentage calculations (LTV) won't crash via double.MaxValue overflows.
   - **Routing Resilience:** Addressed missing 404 responses. Updated LoanApplicationService.GetSimulationAsync to return 
ull when an ID doesn't exist, and the controller correctly intercepts this to throw an explicit 404 Not Found ProblemDetails response.
   - **CORS:** Tightened the CORS policy internally to map precisely to the Vite standard development port (http://localhost:5173) mitigating wide open network requests while preserving sandbox functionality.
2. **Frontend Validation & Accessibility (A11Y):**
   - **Native JS Validation:** Attached maxLength={15} and maxLength={3} attributes directly to the React input bindings. This prevents the user from pasting 100+ digit strings that cause JS parseFloat to yield Infinity, resulting in silent DOM failures or API 
ull payloads.
   - **Accessibility Hardening:** Injected semantic id definitions onto inputs and tethered them structurally to their respective descriptive text using htmlFor attributes, guaranteeing screen-reader traversal functionality.
3. **End-to-End Validation:**
   - Ran dotnet test confirming all 88 unit and integration tests successfully navigated the new bounds and strict routing limits.
   - Ran 
pm run build triggering 	sc -b && vite build which successfully bundled the final UI structure containing the a11y improvements without TypeScript collisions.


## Phase 15: Final Senior Engineer Review & Corrections

**Objective:** Inspect codebase acting as Senior Engineer, document findings, and implement strict corrections preserving domain integrity.

**Findings & Corrections:**
1. **[High] OOM Vulnerability in Dashboard Stats:**
   - *Issue:* GetDashboardStatsAsync pulled the entire DB table into memory using GetAllAsync() for .Count() and .Average().
   - *Correction:* Updated LoanApplicationRepository.GetDashboardAggregatesAsync() to leverage AsAsyncEnumerable() in EF Core. This streams rows out of SQLite one-by-one, computing aggregations progressively without memory overhead. It safely bypasses the limitation of EF Core being unable to translate JSON ValueConverters inside Select clauses.
2. **[Medium] Decision Simulator Precision:**
   - *Issue:* The DecisionSimulator used an arbitrary  .899m threshold to ensure standard loans were kept safely beneath 90% LTV.
   - *Correction:* Updated the mathematical divisor to  .8999m to align as tightly as mathematically possible with the < 90.00% requirement without triggering the strict threshold boundary.
3. **[Low] Hardcoded Rule Strings:**
   - *Issue:* Domain rule classes and testing assertions relied on loosely typed hardcoded strings (e.g. "General Loan Limits Rule").
   - *Correction:* Extracted these identities into a statically typed RuleNames constants container injected across both Domain and Unit Test projects.
4. **[Low] Interactive Ledger Sorting:**
   - *Issue:* The Applications.tsx UI lacked column-header sorting.
   - *Correction:* Re-engineered the Applications Ledger to maintain a discrete React state for sortKey and sortAsc. Hooked Lucide icons (ChevronUp, ChevronDown) to the <th> components enabling seamless multi-directional sorting by LTV, amounts, scores, and dates.

**Final Test Results:**
- **Backend Validation:** dotnet test executed 88 Integration & Domain tests. Result: 100% Passed.
- **Frontend Validation:** 
pm run build executed successfully via Vite+TSC with 0 compilation collisions.

## Phase 18: Final Documentation
- Investigated final code state.
- Generated docs/architecture.md to map DDD boundaries and SQLite EF configurations.
- Generated docs/business-rules.md explicitly defining strict threshold processing ( .8999m bound clearances).
- Generated docs/test-coverage.md enumerating the 88 unit, domain, integration, and infrastructure tests.
- Generated docs/quality-report.md summarizing the UX, memory optimization (AsAsyncEnumerable), exception hardening, and precision enhancements from Phase 14 & 15.
- Formatted root README.md containing exactly the 19 requested segments. Validated assumptions around missing functionality (explicitly flagged the lack of Auth/CI-CD).

## Phase 20: Final Submission Preparation
- Initialized local Git repository and injected rigorous .gitignore (ignoring .dotnet, Lending.db, 
ode_modules).
- Deleted legacy scaffold artifacts (Blackfinch.Lending.Domain, Blackfinch.Lending.Tests).
- Systematically audited source for hardcoded secrets, TODO markers, and console.log bleed; 100% clean.
- Performed final end-to-end continuous integration checks: dotnet build, dotnet test (88/88 passed), and 
pm run build (0 TSC emissions).
- Ready for final zip/submission.
