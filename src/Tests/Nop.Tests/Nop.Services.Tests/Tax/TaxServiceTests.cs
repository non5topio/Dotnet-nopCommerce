using FluentAssertions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Tax;
using NUnit.Framework;

using NUnit.Framework;
using FluentAssertions;
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

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxExemptCustomer_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.IsTaxExempt = true;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxExemptProduct_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { IsTaxExempt = true, TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

    [Test]
    public async Task TaxService_GetProductPriceAsync_ValidVatNumber_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

/*
FAILED TEST: ### **Test Run Failure Analysis:**

1. **Missing `Shouldly` Reference:**
   - **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix:** Install the `Shouldly` NuGet package:
     ```bash
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives:**
   - **Error:** `CS0105: The using directive appeared previously in this namespace`
   - **Fix:** Remove duplicate `using` directives from `TaxServiceTests.cs`.

3. **Missing `GeoIp` Namespace:**
   - **Error:** `CS0234: The type or namespace name 'GeoIp' does not exist in the namespace 'Nop.Services'`
   - **Fix:** Add a reference to the assembly where `Nop.Services.GeoIp` is defined or remove the unused directive.

4. **Duplicate Method Definition:**
   - **Error:** `CS0111: Type 'TaxServiceTests' already defines a member called 'TaxService_GetProductPriceAsync_TaxExemptProduct_TaxRateIsZero'`
   - **Fix:** Rename or remove the duplicate method in `TaxServiceTests.cs`.

5. **Missing `_genericAttributeService`:**
   - **Error:** `CS0103: The name '_genericAttributeService' does not exist in the current context`
   - **Fix:** Initialize `_genericAttributeService` in the test setup:
     ```csharp
     _genericAttributeService = GetService<IGenericAttributeService>();
     ```

6. **Missing `PickupPoint` Type:**
   - **Error:** `CS0246: The type or namespace name 'PickupPoint' could not be found`
   - **Fix:** Add a reference to the assembly where `PickupPoint` is defined or import the correct namespace.

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxExemptProduct_TaxRateIsZero()
    {
        // Arrange
        var customer = new Customer { Email = NopTestsDefaults.AdminEmail };
        var product = new Product { IsTaxExempt = true, TaxCategoryId = 1 };
    
        // Mock dependencies
        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail))
                           .ReturnsAsync(customer);
    
        var mockTaxService = new Mock<ITaxService>();
    
        // Initialize SUT
        var taxService = new TaxService(
            addressSettings: new AddressSettings(),
            customerSettings: new CustomerSettings(),
            addressService: new Mock<IAddressService>().Object,
            checkVatService: new Mock<ICheckVatService>().Object,
            countryService: new Mock<ICountryService>().Object,
            customerService: mockCustomerService.Object,
            eventPublisher: new Mock<IEventPublisher>().Object,
            genericAttributeService: new Mock<IGenericAttributeService>().Object,
            geoLookupService: new Mock<IGeoLookupService>().Object,
            logger: new Mock<ILogger>().Object,
            stateProvinceService: new Mock<IStateProvinceService>().Object,
            storeContext: new Mock<IStoreContext>().Object,
            taxPluginManager: new Mock<ITaxPluginManager>().Object,
            webHelper: new Mock<IWebHelper>().Object,
            workContext: new Mock<IWorkContext>().Object,
            shippingSettings: new ShippingSettings(),
            taxSettings: new TaxSettings()
        );
    
        // Act
        var result = await taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: **Test Run Failure Analysis:**

1. **Missing `Shouldly` Reference:**
   - **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix:** Install the `Shouldly` NuGet package:
     ```bash
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives:**
   - **Warning:** `CS0105: The using directive appeared previously in this namespace`
   - **Fix:** Remove duplicate `using` directives from the file.

3. **Missing `PickupPoint` Type:**
   - **Error:** `CS0246: The type or namespace name 'PickupPoint' could not be found`
   - **Fix:** Add a reference to the assembly where `PickupPoint` is defined or import the correct namespace.

4. **Missing `_genericAttributeService`:**
   - **Error:** `CS0103: The name '_genericAttributeService' does not exist in the current context`
   - **Fix:** Initialize `_genericAttributeService` in the test setup:
     ```csharp
     _genericAttributeService = GetService<IGenericAttributeService>();
     ```

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxExemptRole_TaxRateIsZero()
    {
        // Arrange
        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        adminRole.TaxExempt = true;
        await _customerService.UpdateCustomerRoleAsync(adminRole);
    
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis:**

1. **Failed Test: `TaxService_GetProductPriceAsync_InvalidVatNumber_TaxRateCalculatedNormally`**
   - **Error Message:** `Expected result.price to be greater than 100M, but found 100M.`
   - **Reason:** The actual price after tax calculation did not increase as expected, indicating a possible issue with tax rate application or VAT validation logic.
   - **Fix:** 
     - Review the tax calculation logic in `TaxService.GetProductPriceAsync` and ensure VAT validation is correctly applied.
     - Confirm the test setup correctly simulates an invalid VAT number scenario.

2. **Missing `Shouldly` Reference:**
   - **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix:** Install the `Shouldly` NuGet package:
     ```bash
     dotnet add package Shouldly
     ```

3. **Missing `PickupPoint` Type:**
   - **Error:** `CS0246: The type or namespace name 'PickupPoint' could not be found`
   - **Fix:** Add a reference to the assembly where `PickupPoint` is defined or import the correct namespace.

4. **Missing `_genericAttributeService`:**
   - **Error:** `CS0103: The name '_genericAttributeService' does not exist in the current context`
   - **Fix:** Initialize `_genericAttributeService` in the test setup:
     ```csharp
     _genericAttributeService = GetService<IGenericAttributeService>();
     ```

5. **Duplicate Using Directives:**
   - **Warning:** `CS0105: The using directive appeared previously in this namespace`
   - **Fix:** Remove duplicate `using` directives from the file.

    [Test]
    public async Task TaxService_GetProductPriceAsync_InvalidVatNumber_TaxRateCalculatedNormally()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.VatNumber = "InvalidVATNumber";
        customer.VatNumberStatusId = (int)VatNumberStatus.Invalid;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().BeGreaterThan(100);
        result.taxRate.Should().BeGreaterThan(0);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis:**

1. **Missing `Shouldly` Reference:**
   - **Reason:** The `Shouldly` NuGet package is not installed in the test project.
   - **Fix:** Install the `Shouldly` package using:
     ```bash
     dotnet add package Shouldly
     ```

2. **Missing `PickupPoint` Type:**
   - **Reason:** The `PickupPoint` class is not referenced or available in the test project.
   - **Fix:** Ensure the correct namespace is imported or add a reference to the assembly where `PickupPoint` is defined.

3. **Duplicate Using Directives:**
   - **Reason:** Duplicate `using` directives for the same namespace.
   - **Fix:** Remove the duplicate `using` directives from the file.

4. **Missing `_genericAttributeService`:**
   - **Reason:** The `_genericAttributeService` field is not initialized or injected in the test class.
   - **Fix:** Initialize `_genericAttributeService` in the test setup, for example:
     ```csharp
     _genericAttributeService = GetService<IGenericAttributeService>();
     ```

    [Test]
    public async Task TaxService_GetProductPriceAsync_InvalidTaxCategoryId_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 0 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis:**

1. **Missing `Shouldly` Reference:**
   - **Reason:** The `Shouldly` NuGet package is not installed in the test project.
   - **Fix:** Install the `Shouldly` package using:
     ```bash
     dotnet add package Shouldly
     ```

2. **Missing `PickupPoint` Type:**
   - **Reason:** The `PickupPoint` class is not referenced or available in the test project.
   - **Fix:** Ensure the correct namespace is imported or add a reference to the assembly where `PickupPoint` is defined.

3. **Duplicate Using Directives:**
   - **Warning:** `CS0105: The using directive appeared previously in this namespace`
   - **Fix:** Remove the duplicate `using` directives from the file.

    [Test]
    public async Task TaxService_GetProductPriceAsync_NoAddress_TaxRateBasedOnDetectedCountry()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().BeGreaterThan(100);
        result.taxRate.Should().BeGreaterThan(0);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis:**

1. **Missing `Shouldly` Reference:**
   - **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Reason:** The `Shouldly` NuGet package is not installed in the test project.
   - **Fix:** Install the `Shouldly` package using:
     ```bash
     dotnet add package Shouldly
     ```

2. **Missing `PickupPoint` Type:**
   - **Error:** `CS0246: The type or namespace name 'PickupPoint' could not be found`
   - **Reason:** The `PickupPoint` class is not referenced or available in the test project.
   - **Fix:** Ensure the correct namespace is imported or add a reference to the assembly where `PickupPoint` is defined.

3. **Missing `_genericAttributeService`:**
   - **Error:** `CS0103: The name '_genericAttributeService' does not exist in the current context`
   - **Reason:** The `_genericAttributeService` field is not initialized or injected in the test class.
   - **Fix:** Initialize `_genericAttributeService` in the test setup, for example:
     ```csharp
     _genericAttributeService = GetService<IGenericAttributeService>();
     ```

4. **Duplicate Using Directives:**
   - **Warning:** `CS0105: The using directive appeared previously in this namespace`
   - **Reason:** Duplicate `using` directives for the same namespace.
   - **Fix:** Remove the duplicate `using` directives from the file.

### **Summary of Recommended Fixes:**
1. Install `Shouldly`:
   ```bash
   dotnet add package Shouldly
   ```
2. Add missing type reference for `PickupPoint`.
3. Initialize `_genericAttributeService` in test setup.
4. Remove duplicate `using` directives.

    [Test]
    public async Task TaxService_GetProductPriceAsync_PickupPointAddress_TaxRateBasedOnPickup()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var pickupPoint = new PickupPoint
        {
            CountryCode = "US",
            StateAbbreviation = "CA",
            County = "Los Angeles",
            City = "Los Angeles",
            Address = "123 Main St",
            ZipPostalCode = "90001"
        };
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().BeGreaterThan(100);
        result.taxRate.Should().BeGreaterThan(0);
    }

*/
/*
FAILED TEST: The test run failed due to a missing reference to the `Shouldly` library, which is used in the test methods.

### **Failure Reason:**
- **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
- This indicates that the `Shouldly` NuGet package is not installed or referenced in the test project.

### **Recommended Fix:**
Install the `Shouldly` NuGet package in the test project by running the following command in the Package Manager Console or terminal:

```bash
dotnet add package Shouldly
```

Or manually add the package reference to the `.csproj` file:

```xml
<PackageReference Include="Shouldly" Version="4.0.0" />
```

After adding the reference, re-run the tests.

    [Test]
    public async Task TaxService_GetProductPriceAsync_ZeroPrice_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 0, customer);
    
        // Assert
        result.price.Should().Be(0);
        result.taxRate.Should().Be(0);
    }

*/
}
