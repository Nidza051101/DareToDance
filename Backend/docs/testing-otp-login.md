# Testiranje OTP login flow-a (Swagger)

Privremeni dev vodič dok ne postoji pravi email/SMS servis — kod se čita iz logova kontejnera.

## Priprema

```bash
cd Backend
docker compose up -d
```

Swagger UI: **http://localhost:5015/swagger/index.html**

Logovi API kontejnera (odatle se čita OTP kod):

```bash
docker logs daretodance-api --tail 20 -f
```

---

## A) Flow preko email-a

### 1. Kreiraj korisnika — `POST /users`

```json
{
  "email": "test@example.com",
  "firstName": "Pera",
  "lastName": "Peric",
  "password": "Lozinka123!"
}
```

Očekivano: **201 Created**. Korisnik automatski dobija ulogu `Member` (`UserRole.Member`, podešeno u `User.Create()`).

### 2. Zatraži login kod — `POST /auth/login/email`

```json
{ "email": "test@example.com" }
```

Očekivano: **200**, `{"message":"Ako nalog postoji, kod je poslat na uneti email."}`
(Isti odgovor i ako email ne postoji — namerno, radi sprečavanja otkrivanja da li nalog postoji.)

### 3. Pročitaj kod iz logova

```
[DEV] Login kod za email test@example.com: 483920
```

### 4. Verifikuj kod → dobij JWT — `POST /auth/login/verify`

```json
{ "recipient": "test@example.com", "code": "483920" }
```

Očekivano: **200**

```json
{ "accessToken": "eyJhbGciOi...", "expiresAtUtc": "2026-08-19T..." }
```

---

## B) Flow preko telefona

### 1. Kreiraj korisnika sa telefonom — `POST /users`

```json
{
  "email": "phone.test@example.com",
  "firstName": "Ana",
  "lastName": "Anic",
  "password": "Lozinka123!",
  "phone": "+381601234567"
}
```

Odgovor sada uključuje i `phone` polje — proveri da je sačuvano tačno onako kako je uneto.

### 2. Zatraži login kod — `POST /auth/login/phone`

```json
{ "phone": "+381601234567" }
```

### 3. Pročitaj kod iz logova

```
[DEV] Login kod za telefon +381601234567: 375310
```

### 4. Verifikuj — `POST /auth/login/verify`

```json
{ "recipient": "+381601234567", "code": "375310" }
```

Isti endpoint za oba kanala — prepoznaje da li je `recipient` email ili telefon.

---

## C) Negativni scenariji (namerno izazovi grešku)

| Scenario                                                      | Endpoint                | Očekivano                          |
|-----------------------------------------------------------------|--------------------------|-------------------------------------|
| Pogrešan kod                                                     | `POST /auth/login/verify` | `400 Auth.InvalidCode`              |
| Zahtev za novi kod dok prethodni još važi (< 60s, `ResendCooldownSeconds`) | `POST /auth/login/email` ili `/phone` | `409 Auth.CodeAlreadySent` |
| Kod posle isteka (> 60s, `OtpSettings.ExpirySeconds`)            | `POST /auth/login/verify` | `400 Auth.InvalidCode`              |
| Email/telefon koji ne postoji u bazi                             | `POST /auth/login/email` ili `/phone` | `200` (isti generički odgovor, ne otkriva se) |

---

## Napomene

- Kod se hešuje pre čuvanja u bazi (`login_codes.code_hash`) — nikad se ne čuva u plain-textu.
- Kod se troši (`ConsumedAtUtc`) posle prve uspešne verifikacije — ne može se iskoristiti dvaput.
- `OtpSettings` (dužina koda, trajanje, max pokušaja, cooldown) — `appsettings.json`.
- `JwtSettings:Secret` **ne sme** u `appsettings.json` — lokalno preko `dotnet user-secrets`, u `docker-compose.yml` preko `JwtSettings__Secret` env varijable (dev-only vrednost, zamena za pravu tajnu u produkciji).
