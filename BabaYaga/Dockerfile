FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 6667

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BabaYaga.fsproj", "BabaYaga/"]
COPY ["nuget.config", ""]

RUN dotnet restore "BabaYaga/BabaYaga.fsproj"

WORKDIR $HOME/src
COPY . .
RUN dotnet build "BabaYaga.fsproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BabaYaga.fsproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BabaYaga.dll"]