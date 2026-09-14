# SystemSalesTickets

## תיאור המערכת

SystemSalesTickets הוא Web API לניהול אירועים, מושבים והזמנת כרטיסים, שפותח כפרויקט סיום בקורס .NET — Web API.

המערכת מאפשרת:

* הרשמה והתחברות משתמשים עם JWT.
* ניהול אירועים ומושבים (Manager בלבד).
* הצגת זמינות מושבים לאירוע, כולל pagination אמיתי.
* ביצוע הזמנה של מושב לאירוע, עם מניעת הזמנה כפולה על אותו מושב.
* ניהול הרשאות לפי תפקידים (`User` / `Manager`).
* לוגים מובנים ומעקב אחר בקשות באמצעות CorrelationId.

---

## טכנולוגיות

* .NET 8 / ASP.NET Core Web API
* Entity Framework Core + Npgsql (PostgreSQL)
* JWT Authentication
* AutoMapper
* NLog
* xUnit + Moq
* Swagger / OpenAPI

---

## ארכיטקטורת המערכת

הפתרון בנוי בארבעה פרויקטים עיקריים בתוך `SystemSalesTickets.sln`:

text
SystemSalesTicketsDomain          →  SystemSalesTickets.Core     (Models, DTOs, Enums, Interfaces)
SystemSalesTicketsInfrastructure  →  SystemSalesTickets.Data     (DataContext, Repositories, Migrations)
SystemSalesTicketsApplication     →  SystemSalesTickets.Service  (Business Logic, MappingProfile)
SystemSalesTickets                →  SystemSalesTickets.Api      (Controllers, Middleware, Program.cs)
```

כיוון התלויות: `Core` אינו מפנה לאף פרויקט אחר. `Data` ו-`Service` מפנים ל-`Core` בלבד. `Api` מפנה ל-`Service`, ל-`Data` (לצורך רישום DI) ול-`Core`. אין Controller שמזריק `DataContext` ישירות — הגישה לנתונים עוברת תמיד דרך שכבת ה-Service.

> **הערה לתיקון:** כרגע `SystemSalesTickets.Service.csproj` מכיל הפניה ישירה לחבילות `Microsoft.EntityFrameworkCore` ו-`Npgsql`. מומלץ להסיר אותן — ה-Service אמור לדעת רק על ה-interfaces שב-Core, לא על ה-ORM.

פרויקט נוסף, `PasswordGenerator`, הוא כלי עזר עצמאי (console app) להפקת hash לסיסמאות ה-Seed — הוא אינו חלק מהרצת המערכת.

---

## משאב מוגבל ותחרות עליו

המשאב המוגבל הוא **`EventSeat`** — הצירוף של מושב (`Seat`) לאירוע (`Event`), עם השדה `IsAvailable`.

* התחרות מתרחשת כאשר שני משתמשים מנסים להזמין באותו רגע את אותו `EventSeat`.
* לישות `EventSeat` (וגם ל-`Seat`) קיים שדה `Guid Version` המסומן כ-`[ConcurrencyCheck]` ומוגדר כ-Concurrency Token דרך Fluent API ב-`DataContext.OnModelCreating`.
* `DataContext` דורס את `SaveChangesAsync` ומחליף את ה-`Version` בכל שורה שסומנה כ-`Modified`, כך שאין תלות בכך ש-service בודד יזכור לעדכן אותו.
* ב-`OrderService.AddOrder`: קריאה ל-`EventSeat`, בדיקה ש-`IsAvailable == true`, סימון `IsAvailable = false` ושמירה — הכול בתוך אותו scope/DbContext יחד עם יצירת ה-`Order`.
* אם בין הקריאה לשמירה מישהו אחר כבר תפס את המושב, נזרקת `DbUpdateConcurrencyException`, שנתפסת ב-`catch` ייעודי ומוחזרת כ-`Conflict` (409) עם הודעה ברורה, ונרשמת ללוג ברמת `Warning`.
* בנוסף קיים אינדקס ייחודי ברמת בסיס הנתונים על (`EventId`, `SeatId`) בטבלת `Orders`, כשכבת הגנה משנית.

---

## Authentication & Authorization

* התחברות (`POST /api/Auth`) מייצרת JWT עם Claims של `Role` ו-`NameIdentifier` (UserId), בתוקף ל-6 דקות.
* קיימים שני תפקידים: `User` ו-`Manager`.
* endpoints ניהוליים (ניהול אירועים, מושבים, משתמשים, צפייה בכל ההזמנות) מוגנים ב-`[Authorize(Roles = nameof(UserRole.Manager))]`.
* endpoints רגילים (הזמנה, הרשמה) מוגנים ב-`[Authorize]` בלבד או פתוחים (הרשמה).

---

## Endpoints עיקריים

| Controller | Method & Route | הרשאה |
|---|---|---|
| Auth | `POST /api/Auth` — התחברות | ללא |
| User | `POST /api/User` — הרשמה | ללא |
| User | `GET /api/User` — כל המשתמשים | Manager |
| User | `GET /api/User/{id}` | Manager |
| User | `PUT /api/User?id=` — הפיכת משתמש ל-Manager | Manager |
| Event | `GET /api/Event` — pagination | Manager |
| Event | `GET /api/Event/{name}` | User/Manager |
| Event | `POST /api/Event` | User/Manager |
| Seat | `GET /api/Seat`, `GET /api/Seat/{id}` | Manager |
| Seat | `POST /api/Seat` | Manager |
| Seat | `DELETE /api/Seat/{id}` | Manager |
| Order | `POST /api/Order` — הזמנת מושב | User/Manager |
| Order | `GET /api/Order`, `GET /api/Order/{id}` | Manager |

הרשימה המלאה והמעודכנת זמינה תמיד ב-Swagger לאחר הרצת השרת.

---

## Pagination, Validation, Async

* שליפות רשימה (`Users`, `Orders`, `Seats`, `Events`) תומכות ב-`pageNumber`/`pageSize`, עם `Skip`/`Take` בתוך השאילתה ו-`AsNoTracking` לקריאות בלבד.
* Validation בסיסי דרך Data Annotations על ה-Models/DTOs (`[Required]`, `[StringLength]` וכו').
* Validation עסקי (זמינות מושב, קיום Event/Seat, מניעת הזמנה כפולה) מתבצע בשכבת ה-Service.
* שרשרת הקריאה כולה אסינכרונית עם `CancellationToken` מועבר מה-Controller ועד ה-DbContext.

---

## Middleware

* **ExceptionHandlingMiddleware** — תופס חריגות לא מטופלות ומחזיר JSON אחיד.
* **LoggingMiddleware** — מייצר/מפיץ `CorrelationId` לכל בקשה ומתעד אותה בלוג.
* **PerformanceMiddleware** — מודד זמן ביצוע לכל בקשה.

---

## Logging (NLog)

הגדרות ב-`nlog.config`. ה-Api ממשיך להשתמש ב-`ILogger<T>` הרגיל; NLog מתחבר מתחתיו דרך `builder.Host.UseNLog()`.

* לוג לכל בקשה נכנסת עם ה-`CorrelationId`.
* חריגות — ברמת `Error`.
* התנגשויות concurrency (409) — ברמת `Warning`.
* לא נכתבים ללוג סיסמאות, JWT tokens או גוף בקשות הרשמה/התחברות.

---

## בסיס נתונים

PostgreSQL + EF Core Code-First. ה-`DataContext` כולל Seed data ל-`Users`, `Events`, `Seats`, `EventSeats` ו-`Order` לדוגמה, כך שניתן להריץ מיד לאחר `dotnet ef database update`.

> **הערה אבטחתית:** כרגע ה-connection string (כולל סיסמה) וה-JWT secret key שמורים בפועל בקבצים `appsettings.json` / `appsettings.Development.json` שנכנסים ל-repo. יש להעביר אותם ל-User Secrets לפני ההגשה (ראו הוראות למטה) ולוודא שהערכים לא מגיעים ל-Git.

---

## הרצה מקומית

### 1. שכפול הפרויקט

```bash
git clone <repository-url>
cd SystemSalesTickets
```

### 2. הגדרת Secrets (במקום appsettings)

```bash
dotnet user-secrets init --project SystemSalesTickets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SystemSalesTickets;Username=postgres;Password=<your-password>" --project SystemSalesTickets
dotnet user-secrets set "Jwt:Key" "<a-long-random-secret>" --project SystemSalesTickets
```

לאחר מכן יש להסיר את הסיסמה ואת ה-JWT key מ-`appsettings.json` / `appsettings.Development.json`.

### 3. הרמת PostgreSQL (אם אין התקנה קיימת)

```bash
docker run --name pg -e POSTGRES_PASSWORD=<your-password> -p 5432:5432 -d postgres:16
```

### 4. התקנה ובנייה

```bash
dotnet restore
dotnet build
```

### 5. Migrations

```bash
dotnet ef database update -p SystemSalesTicketsInfrastructure -s SystemSalesTickets
```

### 6. הרצה

```bash
dotnet run --project SystemSalesTickets
```

לאחר ההרצה, Swagger זמין בכתובת המוצגת בטרמינל (למשל `https://localhost:7074/swagger`).

---

## Unit Tests

בדיקות ב-`UnitTest` (xUnit + Moq) על שכבת ה-Service: `OrderServiceTests`, `SeatServiceTests`, `EventServiceTests`, `UserServiceTests` — כולל בדיקה על הזמנה מוצלחת, מושב לא זמין/לא קיים, וטיפול ב-`DbUpdateConcurrencyException`.

```bash
dotnet test
```

---

## משתמשי Demo

המערכת נטענת עם שני משתמשים בסיס-נתונים (ראו Seed ב-`DataContext`):

| Email | Role | סיסמה |
|---|---|---|
| `admin@example.com` | Manager | `111` |
| `user@example.com` | User | `222` |

> הסיסמאות נוצרו באמצעות פרויקט העזר `PasswordGenerator` ומאוחסנות ב-DB כ-hash בלבד. יש לוודא שהמיפוי בין המשתמשים לסיסמאות תואם בפועל למה שנוצר אצלך לפני ההגשה.

---

## מבנה עיקרי של הפתרון

```text
SystemSalesTickets.sln
│
├── SystemSalesTicketsDomain          (Core)
│   ├── Models
│   ├── DTOs
│   ├── Enums
│   ├── Interfaces
│   └── Repository (interfaces)
│
├── SystemSalesTicketsInfrastructure  (Data)
│   ├── DataContext.cs
│   ├── Repository.cs + <Entity>Repository.cs
│   └── Migrations
│
├── SystemSalesTicketsApplication     (Service)
│   ├── Service/*.cs
│   └── (MappingProfile ב-Core)
│
├── SystemSalesTickets                (Api)
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│   ├── appsettings*.json
│   └── nlog.config
│
├── UnitTest
└── PasswordGenerator                 (כלי עזר, לא חלק מהריצה)
```
