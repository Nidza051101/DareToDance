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
  "password": "Password123!"
}
```

Očekivano: **201 Created**. Korisnik automatski dobija ulogu `Member` (`UserRole.Member`, podešeno u `User.Create()`).

### 2. Zatraži login kod — `POST /auth/login/email`

```json
{ "email": "test@example.com" }
```

Očekivano: **200**, `{"message":"If an account exists, a code has been sent to the given email."}`
(Isti odgovor i ako email ne postoji — namerno, radi sprečavanja otkrivanja da li nalog postoji.)

### 3. Pročitaj kod iz logova

```
[DEV] Login code for email test@example.com: 483920
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
  "password": "Password123!",
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
[DEV] Login code for phone +381601234567: 375310
```

### 4. Verifikuj — `POST /auth/login/verify`

```json
{ "recipient": "+381601234567", "code": "375310" }
```

Isti endpoint za oba kanala — prepoznaje da li je `recipient` email ili telefon.

---

## C) Automatsko blokiranje posle uzastopnih pogrešnih kodova

Blokiranje korisnika (`UserStatus.Blocked`) može da se desi na dva načina: admin ga ručno blokira
(`POST /users/{id}/block`), ili sistem automatski blokira nalog posle `OtpSettings.MaxFailedAttempts`
(trenutno **3**) uzastopnih pogrešnih kodova na istom aktivnom login kodu.

**Bitno je vreme:** kod ističe posle `OtpSettings.ExpirySeconds` (60s), a svaki novi zahtev za kod
resetuje brojač pogrešnih pokušaja na 0. Zato radi brzo i ne pozivaj ponovo `/auth/login/email` ili
`/phone` između pokušaja.

### 1. Kreiraj korisnika i zatraži kod (koraci A.1-A.3 iznad)

### 2. Unesi pogrešan kod 3 puta zaredom — `POST /auth/login/verify`

```json
{ "recipient": "test@example.com", "code": "000000" }
```

- 1. i 2. put: `400 Auth.InvalidCode`
- 3. put: **`403 Auth.AccountBlocked`** — nalog je sada `Blocked`

### 3. Proveri da je nalog stvarno blokiran

Zatraži nov kod ponovo (`POST /auth/login/email`) — dobićeš isti generički `200` odgovor, ali
**nema novog reda u logovima** — blokiran nalog nikad zapravo ne dobija kod.

### 4. Odblokiraj ga (treba Admin JWT)

Pošto još nema admin-creation flow-a, ručno promoviši korisnika na `Admin` direktno u bazi:

```bash
docker exec daretodance-postgres psql -U daretodance -d daretodance -c "UPDATE users SET user_role = 'Admin' WHERE email = 'admin@example.com';"
```

Uloguj se tim nalogom kroz standardni email/verify flow da dobiješ JWT sa `role: Admin`, klikni
**Authorize** u Swagger-u, nalepi token, pa pozovi:

```
POST /users/{id}/unblock
```

sa id-jem blokiranog korisnika. (`POST /users/{id}/block` radi isto, za ručno blokiranje nekog.)

---

## D) Negativni scenariji (namerno izazovi grešku)

| Scenario                                                         | Endpoint                              | Očekivano                                           |
|--------------------------------------------------------------------|----------------------------------------|------------------------------------------------------|
| Pogrešan kod                                                        | `POST /auth/login/verify`             | `400 Auth.InvalidCode`                                |
| Zahtev za novi kod dok prethodni još važi (< 60s, `ResendCooldownSeconds`) | `POST /auth/login/email` ili `/phone` | `409 Auth.CodeAlreadySent`                            |
| Kod korišćen posle isteka (> 60s, `OtpSettings.ExpirySeconds`)      | `POST /auth/login/verify`             | `400 Auth.InvalidCode`                                |
| Email/telefon koji ne postoji u bazi                                | `POST /auth/login/email` ili `/phone` | `200` (isti generički odgovor, ne otkriva se)         |
| 3. pogrešan kod zaredom (`OtpSettings.MaxFailedAttempts`)           | `POST /auth/login/verify`             | `403 Auth.AccountBlocked` (vidi sekciju C)            |
| Bilo koji login pokušaj na `Blocked`/`Inactive` nalogu               | `POST /auth/login/*`                  | Isti generički odgovori kao uvek (ne otkriva se stanje) |

---

## Napomene

- Kod se hešuje pre čuvanja u bazi (`users.login_code_hash`) — nikad se ne čuva u plain-textu.
- Kod se troši (briše) posle prve uspešne verifikacije — ne može se iskoristiti dvaput.
- `OtpSettings` (dužina koda, trajanje, max pokušaja, cooldown) — `appsettings.json`.
- `JwtSettings:Secret` **ne sme** u `appsettings.json` — lokalno preko `dotnet user-secrets`, u `docker-compose.yml` preko `JwtSettings__Secret` env varijable (dev-only vrednost, zamena za pravu tajnu u produkciji).
