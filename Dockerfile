# create the build and test instance 
# Change from Alpine to Ubuntu/Debian based image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /app

# First copy global.json to ensure the right SDK version is used
COPY ./global.json ./

# Copy solution file and build configuration
COPY ./src/NopCommerce.sln ./
COPY ./src/Directory.Build.props ./
COPY ./src/.deployment ./
COPY ./src/deploy.cmd ./

# Copy all project files first (for better Docker layer caching)
COPY ./src/Libraries/Nop.Core/*.csproj ./Libraries/Nop.Core/
COPY ./src/Libraries/Nop.Data/*.csproj ./Libraries/Nop.Data/
COPY ./src/Libraries/Nop.Services/*.csproj ./Libraries/Nop.Services/
COPY ./src/Presentation/Nop.Web/*.csproj ./Presentation/Nop.Web/
COPY ./src/Presentation/Nop.Web.Framework/*.csproj ./Presentation/Nop.Web.Framework/
COPY ./src/Tests/Nop.Tests/*.csproj ./Tests/Nop.Tests/

# Copy ALL plugin project files (create directory structure first)
RUN find /app -name "Plugins" -type d -exec mkdir -p {} \; 2>/dev/null || true
COPY ./src/Plugins/ ./Plugins/

# Copy build tools and configuration
COPY ./src/Build/ ./Build/

# Copy the test config file
COPY ./test-gen-config.json ./

# Restore dependencies with specific parameters to handle framework issues
COPY ./src/ ./src/
COPY ./global.json ./
COPY ./test-gen-config.json ./

WORKDIR /app/src
RUN dotnet restore --disable-parallel --force

# Copy the rest of the source code
COPY ./src/ ./

# Install ReportGenerator tool for coverage reports
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Install required packages for nopCommerce
RUN apt-get update && apt-get install -y \
    libicu-dev \
    libgdiplus \
    libc6-dev \
    tzdata \
    && rm -rf /var/lib/apt/lists/*

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Create necessary directories early in the process
RUN mkdir -p App_Data/DataProtectionKeys logs Presentation/Nop.Web/Plugins

# Set permissions
RUN chmod 775 App_Data App_Data/DataProtectionKeys logs

# Build the solution in release mode
RUN dotnet build --configuration Release

# Run tests with coverage
CMD ["sh", "-c", "echo .NET VERSION && dotnet --version && echo RUNNING TESTS && dotnet test --configuration Release --logger trx --collect:'XPlat Code Coverage' --results-directory ./TestResults"]