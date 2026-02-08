# Quick Start Guide - Options Tracker

## Step 1: Setup Database

1. Open SQL Server Management Studio (or use your preferred SQL tool)
2. Create a new database named `OptionsTrackerDB` (or let EF Core create it automatically)

## Step 2: Start the Backend

```bash
# Navigate to Backend folder
cd OptionsTracker/Backend

# Restore NuGet packages
dotnet restore

# Apply database migrations (creates tables)
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API
dotnet run
```

You should see: "Now listening on: https://localhost:7001"

## Step 3: Start the Frontend

Open a new terminal:

```bash
# Navigate to Frontend folder
cd OptionsTracker/Frontend

# Install npm packages
npm install

# Start development server
npm run dev
```

You should see: "Local: http://localhost:3000"

## Step 4: Access the Application

Open your browser and go to: **http://localhost:3000**

## First Steps in the App

### 1. Import Transactions (Recommended)
- Click "Import" in the navigation
- Select your broker (Fidelity or Schwab)
- Enter an account name (e.g., "My Brokerage")
- Upload your CSV file
- Click "Import Transactions"

### 2. Or Manually Add Positions
- Click "Positions" > "Add Position"
- Enter symbol, quantity, and price
- Click "Create Position"

### 3. Create a Covered Call
- Go to "Positions"
- Find a stock position
- Click "Sell CC"
- Enter strike, expiration, contracts, and premium
- Click "Create"

### 4. View Dashboard
- Click "Dashboard" to see portfolio overview
- View total value, P&L, and premium collected
- See expiring options

## Common Issues

### Backend won't start
- Make sure SQL Server is running
- Check connection string in `Backend/appsettings.json`
- Run: `dotnet ef database update` to ensure database is created

### Frontend won't start
- Make sure Node.js is installed (v18+)
- Delete `node_modules` and run `npm install` again
- Check if port 3000 is already in use

### Can't connect to API
- Ensure backend is running on https://localhost:7001
- Check CORS settings in `Backend/Program.cs`
- Clear browser cache and reload

## Next Steps

1. **Import your transaction history** from Fidelity or Schwab
2. **Review positions** and verify cost basis
3. **Create covered calls** on stocks you own
4. **Create cash-secured puts** on stocks you want to own
5. **Roll options** as they approach expiration

## CSV Export Instructions

### Fidelity
1. Log into Fidelity.com
2. Go to Accounts & Trade > Portfolio
3. Select your account
4. Click "Download" > "History"
5. Select date range and export as CSV

### Schwab
1. Log into Schwab.com
2. Go to Accounts > History
3. Select your account and date range
4. Export Transactions as CSV

## Need Help?

- Check the full README.md for detailed documentation
- Review API endpoints at https://localhost:7001/swagger
- Check browser console for errors (F12)
- Review backend logs in terminal

## Development Tips

### Hot Reload
- Backend: Changes require restart (`Ctrl+C`, then `dotnet run`)
- Frontend: Changes auto-reload in browser

### Database Changes
When you modify models:
```bash
cd Backend
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### View API Documentation
Backend includes Swagger UI at: https://localhost:7001/swagger

Enjoy tracking your options! 🚀
