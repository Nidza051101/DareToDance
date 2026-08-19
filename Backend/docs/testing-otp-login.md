# Testing the OTP login flow (Swagger)

Temporary dev guide until a real email/SMS service exists — the code is read from container logs.

## Setup

```bash
cd Backend
docker compose up -d
```

Swagger UI: **http://localhost:5015/swagger/index.html**

API container logs (where the OTP code is read from):

```bash
docker logs daretodance-api --tail 20 -f
```

---

## A) Email flow

### 1. Create a user — `POST /users`

```json
{
  "email": "test@example.com",
  "firstName": "Pera",
  "lastName": "Peric",
  "password": "Password123!"
}
```

Expected: **201 Created**. The user automatically gets the `Member` role (`UserRole.Member`, set in `User.Create()`).

### 2. Request a login code — `POST /auth/login/email`

```json
{ "email": "test@example.com" }
```

Expected: **200**, `{"message":"If an account exists, a code has been sent to the given email."}`
(Same response even if the email doesn't exist — intentional, to prevent account enumeration.)

### 3. Read the code from the logs

```
[DEV] Login code for email test@example.com: 483920
```

### 4. Verify the code → get a JWT — `POST /auth/login/verify`

```json
{ "recipient": "test@example.com", "code": "483920" }
```

Expected: **200**

```json
{ "accessToken": "eyJhbGciOi...", "expiresAtUtc": "2026-08-19T..." }
```

---

## B) Phone flow

### 1. Create a user with a phone number — `POST /users`

```json
{
  "email": "phone.test@example.com",
  "firstName": "Ana",
  "lastName": "Anic",
  "password": "Password123!",
  "phone": "+381601234567"
}
```

The response now includes a `phone` field — check that it was saved exactly as entered.

### 2. Request a login code — `POST /auth/login/phone`

```json
{ "phone": "+381601234567" }
```

### 3. Read the code from the logs

```
[DEV] Login code for phone +381601234567: 375310
```

### 4. Verify — `POST /auth/login/verify`

```json
{ "recipient": "+381601234567", "code": "375310" }
```

Same endpoint for both channels — it detects whether `recipient` is an email or a phone number.

---

## C) Negative scenarios (trigger these on purpose)

| Scenario                                                         | Endpoint                              | Expected                                          |
|--------------------------------------------------------------------|----------------------------------------|----------------------------------------------------|
| Wrong code                                                          | `POST /auth/login/verify`             | `400 Auth.InvalidCode`                              |
| Requesting a new code while the previous one is still valid (< 60s, `ResendCooldownSeconds`) | `POST /auth/login/email` or `/phone`  | `409 Auth.CodeAlreadySent`                          |
| Code used after it expired (> 60s, `OtpSettings.ExpirySeconds`)    | `POST /auth/login/verify`             | `400 Auth.InvalidCode`                              |
| Email/phone that doesn't exist in the database                     | `POST /auth/login/email` or `/phone`  | `200` (same generic response, not revealed)         |

---

## Notes

- The code is hashed before being stored (`users.login_code_hash`) — it's never stored in plain text.
- The code is consumed (cleared) after the first successful verification — it can't be reused.
- `OtpSettings` (code length, expiry, max attempts, cooldown) — `appsettings.json`.
- `JwtSettings:Secret` **must not** go into `appsettings.json` — locally via `dotnet user-secrets`, in `docker-compose.yml` via the `JwtSettings__Secret` env variable (dev-only value, replace with a real secret in production).
