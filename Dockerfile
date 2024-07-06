FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY ./onboarding.api/*.csproj ./onboarding.api/
COPY ./onboarding.bll/*.csproj ./onboarding.bll/
COPY ./onboarding.dal/*.csproj ./onboarding.dal/
RUN dotnet restore ./onboarding.api/onboarding.api.csproj

COPY . .
RUN dotnet publish ./onboarding.api/onboarding.api.csproj -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose ports http & https
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "onboarding.api.dll"]