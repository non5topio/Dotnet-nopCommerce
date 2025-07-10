FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS test

WORKDIR /app

# Copy global.json and solution file
COPY ./global.json ./
COPY ./src/NopCommerce.sln ./

# Copy project files for better Docker layer caching
COPY ./src/Libraries/Nop.Core/*.csproj ./Libraries/Nop.Core/
COPY ./src/Libraries/Nop.Data/*.csproj ./Libraries/Nop.Data/
COPY ./src/Libraries/Nop.Services/*.csproj ./Libraries/Nop.Services/
COPY ./src/Presentation/Nop.Web/*.csproj ./Presentation/Nop.Web/
COPY ./src/Presentation/Nop.Web.Framework/*.csproj ./Presentation/Nop.Web.Framework/
COPY ./src/Tests/Nop.Tests/*.csproj ./Tests/Nop.Tests/

# Copy MSBuild files
COPY ./src/*.props ./
COPY ./src/Directory.Build.props ./

# Copy plugin project files
COPY ./src/Plugins/*/*.csproj ./Plugins/*/

# Copy test config
COPY ./test-gen-config.json ./

# Restore dependencies
RUN dotnet restore --disable-parallel --force

# Copy the rest of the source code
COPY ./src/ ./

# Install required tools and packages
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
ENV PATH="${PATH}:/root/.dotnet/tools"

# Install Alpine packages for nopCommerce
RUN apk add --no-cache icu-libs icu-data-full libc-dev tzdata curl
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install optional graphics packages (suppress errors if not available)
RUN apk add tiff --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/main/ --allow-untrusted || true
RUN apk add libgdiplus --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/community/ --allow-untrusted || true

# Create necessary directories
RUN mkdir -p App_Data/DataProtectionKeys logs TestResults
RUN chmod 775 App_Data App_Data/DataProtectionKeys logs

# Remove the build step - dotnet test will build automatically
CMD ["sh", "-c", "echo .NET VERSION && dotnet --version && echo RUNNING TESTS && dotnet test --configuration Release --logger trx --collect:'XPlat Code Coverage' --results-directory ./TestResults"]