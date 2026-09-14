# SystemSalesTickets

## תיאור הפרויקט

**SystemSalesTickets** הוא Web API למערכת מכירת כרטיסים לאירועים.

המערכת מאפשרת:

* ניהול משתמשים והרשאות
* הרשמה והתחברות באמצעות JWT
* ניהול אירועים
* ניהול מושבים
* הזמנת מושבים לאירועים
* מניעת הזמנה כפולה
* Optimistic Concurrency
* בדיקות Unit ו־Concurrency
* Logging באמצעות NLog
* Health Check
* Swagger / OpenAPI

---

## טכנולוגיות

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Npgsql
* AutoMapper
* JWT Authentication
* ASP.NET Core Identity PasswordHasher
* xUnit
* Moq
* SQLite In-Memory עבור בדיקות Concurrency
* NLog
* Swagger / OpenAPI

---

## מבנה הפרויקט

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
│   ├── DTOs
│   ├── Enums
│   ├── Interfaces
│   ├── Models
│   ├── Repository
│   ├── MappingProfile.cs
│   └── SystemSalesTickets.Core.csproj
│
├── SystemSalesTicketsInfrastructure
│   ├── Migrations
│   ├── Repositories
│   ├── DataContext.cs
│   └── SystemSalesTickets.Data.csproj
│
├── SystemSalesTicketsApplication
│   ├── Service
│   ├── Background
│   └── SystemSalesTickets.Service.csproj
│
├── UnitTest
│   ├── EventServiceTests.cs
│   ├── OrderServiceTests.cs
│   ├── SeatServiceTests.cs
│   ├── UserServiceTests.cs
│   ├── ConcurrencyTests.cs
│   └── UnitTest.csproj
│
├── PasswordGenerator
│
├── nlog.config
├── .gitignore
└── README.md
```

### אחריות הפרויקטים

**SystemSalesTickets**

שכבת ה־API. מכילה Controllers, Middleware, Authentication, Authorization, Swagger ו־Dependency Injection.

**SystemSalesTicketsDomain**

שכבת ה־Core. מכילה Models, DTOs, Enums, Interfaces, Repository contracts ו־AutoMapper configuration.

**SystemSalesTicketsInfrastructure**

שכבת ה־Data. מכילה את `DataContext`, מימושי ה־Repositories ו־EF Core Migrations.

**SystemSalesTicketsApplication**

שכבת ה־Service. מכילה את הלוגיקה העסקית של המערכת.

**UnitTest**

מכיל את בדיקות היחידה ובדיקות ה־Optimistic Concurrency.

---

## Authentication ו־Authorization

המערכת משתמשת ב־JWT Authentication.

בעת Login:

1. המשתמש מאותר לפי Email.
2. הסיסמה נבדקת באמצעות `IPasswordHasher<User>`.
3. נוצר JWT.
4. ה־Role של המשתמש נוסף ל־Token.
5. ה־Token מוחזר ללקוח.

קיימים שני תפקידים:

* `User`
* `Manager`

Endpoints מוגנים משתמשים ב־`[Authorize]` וב־`[Authorize(Roles = "...")]`.

---

## Password Security

סיסמאות אינן נשמרות כטקסט רגיל.

המערכת משתמשת ב:

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

הגישה ל־Database מתבצעת באמצעות `DataContext` ו־Repository Pattern.

מבנה בסיס הנתונים מנוהל באמצעות EF Core Migrations.

יצירת Migration:

```powershell
Add-Migration MigrationName
```

עדכון בסיס הנתונים:

```powershell
Update-Database
```

ה־Connection String מוגדר באמצעות Configuration ואינו נשמר ב־Git.

---

## Resource Concurrency

`EventSeat` הוא המשאב המוגבל במערכת.

המערכת מונעת מצב שבו שני משתמשים מצליחים להזמין את אותו מושב לאותו אירוע.

ל־`EventSeat` קיים:

```csharp
Version
```

השדה מוגדר כ־Concurrency Token.

בעת שינוי `EventSeat`, ה־Version משתנה.

במקרה שבו שני משתמשים טוענים את אותו משאב:

```text
User 1 → Update → Success
User 2 → Update → Concurrency Conflict
```

ה־Concurrency exception מטופלת במערכת ומוחזרת ללקוח כ־HTTP `409 Conflict`.

בנוסף קיים Unique Index על:

```text
EventId + SeatId
```

כך שבסיס הנתונים מספק שכבת הגנה נוספת מפני הזמנה כפולה.

---

## Validation

ה־API משתמש ב־ASP.NET Core Model Validation וב־DataAnnotations.

בדיקות עסקיות מתבצעות בשכבת Service.

לדוגמה:

* נתונים חסרים
* אירוע שאינו קיים
* מושב שאינו קיים
* מושב שאינו פנוי
* הזמנה כפולה
* הרשאות משתמש

---

## DTOs

ה־API אינו חושף ישירות את ה־Entities של בסיס הנתונים.

המערכת משתמשת ב־DTOs להעברת מידע בין שכבות.

המיפוי בין DTOs ל־Entities מתבצע באמצעות AutoMapper.

---

## Async

פעולות I/O מבוצעות בצורה אסינכרונית.

לדוגמה:

```csharp
await repository.GetAll(...);
await repository.Add(...);
await repository.Save(...);
```

פעולות Database תומכות ב־`CancellationToken` במקומות שבהם הוא נדרש.

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

המימושים נמצאים בשכבת:

```text
SystemSalesTicketsInfrastructure
```

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

מתעד בקשות ומאפשר מעקב אחר פעילות ה־API.

### Performance Middleware

מודד את זמן ביצוע הבקשה.

### Correlation ID

לכל בקשה ניתן Correlation ID המאפשר לקשר בין רשומות Log השייכות לאותה בקשה.

---

## Logging

המערכת משתמשת ב־NLog דרך `ILogger`.

רמות הלוג העיקריות:

```text
Debug
Information
Warning
Error
```

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

```http
GET /health
```

ה־Health Check בודק את זמינות בסיס הנתונים.

---

## Swagger

בסביבת Development ניתן להשתמש ב־Swagger לצורך תיעוד ובדיקת ה־API.

Swagger מוגדר עם תמיכה ב־JWT Bearer.

לאחר קבלת Token ניתן להשתמש ב־`Authorize` ולהזין:

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
* משאבים שאינם קיימים
* משאבים שכבר תפוסים
* פעולות משתמשים
* Login
* הרשאות
* טיפול בהתנגשות
* Optimistic Concurrency

### Concurrency Tests

בדיקות ה־Concurrency משתמשות ב־SQLite In-Memory כדי לבודד את הבדיקות מבסיס הנתונים המקומי.

נבדק תרחיש שבו שני Contexts טוענים את אותו `EventSeat`:

```text
Context 1 → Update → Success
Context 2 → Update → DbUpdateConcurrencyException
```

---

## Dependency Injection

השירותים וה־Repositories נרשמים באמצעות Dependency Injection ב־`Program.cs`.

לדוגמה:

```csharp
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IUserService, UserService>();
```

גם PasswordHasher נרשם באמצעות DI:

```csharp
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

אין ליצור Services באמצעות `new` בתוך Services אחרים.

---

## Configuration and Secrets

מידע רגיש אינו נשמר ב־Git.

הגדרות מקומיות כגון:

* Connection Strings
* JWT Key
* הגדרות Development

צריכות להישמר באמצעות Configuration או User Secrets.

קבצי Configuration המכילים מידע רגיש אינם מיועדים להיכלל ב־Repository.

---

## HTTP Status Codes

ה־API משתמש בקודי HTTP בהתאם לתוצאת הפעולה.

דוגמאות:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
```

במקרה של Concurrency Conflict מוחזר:

```text
409 Conflict
```

---

## Error Handling

שגיאות לא צפויות מטופלות באמצעות Global Exception Middleware.

הלקוח מקבל תשובת שגיאה כללית ואינו מקבל פרטי Exception פנימיים.

לדוגמה:

```json
{
  "message": "An unexpected error occurred. Please try again later.",
  "statusCode": 500
}
```

---

## Git

הפרויקט משתמש ב־`.gitignore` כדי למנוע העלאה של קבצים שאינם צריכים להיות ב־Repository.

בין היתר:

```text
.vs/
bin/
obj/
appsettings.json
appsettings.Development.json
logs/
*.log
```

אין להעלות ל־Git סיסמאות, JWT Keys, Connection Strings או מידע רגיש אחר.

---

## Running the Project

### דרישות

* .NET 8 SDK
* PostgreSQL
* Visual Studio / Rider / VS Code

### הפעלה

יש להגדיר את ה־Connection String ואת הגדרות ה־JWT באמצעות Configuration / User Secrets.

לאחר מכן:

```powershell
dotnet restore
dotnet build
dotnet run --project SystemSalesTickets
```

Swagger זמין בסביבת Development.

Health Check:

```text
/health
```

---

## Project Architecture

המערכת בנויה בארכיטקטורה שכבתית:

```text
                ┌──────────────────────┐
                │         API          │
                │ Controllers/Middleware│
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │       Service        │
                │   Business Logic     │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │        Core          │
                │ Models / DTOs / APIs │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │        Data          │
                │ EF Core / PostgreSQL │
                └──────────────────────┘
```

ה־Core אינו תלוי בשכבות החיצוניות.

ה־Data וה־Service משתמשים ב־Core.

ה־API מחבר בין השכבות באמצעות Dependency Injection.
