# SystemSalesTickets

## תיאור הפרויקט

**SystemSalesTickets** הוא Web API למערכת מכירת כרטיסים לאירועים.

המערכת מאפשרת:

* ניהול משתמשים והרשאות
* הרשמה והתחברות באמצעות JWT
* ניהול אירועים
* ניהול מושבים
* הזמנת מושבים לאירועים
* מניעת הזמנה כפולה של אותו משאב
* טיפול ב־Optimistic Concurrency
* בדיקות יחידה ובדיקות Concurrency
* Logging באמצעות NLog
* Health Check
* Swagger לתיעוד ובדיקת ה־API

---

## טכנולוגיות

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Npgsql
* AutoMapper
* JWT Authentication
* xUnit
* Moq
* NLog
* Swagger / OpenAPI

---

## מבנה הפרויקט

הפתרון מחולק למספר פרויקטים:

```text
SystemSalesTickets
│
├── SystemSalesTickets.sln
│
├── SystemSalesTickets
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│   └── SystemSalesTickets.Api.csproj
│
├── SystemSalesTicketsDomain
│   └── Core
│       ├── DTOs
│       ├── Interfaces
│       ├── Models
│       ├── Repository
│       ├── MappingProfile
│       └── SystemSalesTickets.Core.csproj
│
├── SystemSalesTicketsInfrastructure
│   └── Data
│       ├── Migrations
│       ├── Repositories
│       ├── DataContext
│       └── SystemSalesTickets.Data.csproj
│
├── SystemSalesTicketsApplication
│   └── Service
│       ├── Service
│       ├── Background
│       └── SystemSalesTickets.Service.csproj
│
├── UnitTest
│   ├── EventServiceTests
│   ├── OrderServiceTests
│   ├── SeatServiceTests
│   ├── UserServiceTests
│   ├── ConcurrencyTests
│   └── UnitTest.csproj
│
├── PasswordGenerator
│
└── nlog.config
```

### אחריות הפרויקטים

**SystemSalesTickets**
פרויקט ה־API. מכיל את ה־Controllers, Middleware, `Program.cs`, Authentication, Authorization ו־Dependency Injection.

**SystemSalesTicketsDomain**
שכבת ה־Core. מכילה Models, DTOs, Interfaces, Repository interfaces ו־AutoMapper configuration.

**SystemSalesTicketsInfrastructure**
שכבת ה־Data. מכילה את `DataContext`, מימושי ה־Repositories ו־EF Core Migrations.

**SystemSalesTicketsApplication**
שכבת ה־Service. מכילה את הלוגיקה העסקית ואת שירותי המערכת.

**UnitTest**
מכיל את בדיקות ה־Unit ואת בדיקות ה־Concurrency.


### אחריות השכבות

**Core**
מכיל את המודלים, DTOs, interfaces, repositories contracts ו־mapping.

**Data**
אחראי על Entity Framework Core, PostgreSQL, `DataContext`, repositories ו־migrations.

**Service**
מכיל את הלוגיקה העסקית של המערכת.

**API**
מכיל Controllers, Middleware, Authentication, Swagger ו־Dependency Injection.

**UnitTest**
מכיל בדיקות לשכבת השירות ובדיקות Concurrency.

---

## Authentication ו־Authorization

המערכת משתמשת ב־JWT.

בעת Login המערכת:

1. מאתרת את המשתמש לפי Email.
2. בודקת את הסיסמה באמצעות `IPasswordHasher<User>`.
3. יוצרת JWT.
4. מוסיפה ל־Token את פרטי המשתמש וה־Role.
5. מחזירה את ה־Token ללקוח.

קיימים שני תפקידים:

* `User`
* `Manager`

Endpoints מוגנים משתמשים ב־`[Authorize]` או ב־`[Authorize(Roles = "...")]`.

---

## Password Security

סיסמאות אינן נשמרות כטקסט רגיל.

המערכת משתמשת ב־ASP.NET Core:

```csharp
IPasswordHasher<User>
```

ה־PasswordHasher מוזרק באמצעות Dependency Injection:

```csharp
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

---

## Database

המערכת משתמשת ב־PostgreSQL וב־Entity Framework Core Code First.

ה־Connection String מוגדר באמצעות Configuration ואינו אמור להישמר ב־Git.

הפרויקט משתמש ב־EF Core Migrations לצורך יצירה ועדכון של מבנה בסיס הנתונים.

להפעלת migrations:

```powershell
Add-Migration MigrationName
```

ולעדכון בסיס הנתונים:

```powershell
Update-Database
```

---

## Resource Concurrency

המושב של אירוע הוא משאב מוגבל.

המערכת מונעת מצב שבו שני משתמשים מצליחים להזמין את אותו מושב במקביל.

לצורך כך נעשה שימוש ב־Optimistic Concurrency באמצעות `EventSeat`.

ל־`EventSeat` קיים שדה `Version` המשמש כ־Concurrency Token.

בעת שינוי המשאב ה־Version משתנה.

אם שני משתמשים מנסים לבצע את אותה הזמנה במקביל:

1. המשתמש הראשון מצליח.
2. המשתמש השני מקבל `DbUpdateConcurrencyException`.
3. החריגה מטופלת בשכבת השירות.
4. למשתמש השני מוחזר Conflict (`409`).

בנוסף קיים Unique Index על:

```text
EventId + SeatId
```

כך שגם בסיס הנתונים מספק הגנה מפני הזמנה כפולה.

---

## Validation

ה־API משתמש ב־DataAnnotations וב־ASP.NET Core Model Validation.

בנוסף, בדיקות עסקיות מבוצעות בשכבת ה־Service.

לדוגמה:

* בדיקת נתונים חסרים
* בדיקת קיום אירוע
* בדיקת קיום מושב
* בדיקה שהמושב פנוי
* בדיקת הרשאות
* מניעת הזמנה כפולה

---

## DTOs

ה־API אינו חושף ישירות את ה־Entities של בסיס הנתונים.

המערכת משתמשת ב־DTOs להעברת מידע בין ה־API לשכבת השירות.

המיפוי בין DTOs ל־Entities מתבצע באמצעות AutoMapper.

---

## Async

פעולות I/O במערכת מבוצעות בצורה אסינכרונית.

לדוגמה:

```csharp
await repository.GetAll(...);
await repository.Add(...);
await repository.Save(...);
```

פעולות אלו תומכות גם ב־`CancellationToken` כאשר הדבר נדרש.

---

## Repository Pattern

הגישה ל־Database מתבצעת באמצעות Repository interfaces.

לדוגמה:

```text
IUserRepository
IEventRepository
IOrderRepository
ISeatRepository
IEventSeatRepository
```

המימושים נמצאים בשכבת Data.

השירותים מקבלים את ה־Repositories באמצעות Dependency Injection.

---

## Middleware

המערכת כוללת Middleware לטיפול רוחבי בבקשות.

### Exception Handling Middleware

מטפל בחריגות שלא טופלו ומחזיר תשובת שגיאה אחידה ללקוח.

חריגות לא צפויות נרשמות ברמת:

```text
Error
```

### Logging Middleware

מתעד את הבקשות והתגובות ומאפשר מעקב אחר פעילות ה־API.

### Performance Middleware

מודד את זמן ביצוע הבקשה.

### Correlation ID

לכל בקשה ניתן Correlation ID המאפשר לקשר בין רשומות Log השייכות לאותה בקשה.

---

## Logging

המערכת משתמשת ב־NLog דרך `ILogger`.

רמות הלוג העיקריות:

text
Debug
Information
Warning
Error

דוגמאות:

* בקשה נכנסת → `Information`
* התנגשות על משאב → `Warning`
* חריגה שלא טופלה → `Error`

אין לשמור בלוגים:

* סיסמאות
* JWT Tokens
* Request Bodies המכילים מידע רגיש

---

## Health Check

קיים endpoint:

```text
GET /health
```

ה־Health Check בודק את זמינות בסיס הנתונים.

---

## Swagger

בסביבת Development ניתן להשתמש ב־Swagger לצורך בדיקה ותיעוד של ה־API.

Swagger כולל תמיכה ב־JWT Bearer.

לאחר קבלת Token ניתן להזין אותו באמצעות:

```text
Authorize
```

בפורמט:

```text
Bearer <JWT>
```

---

## Tests

הפרויקט כולל Unit Tests באמצעות:

* xUnit
* Moq

הבדיקות מתמקדות בעיקר בשכבת Service.

נבדקים בין היתר:

* הצלחת פעולות
* כשלי Validation עסקיים
* משאבים שאינם קיימים
* משאבים שכבר נתפסו
* התנהגות משתמשים
* Authentication-related logic
* Optimistic Concurrency

### Concurrency Test

קיים Test המדגים מצב שבו שני משתמשים מנסים להזמין את אותו EventSeat:

```text
User 1 → Success
User 2 → Concurrency Conflict
```

---

## Dependency Injection

השירותים וה־Repositories נרשמים באמצעות Dependency Injection ב־`Program.cs`.

דוגמה:

```csharp
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IUserService, UserService>();
```

גם `IPasswordHasher<User>` נרשם דרך DI:

```csharp
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

אין ליצור Services באמצעות `new` בתוך Services אחרים.

---

## Configuration and Secrets

מידע רגיש אינו נשמר ב־Git.

הגדרות מקומיות כגון:

* Connection String
* JWT Key

צריכות להינתן באמצעות Configuration / User Secrets / Environment Variables.

אין להעלות ל־Git סיסמאות, מפתחות JWT או Connection Strings המכילים credentials.

---

## Running the Project

### 1. Clone

```bash
git clone <repository-url>
```

### 2. Configure PostgreSQL

יש לוודא ש־PostgreSQL מותקן ופועל.

יש להגדיר את `DefaultConnection` בסביבת הפיתוח המקומית.

### 3. Configure JWT

יש להגדיר:

```text
JWT:Key
JWT:Issuer
JWT:Audience
```

באמצעות User Secrets או Configuration מתאים.

### 4. Apply Migrations

```powershell
Update-Database
```

### 5. Run

```bash
dotnet run
```

או להפעיל את הפרויקט דרך Visual Studio.

### 6. Swagger

בסביבת Development ניתן לפתוח את כתובת ה־Swagger שה־API מציג בעת ההרצה.

---

## Main API Areas

המערכת מספקת endpoints עבור:

### Authentication

```text
POST /api/Auth/Login
```

### Users

ניהול משתמשים והרשאות.

### Events

יצירה ושליפה של אירועים.

### Seats

ניהול מושבים.

### Orders

יצירת הזמנות ובדיקת זמינות מושבים.

---

## Important Design Principles

הפרויקט מקפיד על:

* Layered Architecture
* Dependency Injection
* Repository Pattern
* DTO Pattern
* AutoMapper
* Async/Await
* Entity Framework Core
* PostgreSQL
* JWT Authentication
* Role-Based Authorization
* Optimistic Concurrency
* Global Exception Handling
* Correlation ID
* Structured Logging
* Unit Testing
* Secure password hashing

---

## Project Status

ה־Server כולל את שכבות ה־API, Service, Data ו־Core, כולל Authentication, Authorization, Database, Logging, Concurrency ו־Tests.

ה־React Client מפותח בנפרד ומתחבר ל־Web API.
