# SystemSalesTickets

## תיאור המערכת

SystemSalesTickets הוא Web API לניהול אירועים, מושבים והזמנת כרטיסים.

המערכת מאפשרת:

* ניהול משתמשים והרשאות.
* התחברות באמצעות JWT.
* ניהול אירועים.
* ניהול מושבים.
* הצגת זמינות מושבים לאירועים.
* ביצוע הזמנות.
* מניעת הזמנה כפולה של אותו מושב.
* ניהול הרשאות לפי תפקידים.
* שמירת לוגים ומעקב אחר בקשות באמצעות CorrelationId.

המערכת פותחה במסגרת פרויקט סיום בקורס .NET Web API.

---

## טכנולוגיות

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Npgsql
* JWT Authentication
* AutoMapper
* xUnit
* Moq
* NLog
* Swagger / OpenAPI

---

## ארכיטקטורת המערכת

המערכת מחולקת לשכבות:

```text
SystemSalesTickets.Core
        ↑
        │
SystemSalesTickets.Data

SystemSalesTickets.Core
        ↑
        │
SystemSalesTickets.Service

SystemSalesTickets.Core
        ↑
        │
SystemSalesTickets.API
```

### Core

מכיל את:

* Models
* DTOs
* Interfaces
* Repository interfaces
* Service interfaces

השכבה אינה תלויה ב־Data או ב־Service.

### Data

מכילה:

* DbContext
* Entity Framework Core
* Repositories
* Migrations
* PostgreSQL configuration

### Service

מכילה:

* Business Logic
* Services
* AutoMapper Profiles
* טיפול בחריגות עסקיות

### API

מכילה:

* Controllers
* Middleware
* Authentication / Authorization
* Dependency Injection
* Configuration
* Swagger

---

## משאב מוגבל

המשאב המוגבל במערכת הוא **מושב לאירוע** (`EventSeat`).

לכל שילוב של:

```text
EventId + SeatId
```

קיים `EventSeat` אחד.

לכל EventSeat נשמר:

```text
IsAvailable
Version
```

כאשר מושב מוזמן, הערך:

```text
IsAvailable = false
```

ולכן לא ניתן להזמין אותו שוב.

בנוסף קיימת הגבלה ברמת בסיס הנתונים על הזמנה כפולה של אותו מושב לאותו אירוע.

---

## מניעת הזמנה כפולה – Optimistic Concurrency

המערכת משתמשת ב־Optimistic Concurrency.

ל־`EventSeat` קיים שדה:

```csharp
public Guid Version { get; set; }
```

השדה מוגדר כ־Concurrency Token ב־Entity Framework Core.

בעת שינוי EventSeat, ערך ה־Version משתנה.

אם שני משתמשים מנסים להזמין את אותו מושב במקביל, רק אחד מהם יצליח לשמור את השינוי.

הבקשה השנייה תגרום ל־:

```text
DbUpdateConcurrencyException
```

והמערכת מחזירה:

```text
HTTP 409 Conflict
```

בנוסף קיימת הגבלת Unique Database עבור:

```text
EventId + SeatId
```

כדי לספק שכבת הגנה נוספת מפני הזמנה כפולה.

---

## Authentication ו־Authorization

המערכת משתמשת ב־JWT.

לאחר התחברות מוצלח נוצר JWT המכיל Claims כגון:

* UserId
* Role

קיימים לפחות שני תפקידים במערכת:

* User
* Manager

פעולות מסוימות מוגבלות למשתמשים בעלי הרשאת Manager.

לדוגמה:

```text
[Authorize(Roles = nameof(UserRole.Manager))]
```

פעולות הזמנה דורשות משתמש מחובר.

---

## REST API

ה־API משתמש ב־HTTP methods בהתאם לפעולה:

```text
GET     - שליפת מידע
POST    - יצירת משאב
PUT     - עדכון
DELETE  - מחיקה
```

ה־API משתמש בקודי HTTP מתאימים, ביניהם:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Swagger זמין לצורך צפייה ובדיקת ה־API.

---

## Pagination

שליפת רשימות תומכת ב־pagination.

הנתונים נשלפים מבסיס הנתונים באמצעות:

```csharp
Skip()
Take()
```

ולאחר מכן מוחזרים ללקוח באמצעות `PagedResponse`.

פעולות קריאה שאינן דורשות Tracking משתמשות ב־:

```csharp
AsNoTracking()
```

---

## Validation

קיימות שתי רמות Validation.

### Data Annotations

Validation בסיסי מתבצע ב־DTOs באמצעות:

```csharp
[Required]
[StringLength]
[Range]
[EmailAddress]
[Phone]
```

### Business Validation

חוקים עסקיים נבדקים בשכבת Service.

לדוגמה:

* בדיקה שהמושב קיים.
* בדיקה שהמושב זמין.
* מניעת הזמנה כפולה.
* בדיקות הקשורות לאירוע ולהזמנה.

---

## Async ו־CancellationToken

פעולות גישה לנתונים ולוגיקה עסקית מתבצעות באופן אסינכרוני.

לדוגמה:

```csharp
Task<T>
Task<List<T>>
Task<int>
```

פעולות EF Core מקבלות `CancellationToken` כאשר הדבר נדרש.

---

## Middleware

המערכת כוללת Middleware עבור:

### Exception Handling

מטפל בחריגות ומחזיר תשובת JSON אחידה ללקוח.

### CorrelationId / Logging

לכל Request נוצר `CorrelationId`.

ה־CorrelationId מאפשר לעקוב אחר Request מסוים בלוגים.

### Performance

נמדד זמן הביצוע של Requests.

---

## Logging

המערכת משתמשת ב־NLog.

הלוגים כוללים מידע כגון:

```text
Date/Time
Log Level
CorrelationId
UserId
Logger
Message
Exception
```

רמות הלוגים כוללות:

```text
Information
Warning
Error
```

אירועי Conflict נרשמים כ־Warning.

חריגות נרשמות כ־Error.

לא נשמרים בלוגים:

* Passwords
* JWT Tokens
* מידע רגיש

---

## Database

המערכת משתמשת ב־PostgreSQL וב־Entity Framework Core Code First.

קיימות Migration-ים עבור יצירת ועדכון מבנה בסיס הנתונים.

לפני הרצת המערכת יש לוודא ש־PostgreSQL מותקן ופועל.

---

## הגדרת הפרויקט מקומית

### 1. Clone

יש לשכפל את ה־Repository:

```bash
git clone <repository-url>
```

ולהיכנס לתיקיית הפרויקט:

```bash
cd SystemSalesTickets
```

### 2. הגדרת Connection String

יש להגדיר Connection String עבור PostgreSQL.

אין לשמור סיסמאות או Secrets בתוך Git.

מומלץ להשתמש ב־ASP.NET Core User Secrets בסביבת Development.

לדוגמה:

```bash
dotnet user-secrets init
```

ולאחר מכן להגדיר את ה־Connection String באמצעות User Secrets.

### 3. התקנת Dependencies

```bash
dotnet restore
```

### 4. בניית הפרויקט

```bash
dotnet build
```

### 5. הרצת Migrations

יש להריץ את ה־EF Core migrations על בסיס הנתונים.

לדוגמה:

```bash
dotnet ef database update
```

### 6. הרצת ה־API

```bash
dotnet run
```

לאחר ההרצה ניתן להשתמש ב־Swagger כדי לבדוק את ה־API.

---

## Swagger

לאחר הפעלת ה־API ניתן לפתוח את Swagger דרך כתובת ה־Swagger המוצגת בטרמינל בעת ההרצה.

Swagger מאפשר:

* צפייה ב־Endpoints.
* בדיקת Request / Response.
* בדיקת Authentication.
* ביצוע קריאות API.

---

## Unit Tests

המערכת כוללת Unit Tests עבור שכבת ה־Service.

הבדיקות משתמשות ב:

* xUnit
* Moq

הבדיקות מכסות בין היתר:

* פעולות Service.
* הצלחת הזמנה.
* מושב שאינו זמין.
* EventSeat שאינו קיים.
* טיפול ב־Concurrency Exception.
* החזרת Conflict במקרה של התנגשות.

להרצת הבדיקות:

```bash
dotnet test
```

---

## Demo Users

לצורך הדגמת המערכת קיימים משתמשי Demo בעלי תפקידים שונים.

### User

```text
Role: User
```

### Manager

```text
Role: Manager
```

יש להשתמש בפרטי המשתמשים המוגדרים ב־Seed של הפרויקט לצורך התחברות ובדיקת הרשאות.

אין להשתמש ב־Secrets אמיתיים בסביבת Production.

---

## מבנה עיקרי של המערכת

```text
SystemSalesTickets
│
├── Core
│   ├── DTOs
│   ├── Models
│   ├── Interfaces
│   └── Repository
│
├── Data
│   ├── DataContext
│   ├── Repositories
│   └── Migrations
│
├── Service
│   ├── Services
│   └── MappingProfile
│
├── API
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│   └── Configuration
│
└── UnitTest
    └── Service Tests
```

---

## מטרת הפרויקט

מטרת הפרויקט היא להדגים פיתוח Web API ב־.NET תוך שימוש ב:

* Layered Architecture
* Dependency Injection
* Entity Framework Core
* DTOs
* AutoMapper
* REST
* Async / Await
* JWT
* Authorization
* Middleware
* Logging
* Unit Testing
* PostgreSQL
* Optimistic Concurrency
* Pagination
* Validation
