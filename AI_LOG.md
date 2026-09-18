# AI Collaboration Log

This log documents the iterative prompts used to build the Blackfinch Lending Platform, alongside my candid notes on how the AI's output was utilized, corrected, or refactored.

**Prompt 1:** *"Scaffold a new .NET 8 Web API and a React 18 frontend using Vite and TypeScript. Set them up in a monolithic repository structure."*
* **Notes:** Used the AI to quickly generate the boilerplate. I had to manually step in to fix some local Windows PATH issues with the .NET SDK, but the base structure was perfectly usable.

**Prompt 2:** *"Organize the .NET backend using strict Domain-Driven Design (DDD). Create Lending.Domain, Lending.Application, Lending.Infrastructure, and Lending.Api projects. Ensure Domain has no external dependencies."*
* **Notes:** AI did a great job setting up project references. I manually verified the `.csproj` files to ensure EF Core didn't accidentally leak into the Domain layer.

**Prompt 3:** *"Implement the lending business rules in the Domain layer. Use exact decimal math for LTV. Rules: Min £100k, Max £1.5M. >= £1M requires LTV <= 60% and Score >= 950. < £1M uses tiered requirements."*
* **Notes:** The AI wrote the initial rule classes. I had to manually intervene to fix how it handled the floating-point math—I enforced strict `decimal` types and prevented it from arbitrarily rounding borderline LTVs (e.g., 59.999%).

**Prompt 4:** *"Generate comprehensive xUnit tests for the LoanDecisionEngine and individual rules. Test all edge cases, especially exactly £1M loans and boundary LTV percentages."*
* **Notes:** Huge time saver. The AI generated over 80 test permutations. I tweaked a few assertions where the AI misunderstood the exact `<` vs `<=` requirements from the spec.

**Prompt 5:** *"Set up EF Core with SQLite in the Infrastructure layer. Map the LoanApplication entity, but use a ValueConverter to store the complex LoanDecision object as a JSON string in a TEXT column."*
* **Notes:** Brainstormed this approach with the AI to keep the relational schema clean. The AI provided the exact `JsonSerializer` syntax for the EF 8 configuration, which worked perfectly.

**Prompt 6:** *"Create the REST API Controllers (POST to submit, GET for ledger, GET for dashboard stats). Wire up the Application layer DTOs and handle exceptions smoothly."*
* **Notes:** The AI scaffolded the endpoints. I manually added an `ExceptionHandlingMiddleware` to catch domain validation errors and obscure 500 server crashes so we don't leak stack traces in production.

**Prompt 7:** *"Build the React frontend using Tailwind CSS. Create a submission form with inputs for Loan Amount, Asset Value, and Credit Score. Add a 'Live LTV' preview that updates as the user types."*
* **Notes:** AI generated a clean, responsive layout using Lucide icons. I ensured that the frontend "Live LTV" is strictly visual—I made sure the UI makes no actual lending decisions to maintain the backend as the single source of truth.

**Prompt 8:** *"Refactor the domain rules to be 'explainable'. Instead of just returning a bool, have them return specific strings explaining exactly why a rule passed or failed."*
* **Notes:** The AI mapped this out well, but it hardcoded rule names everywhere. I manually extracted all the strings into a `RuleNames` Constants class to clean up the architecture and stop the tests from breaking on typos.

**Prompt 9:** *"Let's build a 'Decision Simulator'. If a loan is declined, use algebraic inversion to calculate the exact asset value needed to drop the LTV into an acceptable tier."*
* **Notes:** One of the cooler collaborations. We worked out the math together. I had to refine the AI's divisor to `0.8999m` to guarantee the simulated asset value safely clears the strict `< 90%` threshold on resubmission.

**Prompt 10:** *"Wire up the React frontend to the real backend API. Create a dashboard displaying total applicants, success rates, and mean LTV, plus a sortable ledger table."*
* **Notes:** The AI handled the `fetch` logic and state management. I manually implemented the interactive column sorting (Ascending/Descending clicks) on the ledger table because the AI's first attempt was a bit clunky.

**Prompt 11:** *"Act as a Senior Engineer and audit the code for robustness, security, and scalability bugs. Fix any issues without changing the core business rules."*
* **Notes:** The AI caught a critical memory flaw. Because we stored decisions as JSON, EF Core couldn't translate a dashboard `.Count()` query to SQL, meaning it was pulling the entire database into RAM. The AI suggested fixing it via `AsAsyncEnumerable()` to stream the data efficiently. Brilliant catch.

**Prompt 12:** *"Generate final documentation. I need a README, architecture overview, strict business rules breakdown, and test coverage report. Only document what actually exists (no fake features)."*
* **Notes:** AI generated highly accurate markdown files summarizing the DDD structure, how to run the project, and explicitly noting the lack of Auth/CI-CD as a production consideration.