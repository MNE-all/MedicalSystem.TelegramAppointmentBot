FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TelegramAppointmentBot/TelegramAppointmentBot.csproj", "TelegramAppointmentBot/"]
RUN dotnet restore "TelegramAppointmentBot/TelegramAppointmentBot.csproj"
COPY . .
WORKDIR "/src/TelegramAppointmentBot"
RUN dotnet build "TelegramAppointmentBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramAppointmentBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramAppointmentBot.dll"]
