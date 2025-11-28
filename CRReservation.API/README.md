# CRReservation.API - Backend

ASP.NET Core Web API dla systemu rezerwacji sal uczelnianych CRReservation.

## Wymagania wstępne

- **.NET 8.0 SDK** - [Pobierz i zainstaluj](https://dotnet.microsoft.com/download/dotnet/8.0)
- Dla developmentu: SQLite (domyślnie skonfigurowany)
- Dla produkcji: MS SQL Server 2022

## Instalacja i uruchomienie

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

## Konfiguracja bazy danych

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

## Testowanie API

Po uruchomieniu aplikacji, przetestuj podstawowe endpointy:

```bash
# Test wszystkich endpointów
curl http://localhost:5000/api/classrooms
curl http://localhost:5000/api/users
curl http://localhost:5000/api/reservations

# Test pojedynczych zasobów
curl http://localhost:5000/api/users/1
curl http://localhost:5000/api/classrooms/1
```

## Dostępne endpointy

### 🏫 Sale (ClassRooms)
- `GET /api/classrooms` - Pobierz wszystkie sale
- `GET /api/classrooms/{id}` - Pobierz salę po ID
- `POST /api/classrooms` - Dodaj nową salę
- `PUT /api/classrooms/{id}` - Aktualizuj salę
- `DELETE /api/classrooms/{id}` - Usuń salę

### 👥 Użytkownicy (Users)
- `GET /api/users` - Pobierz wszystkich użytkowników z rolami
- `GET /api/users/{id}` - Pobierz użytkownika po ID
- `POST /api/users` - Dodaj nowego użytkownika
- `PUT /api/users/{id}` - Aktualizuj użytkownika
- `DELETE /api/users/{id}` - Usuń użytkownika

### 📅 Rezerwacje (Reservations)
- `GET /api/reservations` - Pobierz wszystkie rezerwacje
- `GET /api/reservations/{id}` - Pobierz rezerwację po ID
- `POST /api/reservations` - Dodaj nową rezerwację
- `PUT /api/reservations/{id}` - Aktualizuj rezerwację
- `DELETE /api/reservations/{id}` - Usuń rezerwację

### 🔧 Dodatkowe endpointy
- `GET /api/simple-users` - Licznik użytkowników (testowy)

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

## Dane testowe (Seed Data)

Przy pierwszym uruchomieniu aplikacja automatycznie utworzy i wypełni bazę danych:

### Role użytkowników
- `admin` - Administrator systemu
- `prowadzacy` - Prowadzący zajęcia
- `student` - Student

### Przykładowe dane
- **3 sale**: Sala 101, 202, 303
- **3 użytkownicy**:
  - Jan Kowalski (admin) - `jan.kowalski@example.com`
  - Anna Nowak (prowadzący)
  - Piotr Wiśniewski (student)
- **2 grupy**: Informatyka I rok, Zarządzanie II rok
- **2 rezerwacje**: przykładowe rezerwacje sal

## Rozwiązywanie problemów

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

## Struktura projektu

```
CRReservation.API/
├── Controllers/          # API endpoints
├── Data/                 # DbContext, SeedData
├── Models/               # EF Core entities
├── DTOs/                 # Data Transfer Objects
├── SQLScripts/           # MS SQL skrypt
├── appsettings.json      # Konfiguracja
├── Program.cs           # Startup
└── README.md            # Ten plik
```