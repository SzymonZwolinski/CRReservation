# CRReservation.API - Backend

ASP.NET Core Web API dla systemu rezerwacji sal uczelnianych CRReservation z pełną autoryzacją JWT i role-based access control.

## ✨ Funkcjonalności

- 🔐 **JWT Authentication** - Bezpieczne logowanie i autoryzacja
- 👥 **Role-Based Access Control** - Administrator, Prowadzący, Student
- 📅 **Rezerwacje sal** - Tworzenie, filtrowanie, zatwierdzanie
- 🏫 **Zarządzanie salami** - CRUD operacje z kontrolą dostępu
- 🔍 **Filtrowanie i wyszukiwanie** - Zaawansowane query parameters
- ✅ **Approval Workflow** - Zatwierdzanie/odrzucanie rezerwacji
- 📊 **Sprawdzanie dostępności** - Zapobieganie konfliktom czasowym

## Wymagania wstępne

- **.NET 8.0 SDK** - [Pobierz i zainstaluj](https://dotnet.microsoft.com/download/dotnet/8.0)
- Dla developmentu: SQLite (domyślnie skonfigurowany)
- Dla produkcji: MS SQL Server 2022

## 🚀 Instalacja i uruchomienie

### Krok 1: Instalacja .NET SDK

```bash
# Na macOS (jeśli nie masz .NET)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0

# Dodaj do PATH (w ~/.zshrc lub ~/.bashrc)
export PATH="$PATH:$HOME/.dotnet"
```

### Krok 2: Przygotowanie projektu

```bash
# Przejdź do katalogu API
cd CRReservation.API

# Przywróć pakiety NuGet
dotnet restore

# Zbuduj projekt
dotnet build
```

### Krok 3: Uruchomienie aplikacji

```bash
# Uruchom API
dotnet run
```

**API będzie dostępne pod adresem:** `http://localhost:5000`

## 🔐 Autoryzacja i Role

### Role użytkowników
- **`admin`** - Pełny dostęp do wszystkich zasobów
- **`prowadzacy`** - Dostęp do rezerwacji i przeglądania sal
- **`student`** - Ograniczony dostęp, rezerwacje wymagają zatwierdzenia

### Logowanie

```bash
# Zaloguj się jako admin
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"jan.kowalski@example.com","password":"admin123"}'

# Zaloguj się jako student
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"piotr.wisniewski@example.com","password":"student123"}'
```

**Odpowiedź zawiera token JWT do użycia w kolejnych requestach.**

### Używanie tokena

```bash
# Przykładowe użycie tokena w requestach
curl -X GET "http://localhost:5000/api/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📊 Konfiguracja bazy danych

### Opcja 1: SQLite (domyślna - development)

Projekt jest domyślnie skonfigurowany do używania SQLite. Przy pierwszym uruchomieniu:
- Automatycznie utworzy plik `CRReservation.db`
- Wczyta dane testowe (sale, użytkownicy, rezerwacje)

### Opcja 2: MS SQL Server (produkcja)

1. Zainstaluj MS SQL Server
2. Uruchom SQL Server Management Studio
3. Połącz się z serwerem
4. Otwórz plik `SQLScripts/CreateDatabase_MS_SQL.sql`
5. Wykonaj skrypt - utworzy bazę `SalaRezerwacja` z pełną strukturą

Alternatywnie, zaktualizuj `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SalaRezerwacja;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

## 📋 Pełna dokumentacja API

### 🔑 Autoryzacja (Auth)

#### POST /api/auth/login
**Logowanie użytkownika**

```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"jan.kowalski@example.com","password":"admin123"}'
```

**Odpowiedź:**
```json
{
  "success": true,
  "message": "Zalogowano pomyślnie",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "role": "admin",
  "userName": "Jan Kowalski",
  "email": "jan.kowalski@example.com",
  "expiration": "2025-11-30T14:15:38.316257+01:00"
}
```

---

### 🏫 Sale (ClassRooms)

#### GET /api/classrooms
**Pobierz wszystkie sale**
- **Autoryzacja:** Wymagana (dowolna rola)

```bash
curl -X GET "http://localhost:5000/api/classrooms" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### GET /api/classrooms/{id}
**Pobierz salę po ID**
- **Autoryzacja:** Wymagana (dowolna rola)

#### GET /api/classrooms/available
**Sprawdź dostępne sale w podanym terminie**
- **Autoryzacja:** Wymagana (dowolna rola)
- **Parametry:** `start` (DateTime), `end` (DateTime)

```bash
curl -X GET "http://localhost:5000/api/classrooms/available?start=2025-12-02T10:00&end=2025-12-02T12:00" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### POST /api/classrooms
**Dodaj nową salę**
- **Autoryzacja:** Wymagana (rola: admin)

#### PUT /api/classrooms/{id}
**Aktualizuj salę**
- **Autoryzacja:** Wymagana (rola: admin)

#### DELETE /api/classrooms/{id}
**Usuń salę**
- **Autoryzacja:** Wymagana (rola: admin)

---

### 👥 Użytkownicy (Users)

#### GET /api/users
**Pobierz wszystkich użytkowników**
- **Autoryzacja:** Wymagana (rola: admin)

```bash
curl -X GET "http://localhost:5000/api/users" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

#### GET /api/users/{id}
**Pobierz użytkownika po ID**
- **Autoryzacja:** Wymagana (dowolna rola)

---

### 📅 Rezerwacje (Reservations)

#### GET /api/reservations
**Pobierz wszystkie rezerwacje**
- **Autoryzacja:** Wymagana (dowolna rola)

```bash
curl -X GET "http://localhost:5000/api/reservations" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### GET /api/reservations/filter
**Filtrowanie rezerwacji**
- **Autoryzacja:** Wymagana (dowolna rola)
- **Parametry query:**
  - `startDate` (DateTime) - Data początkowa
  - `endDate` (DateTime) - Data końcowa
  - `userId` (int) - ID użytkownika
  - `status` (string) - Status rezerwacji

```bash
# Filtruj rezerwacje potwierdzone
curl -X GET "http://localhost:5000/api/reservations/filter?status=potwierdzona" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filtruj rezerwacje użytkownika w zakresie dat
curl -X GET "http://localhost:5000/api/reservations/filter?userId=3&startDate=2025-12-01&endDate=2025-12-31" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### GET /api/reservations/{id}
**Pobierz rezerwację po ID**
- **Autoryzacja:** Wymagana (dowolna rola)

#### POST /api/reservations
**Utwórz nową rezerwację**
- **Autoryzacja:** Wymagana (role: student, prowadzący, admin)
- **Logika biznesowa:**
  - Studenci: Status "oczekująca" (wymaga zatwierdzenia)
  - Admini: Status "potwierdzona" (natychmiastowa)
  - Sprawdzanie dostępności sali

```bash
curl -X POST "http://localhost:5000/api/reservations" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "classRoomId": 1,
    "startDateTime": "2025-12-02T10:00:00",
    "endDateTime": "2025-12-02T12:00:00",
    "reservationDate": "2025-12-02T00:00:00",
    "isRecurring": false
  }'
```

#### PUT /api/reservations/{id}/approve
**Zatwierdź rezerwację**
- **Autoryzacja:** Wymagana (rola: admin)

```bash
curl -X PUT "http://localhost:5000/api/reservations/1/approve" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

#### PUT /api/reservations/{id}/reject
**Odrzuć rezerwację**
- **Autoryzacja:** Wymagana (rola: admin)

#### PUT /api/reservations/{id}/revoke
**Anuluj rezerwację**
- **Autoryzacja:** Wymagana (rola: admin)

#### PUT /api/reservations/{id}
**Aktualizuj rezerwację**
- **Autoryzacja:** Wymagana (dowolna rola)

#### DELETE /api/reservations/{id}
**Usuń rezerwację**
- **Autoryzacja:** Wymagana (dowolna rola)

---

## Struktura bazy danych

### Tabele
- **Role** - Role użytkowników (admin, prowadzący, student)
- **Users** - Użytkownicy systemu
- **ClassRooms** - Sale dydaktyczne
- **Groups** - Grupy użytkowników
- **UserGroups** - Relacja wiele-do-wielu między użytkownikami a grupami
- **Reservations** - Rezerwacje sal

### Kluczowe relacje
- User -> Role (wiele-do-jednego)
- Reservation -> ClassRoom (wiele-do-jednego)
- Reservation -> User (wiele-do-jednego)
- Reservation -> Group (wiele-do-jednego, opcjonalne)
- User <-> Group (wiele-do-wielu przez UserGroups)

## 👤 Dane testowe (Seed Data)

Przy pierwszym uruchomieniu aplikacja automatycznie utworzy i wypełni bazę danych:

### Role użytkowników
- `admin` - Pełny dostęp do wszystkich funkcji
- `prowadzacy` - Dostęp do rezerwacji i przeglądania
- `student` - Ograniczony dostęp, rezerwacje wymagają zatwierdzenia

### Konta testowe
| Email | Hasło | Rola | Opis |
|-------|-------|------|------|
| `jan.kowalski@example.com` | `admin123` | admin | Administrator systemu |
| `anna.nowak@example.com` | `prowadzacy123` | prowadzący | Prowadzący zajęcia |
| `piotr.wisniewski@example.com` | `student123` | student | Student |

### Przykładowe dane
- **3 sale**: Sala 101 (30 miejsc), Sala 202 (50 miejsc), Sala 303 (20 miejsc)
- **3 użytkownicy** z różnymi rolami
- **2 grupy**: Informatyka I rok, Zarządzanie II rok
- **2 rezerwacje**: przykładowe rezerwacje z różnymi statusami

### Statusy rezerwacji
- `oczekujaca` - Oczekuje na zatwierdzenie (studenci)
- `potwierdzona` - Zatwierdzona rezerwacja
- `odrzucona` - Odrzucona przez administratora
- `anulowana` - Anulowana rezerwacja

## 🧪 Przykłady testowania

### Scenariusz 1: Logowanie i podstawowe operacje

```bash
# 1. Zaloguj się jako student
TOKEN=$(curl -s -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"piotr.wisniewski@example.com","password":"student123"}' | jq -r '.token')

# 2. Pobierz listę sal
curl -X GET "http://localhost:5000/api/classrooms" \
  -H "Authorization: Bearer $TOKEN"

# 3. Sprawdź dostępne sale
curl -X GET "http://localhost:5000/api/classrooms/available?start=2025-12-02T10:00&end=2025-12-02T12:00" \
  -H "Authorization: Bearer $TOKEN"

# 4. Utwórz rezerwację (status: oczekująca)
curl -X POST "http://localhost:5000/api/reservations" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "classRoomId": 1,
    "startDateTime": "2025-12-02T10:00:00",
    "endDateTime": "2025-12-02T12:00:00",
    "reservationDate": "2025-12-02T00:00:00",
    "isRecurring": false
  }'
```

### Scenariusz 2: Zarządzanie rezerwacjami przez administratora

```bash
# 1. Zaloguj się jako admin
ADMIN_TOKEN=$(curl -s -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"jan.kowalski@example.com","password":"admin123"}' | jq -r '.token')

# 2. Pobierz wszystkie rezerwacje oczekujące
curl -X GET "http://localhost:5000/api/reservations/filter?status=oczekujaca" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# 3. Zatwierdź rezerwację (zmień ID na rzeczywisty)
curl -X PUT "http://localhost:5000/api/reservations/1/approve" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# 4. Pobierz wszystkich użytkowników (tylko admin)
curl -X GET "http://localhost:5000/api/users" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

### Scenariusz 3: Filtrowanie i wyszukiwanie

```bash
# Filtrowanie po statusie
curl -X GET "http://localhost:5000/api/reservations/filter?status=potwierdzona" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filtrowanie po użytkowniku i zakresie dat
curl -X GET "http://localhost:5000/api/reservations/filter?userId=3&startDate=2025-12-01&endDate=2025-12-31" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Sprawdzenie dostępności sali w konkretnym terminie
curl -X GET "http://localhost:5000/api/classrooms/available?start=2025-12-15T09:00&end=2025-12-15T11:00" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🔧 Rozwiązywanie problemów

### Problem: "dotnet: command not found"
```bash
# Zainstaluj .NET 8.0
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0

# Dodaj do PATH w ~/.zshrc lub ~/.bashrc
export PATH="$PATH:$HOME/.dotnet"
```

### Problem: "Connection refused" na porcie 5001
- Aplikacja działa na **HTTP 5000**, nie HTTPS 5001
- Użyj: `http://localhost:5000`

### Problem: "401 Unauthorized" podczas logowania
- Sprawdź czy używasz prawidłowych danych logowania
- Hasła są zahashowane BCrypt w bazie danych
- Upewnij się, że używasz kont testowych z sekcji "Dane testowe"

### Problem: "401 Unauthorized" na chronionych endpointach
- Musisz najpierw się zalogować i uzyskać JWT token
- Token wygasa po 24 godzinach
- Sprawdź czy token jest prawidłowy: `curl -H "Authorization: Bearer TOKEN" URL`

### Problem: "403 Forbidden" na endpointach administracyjnych
- Użyj konta administratora do logowania
- Email: `jan.kowalski@example.com`, Hasło: `admin123`
- Tylko administrator ma dostęp do zarządzania użytkownikami i salami

### Problem: POST endpointy nie działają
- Wszystkie POST endpointy wymagają `[FromBody]` w parametrach
- Upewnij się, że wysyłasz prawidłowy JSON
- Sprawdź czy masz odpowiednie uprawnienia (rola)

### Problem: Rezerwacja nie została utworzona
- Sprawdź dostępność sali w podanym terminie
- Studenci mogą tworzyć tylko rezerwacje ze statusem "oczekująca"
- Admini automatycznie zatwierdzają rezerwacje

### Problem: Po zmianie kodu API nie działa inaczej
```bash
# Zawsze po zmianie kodu:
dotnet build
dotnet run
```

### Problem: Baza danych nie została utworzona
- Sprawdź czy aplikacja uruchomiła się bez błędów
- Sprawdź logi konsoli
- Plik `CRReservation.db` powinien pojawić się w katalogu API

### Problem: "The signature key was not found"
- Token JWT wygasł lub został unieważniony
- Zaloguj się ponownie, aby uzyskać nowy token

## 📁 Struktura projektu

```
CRReservation.API/
├── Controllers/              # API endpoints z autoryzacją
│   ├── AuthController.cs     # Logowanie i JWT
│   ├── UsersController.cs    # Zarządzanie użytkownikami
│   ├── ClassRoomsController.cs # Zarządzanie salami
│   └── ReservationsController.cs # Rezerwacje z workflow
├── Data/                     # Konfiguracja bazy danych
│   ├── ApplicationDbContext.cs # EF Core DbContext
│   └── SeedData.cs          # Dane testowe
├── Models/                   # Encje EF Core
├── DTOs/                     # Data Transfer Objects
├── Services/                 # Logika biznesowa
│   ├── AuthService.cs        # Autoryzacja i hasła
│   └── JwtService.cs         # Generowanie JWT
├── SQLScripts/               # Skrypt MS SQL
├── appsettings.json          # Konfiguracja JWT
├── Program.cs               # Startup i middleware
└── README.md                # Ten plik
```

## 🎯 Podsumowanie funkcjonalności

✅ **JWT Authentication** - Bezpieczne logowanie z tokenami
✅ **Role-Based Access Control** - Admin/Prowadzący/Student
✅ **Rezerwacje z workflow** - Tworzenie, zatwierdzanie, odrzucanie
✅ **Filtrowanie i wyszukiwanie** - Zaawansowane query parameters
✅ **Sprawdzanie dostępności** - Zapobieganie konfliktom
✅ **Zarządzanie salami** - CRUD z kontrolą dostępu
✅ **SQLite/MS SQL** - Wsparcie dla różnych baz danych
✅ **Seed Data** - Gotowe dane testowe
✅ **RESTful API** - Standardowe endpointy HTTP