#!/bin/bash

# William Metal API Deployment Script
# This script sets up the backend API for production deployment

echo "🚀 William Metal API Deployment Script"
echo "======================================"

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK is not installed. Please install it first."
    echo "   Visit: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

echo "✅ .NET 8 SDK found"

# Check if PostgreSQL is running
if ! command -v psql &> /dev/null; then
    echo "❌ PostgreSQL client (psql) is not installed."
    echo "   Please install PostgreSQL and ensure it's running."
    exit 1
fi

echo "✅ PostgreSQL client found"

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore dependencies"
    exit 1
fi

echo "✅ Dependencies restored"

# Build the project
echo "🔨 Building the project..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Failed to build the project"
    exit 1
fi

echo "✅ Project built successfully"

# Check if database exists and create if needed
echo "🗄️  Setting up database..."
DB_NAME="williammetal"
DB_USER="postgres"
DB_HOST="localhost"
DB_PORT="5432"

# Try to connect to database
if PGPASSWORD="" psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT 1;" &> /dev/null; then
    echo "✅ Database $DB_NAME exists"
else
    echo "📝 Database $DB_NAME does not exist. Creating..."
    createdb -h $DB_HOST -p $DB_PORT -U $DB_USER $DB_NAME
    if [ $? -ne 0 ]; then
        echo "❌ Failed to create database"
        echo "   Please create the database manually:"
        echo "   createdb -h $DB_HOST -p $DB_PORT -U $DB_USER $DB_NAME"
        exit 1
    fi
    echo "✅ Database created"
fi

# Apply database migrations
echo "🔄 Applying database migrations..."
dotnet ef database update
if [ $? -ne 0 ]; then
    echo "❌ Failed to apply database migrations"
    echo "   Please check your connection string in appsettings.json"
    exit 1
fi

echo "✅ Database migrations applied"

# Publish the application
echo "📤 Publishing application..."
dotnet publish --configuration Release --output ./publish
if [ $? -ne 0 ]; then
    echo "❌ Failed to publish application"
    exit 1
fi

echo "✅ Application published to ./publish directory"

# Create systemd service file
echo "🔧 Creating systemd service..."
cat > williammetal-api.service << EOF
[Unit]
Description=William Metal API
After=network.target

[Service]
WorkingDirectory=/opt/williammetal-api
ExecStart=/opt/williammetal-api/WilliamMetalAPI
Restart=always
RestartSec=10
SyslogIdentifier=williammetal-api
User=williammetal-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

echo "✅ Systemd service file created: williammetal-api.service"

# Create nginx configuration
echo "🔧 Creating nginx configuration..."
cat > williammetal-api.conf << EOF
server {
    listen 80;
    server_name api.williammetal.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

echo "✅ Nginx configuration created: williammetal-api.conf"

# Create environment file template
echo "🔧 Creating environment file template..."
cat > .env.template << EOF
# Database Configuration
DB_HOST=localhost
DB_PORT=5432
DB_NAME=williammetal
DB_USER=postgres
DB_PASSWORD=your_password_here

# JWT Configuration
JWT_SECRET_KEY=your-very-secure-secret-key-here-should-be-at-least-32-characters-long
JWT_ISSUER=WilliamMetalAPI
JWT_AUDIENCE=WilliamMetalUsers

# API Configuration
API_URL=https://api.williammetal.com
ALLOWED_ORIGINS=https://williammetal.com,https://www.williammetal.com
EOF

echo "✅ Environment file template created: .env.template"

# Create systemd installation script
echo "🔧 Creating systemd installation script..."
cat > install-service.sh << 'EOF'
#!/bin/bash

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root (use sudo)"
    exit 1
fi

echo "🔧 Installing William Metal API service..."

# Create user
useradd -r -s /bin/false williammetal-api

# Create application directory
mkdir -p /opt/williammetal-api
cp -r publish/* /opt/williammetal-api/
chown -R williammetal-api:williammetal-api /opt/williammetal-api
chmod +x /opt/williammetal-api/WilliamMetalAPI

# Copy environment file
if [ -f .env ]; then
    cp .env /opt/williammetal-api/
    chown williammetal-api:williammetal-api /opt/williammetal-api/.env
fi

# Install systemd service
cp williammetal-api.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable williammetal-api

echo "✅ Service installed successfully"
echo "📋 To start the service, run: systemctl start williammetal-api"
echo "📋 To check status, run: systemctl status williammetal-api"
echo "📋 To view logs, run: journalctl -u williammetal-api -f"
EOF

chmod +x install-service.sh

echo "✅ Systemd installation script created: install-service.sh"

# Summary
echo ""
echo "🎉 Deployment preparation complete!"
echo "=================================="
echo ""
echo "📋 Next steps:"
echo "1. Copy the publish directory to your server:"
echo "   scp -r publish user@your-server:/opt/williammetal-api"
echo ""
echo "2. SSH to your server and run the installation:"
echo "   ssh user@your-server"
echo "   cd /opt/williammetal-api"
echo "   sudo ./install-service.sh"
echo ""
echo "3. Configure the application:"
echo "   - Copy .env.template to .env and fill in your values"
echo "   - Update appsettings.json with production settings"
echo "   - Configure nginx with williammetal-api.conf"
echo ""
echo "4. Start the service:"
echo "   sudo systemctl start williammetal-api"
echo ""
echo "5. Check the service status:"
echo "   sudo systemctl status williammetal-api"
echo ""
echo "🔍 The API will be available at: http://localhost:5000"
echo "📚 Swagger documentation at: http://localhost:5000/swagger"
echo ""
echo "Happy deploying! 🚀"