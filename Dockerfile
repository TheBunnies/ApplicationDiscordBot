# Build it
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /ApplicationDiscordBot
COPY . ./
RUN dotnet restore 

RUN dotnet publish ./ApplicationDiscordBot/ApplicationDiscordBot.csproj -c Release -o out 

# Run it
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

# Install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs

# Expose some ports
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

# Disable the invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Update OpenSSL for the bot to properly work
RUN apk upgrade --update-cache --available && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

WORKDIR /ApplicationDiscordBot
COPY --from=build /ApplicationDiscordBot/out .

RUN chmod +x ./ApplicationDiscordBot

CMD ["./ApplicationDiscordBot"]