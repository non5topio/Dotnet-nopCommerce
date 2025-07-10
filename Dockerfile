# create the build and test instance 
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /app

# Copy solution file
COPY ./src/NopCommerce.sln ./

# Copy all project files first (for better Docker layer caching)
COPY ./src/Libraries/Nop.Core/*.csproj ./Libraries/Nop.Core/
COPY ./src/Libraries/Nop.Data/*.csproj ./Libraries/Nop.Data/
COPY ./src/Libraries/Nop.Services/*.csproj ./Libraries/Nop.Services/
COPY ./src/Presentation/Nop.Web/*.csproj ./Presentation/Nop.Web/
COPY ./src/Presentation/Nop.Web.Framework/*.csproj ./Presentation/Nop.Web.Framework/
COPY ./src/Tests/Nop.Tests/*.csproj ./Tests/Nop.Tests/

# Copy ALL plugin project files
COPY ./src/Plugins/*/*.csproj ./Plugins/*/

# Copy any other test projects if they exist
COPY ./src/Tests/ ./Tests/

# Create plugin directory structure
RUN mkdir -p Plugins
COPY ./src/Plugins/ ./Plugins/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY ./src/ ./

# Install ReportGenerator tool for coverage reports
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Install required packages for nopCommerce (similar to runtime stage)
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install additional packages that might be needed for tests
RUN apk add tiff --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/main/ --allow-untrusted || true
RUN apk add libgdiplus --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/community/ --allow-untrusted || true
RUN apk add libc-dev tzdata --no-cache

# Create necessary directories
RUN mkdir -p App_Data/DataProtectionKeys logs

# Set permissions
RUN chmod 775 App_Data App_Data/DataProtectionKeys logs

# Build the solution in release mode
RUN dotnet build --configuration Release

# Run tests with coverage
CMD ["sh", "-c", "echo .NET VERSION && dotnet --version && echo RUNNING TESTS && dotnet test --configuration Release --logger trx --collect:'XPlat Code Coverage' --results-directory ./TestResults"]

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

WORKDIR /app

# Copy build output from build stage
COPY --from=build /app/Presentation/Nop.Web/bin/Release/net9.0/publish/ ./

# Install required packages for nopCommerce
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Create necessary directories for data persistence
RUN mkdir -p App_Data/DataProtectionKeys logs
RUN chmod 775 App_Data App_Data/DataProtectionKeys logs

# Expose port 80
EXPOSE 80

# Set entrypoint
ENTRYPOINT ["dotnet", "Nop.Web.dll"]