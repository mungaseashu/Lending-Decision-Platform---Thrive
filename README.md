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

```text
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
