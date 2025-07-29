FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
RUN adduser --disabled-password --gecos "" --uid $APP_UID appuser
USER appuser
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_CONFIGURATION=$BUILD_CONFIGURATION
WORKDIR /src
COPY ["Eclipse.csproj", "."]
RUN dotnet restore "Eclipse.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "Eclipse.csproj" -c $DOTNET_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Eclipse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eclipse.dll"]
