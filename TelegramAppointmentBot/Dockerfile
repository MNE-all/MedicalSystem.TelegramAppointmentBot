FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

RUN /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
RUN brew install mysql


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
