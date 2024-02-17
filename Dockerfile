FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG BY_VAULT
ENV BY_VAULT ${BY_VAULT}
WORKDIR /app
EXPOSE 6667

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BY_VAULT
ENV BY_VAULT ${BY_VAULT}
WORKDIR /src

COPY ["BabaYaga/BabaYaga.fsproj", "BabaYaga/"]
COPY ["nuget.config", ""]

RUN dotnet restore "BabaYaga/BabaYaga.fsproj"

WORKDIR $HOME/src
COPY . .
RUN dotnet build "BabaYaga/BabaYaga.fsproj" -c Release -o /app

FROM build AS publish
ARG BY_VAULT
ENV BY_VAULT ${BY_VAULT}
RUN dotnet publish "BabaYaga/BabaYaga.fsproj" -c Release -o /app

FROM base AS final
ARG BY_VAULT
ENV BY_VAULT ${BY_VAULT}
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BabaYaga.dll"]