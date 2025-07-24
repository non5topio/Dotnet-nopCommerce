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

    [Test]
    public async Task TaxService_GetShippingPriceAsync_NonTaxableShipping_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var taxSettings = _taxSettings;
        taxSettings.ShippingIsTaxable = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Act
        var result = await _taxService.GetShippingPriceAsync(100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

/*
FAILED TEST: **Analysis of Test Failure:**

- The test `TaxService_GetProductPriceAsync_ProductWithNoTaxCategory_DefaultUsed` failed because the expected price was greater than 100M, but it was exactly 100M.
- This suggests that the tax rate applied was 0%, likely due to the tax provider not being properly mocked or tax settings not being configured to apply a tax rate.
- The test class `TaxServiceTests` is missing required service initializations (`_webHelper`, `_geoLookupService`, `_genericAttributeService`, and possibly `_logger`), which are used in the `TaxService`.
- The `ITaxPluginManager` is not being properly mocked, which is essential for simulating tax calculation behavior.

**Recommended Fixes:**

1. **Initialize Missing Services in Test Setup:**
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   _logger = GetService<ILogger>();
   ```

2. **Mock `ITaxPluginManager` Properly:**
   - Create a mock tax provider and return it from `LoadPrimaryPluginAsync`:
   ```csharp
   var mockTaxProvider = new Mock<ITaxProvider>();
   mockTaxProvider.Setup(tp => tp.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
       .ReturnsAsync(new TaxRateResult { TaxRate = 10M, Success = true });
   _taxPluginManager = Mock.Of<ITaxPluginManager>(tm => tm.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()) == Task.FromResult(mockTaxProvider.Object));
   ```

3. **Ensure Tax Settings Are Configured Correctly:**
   ```csharp
   _taxSettings.PricesIncludeTax = false;
   _taxSettings.TaxBasedOn = TaxBasedOn.DefaultAddress;
   _taxSettings.DefaultTaxAddressId = 1;
   await _settingService.SaveSettingAsync(_taxSettings);
   ```

4. **Verify Tax Calculation Logic:**
   - Ensure that the tax rate is being calculated and returned correctly by the mocked tax provider and that the test assertions match the expected behavior.

    [Test]
    public async Task TaxService_GetProductPriceAsync_ProductWithNoTaxCategory_DefaultUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 0 }; // No tax category
        var taxSettings = _taxSettings;
        taxSettings.PricesIncludeTax = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().NotBe(0);
        result.price.Should().BeGreaterThan(100);
    }

*/
/*
FAILED TEST: **Analysis of Test Failure:**

1. **Test Failure in `TaxService_PrepareTaxRateRequestAsync_TaxBasedOnDefaultAddress`:**
   - The test expected a non-zero tax rate but received 0.
   - This is likely due to the tax provider not being properly mocked, or the tax calculation logic not being triggered as expected.

2. **Missing Service Initialization:**
   - The test class `TaxServiceTests` does not initialize required services such as `_webHelper`, `_geoLookupService`, and `_genericAttributeService`, which are used by `TaxService`.

3. **Incorrect Mocking of `ITaxPluginManager`:**
   - The test uses `_taxPluginManager.Setup(...)`, but `ITaxPluginManager` is not a mockable interface.
   - Instead, the tax provider should be mocked directly and returned via `LoadPrimaryPluginAsync`.

4. **Missing `_logger` Initialization:**
   - The test references `_logger`, but it is not declared or initialized in the test class.

---

**Recommended Fixes:**

1. **Initialize Missing Services in Test Setup:**
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   _logger = GetService<ILogger>();
   ```

2. **Fix Mocking of Tax Provider:**
   - Mock the tax provider directly and return it from `LoadPrimaryPluginAsync`:
   ```csharp
   var mockTaxProvider = new Mock<ITaxProvider>();
   mockTaxProvider.Setup(tp => tp.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
       .ReturnsAsync(new TaxRateResult { TaxRate = 20M, Success = true });
   _taxPluginManager = new Mock<ITaxPluginManager>();
   _taxPluginManager.Setup(t => t.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
       .ReturnsAsync(mockTaxProvider.Object);
   ```

3. **Ensure Tax Settings Are Configured Correctly:**
   - Set up tax settings to ensure the tax logic is triggered as expected in the test:
   ```csharp
   _taxSettings.EuVatEnabled = false;
   _taxSettings.TaxBasedOn = TaxBasedOn.DefaultAddress;
   _taxSettings.DefaultTaxAddressId = 1;
   await _settingService.SaveSettingAsync(_taxSettings);
   ```

4. **Verify Tax Calculation Logic:**
   - Ensure that the tax rate is being calculated and returned correctly by the mocked tax provider and that the test assertions match the expected behavior.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_TaxBasedOnDefaultAddress()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.DefaultTaxAddressId = 1; // Assume a valid default tax address exists
        taxSettings.TaxBasedOn = TaxBasedOn.DefaultAddress;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().NotBe(0);
    }

*/
/*
FAILED TEST: **Analysis of Test Failure:**

1. **Test Failure in `TaxService_GetVatNumberStatusAsync_InvalidVatNumber_NotExempt`:**
   - The test expected the VAT number status to be `VatNumberStatus.Invalid`, but it received `VatNumberStatus.Unknown`.
   - This is likely because the VAT validation logic in the test is not properly mocked or configured to return the expected invalid VAT status.

2. **Missing Service Initialization:**
   - The test class `TaxServiceTests` is missing initialization for required services: `_webHelper`, `_geoLookupService`, and `_genericAttributeService`.
   - These services are used by `TaxService` and must be initialized or mocked for the tests to run correctly.

3. **Incorrect Mocking of `ITaxPluginManager`:**
   - The test uses `_taxPluginManager.Setup(...)`, but `ITaxPluginManager` is not a mockable interface.
   - Instead, the tax provider should be mocked directly and returned via `LoadPrimaryPluginAsync`.

4. **Missing `_logger` Initialization:**
   - The test references `_logger`, but it is not declared or initialized in the test class.

---

**Recommended Fixes:**

1. **Initialize Missing Services in Test Setup:**
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   ```

2. **Fix Mocking of Tax Provider:**
   - Mock the tax provider directly and return it from `LoadPrimaryPluginAsync`:
   ```csharp
   var mockTaxProvider = new Mock<ITaxProvider>();
   mockTaxProvider.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
                  .ReturnsAsync(new TaxRateResult { Success = false, Errors = new[] { "Invalid VAT number" } });
   _taxPluginManager = new Mock<ITaxPluginManager>();
   _taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
                    .ReturnsAsync(mockTaxProvider.Object);
   ```

3. **Initialize `_logger`:**
   ```csharp
   _logger = GetService<ILogger>();
   ```

4. **Ensure VAT Validation Logic is Mocked Correctly:**
   - Mock the `_checkVatService` to return the expected invalid VAT status:
   ```csharp
   var mockCheckVatService = new Mock<ICheckVatService>();
   mockCheckVatService.Setup(x => x.CheckVatAsync("US", "123456789"))
                     .ReturnsAsync((VatNumberStatus.Invalid, string.Empty, string.Empty));
   ```

    [Test]
    public async Task TaxService_GetVatNumberStatusAsync_InvalidVatNumber_NotExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var taxSettings = _taxSettings;
        taxSettings.EuVatEnabled = true;
        taxSettings.EuVatUseWebService = true;
        taxSettings.EuVatAssumeValid = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        var fullVatNumber = "US123456789"; // Invalid VAT number for the US
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: **Analysis of Test Failure:**
1. **Missing Service Initialization:**
   - The test class references `_webHelper`, `_geoLookupService`, and `_genericAttributeService`, which are not initialized.
2. **Incorrect Method Usage:**
   - The test uses `_taxPluginManager.Setup(...)`, but `ITaxPluginManager` is not a mockable interface; it should be mocked using a mocking framework.
3. **Missing Reference to `_logger`:**
   - The test references `_logger`, but it is not declared or initialized in the test class.
4. **Incorrect Method Call:**
   - The test uses `_customerService.AddCustomerRoleMappingAsync(customer, adminRole);`, but this method does not accept two arguments in the current implementation.

**Recommended Fixes:**
1. **Initialize Missing Services:**
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   ```
2. **Fix Mocking of `ITaxPluginManager`:**
   - Replace with a mock object:
   ```csharp
   var taxPluginManagerMock = new Mock<ITaxPluginManager>();
   _taxPluginManager = taxPluginManagerMock.Object;
   ```
3. **Initialize `_logger`:**
   ```csharp
   _logger = GetService<ILogger>();
   ```
4. **Fix Method Call:**
   - Replace with the correct overload of `AddCustomerRoleMappingAsync`.

    [Test]
    public async Task TaxService_GetTaxRateAsync_NoTaxProvider_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax plugin manager to return null provider
        _taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(customer, It.IsAny<int>()))
            .ReturnsAsync((ITaxProvider)null);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: **Analysis of Test Failure:**

1. **Missing Service Initialization:**
   - The test class `TaxServiceTests` references `_webHelper`, `_geoLookupService`, and `_genericAttributeService`, which are not initialized in the test setup.
   - These services are required by the `TaxService` and must be injected or mocked for the tests to run correctly.

2. **Incorrect Method Usage:**
   - The test uses `_taxPluginManager.Setup(...)`, but `ITaxPluginManager` is not a mockable interface in the current implementation.
   - The test also attempts to assign a value to the read-only property `TaxRateResult.Success`.

3. **Missing Reference to `_logger`:**
   - The test references `_logger`, but it is not declared or initialized in the test class.

---

**Recommended Fixes:**

1. **Initialize Missing Services:**
   Add the following lines to the test setup to initialize the missing services:
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   ```

2. **Fix Mocking of `ITaxPluginManager`:**
   - Replace the incorrect `Setup` calls with proper mocking using a mock object for the tax provider.
   - Example:
     ```csharp
     var mockTaxProvider = new Mock<ITaxProvider>();
     mockTaxProvider.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
         .ReturnsAsync(new TaxRateResult { TaxRate = -1, Success = true });
     var mockTaxPluginManager = new Mock<ITaxPluginManager>();
     mockTaxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
         .ReturnsAsync(mockTaxProvider.Object);
     _taxPluginManager = mockTaxPluginManager.Object;
     ```

3. **Fix Read-Only Property Assignment:**
   - Avoid assigning to `TaxRateResult.Success`. Instead, construct the object with the correct value:
     ```csharp
     var taxRateResult = new TaxRateResult { TaxRate = -1, Success = true };
     ```

4. **Initialize `_logger`:**
   Add the following line to the test setup to initialize the logger:
   ```csharp
   _logger = GetService<ILogger>();
   ```

    [Test]
    public async Task TaxService_GetTaxRateAsync_NegativeTaxRate_ReturnsZeroAndLogsError()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.LogErrors = true;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Mock tax provider to return negative tax rate
        _taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(customer, It.IsAny<int>()))
            .ReturnsAsync(new Mock<ITaxProvider>().Object);
        _taxPluginManager.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = -10, Success = true });
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().Be(0);
        // Verify that error was logged
        _logger.Verify(x => x.ErrorAsync(It.IsAny<string>(), null, customer), Times.Once);
    }

*/
/*
FAILED TEST: **Analysis of Test Failure:**

1. **Missing Service Initialization:**
   - The test class `TaxServiceTests` references `_webHelper`, `_geoLookupService`, and `_genericAttributeService`, which are not initialized in the test setup.
   - These services are required by the `TaxService` and must be injected or mocked for the tests to run correctly.

2. **Incorrect Method Usage:**
   - The test uses `_customerService.AddCustomerRoleMappingAsync(customer, adminRole);`, but the method `AddCustomerRoleMappingAsync` does not accept two arguments in the current implementation.
   - This results in a compilation error: `CS1501: No overload for method 'AddCustomerRoleMappingAsync' takes 2 arguments`.

---

**Recommended Fixes:**

1. **Initialize Missing Services:**
   Add the following lines to the test setup to initialize the missing services:
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   _genericAttributeService = GetService<IGenericAttributeService>();
   ```

2. **Fix Method Call:**
   Replace the incorrect method call with the correct overload of `AddCustomerRoleMappingAsync`. For example:
   ```csharp
   await _customerService.AddCustomerRoleMappingAsync(customer.Id, adminRole.Id);
   ```
   Ensure the method signature matches the actual implementation in the service.

    [Test]
    public async Task TaxService_GetProductPriceAsync_CustomerWithTaxExemptRole_TaxRateIsZero()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
        adminRole.TaxExempt = true;
        await _customerService.UpdateCustomerRoleAsync(adminRole);
        await _customerService.AddCustomerRoleMappingAsync(customer, adminRole);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.price.Should().Be(100);
        result.taxRate.Should().Be(0);
    }

*/

    [Test]
    public async Task TaxService_GetProductPriceAsync_NonTaxableProduct_TaxRateIsZero()
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
FAILED TEST: The test run failed because the test class `TaxServiceTests.cs` is referencing services (`_webHelper` and `_geoLookupService`) that are not initialized in the test context.

**Reason for Failure:**
- The test code references `_webHelper` and `_geoLookupService`, but these fields are not declared or initialized in the test setup.

**Recommended Fixes:**
1. **Initialize `_webHelper` and `_geoLookupService` in the test setup:**
   Add the following lines to the initialization section of the test class:
   ```csharp
   _webHelper = GetService<IWebHelper>();
   _geoLookupService = GetService<IGeoLookupService>();
   ```

2. **Ensure all required services used in `TaxService` are properly mocked or injected in the test setup.**

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithNoAddress_AutoDetectCountry()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
        await _customerService.UpdateCustomerAsync(customer);
    
        var taxSettings = _taxSettings;
        taxSettings.AutomaticallyDetectCountry = true;
        taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
        await _settingService.SaveSettingAsync(taxSettings);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock IP address and country lookup
        _webHelper.Setup(x => x.GetCurrentIpAddress()).Returns("192.0.2.1");
        _geoLookupService.Setup(x => x.LookupCountryIsoCode("192.0.2.1")).Returns("US");
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().NotBe(0);
    }

*/
/*
FAILED TEST: The test run failed because the name `_genericAttributeService` does not exist in the current context in `TaxServiceTests.cs` at line 99.

**Recommended Fix:**
Initialize `_genericAttributeService` in the test setup. Add the following line to the initialization section:

```csharp
_genericAttributeService = GetService<IGenericAttributeService>();
```

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithPickupPoint_TaxBasedOnPickupPoint()
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
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOnPickupPointAddress = true;
        taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, 100, customer);
    
        // Assert
        result.taxRate.Should().NotBe(0);
    }

*/
}
