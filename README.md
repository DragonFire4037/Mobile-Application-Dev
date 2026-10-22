# Personal Budgeting API

## Overview

The Personal Budgeting API is a RESTful web API designed to support a mobile application focused on personal finance management. It will allow users to record income and expenses, establish budgets, analyze spending habits, and track financial performance over extended periods.

The API will be developed using ASP.NET Core Web API on .NET 10. Data will initially be stored in JSON files, allowing the API to operate independently of a database server and the mobile application.

This project focuses on implementing a well-structured API with clear separation of concerns, input validation, consistent HTTP responses, and reliable data persistence.

## Project Goals

- Record and manage income, expenses, and transfers.
- Categorize transactions to analyze spending habits.
- Create and maintain budgets for defined periods.
- Calculate income, expenses, net cash flow, and savings rate.
- Compare actual spending against budget limits.
- Analyze financial trends across days, months, years, and custom date ranges.
- Provide documented endpoints and predictable error responses.
- Support automated API testing through a Postman collection.

## Scope

The initial implementation will support manually entered financial data.

The API will not integrate with the MAUI application during this development phase. Bank account integrations, automatic transaction imports, authentication, and external financial services are outside the initial scope unless introduced in a later phase.

## Technology and Architecture

The project will use the following technologies and architectural components:

- **Framework:** ASP.NET Core Web API on .NET 10.
- **Controllers:** Handle HTTP requests and map service outcomes to HTTP responses.
- **DTOs:** Define separate request and response contracts.
- **Models:** Represent the application's internal data.
- **Repositories:** Define data access interfaces and JSON file persistence.
- **Services:** Implement business rules and financial calculations.
- **Data:** Store JSON files under the application's content root.
- **OpenAPI:** Document the API endpoints and their request and response formats.
- **Postman:** Verify endpoint behavior, validation, error handling, and repeatable test execution.

The API will use plural resource names and RESTful HTTP methods. Controllers will depend on service interfaces, and services will depend on repository interfaces.

## API Conventions

### Base URL

During local development, the base URL will be determined by the ASP.NET Core launch configuration.

All endpoints documented below are relative to the API base URL.

### Data Formats

- Request and response bodies use JSON.
- Dates use the ISO 8601 `YYYY-MM-DD` format unless otherwise specified.
- Monetary amounts use decimal values and must be greater than zero when entered as transaction amounts.
- Transaction types are `Income`, `Expense`, and `Transfer`.
- Resource IDs are integers assigned by the server.
- Resource IDs and creation timestamps cannot be set or overwritten by clients.
- Date ranges use inclusive start and end dates.

### HTTP Status Codes

| Status | Meaning |
|---|---|
| `200 OK` | The request succeeded and returned data. |
| `201 Created` | A resource was created successfully. The response includes a `Location` header identifying the created resource. |
| `204 No Content` | The request succeeded without a response body. |
| `400 Bad Request` | The request contains invalid data or invalid query parameters. |
| `404 Not Found` | The requested resource does not exist. |
| `409 Conflict` | The request conflicts with an existing resource or violates a documented duplicate rule. |

Validation failures will use consistent problem details, including validation messages keyed by JSON property names. Unexpected server errors will be handled centrally.

# API Endpoints

## Transactions

Transactions represent money entering or leaving a user's finances, as well as transfers between accounts. They are the primary data source for financial reports and budget calculations.

### Transaction Endpoint Table

| Method | URL | Description | Success Code | Failure Codes |
|---|---|---|---|---|
| GET | `/api/transactions` | Retrieves transactions, optionally filtered by type, category, and date range. | `200 OK` | `400 Bad Request` |
| GET | `/api/transactions/{id:int}` | Retrieves a transaction by its ID. | `200 OK` | `404 Not Found` |
| POST | `/api/transactions` | Creates a transaction and assigns its ID and creation timestamp on the server. | `201 Created` | `400 Bad Request`, `409 Conflict` |
| PUT | `/api/transactions/{id:int}` | Replaces the editable fields of an existing transaction. | `204 No Content` | `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| DELETE | `/api/transactions/{id:int}` | Deletes a transaction by its ID. | `204 No Content` | `404 Not Found` |

### Transaction Query Parameters

The `GET /api/transactions` endpoint supports the following optional query parameters:

| Parameter | Type | Description |
|---|---|---|
| `type` | String | Filters by `Income`, `Expense`, or `Transfer`. |
| `category` | String | Filters by transaction category. |
| `startDate` | Date | Includes transactions on or after this date. |
| `endDate` | Date | Includes transactions on or before this date. |

When multiple filters are supplied, all filters must be satisfied. Invalid transaction types, malformed dates, or a start date later than the end date will result in `400 Bad Request`.

Example:

`GET /api/transactions?type=Expense&category=Food&startDate=2026-01-01&endDate=2026-01-31`

### Transaction Data Rules

A transaction will contain the following information:

- **ID:** A server-assigned integer identifier.
- **Description:** A required description of the transaction.
- **Amount:** A positive decimal monetary amount.
- **Type:** The transaction classification.
- **Category:** A required category used for reporting and budgeting.
- **TransactionDate:** The date the transaction occurred.
- **Notes:** Optional additional information.
- **CreatedAtUtc:** A server-assigned creation timestamp in UTC.

The ID and creation timestamp are server-controlled fields and cannot be modified by the client.

### Transaction Business Rules

1. Descriptions and categories must not be empty or exceed their documented length limits.
2. Transaction amounts must be greater than zero and within the supported monetary limit.
3. Transaction dates must be valid.
4. Transfers must not be counted as income or expenses in financial reports.
5. Creating a transaction with the same normalized description, amount, type, and transaction date as an existing transaction will result in `409 Conflict`.
6. Updating a transaction must not create a duplicate of another transaction. The transaction being updated is excluded from its own duplicate check.
7. Missing transaction IDs will result in `404 Not Found`.

The duplicate rule is intended to prevent accidental duplicate entries. It does not guarantee that identical real-world purchases cannot occur.

## Budgets

Budgets define spending limits over specific date ranges. They will be used to compare planned spending against recorded expenses.

### Budget Endpoint Table

| Method | URL | Description | Success Code | Failure Codes |
|---|---|---|---|---|
| GET | `/api/budgets` | Retrieves budgets, optionally filtered by period. | `200 OK` | `400 Bad Request` |
| GET | `/api/budgets/{id:int}` | Retrieves a budget by its ID. | `200 OK` | `404 Not Found` |
| POST | `/api/budgets` | Creates a budget and assigns its ID and creation timestamp on the server. | `201 Created` | `400 Bad Request`, `409 Conflict` |
| PUT | `/api/budgets/{id:int}` | Replaces the editable fields of an existing budget. | `204 No Content` | `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| DELETE | `/api/budgets/{id:int}` | Deletes a budget by its ID. | `204 No Content` | `404 Not Found` |

### Budget Query Parameters

The `GET /api/budgets` endpoint supports the following optional parameters:

| Parameter | Type | Description |
|---|---|---|
| `startDate` | Date | Filters budgets by their start date. |
| `endDate` | Date | Filters budgets by their end date. |

When both parameters are supplied, the start date must not be later than the end date. Invalid date values or ranges result in `400 Bad Request`.

### Budget Data Rules

A budget will contain the following information:

- **ID:** A server-assigned integer identifier.
- **Name:** A required, human-readable budget name.
- **StartDate:** The first date covered by the budget.
- **EndDate:** The final date covered by the budget.
- **SpendingLimit:** The maximum planned spending for the budget period.
- **CreatedAtUtc:** A server-assigned creation timestamp in UTC.

The initial implementation will support an overall spending limit per budget. Category-specific limits may be introduced in a later phase.

### Budget Business Rules

1. Budget names must not be empty or exceed their documented length limits.
2. The start date must not be later than the end date.
3. Spending limits must be greater than zero and within the supported monetary limit.
4. Two budgets cannot have the same normalized name and identical start and end dates.
5. Updating a budget must not create a duplicate of another budget. The budget being updated is excluded from its own duplicate check.
6. Duplicate budgets result in `409 Conflict`.
7. Missing budget IDs result in `404 Not Found`.

Budgets will not automatically create transactions. They will be evaluated against existing expense transactions when financial summaries and budget reports are requested.

## Financial Reports

Financial reports provide calculated views of existing transactions and budgets. They are read-only resources: report requests will not create, update, or delete financial records.

All report endpoints support an inclusive date range through `startDate` and `endDate` query parameters.

### Financial Report Endpoint Table

| Method | URL | Description | Success Code | Failure Codes |
|---|---|---|---|---|
| GET | `/api/reports/summary` | Calculates total income, expenses, net cash flow, savings rate, and transaction count. | `200 OK` | `400 Bad Request` |
| GET | `/api/reports/spending` | Calculates expense totals grouped by category. | `200 OK` | `400 Bad Request` |
| GET | `/api/reports/trends` | Calculates financial metrics grouped by day, month, or year. | `200 OK` | `400 Bad Request` |
| GET | `/api/reports/budgets` | Compares budget limits against actual expenses for the requested period. | `200 OK` | `400 Bad Request` |

### Report Query Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `startDate` | Date | Yes | The first date included in the report. |
| `endDate` | Date | Yes | The last date included in the report. |
| `interval` | String | For trends | The grouping interval: `day`, `month`, or `year`. |

For example:

`GET /api/reports/summary?startDate=2026-01-01&endDate=2026-12-31`

`GET /api/reports/trends?startDate=2025-01-01&endDate=2026-12-31&interval=month`

Report requests with missing required dates, invalid date formats, reversed date ranges, or unsupported intervals will return `400 Bad Request`.

### Summary Metrics

The summary endpoint will return the following calculated metrics:

- **TotalIncome:** The sum of income transactions during the requested period.
- **TotalExpenses:** The net expense amount during the requested period, adjusted for refunds.
- **NetCashFlow:** Total income minus total expenses.
- **SavingsRate:** The percentage of income remaining after expenses.
- **TransactionCount:** The number of included transactions.

The savings rate is defined as:

`SavingsRate = ((TotalIncome - TotalExpenses) / TotalIncome) * 100`

When total income is zero, the savings rate will be `null` to avoid division by zero.

### Spending Breakdown

The spending endpoint will group expenses by category and return each category's total amount and proportion of total expenses.

Categories with no expenses during the requested period will not be included unless a later requirement specifies otherwise.

### Financial Trends

The trends endpoint will group financial activity according to the requested interval.

Supported intervals:

- `day`: Daily financial activity.
- `month`: Monthly financial activity.
- `year`: Annual financial activity.

Each period will include income, expenses, net cash flow, and savings rate. Periods with no income will report a `null` savings rate.

This endpoint is intended to support long-term analysis, including month-to-month spending comparisons and year-over-year financial trends.

### Budget Comparisons

The budget report endpoint will compare recorded expenses with budgets that overlap the requested date range.

Budget metrics will include:

- Budget name and identifier.
- Budget spending limit.
- Actual eligible expenses.
- Remaining budget.
- Percentage of the spending limit used.

Remaining budget is calculated as the spending limit minus eligible expenses. Percentage used is calculated as eligible expenses divided by the spending limit, multiplied by 100.

If expenses exceed the spending limit, the remaining budget may be negative and the percentage used may exceed 100%.

### Financial Calculation Rules

All report calculations will follow these rules:

1. Only income transactions contribute to total income.
2. Only expense transactions contribute to total expenses.
3. Transfers are excluded from income, expenses, net cash flow, and savings rate.
4. Refunds reduce net expenses rather than being treated as ordinary income.
5. All monetary calculations use decimal arithmetic.
6. Start and end dates are inclusive.
7. Empty datasets return valid summaries with zero totals and a `null` savings rate when income is zero.
8. Report calculations are derived from stored transaction and budget records rather than separately persisted aggregate totals.

## Validation and Error Handling

All create and update requests will be validated before the service attempts to persist changes.

Request DTOs will enforce required fields, string length limits, valid transaction types, positive monetary amounts, and valid date ranges. Validation attributes will provide descriptive error messages.

The API will use consistent problem details for errors. Validation error keys will use JSON property names.

The API will distinguish between:

- Invalid input, reported as `400 Bad Request`.
- Missing resources, reported as `404 Not Found`.
- Duplicate resources, reported as `409 Conflict`.
- Unexpected server errors, handled by centralized exception handling.

Controllers will not contain repository file operations or business rules. They will delegate operations to services and translate service outcomes into the appropriate HTTP response.

## Data Persistence

The initial implementation will use JSON files stored under the API application's content root.

Transaction and budget data will be persisted separately. Missing or empty data files will be treated as empty collections.

The repository layer will assign resource IDs and coordinate concurrent access to each JSON file. Writes will use a temporary file before replacing the destination file to reduce the risk of partially written data.

Data files and temporary files will be excluded from version control. Any optional starting data will be maintained separately as seed data.

## Testing

The API will be tested independently of the MAUI application.

Testing will include:

- Successful CRUD operations.
- Request validation and malformed query parameters.
- Missing resource handling.
- Duplicate transaction and budget rules.
- Financial calculations across multiple date ranges.
- Empty datasets and zero-income scenarios.
- Repository concurrency and persistence behavior.
- Correct HTTP status codes and response headers.
- Over-posting protection for server-controlled fields.

A Postman collection will be generated from the API's OpenAPI document and committed under the `postman/` directory.

The collection will use a configurable `{{baseUrl}}` variable and capture created resource IDs from POST responses. It will include positive and negative test cases, verify status codes and relevant response headers, and clean up created test data.

The collection must pass twice consecutively in the Postman Collection Runner without manual data cleanup.

## Development Roadmap

1. Define and commit the API contract.
2. Configure the .NET 10 Web API project and OpenAPI.
3. Implement transaction models, DTOs, repository, service, and controller.
4. Implement budget models, DTOs, repository, service, and controller.
5. Implement financial reporting services and endpoints.
6. Standardize validation and error responses.
7. Add automated tests.
8. Generate and verify the Postman collection.
9. Finalize documentation and verify the repository.

The API contract will be committed before controller implementation. Subsequent changes to endpoint behavior will be reflected in the documentation and verified against the implemented API.
