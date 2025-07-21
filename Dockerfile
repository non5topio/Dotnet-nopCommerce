# create the build and test instance 
# Change from Alpine to Ubuntu/Debian based image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app

# Install system dependencies first
RUN apt-get update && apt-get install -y \
    libicu-dev \
    libgdiplus \
    libc6-dev \
    tzdata \
    && rm -rf /var/lib/apt/lists/*

# Copy project files for dependency restore - preserve directory structure
COPY ./global.json ./

# Copy the entire source code
COPY ./src/ ./

# Copy the test config file
COPY ./test-gen-config.json ./

# Run specific TaxServiceTests with coverage
CMD ["sh", "-c", "time dotnet test ./Tests/Nop.Tests/Nop.Tests.csproj --filter \"FullyQualifiedName~TaxServiceTests\" --collect:'XPlat Code Coverage' --results-directory ./TestResults --verbosity minimal && find ./TestResults -name 'coverage.cobertura.xml' -exec cp {} ./TestResults/coverage.cobertura.xml \\; && echo 'Tests completed with coverage'"]
