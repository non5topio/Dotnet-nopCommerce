using FluentAssertions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Tax;
using NUnit.Framework;

using NUnit.Framework;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Tests;
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
/*
FAILED TEST: **Short Analysis:**
The test run failed due to two main issues:
1. Missing `Shouldly` NuGet package and `using Shouldly;` directive, causing `CS0246` errors.
2. Duplicate `using` directives in `TaxServiceTests.cs`, causing `CS0105` warnings.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet Package:**
   ```
   dotnet add package Shouldly
   ```
2. **Add `using Shouldly;` Directive:**
   Add `using Shouldly;` at the top of `TaxServiceTests.cs`.
3. **Remove Duplicate Using Directives:**
   Clean up the file by removing duplicate `using` statements to resolve CS0105 warnings.

    [Test]
    public async Task TaxService_GetProductPriceAsync_NullPickupPoint_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
        _taxSettings.TaxBasedOnPickupPointAddress = true;
        _shippingSettings.AllowPickupInStore = true;
    
        // Ensure no pickup point is selected
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, (PickupPoint)null);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().NotBe(100); // Price should be adjusted based on tax rate
        result.taxRate.Should().NotBe(0); // Tax rate should be calculated based on default address
    }

*/
/*
FAILED TEST: **Short Analysis:**
The test run failed due to two main issues:
1. **Missing `Shouldly` Reference:** The test file uses `Shouldly` assertions, but the `Shouldly` NuGet package is not installed, and the `using Shouldly;` directive is missing.
2. **Duplicate Using Directives:** Multiple duplicate `using` directives in the file cause compiler warnings and may lead to confusion.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet Package:**
   ```
   dotnet add package Shouldly
   ```
2. **Add `using Shouldly;` Directive:**
   Add `using Shouldly;` at the top of `TaxServiceTests.cs`.
3. **Remove Duplicate Using Directives:**
   Clean up the file by removing duplicate `using` statements to resolve CS0105 warnings.

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxBasedOnDetectedCountry()
    {
        // Arrange
        var customer = new Customer { Email = "test@example.com" };
        await _customerService.InsertCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        _taxSettings.AutomaticallyDetectCountry = true;
        _taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
    
        // Simulate no billing address
        customer.BillingAddressId = null;
    
        // Simulate IP address lookup
        var ipAddress = "192.0.2.1";
        var countryIsoCode = "US";
        _webHelper.GetCurrentIpAddress().Returns(ipAddress);
        _geoLookupService.LookupCountryIsoCode(ipAddress).Returns(countryIsoCode);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().NotBe(100); // Price should be adjusted based on tax rate
        result.taxRate.Should().NotBe(0); // Tax rate should be calculated based on detected country
    }

*/
/*
FAILED TEST: **Short Analysis:**
The test run failed due to two main issues:
1. **Missing `Shouldly` Reference:** The test file `TaxServiceTests.cs` uses `Shouldly` assertions, but the `Shouldly` NuGet package is not installed, and the `using Shouldly;` directive is missing.
2. **Duplicate Using Directives:** Multiple duplicate `using` directives in the file cause compiler warnings and may lead to confusion.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet Package:**
   ```
   dotnet add package Shouldly
   ```
2. **Add `using Shouldly;` Directive:**
   Add `using Shouldly;` at the top of `TaxServiceTests.cs`.
3. **Remove Duplicate Using Directives:**
   Clean up the file by removing duplicate `using` statements to resolve CS0105 warnings.

    [Test]
    public async Task TaxService_GetVatNumberStatusAsync_InvalidVatNumber()
    {
        // Arrange
        var fullVatNumber = "DE123456789";
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.ShouldBe(VatNumberStatus.Invalid);
        result.name.ShouldBeEmpty();
        result.address.ShouldBeEmpty();
    }

*/
/*
FAILED TEST: **Short Analysis:**

The test run failed due to two main issues:

1. **Missing `Shouldly` Reference:** The test file `TaxServiceTests.cs` uses `Shouldly` assertions, but the required `using Shouldly;` directive is missing, and the `Shouldly` NuGet package is not installed.
2. **Duplicate Using Directives:** Multiple duplicate `using` directives are present in the file, causing compiler warnings and potential confusion.

**Recommended Fixes:**

1. **Install `Shouldly` NuGet Package:**
   ```
   dotnet add package Shouldly
   ```

2. **Add `using Shouldly;` Directive:**
   Add `using Shouldly;` at the top of `TaxServiceTests.cs`.

3. **Remove Duplicate Using Directives:**
   Clean up the file by removing duplicate `using` statements to resolve CS0105 warnings.

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxBasedOnPickupPointAddress()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
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
    
        _taxSettings.TaxBasedOnPickupPointAddress = true;
        _shippingSettings.AllowPickupInStore = true;
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().NotBe(100); // Price should be adjusted based on tax rate
        result.taxRate.Should().NotBe(0); // Tax rate should be calculated based on pickup point
    }

*/
/*
FAILED TEST: **Analysis:**

The test run failed due to two main issues:

1. **Missing `Shouldly` Reference:** The test file `TaxServiceTests.cs` uses `Shouldly` assertions (e.g., `result.price.Should().Be(100);`), but the necessary `using Shouldly;` directive is missing, and the `Shouldly` NuGet package is not referenced.

2. **Missing `_countryService` and `Arg` in Test Context:** The test attempts to use `_countryService` and `Arg` (from `NUnit.Framework` or `NSubstitute`), but these are not defined or referenced in the current test setup.

**Recommended Fixes:**

1. **Install `Shouldly` NuGet Package:**
   ```
   dotnet add package Shouldly
   ```

2. **Add `using Shouldly;` Directive:**
   Add `using Shouldly;` at the top of `TaxServiceTests.cs`.

3. **Resolve `_countryService` and `Arg` Issues:**
   - Ensure `_countryService` is initialized in the test setup (e.g., via `GetService<ICountryService>()`).
   - If using `Arg` from `NSubstitute`, install the `NSubstitute` package and add `using NSubstitute;`.

    [Test]
    public async Task TaxService_GetProductPriceAsync_EuVatExempt_TaxRateIsZero()
    {
        // Arrange
        var customer = new Customer { Email = NopTestsDefaults.AdminEmail, IsTaxExempt = false, VatNumberStatusId = (int)VatNumberStatus.Valid };
        await _customerService.InsertCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock country and shop country to be different
        var country = new Country { Id = 2, SubjectToVat = true };
        var shopCountry = new Country { Id = 1, SubjectToVat = true };
        _countryService.GetCountryByIdAsync(Arg.Any<int>()).Returns(country);
        _taxSettings.EuVatShopCountryId = 1;
        _taxSettings.EuVatAllowVatExemption = true;
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: The test run failed due to a missing reference to the `Shouldly` library, which is used in the `TaxServiceTests.cs` file. The compiler error `CS0246` indicates that the type or namespace name 'Shouldly' could not be found.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package in the test project.

    [Test]
    public async Task TaxService_GetProductPriceAsync_NoActiveTaxProvider_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax plugin manager to return null (no active tax provider)
        var taxPluginManager = Substitute.For<ITaxPluginManager>();
        taxPluginManager.When(x => x.LoadPrimaryPluginAsync(customer, Arg.Any<int>())).Return(Task.FromResult<ITaxProvider>(null));
    
        // Create TaxService with mocked dependencies
        var taxService = new TaxService(
            _addressSettings,
            _customerSettings,
            _addressService,
            _checkVatService,
            _countryService,
            _customerService,
            _eventPublisher,
            _genericAttributeService,
            _geoLookupService,
            _logger,
            _stateProvinceService,
            _storeContext,
            taxPluginManager,
            _webHelper,
            _workContext,
            _shippingSettings,
            _taxSettings
        );
    
        // Act
        var result = await taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/

    [Test]
    public async Task TaxService_GetProductPriceAsync_TaxExemptCustomerRole_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        adminRole.TaxExempt = true;
        await _customerService.UpdateCustomerRoleAsync(adminRole);
        await _customerService.UpdateCustomerAsync(customer); // Ensure role is applied
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

/*
FAILED TEST: The test run failed due to a missing reference or using directive for the `Shouldly` library, which is used in the test file `TaxServiceTests.cs`.

**Error:**
```
/app/src/Tests/Nop.Tests/Nop.Services.Tests/Tax/TaxServiceTests.cs(12,7): error CS0246: The type or namespace name 'Shouldly' could not be found (are you missing a using directive or an assembly reference?)
```

**Recommended Fix:**
1. Install the `Shouldly` NuGet package in the test project.
2. Ensure the `using Shouldly;` directive is included in the file.

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
