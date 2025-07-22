using FluentAssertions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Tax;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Tax;

[TestFixture]
public class TaxServiceTests : ServiceTest
{
    private TaxSettings _taxSettings;
    private bool _defaultEuVatAssumeValid;
    private ITaxPluginManager _taxPluginManager;
    private ISettingService _settingService;
    private ITaxService _taxService;
    private ICustomerService _customerService;
    private bool _defaultAdminRoleTaxExempt;
    private bool _defaultAdminTaxExempt;
    private bool _defaultEuVatUseWebService;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _settingService = GetService<ISettingService>();
        _taxSettings = GetService<TaxSettings>();
        _defaultEuVatUseWebService = _taxSettings.EuVatUseWebService;
        _defaultEuVatAssumeValid = _taxSettings.EuVatAssumeValid;
        _taxSettings.EuVatAssumeValid = false;
        _taxSettings.EuVatUseWebService = true;
        _taxSettings.HmrcApiUrl = "https://test-api.service.hmrc.gov.uk";
        _taxSettings.HmrcClientId = "TTLMDds3fJQhQjJOO5JHo1R6nxxT";
        _taxSettings.HmrcClientSecret = "cb9da68a-4184-4e8a-a071-11bfcb654ae8";

        await _settingService.SaveSettingAsync(_taxSettings);

        _taxService = GetService<ITaxService>();
        _taxPluginManager = GetService<ITaxPluginManager>();
            
        _customerService = GetService<ICustomerService>();

        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        _defaultAdminRoleTaxExempt = adminRole.TaxExempt;
        var admin = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        _defaultAdminTaxExempt = admin.IsTaxExempt;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        _taxSettings.EuVatAssumeValid = _defaultEuVatAssumeValid;
        _taxSettings.EuVatUseWebService = _defaultEuVatUseWebService;

        await _settingService.SaveSettingAsync(_taxSettings);

        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        adminRole.TaxExempt = _defaultAdminRoleTaxExempt;
        adminRole.Active = true;
        await _customerService.UpdateCustomerRoleAsync(adminRole);

        var admin = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        admin.IsTaxExempt = _defaultAdminTaxExempt;
        await _customerService.UpdateCustomerAsync(admin);
    }

    [Test]
    public async Task CanLoadTaxProviders()
    {
        var providers = await _taxPluginManager.LoadAllPluginsAsync();
        providers.Should().NotBeNull();
        providers.Any().Should().BeTrue();
    }

    [Test]
    public async Task CanLoadTaxProviderBySystemKeyword()
    {
        var provider = await _taxPluginManager.LoadPluginBySystemNameAsync("FixedTaxRateTest");
        provider.Should().NotBeNull();
    }

    [Test]
    public async Task CanLoadActiveTaxProvider()
    {
        var provider = await _taxPluginManager.LoadPrimaryPluginAsync();
        provider.Should().NotBeNull();
    }

    [Test]
    public async Task CanCheckIsPluginActive()
    {
        var provider = await _taxPluginManager.LoadPrimaryPluginAsync();
        provider.Should().NotBeNull();
        _taxPluginManager.IsPluginActive(provider).Should().BeTrue();
        var isActive = await _taxPluginManager.IsPluginActiveAsync(provider.PluginDescriptor.SystemName);
        isActive.Should().BeTrue();
    }

    [Test]
    public async Task CanGetProductPricePriceIncludesTaxIncludingTaxTaxable()
    {
        var customer = new Customer();
        var product = new Product();

        var (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, true);
        price.Should().Be(1000);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, false);
        price.Should().Be(1100);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, false, customer, true);
        price.Should().Be(909.0909090909090909090909091M);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, false, customer, false);
        price.Should().Be(1000);
    }

    [Test]
    public async Task CanGetProductPrice()
    {
        var product = new Product();
        var customer = new Customer();

        var (price, _) = await _taxService.GetProductPriceAsync(product, 1000M);
        price.Should().Be(1000);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, true);
        price.Should().Be(1000);
    }

    [Test]
    public async Task CanGetProductPricePriceIncludesTaxIncludingTaxNonTaxable()
    {
        var customer = new Customer();
        var product = new Product();

        //not taxable
        customer.IsTaxExempt = true;

        var (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, true);
        price.Should().Be(909.0909090909090909090909091M);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, false);
        price.Should().Be(1000);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, false, customer, true);
        price.Should().Be(909.0909090909090909090909091M);
        (price, _) = await _taxService.GetProductPriceAsync(product, 0, 1000M, false, customer, false);
        price.Should().Be(1000);
    }

    [Test]
    [TestCase("GB553557881", VatNumberStatus.Valid)]
    [TestCase("NO974761076", VatNumberStatus.Unknown)]
    [TestCase("GB430479893", VatNumberStatus.Invalid)]
    [TestCase("IT00478390347", VatNumberStatus.Valid)]
    public async Task CanCheckVatNumber(string vatNumber, VatNumberStatus canBeStatus)
    {
        var result = await _taxService.GetVatNumberStatusAsync(vatNumber);

        result.vatNumberStatus.Should().Be(canBeStatus);
    }
/*
FAILED TEST: ### **Analysis of Test Run Failure**
1. **Missing Using Directives**:
   - The test file is missing required `using` directives for namespaces like `Moq`, `Nop.Services.GenericAttributes`, `Nop.Core.Domain.Common`, and `Nop.Services.Common`.
2. **Incorrect Use of `Setup` Method**:
   - The `Setup` method is being used incorrectly on `ICountryService`, likely due to a missing `using Moq;` directive.
3. **Missing Method Reference**:
   - The test is calling `GetTaxRateAsync` on `ITaxService`, but this method is not part of the public interface. It is a protected method in the `TaxService` class.
4. **Deconstruction Errors**:
   - The deconstruction of the tuple `(taxRate, isTaxable)` fails due to the previous error in calling `GetTaxRateAsync`.

---

### **Recommended Fixes**
1. **Add Missing Using Directives**:
   ```csharp
   using Moq;
   using Nop.Services.GenericAttributes;
   using Nop.Core.Domain.Common;
   using Nop.Services.Common;
   ```
2. **Fix Mock Setup**:
   - Ensure `Setup` is used correctly with Moq syntax for `ICountryService` and other interfaces.
3. **Refactor Test to Use Public Methods**:
   - Replace the call to `GetTaxRateAsync` with a public method from `ITaxService`, or refactor the test to mock the behavior of the protected method via a wrapper or testable interface.
4. **Correct Tuple Deconstruction**:
   - Ensure the method being called returns a valid tuple and that the deconstruction syntax is correct.

    [Test]
    public async Task CanGetTaxRateWithNonEuCountry()
    {
        // Arrange
        var customer = new Customer();
        var address = new Address
        {
            CountryId = 1 // Assume this is a non-EU country
        };
        var taxService = GetService<ITaxService>();
        var countryService = GetService<ICountryService>();
    
        // Mock country service to return a non-EU country
        var mockCountry = new Mock<Country>();
        mockCountry.Setup(x => x.SubjectToVat).Returns(false);
        countryService.Setup(x => x.GetCountryByIdAsync(address.CountryId.Value)).ReturnsAsync(mockCountry.Object);
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(null, 0, customer, 100);
    
        // Assert
        taxRate.Should().Be(0);
        isTaxable.Should().BeFalse();
    }

*/
/*
FAILED TEST: The test run failed due to **missing using directives and namespace references** in the `TaxServiceTests.cs` file. These missing references prevent the compiler from recognizing types and interfaces required for the tests to execute.

### **Failures Identified:**
1. **Missing `using Moq;`**  
   - **Error**: Moq extension methods like `Setup` are not recognized.
   - **Fix**: Add `using Moq;` at the top of the file.

2. **Missing `using Nop.Services.GenericAttributes;`**  
   - **Error**: `CS0234: The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Add `using Nop.Services.GenericAttributes;`.

3. **Missing `using Nop.Core.Domain.Common;`**  
   - **Error**: `Address` type is not recognized.
   - **Fix**: Add `using Nop.Core.Domain.Common;`.

4. **Missing `using Nop.Services.Common;`**  
   - **Error**: `IWebHelper` is not recognized.
   - **Fix**: Add `using Nop.Services.Common;`.

### **Recommended Fixes:**
- Add the following using directives at the top of `TaxServiceTests.cs`:
  ```csharp
  using Moq;
  using Nop.Services.GenericAttributes;
  using Nop.Core.Domain.Common;
  using Nop.Services.Common;
  ```
- Ensure all required assemblies (e.g., `Nop.Services`, `Nop.Core`) are properly referenced in the test project.

    [Test]
    public async Task CanGetProductPriceWithNegativePrice()
    {
        // Arrange
        var product = new Product();
        var customer = new Customer();
        var taxService = GetService<ITaxService>();
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 0, -100, true, customer, true);
    
        // Assert
        price.Should().Be(-100);
        taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis**

1. **Missing Using Directives**  
   - `CS1061` errors indicate missing Moq extension methods (`Setup`) and missing references to `ITaxPluginManager`.  
   - **Fix**: Add the following using directives at the top of `TaxServiceTests.cs`:
     ```csharp
     using Moq;
     using Nop.Services.GenericAttributes;
     using Nop.Core.Domain.Common;
     ```

2. **Incorrect Assignment to Read-Only Property**  
   - `CS0200`: `TaxRateResult.Success` is read-only and cannot be assigned.  
   - **Fix**: Update the test to use a constructor or object initializer for `TaxRateResult`:
     ```csharp
     var taxRateResult = new TaxRateResult { Success = true, TaxRate = 20m };
     ```

3. **Missing Method `GetTaxRateAsync` on `ITaxService`**  
   - `CS1061`: The method is not found on the interface.  
   - **Fix**: Ensure the test is using the correct version of the `ITaxService` interface. If the method is not exposed, consider mocking the internal logic or refactoring the test to use available public methods.

4. **Deconstruction Type Inference Failure**  
   - `CS8130`: Cannot infer types for deconstruction variables.  
   - **Fix**: Explicitly declare variable types:
     ```csharp
     var (taxRate, isTaxable) = await _taxService.GetTaxRateAsync(...);
     ```

---

### ✅ **Summary of Fixes**
- Add missing `using` directives.
- Fix read-only property assignment.
- Use correct method signatures or mock accordingly.
- Explicitly declare deconstruction variable types.

    [Test]
    public async Task CanGetTaxRateWithFailedTaxProvider()
    {
        // Arrange
        var customer = new Customer();
        var taxService = GetService<ITaxService>();
        var taxPluginManager = GetService<ITaxPluginManager>();
        var logger = GetService<ILogger>();
    
        // Mock tax plugin manager to return a tax provider that fails
        var mockTaxProvider = new Mock<ITaxProvider>();
        mockTaxProvider.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { Success = false, Errors = new[] { "Test error" } });
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(customer, It.IsAny<int>()))
            .ReturnsAsync(mockTaxProvider.Object);
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(null, 0, customer, 100);
    
        // Assert
        taxRate.Should().Be(0);
        isTaxable.Should().BeTrue();
    }

*/
/*
FAILED TEST: ### **Analysis of Test Run Failure**
The test run failed due to two main issues:

1. **Missing Using Directive for `IWebHelper`**  
   - Error: `CS0246: The type or namespace name 'IWebHelper' could not be found`  
   - Cause: The `IWebHelper` interface is not recognized because the necessary `using` directive is missing.

2. **Missing Using Directive for Moq Extension Method on `IGeoLookupService`**  
   - Error: `CS1061: 'IGeoLookupService' does not contain a definition for 'Setup'`  
   - Cause: The Moq extension method `Setup` is not recognized because the `Moq` namespace is not included.

---

### **Recommended Fixes**

1. **Add the missing `using` directive for `IWebHelper`:**
   ```csharp
   using Nop.Services.Common;
   ```

2. **Add the missing `using` directive for Moq extension methods:**
   ```csharp
   using Moq;
   ```

Include these at the top of `TaxServiceTests.cs` to resolve the compilation errors and allow the test to run.

    [Test]
    public async Task CanGetTaxRateWithAutoDetectedCountry()
    {
        // Arrange
        var customer = new Customer();
        var taxService = GetService<ITaxService>();
        var webHelper = GetService<IWebHelper>();
        var geoLookupService = GetService<IGeoLookupService>();
        var countryService = GetService<ICountryService>();
    
        // Mock web helper to return a known IP address
        var ipAddress = "192.0.2.1";
        var countryIsoCode = "US";
        var country = await countryService.GetCountryByTwoLetterIsoCodeAsync(countryIsoCode);
        webHelper.Setup(x => x.GetCurrentIpAddress()).Returns(ipAddress);
        geoLookupService.Setup(x => x.LookupCountryIsoCode(ipAddress)).Returns(countryIsoCode);
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(null, 0, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0);
    }

*/
/*
FAILED TEST: The test run failed due to a **missing namespace reference** in `TaxServiceTests.cs`.

**Analysis:**
- The error `CS0234: The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'` indicates that the `Nop.Services.GenericAttributes` namespace is missing from the project references or is not available in the current solution.

**Recommended Fix:**
1. Add the correct using directive at the top of `TaxServiceTests.cs`:
   ```csharp
   using Nop.Services.GenericAttributes;
   ```
2. Ensure that the project containing `Nop.Services.GenericAttributes` is referenced in the `Nop.Tests.csproj` file. If it's a separate library, add the reference like so in the `.csproj`:
   ```xml
   <ProjectReference Include="..\..\Libraries\Nop.Services\Nop.Services.csproj" />
   ```

    [Test]
    public async Task CanGetTaxRateWithPickupPointAddress()
    {
        // Arrange
        var customer = new Customer();
        var pickupPoint = new PickupPoint
        {
            CountryCode = "US",
            StateAbbreviation = "CA",
            County = "Los Angeles",
            City = "Los Angeles",
            Address = "123 Main St",
            ZipPostalCode = "90001"
        };
        var taxService = GetService<ITaxService>();
    
        // Mock generic attribute service to return pickup point
        var genericAttributeService = GetService<IGenericAttributeService>();
        await genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, 1);
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(null, 0, customer, 100);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0);
        isTaxable.Should().BeTrue();
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to **compilation errors** in `TaxServiceTests.cs`:
- Missing `Address` type (missing using directive or assembly reference).
- `GetTaxRateAsync` method is not accessible on `ITaxService` (possibly not implemented or not exposed).
- Tuple deconstruction fails due to inability to infer types for `taxRate` and `isTaxable`.

**Recommended Fixes:**
1. Add the necessary `using` directive for the `Address` type (e.g., `using Nop.Core.Domain.Common;`).
2. Verify if `GetTaxRateAsync` is available on `ITaxService`; if not, adjust the test to use a valid method or mock accordingly.
3. Explicitly declare the types in the deconstruction (e.g., `var (decimal taxRate, bool isTaxable) = ...`).

    [Test]
    public async Task CanGetTaxRateWithVatExemptCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            VatNumberStatusId = (int)VatNumberStatus.Valid
        };
        var address = new Address
        {
            CountryId = 2 // Assume this is a different EU country than the shop
        };
        var taxService = GetService<ITaxService>();
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(null, 0, customer, 100);
    
        // Assert
        taxRate.Should().Be(0);
        isTaxable.Should().BeFalse();
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to **compilation errors** in the test file `TaxServiceTests.cs`. Specifically:
- A call to `GetProductPriceAsync` is missing the required `priceIncludesTax` boolean parameter.
- The deconstruction of the returned tuple (`price, taxRate`) is failing due to type inference issues.

**Recommended Fixes:**
1. **Update the test method call** to include the missing `priceIncludesTax` parameter in `GetProductPriceAsync`.
2. **Explicitly declare the tuple types** when deconstructing the result to resolve type inference errors.

    [Test]
    public async Task CanGetVatNumberStatusForInvalidVatNumber()
    {
        // Arrange
        var taxService = GetService<ITaxService>();
    
        // Act
        var (vatNumberStatus, name, address) = await taxService.GetVatNumberStatusAsync("InvalidVATNumber");
    
        // Assert
        vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        name.Should().BeEmpty();
        address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed due to **compilation errors** in `TaxServiceTests.cs`:

1. **Missing required parameter**: The call to `GetProductPriceAsync` is missing the required `priceIncludesTax` boolean parameter.
2. **Tuple deconstruction issues**: The deconstruction of the returned tuple (`price, taxRate`) is failing due to type inference issues.

**Recommended Fixes**:
1. **Update the test method call** to include the missing `priceIncludesTax` parameter in `GetProductPriceAsync`.
2. **Explicitly declare the tuple types** when deconstructing the result to resolve type inference errors.

    [Test]
    public async Task CanGetProductPriceWithTaxExemptProduct()
    {
        // Arrange
        var product = new Product { IsTaxExempt = true };
        var customer = new Customer();
        var taxService = GetService<ITaxService>();
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 100, true, customer, true);
    
        // Assert
        price.Should().Be(90.90909090909091M);
        taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to **compilation errors** in the test file `TaxServiceTests.cs`. Specifically:
- A call to `GetProductPriceAsync` is missing the required `priceIncludesTax` boolean parameter.
- The deconstruction of the returned tuple (`price, taxRate`) is failing due to type inference issues.

**Recommended Fixes:**
1. **Update the test method call** to include the missing `priceIncludesTax` parameter in `GetProductPriceAsync`.
2. **Explicitly declare the tuple types** when deconstructing the result to resolve type inference errors.

    [Test]
    public async Task CanGetProductPriceWithTaxExemptCustomer()
    {
        // Arrange
        var product = new Product();
        var customer = new Customer { IsTaxExempt = true };
        var taxService = GetService<ITaxService>();
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 100, true, customer, true);
    
        // Assert
        price.Should().Be(90.90909090909091M);
        taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to a **command timeout**, as indicated in the `stderr` output. This typically occurs when the test execution takes longer than the allowed time limit, possibly due to external dependencies (e.g., web service calls in `DoVatCheckAsync`) or inefficient test setup/teardown.

**Recommended Fixes:**
1. **Optimize or mock external services** (e.g., `ICheckVatService`) to avoid real web service calls during tests.
2. **Increase the test timeout** if the delay is expected and acceptable.
3. **Investigate slow tests** (e.g., VAT number validation tests) and ensure they are not waiting on real external resources.

    [Test]
    public async Task CanGetProductPriceWithZeroPrice()
    {
        // Arrange
        var product = new Product();
        var customer = new Customer();
        var taxService = GetService<ITaxService>();
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 0, 0, true, customer, true);
    
        // Assert
        price.Should().Be(0);
        taxRate.Should().Be(0);
    }

*/
}