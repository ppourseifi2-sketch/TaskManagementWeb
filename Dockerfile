# مرحله ۱: ساخت برنامه
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskManagementWeb.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish TaskManagementWeb.csproj -c Release -o /app

# مرحله ۲: اجرای برنامه
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "TaskManagementWeb.dll"]