FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY StudentCourseRegistration.sln ./
COPY src/StudentCourseRegistration.Api/StudentCourseRegistration.Api.csproj src/StudentCourseRegistration.Api/
COPY tests/StudentCourseRegistration.Tests/StudentCourseRegistration.Tests.csproj tests/StudentCourseRegistration.Tests/
RUN dotnet restore StudentCourseRegistration.sln

COPY . .
RUN dotnet publish src/StudentCourseRegistration.Api/StudentCourseRegistration.Api.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StudentCourseRegistration.Api.dll"]
