# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SystemSalesTickets/SystemSalesTickets.Api.csproj", "SystemSalesTickets/"]
COPY ["SystemSalesTicketsApplication/SystemSalesTickets.Service.csproj", "SystemSalesTicketsApplication/"]
COPY ["SystemSalesTicketsDomain/SystemSalesTickets.Core.csproj", "SystemSalesTicketsDomain/"]
COPY ["SystemSalesTicketsInfrastructure/SystemSalesTickets.Data.csproj", "SystemSalesTicketsInfrastructure/"]

RUN dotnet restore "SystemSalesTickets/SystemSalesTickets.Api.csproj"

COPY . .
WORKDIR "/src/SystemSalesTickets"
RUN dotnet publish "SystemSalesTickets.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SystemSalesTickets.Api.dll"]
