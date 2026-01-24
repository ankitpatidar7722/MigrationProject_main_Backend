@echo off
echo =======================================
echo MigraTrack Pro - Backend API
echo =======================================
echo.

REM Kill any process using port 5000
echo Checking for processes on port 5000...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5000') do (
    echo Killing process %%a
    taskkill /F /PID %%a >nul 2>&1
)

echo.
echo Starting Backend API on http://localhost:5000
echo Swagger UI: http://localhost:5000/swagger
echo Health Check: http://localhost:5000/health
echo.
echo Press Ctrl+C to stop the server
echo =======================================
echo.

REM Set development environment
set ASPNETCORE_ENVIRONMENT=Development

dotnet run --urls "http://localhost:5000"
