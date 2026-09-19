# Lending Decision Platform

A full-stack lending decision platform that evaluates loan applications using configurable LTV and credit-score rules, provides explainable decisions, and presents application statistics through a React web interface.

---

## 1. Project Overview

The Lending Decision Platform is a full-stack web application designed to process loan application data:

- Loan Amount
- Asset Value
- Credit Score

The application calculates the Loan-to-Value (LTV) ratio and evaluates the application against the defined lending rules to produce an immediate:

- Approved
- Declined

decision.

The platform is built with a **C# .NET 8 backend** and a **React/TypeScript frontend**.

---

## 2. Features

### Live Decision Engine

Evaluates loan applications against the defined loan amount, LTV, and credit-score requirements.

### Detailed Decision Explanations

Explains why an application was approved or declined, including the relevant rules and conditions.

### Decision Simulator

Provides hypothetical mathematical scenarios that show how changes to values such as asset value or loan amount could affect LTV eligibility.

### Live LTV Preview

Calculates and displays the estimated LTV dynamically while entering loan application details.

> The backend remains the authoritative source for the final LTV and lending decision.

### Application Ledger

Provides a historical view of submitted applications with filtering and sorting capabilities.

### Real-Time Dashboard Statistics

Displays:

- Total applicants
- Successful applicants
- Declined applicants
- Total capital written
- Mean LTV

---

## 3. Technology Stack

### Backend

- C# .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- xUnit
- Moq

### Frontend

- React 18
- TypeScript
- Vite
- Tailwind CSS
- Lucide React Icons

---

## 4. Architecture

The project follows a **Domain-Driven Design (DDD)** and **Clean Architecture** approach.

The core lending rules reside in the `Lending.Domain` project and remain independent of the database and presentation layers.

The React frontend is decoupled from the lending rules and acts as a client of the backend API.

### Architecture Overview


                    ┌──────────────────────┐
                    │     React Client     │
                    │  React + TypeScript  │
                    └──────────┬───────────┘
                               │
                               │ HTTP / JSON
                               ▼
                    ┌──────────────────────┐
                    │     Lending.Api      │
                    │ Controllers / API    │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Lending.Application │
                    │ Services / DTOs      │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │   Lending.Domain     │
                    │ Entities / Rules     │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Lending.Infrastructure│
                    │ EF Core / SQLite     │
                    └──────────────────────┘

## 5. Folder Structure

```text
Lending-Decision-Platform---Thrive/
│
├── backend/
│   ├── src/
│   │   ├── Lending.Api/
│   │   ├── Lending.Application/
│   │   ├── Lending.Domain/
│   │   └── Lending.Infrastructure/
│   │
│   └── tests/
│       ├── Lending.Api.Tests/
│       ├── Lending.Domain.Tests/
│       └── Lending.Infrastructure.Tests/
│
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   └── types/
│   │
│   ├── public/
│   ├── package.json
│   └── ...
│
├── docs/
│   ├── architecture.md
│   ├── business-rules.md
│   └── ...
│
├── AI_LOG.md
├── README.md
└── .gitignore
```


## 6. Business Rules

General Loan Limits

Condition	            Decision

Loan < £100,000	      Declined

Loan > £1,500,000	    Declined

High-Value Loans
For loans greater than or equal to £1,000,000:

LTV must be 60% or less
Credit score must be 950 or greater

Both conditions must be satisfied.

Standard Loans
For loans below £1,000,000:

LTV	       Required Credit Score

LTV < 60%	     ≥ 750

LTV < 80%	     ≥ 800

LTV < 90%	     ≥ 900

LTV ≥ 90%	     Declined

## 7. LTV Calculation

Loan-to-Value is calculated using:

LTV = (Loan Amount / Asset Value) × 100

Example
Loan Amount = £750,000
Asset Value = £1,200,000

LTV = (750,000 / 1,200,000) × 100
    = 62.50%

The backend uses decimal arithmetic for LTV and monetary calculations.
The frontend may display a live LTV preview, but the backend recalculates the value before making the final decision.

## LTV Boundary Interpretation

For loans below £1,000,000, the implementation interprets the specified strict < conditions as:

0% ≤ LTV < 60%

60% ≤ LTV < 80%

80% ≤ LTV < 90%

LTV ≥ 90%

For loans greater than or equal to £1,000,000:
LTV ≤ 60%

Credit Score ≥ 950

Borderline LTV values are evaluated using the calculated decimal value rather than being pre-rounded before rule evaluation.


## 8. API EndPoints

Submit Loan Application

POST /api/loan-applications

Submits a new loan application and evaluates the lending rules.

Retrieve Applications

GET /api/loan-applications

Returns persisted loan applications.

Retrieve Application Details

GET /api/loan-applications/{id}

Returns detailed information about a specific application.

Decision Simulator

GET /api/loan-applications/{id}/simulate

Returns hypothetical mathematical scenarios related to the application's lending conditions.

Dashboard

GET /api/dashboard

Returns aggregated lending statistics.

## 9. Prerequisites

Install:

.NET 8 SDK

Node.js v18+

Git

## 10. Local Setup

Clone the repository:
git clone https://github.com/mungaseashu/Lending-Decision-Platform---Thrive

Backend Setup
Navigate to the backend directory:

cd backend

Run the API:
$env:Path += ";<YOUR_PROJECT_PATH>"
dotnet run --project src/Lending.Api

The backend runs at:
http://localhost:5062

Health check:
http://localhost:5062/health

Frontend Setup

Open a new terminal.
Navigate to the frontend:

cd frontend

Install dependencies:
npm install

Start the development server:
npm run dev

The frontend runs at:
http://localhost:5173

## 11. Database

The application uses SQLite with Entity Framework Core.
No separate SQL Server or PostgreSQL installation is required.
The SQLite database is configured by the backend and is created automatically according to the application's Entity Framework Core configuration.

## 12. Testing

Run all backend tests:
cd backend
dotnet test

The current implementation contains 88 backend xUnit tests covering business rules, validation, boundary conditions, and related functionality.

The test suite includes:

Loan amount boundaries

LTV boundaries

Credit-score thresholds

£1 million high-value loan rules

Invalid input

Decision explanations

Persistence behavior

## 13. Assumptions

Mean LTV
The dashboard's mean LTV calculation applies to all submitted applicants.

Total Value of Loans Written
The total value of loans written applies to approved applications.

Backend as Source of Truth
The frontend may calculate LTV for immediate visual feedback, but all final validation, LTV calculation, and lending decisions are performed by the backend.

Decision Simulator
The Decision Simulator provides hypothetical mathematical scenarios.
It does not modify the official lending decision and does not introduce additional lending criteria.

## 14. Production Considerations

This project is designed as a technical assessment and is not intended to represent a production-ready lending platform.
For a production implementation, potential improvements could include:

PostgreSQL or SQL Server

Authentication and authorization

Audit logging

Structured application logging

Monitoring and alerting

Distributed tracing

Secrets management

CI/CD pipelines

Database backup and recovery

API versioning

Rate limiting

Additional integration and end-to-end testing

Production-grade infrastructure

Stronger financial and audit controls

## 15. AI-Assisted Development

AI tools were used for the development process as engineering assistants.

AI assistance was used for:

Requirements analysis

Architecture design

Business-rule implementation

Test generation

Code review

Debugging

Refactoring

Documentation

AI-generated suggestions were reviewed and tested rather than blindly accepting.

## 16.Preview

1. Dashboard
   
   **Visual dashboard to see Total Applications, Total Approved/Declined Applications, Total Value Written, Mean Average LTV and Recent applications**
   <img width="1891" height="821" alt="image" src="https://github.com/user-attachments/assets/fe0ec24e-6a1b-4dda-a057-2a23a9b22b94" />
2. New Application
   
   **Interface for creating new application with Live LTV preview**
   <img width="1892" height="774" alt="image" src="https://github.com/user-attachments/assets/00872b63-bc90-4cc8-bc3a-77a65df3c472" />
3. Accept/Decline Interface
   
   **Interface of Accepted and Declined applications**
   <img width="1033" height="840" alt="image" src="https://github.com/user-attachments/assets/c3261e93-f758-48b3-909b-d0e45dcebea6" />
   <img width="790" height="898" alt="image" src="https://github.com/user-attachments/assets/767bcf84-de79-4cd9-bf13-2db0da9c2c8b" />
4. View all the application
   
   **List of all the applications can be sorted according to date submitted, approved or declined and also view details of the submitted applications**
   <img width="1189" height="912" alt="image" src="https://github.com/user-attachments/assets/829a4e43-846f-44b5-9b6b-02784ad98e02" />
5. Decision Simulator
    
    **A decision simulator which gives hypothetical mathematical calculations by which the application can get approved** 
   <img width="918" height="903" alt="image" src="https://github.com/user-attachments/assets/1687f832-f8ca-4376-9abb-c0e41ae605da" />





