# Multi-stage Dockerfile for GeoHunt deployment
# Stage 1: Build Frontend
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Copy frontend files
COPY frontend/package*.json ./
RUN npm install

COPY frontend/ ./
RUN npm run build

# Stage 2: Build Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build

WORKDIR /app

# Copy backend project files
COPY psi25-project/*.csproj ./psi25-project/
RUN dotnet restore psi25-project/psi25-project.csproj

# Copy all backend source code
COPY psi25-project/ ./psi25-project/

# Copy frontend build output to wwwroot
COPY --from=frontend-build /app/frontend/dist/ ./psi25-project/wwwroot/

# Publish the application
WORKDIR /app/psi25-project
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copy published app from build stage
COPY --from=backend-build /app/publish ./

# Expose port (Render uses PORT environment variable)
EXPOSE 10000

# Set environment variable for ASP.NET to listen on all interfaces
ENV ASPNETCORE_URLS=http://+:10000

# Run the application
ENTRYPOINT ["dotnet", "psi25-project.dll"]
