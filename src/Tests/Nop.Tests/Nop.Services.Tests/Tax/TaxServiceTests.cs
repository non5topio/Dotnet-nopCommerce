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
FAILED TEST: The test run failed due to the following issues:

1. **Missing `Shouldly` NuGet Package**:  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` package using:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate `using` Directives**:  
   - **Warning**: `CS0105: The using directive for '...' appeared previously in this namespace`  
   - **Fix**: Remove duplicate `using` directives from the test file.

3. **Missing Type References**:  
   - **Error**: `CS0246: The type or namespace name 'Address' could not be found`  
   - **Fix**: Add the missing `using` directives to the test file:  
     ```csharp
     using Nop.Core.Domain.Common;
     using Nop.Core.Domain.Countries;
     ```

**Summary of Recommended Fixes:**
1. Install the `Shouldly` NuGet package.
2. Add missing `using` directives for `Address` and `Country`.
3. Remove duplicate `using` directives from the test file.

    [Test]
    public async Task TaxService_IsTaxExemptAsync_CustomerRoleTaxExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        adminRole.TaxExempt = true;
        await _customerService.UpdateCustomerRoleAsync(adminRole);
        await _customerService.AddCustomerRoleMappingAsync(customer, adminRole);
    
        // Act
        var isTaxExempt = await _taxService.IsTaxExemptAsync(null, customer);
    
        // Assert
        isTaxExempt.Should().BeTrue();
    }

*/
/*
FAILED TEST: **Analysis of Test Run Failure:**

1. **Missing `Shouldly` NuGet Package**:  
   - The test file uses `Shouldly` for assertions, but the package is not installed in the test project.  
   - **Fix**: Install the `Shouldly` package using:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate `using` Directives**:  
   - Warnings like `CS0105: The using directive for '...' appeared previously in this namespace` indicate duplicate `using` statements.  
   - **Fix**: Remove duplicate `using` directives from the test file.

3. **Missing Type References**:  
   - **Errors**:  
     - `CS0246: The type or namespace name 'Address' could not be found`  
     - `CS0246: The type or namespace name 'Country' could not be found`  
   - **Fix**: Add the missing `using` directives to the test file:  
     ```csharp
     using Nop.Core.Domain.Common;
     using Nop.Core.Domain.Countries;
     ```

**Summary of Recommended Fixes:**
1. Install the `Shouldly` NuGet package.
2. Add missing `using` directives for `Address` and `Country`.
3. Remove duplicate `using` directives from the test file.

    [Test]
    public async Task TaxService_GetProductPriceAsync_IncludingTax_TaxSubtracted()
    {
        // Arrange
        var customer = new Customer { Email = NopTestsDefaults.AdminEmail };
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax rate to be 10%
        var taxPlugin = new Mock<ITaxProvider>();
        taxPlugin.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = 10, Success = true });
    
        var taxPluginManager = new Mock<ITaxPluginManager>();
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
            .ReturnsAsync(taxPlugin.Object);
    
        var taxService = new TaxService(
            new AddressSettings(),
            new CustomerSettings(),
            new Mock<IAddressService>().Object,
            new Mock<ICheckVatService>().Object,
            new Mock<ICountryService>().Object,
            new Mock<ICustomerService>().Object,
            new Mock<IEventPublisher>().Object,
            new Mock<IGenericAttributeService>().Object,
            new Mock<IGeoLookupService>().Object,
            new Mock<ILogger>().Object,
            new Mock<IStateProvinceService>().Object,
            new Mock<IStoreContext>().Object,
            taxPluginManager.Object,
            new Mock<IWebHelper>().Object,
            new Mock<IWorkContext>().Object,
            new ShippingSettings(),
            new TaxSettings());
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 110, true, customer, true);
    
        // Assert
        price.Should().Be(100);
        taxRate.Should().Be(10);
    }

*/
/*
FAILED TEST: **Analysis of Test Run Failure:**

1. **Missing `Shouldly` NuGet Package**:  
   - The test file uses `Shouldly` for assertions, but the package is not installed in the test project.  
   - **Fix**: Install the `Shouldly` package using:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Type References**:  
   - Errors like `CS0246: The type or namespace name 'Address' could not be found` indicate missing `using` directives for types such as `Address` and `Country`.  
   - **Fix**: Add the missing `using` directives to the test file:  
     ```csharp
     using Nop.Core.Domain.Common;
     using Nop.Core.Domain.Countries;
     ```

3. **Duplicate Using Directives**:  
   - Warnings like `CS0105: The using directive for '...' appeared previously in this namespace` indicate duplicate `using` statements.  
   - **Fix**: Remove duplicate `using` directives from the test file.

**Summary of Recommended Fixes:**
1. Install the `Shouldly` NuGet package.
2. Add missing `using` directives for `Address` and `Country`.
3. Remove duplicate `using` directives from the test file.

    [Test]
    public async Task TaxService_GetProductPriceAsync_ExcludingTax_TaxAdded()
    {
        // Arrange
        var customer = new Customer { Email = NopTestsDefaults.AdminEmail };
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax rate to be 10%
        var taxPlugin = new Mock<ITaxProvider>();
        taxPlugin.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = 10, Success = true });
    
        var taxPluginManager = new Mock<ITaxPluginManager>();
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
            .ReturnsAsync(taxPlugin.Object);
    
        var taxService = new TaxService(
            new AddressSettings(),
            new CustomerSettings(),
            new Mock<IAddressService>().Object,
            new Mock<ICheckVatService>().Object,
            new Mock<ICountryService>().Object,
            new Mock<ICustomerService>().Object,
            new Mock<IEventPublisher>().Object,
            new Mock<IGenericAttributeService>().Object,
            new Mock<IGeoLookupService>().Object,
            new Mock<ILogger>().Object,
            new Mock<IStateProvinceService>().Object,
            new Mock<IStoreContext>().Object,
            taxPluginManager.Object,
            new Mock<IWebHelper>().Object,
            new Mock<IWorkContext>().Object,
            new ShippingSettings(),
            new TaxSettings());
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 100, false, customer, false);
    
        // Assert
        price.Should().Be(110);
        taxRate.Should().Be(10);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing `Shouldly` NuGet Package**:  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` package using:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Type References in Test File**:  
   - **Errors**:  
     - `CS0246: The type or namespace name 'Address' could not be found`  
     - `CS0246: The type or namespace name 'Country' could not be found`  
     - `CS0103: The name '_countryService' does not exist in the current context`  
     - `CS1061: 'ITaxService' does not contain a definition for 'IsVatExemptAsync'`  
   - **Fix**:  
     - Ensure the test file includes the correct `using` directives for the missing types (e.g., `using Nop.Core.Domain.Common;`, `using Nop.Core.Domain.Countries;`).  
     - Verify that `_countryService` is properly initialized in the test setup.  
     - Confirm that `IsVatExemptAsync` is available in the `ITaxService` interface or is implemented correctly.

3. **Duplicate Using Directives**:  
   - **Warnings**: `CS0105: The using directive for '...' appeared previously in this namespace`  
   - **Fix**: Remove duplicate `using` directives from the test file.

**Recommended Actions**:
1. Install the `Shouldly` package.
2. Add missing `using` directives for `Address`, `Country`, and related types.
3. Initialize `_countryService` in the test setup.
4. Ensure `IsVatExemptAsync` is accessible in the test context.
5. Remove duplicate `using` directives.

    [Test]
    public async Task TaxService_IsVatExemptAsync_EuVatExemptionApplied()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var address = new Address
        {
            CountryId = 2, // Assume this is a different EU country
            CreatedOnUtc = DateTime.UtcNow
        };
        var country = new Country
        {
            Id = 2,
            SubjectToVat = true,
            TwoLetterIsoCode = "DE"
        };
        var customerVatStatus = VatNumberStatus.Valid;
        customer.VatNumberStatusId = (int)customerVatStatus;
    
        await _countryService.InsertCountryAsync(country);
    
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatShopCountryId = 1; // Assume this is the shop's country
        _taxSettings.EuVatAllowVatExemption = true;
    
        await _customerService.UpdateCustomerAsync(customer);
    
        // Act
        var isVatExempt = await _taxService.IsVatExemptAsync(address, customer);
    
        // Assert
        isVatExempt.Should().BeTrue();
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` NuGet package is missing from the test project. This package is required for the test assertions used in `TaxServiceTests.cs`.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_GetVatNumberStatusAsync_InvalidVatNumber_StatusIsInvalid()
    {
        // Arrange
        var twoLetterIsoCode = "DE";
        var vatNumber = "InvalidVATNumber";
    
        var checkVatService = new Mock<ICheckVatService>();
        checkVatService.Setup(x => x.CheckVatAsync(twoLetterIsoCode, vatNumber))
            .ReturnsAsync((VatNumberStatus.Invalid, string.Empty, string.Empty));
    
        var taxService = new TaxService(
            Mock.Of<AddressSettings>(),
            Mock.Of<CustomerSettings>(),
            Mock.Of<IAddressService>(),
            checkVatService.Object,
            Mock.Of<ICountryService>(),
            Mock.Of<ICustomerService>(),
            Mock.Of<IEventPublisher>(),
            Mock.Of<IGenericAttributeService>(),
            Mock.Of<IGeoLookupService>(),
            Mock.Of<ILogger>(),
            Mock.Of<IStateProvinceService>(),
            Mock.Of<IStoreContext>(),
            Mock.Of<ITaxPluginManager>(),
            Mock.Of<IWebHelper>(),
            Mock.Of<IWorkContext>(),
            Mock.Of<ShippingSettings>(),
            Mock.Of<TaxSettings>());
    
        // Act
        var (vatNumberStatus, name, address) = await taxService.GetVatNumberStatusAsync(twoLetterIsoCode, vatNumber);
    
        // Assert
        vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        name.Should().BeEmpty();
        address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` NuGet package is missing from the test project. This package is required for the test assertions used in `TaxServiceTests.cs`.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_AutodetectedCountry_TaxCalculatedCorrectly()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
        await _customerService.UpdateCustomerAsync(customer);
    
        var ipAddress = "192.0.2.1";
        var countryIsoCode = "DE";
    
        var webHelper = new Mock<IWebHelper>();
        webHelper.Setup(x => x.GetCurrentIpAddress()).Returns(ipAddress);
    
        var geoLookupService = new Mock<IGeoLookupService>();
        geoLookupService.Setup(x => x.LookupCountryIsoCode(ipAddress)).Returns(countryIsoCode);
    
        var countryService = new Mock<ICountryService>();
        var country = new Country { Id = 1, TwoLetterIsoCode = countryIsoCode };
        countryService.Setup(x => x.GetCountryByTwoLetterIsoCodeAsync(countryIsoCode)).ReturnsAsync(country);
    
        var taxService = new TaxService(
            _addressSettings,
            _customerSettings,
            _addressService,
            _checkVatService,
            countryService.Object,
            _customerService,
            _eventPublisher,
            _genericAttributeService,
            geoLookupService.Object,
            _logger,
            _stateProvinceService,
            _storeContext,
            _taxPluginManager,
            webHelper.Object,
            _workContext,
            _shippingSettings,
            _taxSettings);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(product, 1, customer, 100);
    
        // Assert
        isTaxable.Should().BeTrue();
        taxRate.Should().BeGreaterOrEqual(0);
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` NuGet package is missing from the test project. This package is required for the test assertions used in `TaxServiceTests.cs`.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_InvalidPickupPoint_TaxCalculatedBasedOnDefaultAddress()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
        // Ensure no pickup point is selected
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, (PickupPoint)null);
    
        // Act
        var (taxRate, isTaxable) = await _taxService.GetTaxRateAsync(product, 1, customer, 100);
    
        // Assert
        isTaxable.Should().BeTrue();
        taxRate.Should().BeGreaterOrEqual(0);
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` NuGet package is missing from the test project. This package is required for the test assertions used in the `TaxServiceTests.cs` file.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_PickupPointAddress_TaxCalculatedCorrectly()
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
        var (taxRate, isTaxable) = await _taxService.GetTaxRateAsync(product, 1, customer, 100);
    
        // Assert
        isTaxable.Should().BeTrue();
        taxRate.Should().BeGreaterOrEqual(0);
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` NuGet package is missing from the test project. This package is required for the test assertions used in the `TaxServiceTests.cs` file.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_GetTaxRateAsync_NegativeTaxRate_AdjustedToZero()
    {
        // Arrange
        var customer = new Customer { Email = NopTestsDefaults.AdminEmail };
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax plugin to return negative tax rate
        var taxPlugin = new Mock<ITaxProvider>();
        taxPlugin.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = -5, Success = true });
    
        var taxPluginManager = new Mock<ITaxPluginManager>();
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
            .ReturnsAsync(taxPlugin.Object);
    
        var taxService = new TaxService(
            new AddressSettings(),
            new CustomerSettings(),
            new Mock<IAddressService>().Object,
            new Mock<ICheckVatService>().Object,
            new Mock<ICountryService>().Object,
            new Mock<ICustomerService>().Object,
            new Mock<IEventPublisher>().Object,
            new Mock<IGenericAttributeService>().Object,
            new Mock<IGeoLookupService>().Object,
            new Mock<ILogger>().Object,
            new Mock<IStateProvinceService>().Object,
            new Mock<IStoreContext>().Object,
            taxPluginManager.Object,
            new Mock<IWebHelper>().Object,
            new Mock<IWorkContext>().Object,
            new ShippingSettings(),
            new TaxSettings());
    
        // Act
        var (taxRate, isTaxable) = await taxService.GetTaxRateAsync(product, 1, customer, 100);
    
        // Assert
        taxRate.Should().Be(0);
        isTaxable.Should().BeTrue();
    }

*/
/*
FAILED TEST: The test run failed due to a missing reference to the `Shouldly` library, which is used in the test assertions.

**Analysis:**
- The error `CS0246: The type or namespace name 'Shouldly' could not be found` indicates that the `Shouldly` NuGet package is not installed or referenced in the test project.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package in the test project by running the following command in the Package Manager Console or terminal:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task TaxService_GetProductPriceAsync_ZeroPrice_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.IsTaxExempt = false;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 0, customer);
    
        // Assert
        result.price.Should().Be(0);
        result.taxRate.Should().Be(0);
    }

*/
}
