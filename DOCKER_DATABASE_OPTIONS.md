# Docker Database Options

## ⚡ Quick Start (Recommended for Testing)

Use **SQLite** - It's simpler and more reliable:

```bash
docker-compose -f docker-compose.sqlite.yml up --build
```

✅ Starts in ~30 seconds
✅ No health check issues
✅ Perfect for development and testing

---

## 🏢 Production Setup

Use **SQL Server** if you need:
- Production-grade database
- Better performance with large datasets
- Advanced SQL Server features

```bash
docker-compose up --build
```

⚠️ Takes ~1-2 minutes to start
⚠️ Requires 2GB RAM minimum

---

## Troubleshooting SQL Server

If you get "database is unhealthy" error:

### Fix 1: Clean Start
```bash
# Stop everything
docker-compose down -v

# Remove old volumes
docker volume rm options-tracker_sqlserver_data

# Start fresh
docker-compose up --build
```

### Fix 2: Increase Timeout
If SQL Server is slow to start on your machine, wait longer:
```bash
# Watch the logs
docker-compose logs -f database

# Wait for: "SQL Server is now ready for client connections"
# Then Ctrl+C and run:
docker-compose up
```

### Fix 3: Check Password Complexity
SQL Server password must have:
- At least 8 characters
- Uppercase, lowercase, numbers, and symbols
- Default: `YourStrong@Passw0rd123`

### Fix 4: Use SQLite Instead
The easiest solution:
```bash
docker-compose -f docker-compose.sqlite.yml up --build
```

---

## Which Should I Use?

| Feature | SQLite | SQL Server |
|---------|--------|------------|
| **Startup Time** | 30 seconds | 1-2 minutes |
| **Reliability** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ (can be finicky in Docker) |
| **Memory** | ~100MB | ~2GB |
| **Best For** | Development, testing, demos | Production, large datasets |
| **Data Persistence** | File-based | Container volume |

**My Recommendation:** Start with **SQLite** for development, then switch to SQL Server if you need it for production.

---

## Switching Between Databases

### From SQLite to SQL Server:
1. Export your data (optional)
2. Stop SQLite: `docker-compose -f docker-compose.sqlite.yml down`
3. Start SQL Server: `docker-compose up`

### From SQL Server to SQLite:
1. Stop SQL Server: `docker-compose down`
2. Start SQLite: `docker-compose -f docker-compose.sqlite.yml up`

Data is stored separately, so you won't lose anything.

---

## Current Status Check

```bash
# Check if containers are running
docker ps

# Check database health
docker logs options-tracker-db

# Check backend logs
docker logs options-tracker-backend

# Check frontend logs
docker logs options-tracker-frontend
```

---

## Still Having Issues?

Try the **manual setup** without Docker:

**Backend:**
```bash
cd Backend
dotnet restore
dotnet run
```

**Frontend:**
```bash
cd Frontend
npm install
npm run dev
```

This bypasses Docker entirely and often works better for development.
