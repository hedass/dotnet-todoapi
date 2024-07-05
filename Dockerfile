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

# Set ASP.NET Core environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose ports http & https
EXPOSE 5207
EXPOSE 7017

ENTRYPOINT ["dotnet", "onboarding.api.dll"]