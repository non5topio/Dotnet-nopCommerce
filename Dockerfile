# create the build and test instance 
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS test

WORKDIR /app

# Copy global.json to ensure the right SDK version is used
COPY ./global.json ./

# Copy the entire source code
COPY ./src/ ./

# Copy the test config file
COPY ./test-gen-config.json ./

RUN touch ./test-gen.env && \
    mkdir -p ./src/App_Data/DataProtectionKeys ./src/logs ./src/Presentation/Nop.Web/Plugins
# Run specific TaxServiceTests with coverage
CMD ["sh", "-c", "time dotnet test ./Tests/Nop.Tests/Nop.Tests.csproj --filter \"FullyQualifiedName~TaxServiceTests\" --collect:'XPlat Code Coverage' --results-directory ./TestResults --verbosity minimal && find ./TestResults -name 'coverage.cobertura.xml' -exec cp {} ./TestResults/coverage.cobertura.xml \\; && echo 'Tests completed with coverage'"]