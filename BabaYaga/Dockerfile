FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG by-vault
ENV by-vault ${by-vault}
WORKDIR /app
EXPOSE 6667

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG by-vault
ENV by-vault ${by-vault}
WORKDIR /src

COPY ["BabaYaga.fsproj", "BabaYaga/"]
COPY ["nuget.config", ""]

RUN dotnet restore "BabaYaga/BabaYaga.fsproj"

WORKDIR $HOME/src
COPY . .
RUN dotnet build "BabaYaga.fsproj" -c Release -o /app

FROM build AS publish
ARG by-vault
ENV by-vault ${by-vault}
RUN dotnet publish "BabaYaga.fsproj" -c Release -o /app

FROM base AS final
ARG by-vault
ENV by-vault ${by-vault}
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BabaYaga.dll"]