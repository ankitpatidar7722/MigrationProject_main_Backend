# Render Deployment Guide for MigraTrack Pro Backend

## Prerequisites
- Render account ([render.com](https://render.com))
- GitHub repository with your code
- Database server accessible from internet (currently using: 3.6.241.120)

## Deployment Steps

### 1. Push Code to GitHub
```bash
cd d:\MigrationProject-main\MigrationProject-main\Backend
git init
git add .
git commit -m "Initial commit for Render deployment"
git remote add origin <your-github-repo-url>
git push -u origin main
```

### 2. Create New Web Service on Render

1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click **"New +"** → **"Web Service"**
3. Connect your GitHub repository
4. Configure the service:

   **Basic Settings:**
   - **Name:** `migratrack-backend`
   - **Region:** Choose closest to your users
   - **Branch:** `main`
   - **Root Directory:** `Backend` (or leave empty if Backend is root)
   - **Runtime:** `Docker`
   - **Instance Type:** Free or Starter (recommend Starter for production)

### 3. Environment Variables

Add these in Render Dashboard → Environment → Environment Variables:

```
ASPNETCORE_ENVIRONMENT=Production
PORT=10000
ConnectionStrings__DefaultConnection=Server=3.6.241.120,1433;Database=IndusMigration;User Id=Indus;Password=Param@99811;MultipleActiveResultSets=true;TrustServerCertificate=True;
Cors__AllowedOrigins__0=https://migration-project-main.vercel.app
Security__RequireHttps=true
Security__EnableStrictTransportSecurity=true
```

**Important:** Mark `ConnectionStrings__DefaultConnection` as a **Secret** environment variable.

### 4. Health Check Configuration

Render will automatically use the `/health` endpoint defined in the app.

- **Health Check Path:** `/health` (already configured in render.yaml)

### 5. Deploy

1. Click **"Create Web Service"**
2. Render will automatically build and deploy your application
3. Wait for deployment to complete (usually 3-5 minutes)
4. Your backend will be available at: `https://migratrack-backend.onrender.com`

### 6. Update Frontend Configuration

Update your Vercel frontend environment variable:

**In Vercel Dashboard:**
- Go to your frontend project settings
- Navigate to **Environment Variables**
- Update `VITE_API_URL` to: `https://migratrack-backend.onrender.com/api`
- Redeploy the frontend

### 7. Update CORS in Backend

If your Render backend URL is different, update the CORS environment variable:

```
Cors__AllowedOrigins__0=https://migration-project-main.vercel.app
```

## Database Firewall Configuration

Make sure your database server (3.6.241.120) allows connections from Render's IP ranges:

1. Check Render's outbound IP addresses in their documentation
2. Add those IPs to your SQL Server firewall rules

## Testing

After deployment:

1. **Health Check:** Visit `https://your-app.onrender.com/health`
2. **API Docs:** Visit `https://your-app.onrender.com/swagger` (only if in Development mode)
3. **Test API:** Use Postman or curl to test endpoints

```bash
curl https://your-app.onrender.com/health
```

## Monitoring

- View logs in Render Dashboard → Logs tab
- Monitor metrics in Render Dashboard → Metrics tab
- Set up alerts for downtime or errors

## Important Notes

1. **Free Tier Limitations:**
   - Spins down after 15 minutes of inactivity
   - First request after spin-down takes 30-60 seconds
   - Consider upgrading to Starter plan for production

2. **Database Connection:**
   - Ensure your SQL Server allows remote connections
   - Connection string uses IP address (3.6.241.120)
   - Port 1433 must be accessible from internet

3. **Static Files:**
   - If you upload files, consider using cloud storage (AWS S3, Azure Blob)
   - Render's filesystem is ephemeral

4. **Environment Variables:**
   - Never commit sensitive data to Git
   - Use Render's environment variable manager
   - Mark passwords and connection strings as "Secret"

## Troubleshooting

### Build Fails
- Check Render build logs
- Ensure all NuGet packages are properly referenced
- Verify .NET 8.0 SDK is available

### Database Connection Fails
- Verify connection string is correct
- Check database server firewall rules
- Test connection from another remote location

### CORS Errors
- Ensure `Cors__AllowedOrigins__0` matches your frontend URL exactly
- Include protocol (https://) in the URL
- No trailing slash

### App Crashes
- Check Render logs for error messages
- Verify all required environment variables are set
- Check database migrations are applied

## Useful Commands

**View Logs:**
```bash
# In Render Dashboard → Logs tab
```

**Restart Service:**
```bash
# In Render Dashboard → Manual Deploy → "Clear build cache & deploy"
```

**Scale:**
```bash
# In Render Dashboard → Settings → Instance Type
```

## Support

- [Render Documentation](https://render.com/docs)
- [.NET on Render](https://render.com/docs/deploy-net-core)
- [Render Community](https://community.render.com)
