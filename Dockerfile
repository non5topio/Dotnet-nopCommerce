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
COPY ./src/NopCommerce.sln ./src/
COPY ./src/Directory.Build.props ./src/

# Copy all project files while preserving directory structure
COPY ./src/Libraries/ ./src/Libraries/
COPY ./src/Plugins/ ./src/Plugins/
COPY ./src/Presentation/ ./src/Presentation/
COPY ./src/Tests/ ./src/Tests/

# Restore dependencies
RUN dotnet restore ./src/NopCommerce.sln --disable-parallel --force

# Install global tools
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
ENV PATH="${PATH}:/root/.dotnet/tools"

FROM base AS test
# Copy everything else
COPY . .

# Create required files and directories
RUN touch ./test-gen.env && \
    mkdir -p ./src/App_Data/DataProtectionKeys ./src/logs ./src/Presentation/Nop.Web/Plugins

# Build the solution
RUN dotnet build ./src/NopCommerce.sln --configuration Release

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run tests
CMD ["dotnet", "test", "./src/Tests/Nop.Tests/Nop.Tests.csproj", "--configuration", "Release", "--logger", "trx", "--collect:XPlat Code Coverage", "--results-directory", "./TestResults"]