
# GeldGuide V1 — Claude Project Instructions

## Project Identity
**GeldGuide** — Free, bilingual (German/English) financial health app for the German market. Users answer progressive questions, the app evaluates 15 financial recommendations using a traffic light system (Red/Yellow/Green). All user data stays on-device. Financial rules are served from a remote server as JSON.

## Tech Stack
- **Framework:** .NET 9, C#
- **Mobile:** MAUI Blazor Hybrid (Android + iOS)
- **Web:** Blazor WASM
- **Server:** ASP.NET Core Minimal API (serves rules only, no user data)
- **Expression Engine:** NCalc (NuGet)
- **PDF Generation:** QuestPDF (on-device)
- **Local Storage:** SQLite (mobile via sqlite-net-pcl), IndexedDB (web via Blazored.LocalStorage)
- **Localization:** IStringLocalizer with de.json / en.json resource files
- **Hosting:** Docker Compose on Hetzner VPS, Nginx/Caddy for TLS
- **Testing:** xUnit

## Solution Structure
```
/src
  /GeldGuide.Core              # Models, RuleEngine, ExpressionEvaluator, ScoreAggregator, IDataStore, ReportGenerator
  /GeldGuide.UI                # Shared Razor components (all pages and components)
  /GeldGuide.Resources         # Localization files (de.json, en.json)
  /GeldGuide.Mobile            # MAUI Blazor Hybrid host (Android + iOS)
  /GeldGuide.Web               # Blazor WASM host (web version)
  /GeldGuide.Server            # ASP.NET Core Minimal API (serves rules.json)
/tests
  /GeldGuide.Core.Tests        # Unit tests for rule engine and expression evaluator
/rules
  /rules.json                  # All 15 rule definitions
/docker
  docker-compose.yml
  Dockerfile.server
```

## Project Dependencies
| Project | References |
|---------|-----------|
| GeldGuide.Core | NCalc, QuestPDF |
| GeldGuide.UI | GeldGuide.Core, GeldGuide.Resources |
| GeldGuide.Mobile | GeldGuide.UI, GeldGuide.Core, GeldGuide.Resources, sqlite-net-pcl |
| GeldGuide.Web | GeldGuide.UI, GeldGuide.Core, GeldGuide.Resources, Blazored.LocalStorage |
| GeldGuide.Server | Standalone — serves static files |
| GeldGuide.Core.Tests | GeldGuide.Core |

## Architecture Principles
1. **Privacy first:** User data NEVER leaves the device. Server only serves rule definitions.
2. **Dynamic rules:** Rules are JSON documents fetched from server with ETag caching. Client has a generic Rule Evaluator.
3. **Progressive data collection:** 3-tier approach — Tier 1 (5 fields at launch), Tier 2 (1-3 questions per rule unlock), Tier 3 (gentle nudges days later).
4. **Shared UI:** All Razor components live in GeldGuide.UI, shared between MAUI and WASM hosts.
5. **Offline-capable:** After first rule fetch, app works fully offline.

## Data Model — UserProfile (29 nullable fields)
| Field | Type | Tier |
|-------|------|------|
| age | int? | 2 |
| employmentType | enum?(Angestellt/Selbständig/Beamter) | 2 |
| maritalStatus | enum?(Single/Married/Divorced) | 2 |
| netMonthlySalary | decimal? | 1 |
| grossAnnualSalary | decimal? | 2 |
| monthlyFixedExpenses | decimal? | 1 |
| monthlyVariableExpenses | decimal? | 1 |
| currentSavings | decimal? | 1 |
| monthlySavingsAmount | decimal? | 2 |
| hasHighInterestDebt | bool? | 1 |
| debts | List<{type,balance,interestRate}>? | 2 |
| subscriptions | List<{name,monthlyCost}>? | 2 |
| hasPrivathaftpflicht | bool?(yes/no/don't know) | 2 |
| hasBU | bool?(yes/no/don't know) | 2 |
| childrenCount | int? | 1 |
| childrenAges | int[]? | 2 |
| receivingKindergeld | bool?(yes/no/don't know) | 2 |
| receivingBuergergeld | bool? | 2 |
| monthlyChildcareCost | decimal? | 2 |
| monthlyRentCost | decimal? | 2 |
| monthlyHeatingCost | decimal? | 2 |
| alreadyReceivingKinderzuschlag | bool? | 2 |
| employerOffersVL | bool?(yes/no/don't know) | 2 |
| commuteDistanceKm | decimal? | 2 |
| homeOfficeDaysPerWeek | int? | 2 |
| claimsPendlerpauschale | bool?(yes/no/don't know) | 2 |
| freistellungsauftragSet | bool?(yes/no/don't know) | 2 |
| expectedStatePension | decimal? | 2 |
| desiredRetirementIncome | decimal? | 2 |

## Local Storage Schema
| Table/Store | Contents |
|------------|----------|
| UserProfile | Single record with all 29 fields |
| CachedRules | Latest rules fetched from server (full JSON) |
| RuleFetchMetadata | Last fetch timestamp, ETag value |
| EvaluationResults | Computed results per rule (ruleId, color, message, benefit) |

## Rule Engine — JSON Schema

### Rule
```
Rule {
  id: string, version: int, isActive: bool,
  category: enum (Foundation|Protection|Family|Investing|Tax|Retirement|Housing),
  priority: int,
  title: { de: string, en: string },
  description: { de: string, en: string },
  icon: string,
  requiredFields: string[],
  conditions: Condition[], conditionLogic: "AND"|"OR",
  evaluation: Evaluation,
  benefitEstimate: BenefitEstimate,
  actions: Action[],
  learnMoreUrl: { de: string, en: string }
}
```

### Condition
```
Condition { field: string, operator: enum(equals|notEquals|greaterThan|lessThan|in|isSet|isNotSet), value: any }
```

### Evaluation
```
Evaluation {
  computedValues: [{ name: string, formula: string }],
  redWhen: string, yellowWhen: string, greenWhen: string,
  displayTemplate: { red: {de,en}, yellow: {de,en}, green: {de,en} }
}
```

### BenefitEstimate
```
BenefitEstimate {
  type: "monthly"|"yearly"|"oneTime",
  category: "income"|"savings"|"taxBack"|"protection",
  whenEvaluated: { formula: string, onlyWhen: ["red","yellow"] },
  whenLocked: { estimatedMax: number, perChild: boolean, label: {de,en} },
  whenGreen: { value: 0 }
}
```

### Expression Evaluator (NCalc)
- Arithmetic: +, -, *, /
- Comparisons: >, <, >=, <=, ==, !=
- Logical: AND, OR, NOT
- Functions: MIN(), MAX(), ROUND(), IF(condition, trueVal, falseVal)
- Variables resolved from UserProfile fields + computed values
- No arbitrary code execution — safe declarative expressions only.

## The 15 Financial Rules

### Foundation (4)
1. **Safety Buffer (Notgroschen)** — 3-6 months expenses as emergency fund. Fields: monthlyExpenses, currentSavings.
2. **Debt Kill-Switch (Schulden-Alarm)** — Flag high-interest debt (Dispo, credit cards). Fields: hasHighInterestDebt, debtDetails.
3. **Ghost Audit (Abo-Check)** — Flag unused/duplicate subscriptions. Fields: subscriptions list.
4. **Payday Automation (Sparautomatik)** — Recommend standing savings order. Fields: netMonthlySalary, monthlyExpenses, currentSavings, monthlySavingsAmount.

### Protection (2)
5. **Privathaftpflicht** — Binary liability insurance check. Fields: hasPrivathaftpflicht.
6. **BU Check** — Income gap if unable to work. Fields: age, employmentType, hasBU, netMonthlySalary.

### Family & Subsidies (3)
7. **Kindergeld** — €259/month per child. Fields: childrenCount, receivingKindergeld. Visible only if childrenCount > 0.
8. **Kinderzuschlag** — Up to €297/month per child for low/mid income. Fields: childrenCount, grossMonthlyIncome, maritalStatus, receivingKindergeld, receivingBuergergeld, monthlyRentCost, monthlyHeatingCost, alreadyReceivingKinderzuschlag. Visible if children > 0, receiving Kindergeld, not receiving Bürgergeld.
9. **Kita-Kosten** — Up to €4,000/year childcare tax deduction per child under 14. Fields: childrenCount, childrenAges, monthlyChildcareCost.

### Investing (2)
10. **VL-Check** — Employer VL contributions up to €40/month. Fields: employerOffersVL.
11. **ETF Sparplan** — Suggest global ETF if excess savings. Fields: currentSavings, monthlyExpenses, monthlySavingsAmount.

### Tax (2)
12. **Freistellungsauftrag** — €1,000 (€2,000 couples) tax-free allowance. Fields: freistellungsauftragSet, maritalStatus.
13. **Commuting Allowance (Pendlerpauschale)** — 38 cents/km tax deduction. Fields: commuteDistanceKm, homeOfficeDaysPerWeek, claimsPendlerpauschale.

### Retirement (1)
14. **Rentenlücke** — Pension gap calculation. Fields: expectedStatePension, desiredRetirementIncome.

### Housing (1)
15. **Wohnungsbauprämie** — 10% state bonus on Bausparvertrag. Fields: grossAnnualSalary, maritalStatus, wantsToOwnProperty.

## Traffic Light Dashboard
- Cards sorted: Red → Yellow → Locked → Green
- Protection rules (BU, Haftpflicht) in separate "Risk Protection" section
- "Don't know" answers → Yellow with actionable guidance

## "Money on the Table" Score
- **Current Benefit:** Sum of Euro amounts from evaluated rules needing action
- **Potential Benefit:** Current + estimated max from locked rules (labeled "up to")
- Progress bar: "X of 15 checked"
- Protection rules NOT in Euro score
- Green rules contribute 0 — framed positively
- Inapplicable rules (e.g., no children → no Kindergeld) excluded from potential

## PDF Report (6 pages, QuestPDF, on-device)
Locked until all 15 rules evaluated. Pages: Cover, Executive Summary, Foundation & Protection, Family & Subsidies, Tax/Investing/Retirement, Action Plan. Generated in user's selected language.

## Server API (2 endpoints, public, read-only, no auth)
- `GET /api/rules` — All active rules with ETag header
- `GET /api/rules/version` — Hash of current rules.json

## Key Files
| File | Purpose |
|------|---------|
| GeldGuide.Core/Models/UserProfile.cs | 29-field user data model (all nullable) |
| GeldGuide.Core/Models/Rule.cs | Rule, Condition, Evaluation, BenefitEstimate models |
| GeldGuide.Core/Models/EvaluationResult.cs | Result of evaluating a rule |
| GeldGuide.Core/Models/DashboardScore.cs | Aggregated score model |
| GeldGuide.Core/Engine/RuleEngine.cs | FilterRules, Evaluate, GetMissingFields |
| GeldGuide.Core/Engine/ExpressionEvaluator.cs | NCalc wrapper with variable resolution |
| GeldGuide.Core/Engine/ScoreAggregator.cs | Current + potential benefit calculation |
| GeldGuide.Core/Storage/IDataStore.cs | Interface for local data storage |
| GeldGuide.Core/Reports/ReportGenerator.cs | PDF report orchestrator |
| GeldGuide.UI/Pages/QuickStart.razor | Tier 1 wizard |
| GeldGuide.UI/Pages/Dashboard.razor | Main dashboard |
| GeldGuide.UI/Pages/RuleDetail.razor | Rule detail view |
| GeldGuide.UI/Components/UnlockSheet.razor | Tier 2 mini-form |
| GeldGuide.UI/Components/ScoreBar.razor | Score display |
| GeldGuide.UI/Components/RuleCard.razor | Individual rule card |
| GeldGuide.UI/Components/ReportCard.razor | PDF report card |
| GeldGuide.Resources/Localization/de.json | German UI strings |
| GeldGuide.Resources/Localization/en.json | English UI strings |
| rules/rules.json | All 15 rule definitions |
| GeldGuide.Server/Program.cs | Minimal API |

## Implementation Phases
1. **Scaffolding & Core** — Solution setup, models, ExpressionEvaluator (NCalc), RuleEngine, ScoreAggregator, IDataStore + implementations
2. **Rules & Localization** — rules.json with all 15 rules, ReportGenerator (QuestPDF), de.json/en.json
3. **Server** (parallel with Phase 4) — Minimal API, Docker config
4. **UI** (parallel with Phase 3) — QuickStart wizard, Dashboard, Unlock forms, RuleDetail, Settings
5. **Platform Builds & Launch** — MAUI config, WASM config, store submissions

## Key Decisions
- No user accounts, no server-side user data, no database on server
- NCalc for safe expression evaluation (no custom parser)
- Progressive 3-tier data collection to prevent drop-off
- QuestPDF for on-device PDF generation
- Rules as JSON with ETag caching for dynamic updates without app republishing
- Hetzner VPS (German data center, DSGVO-friendly)
- Default language: German

## Out of Scope (V2+)
PDF scanning, user accounts, server-side storage, admin panel, PostgreSQL, monetization, AI chatbot, advanced tax/retirement/investing rules, B2B, push notifications.

---

Copy the entire content above into your Claude Project as a knowledge document. It contains everything Claude Code needs to understand the app's architecture, data model, rule engine, and implementation plan to help you build GeldGuide.