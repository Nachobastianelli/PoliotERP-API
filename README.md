cat > README.md << 'EOF'
# Polioterp - Multi-Tenant ERP Base

Complete SaaS starter template with multi-tenancy, authentication, and subscription management.

## 🚀 Features

- ✅ Multi-tenant architecture (shared database with tenant isolation)
- ✅ JWT Authentication & Authorization
- ✅ User management (CRUD with soft delete)
- ✅ Tenant management (CRUD with subscription validation)
- ✅ Subscription system with automatic expiration checks
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ Global Query Filters (automatic tenant isolation & soft delete)
- ✅ Audit trails (CreatedBy, UpdatedBy, DeletedBy)
- ✅ Rate limiting (Login, Register, API)
- ✅ Exception handling middleware
- ✅ BCrypt password hashing with timing attack protection

## 🏗️ Tech Stack

- .NET 10
- PostgreSQL 17
- Entity Framework Core
- JWT Bearer Authentication
- BCrypt.Net
- Scalar API Documentation

## 🚦 Getting Started

### 1. Setup PostgreSQL
```bash
docker compose up -d
```

### 2. Update appsettings.json

Edit `WebApplication1/appsettings.json` with your connection string and JWT secret.

### 3. Run migrations
```bash
dotnet ef database update --project Infrastructure --startup-project WebApplication1
```

### 4. Run the application
```bash
dotnet run --project WebApplication1
```

API: `http://localhost:5290`
Documentation: `http://localhost:5290/scalar/v1`

## 🔐 Security

- Password hashing with BCrypt (work factor 12)
- JWT tokens with 8-hour expiration
- Timing attack protection on login
- Generic error messages (no user enumeration)
- Rate limiting on auth endpoints
- Automatic tenant isolation via query filters

## 🛣️ Roadmap

- [] Roles and Permissions system
- [ ] Feature flags per tenant
- [ ] Refresh tokens
- [ ] Password recovery
- [ ] Email service
- [ ] Stripe integration

## 📄 License

Private - Internal use only
EOF
