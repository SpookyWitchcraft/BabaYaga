FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#ARG by-vault
ENV by-vault google.com
WORKDIR /app
EXPOSE 6667

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG by-vault
ENV by-vault google.com
WORKDIR /src

COPY ["BabaYaga/BabaYaga.fsproj", "BabaYaga/"]
COPY ["nuget.config", ""]

RUN dotnet restore "BabaYaga/BabaYaga.fsproj"

WORKDIR $HOME/src
COPY . .
RUN dotnet build "BabaYaga/BabaYaga.fsproj" -c Release -o /app

FROM build AS publish
#ARG by-vault
ENV by-vault google.com
RUN dotnet publish "BabaYaga/BabaYaga.fsproj" -c Release -o /app

FROM base AS final
#ARG by-vault
ENV by-vault google.com
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BabaYaga.dll"]