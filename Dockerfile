
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

#copy csproj and restore as distinct layers
COPY Stargazer.Orleans.Modules.sln .
COPY modules/Users/src/Stargazer.Orleans.Users.Domain/*.csproj ./src/Stargazer.Orleans.Users.Domain/
COPY modules/Users/src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL/*.csproj ./src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL/
COPY modules/Users/src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations/*.csproj ./src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations/
COPY modules/Users/src/Stargazer.Orleans.Users.Grains/*.csproj ./src/Stargazer.Orleans.Users.Grains/
COPY modules/Users/src/Stargazer.Orleans.Users.Grains.Abstractions/*.csproj ./src/Stargazer.Orleans.Users.Grains.Abstractions/
COPY modules/Users/src/Stargazer.Orleans.Users.Silo/*.csproj ./src/Stargazer.Orleans.Users.Silo/
RUN dotnet restore /app/Stargazer.Orleans.Modules.sln

# # copy everything else and build app
WORKDIR /app/
COPY modules/Users/src/Stargazer.Orleans.Users.Domain/. ./src/Stargazer.Orleans.Users.Domain/
COPY modules/Users/src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL/. ./src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL/
COPY modules/Users/src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations/. ./src/Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations/
COPY modules/Users/src/Stargazer.Orleans.Users.Grains/. ./src/Stargazer.Orleans.Users.Grains/
COPY modules/Users/src/Stargazer.Orleans.Users.Grains.Abstractions/. ./src/Stargazer.Orleans.Users.Grains.Abstractions/
COPY modules/Users/src/Stargazer.Orleans.Users.Silo/. ./src/Stargazer.Orleans.Users.Silo/

WORKDIR /app/src/Stargazer.Orleans.Users.Silo
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/Stargazer.Orleans.Users.Silo/out ./

RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
RUN echo 'Asia/Shanghai' >/etc/timezone

EXPOSE 8080
ENTRYPOINT ["dotnet", "Stargazer.Orleans.Users.Silo.dll", "--urls", "http://*:8080"]