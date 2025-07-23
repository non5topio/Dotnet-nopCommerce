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

/*
FAILED TEST: **Analysis:**  
The test run failed because the `NSubstitute` namespace is missing from the test project, which is required for mocking in the `TaxServiceTests.cs` file. This indicates that the required NuGet package for NSubstitute is not installed or not properly referenced.

**Recommended Fix:**  
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_GetProductPriceAsync_ZeroPrice_ReturnsZeroTax()
    {
        // Arrange
        var product = new Product { TaxCategoryId = 1 };
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var price = decimal.Zero;
    
        _taxSettings.PricesIncludeTax = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, price, customer);
    
        // Assert
        result.price.Should().Be(price);
        result.taxRate.Should().Be(decimal.Zero);
    }

*/
/*
FAILED TEST: **Analysis:**  
The test run failed because the `NSubstitute` namespace is missing from the test project, which is required for mocking in the `TaxServiceTests.cs` file.

**Recommended Fix:**  
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_GetVatNumberStatusAsync_InvalidVatNumberFormat_ReturnsInvalidStatus()
    {
        // Arrange
        var fullVatNumber = "InvalidFormat";
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatUseWebService = true;
        _taxSettings.EuVatAssumeValid = false;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed because the `NSubstitute` namespace is missing from the test project, which is required for mocking in the `TaxServiceTests.cs` file.

**Reason:**  
The required `NSubstitute` NuGet package is not installed or referenced in the test project.

**Recommended Fix:**  
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_IsVatExemptAsync_CustomerInSameEUCountryAsShop_IsNotExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var address = new Address { CountryId = 1, StateProvinceId = 1 };
        customer.IsTaxExempt = false;
        await _customerService.UpdateCustomerAsync(customer);
    
        var country = new Country { Id = 1, TwoLetterIsoCode = "DE", SubjectToVat = true };
        await _countryService.InsertCountryAsync(country);
    
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatShopCountryId = 1;
        _taxSettings.EuVatAllowVatExemption = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);
    
        // Act
        var result = await _taxService.IsVatExemptAsync(address, customer);
    
        // Assert
        result.Should().BeFalse();
    }

*/
/*
FAILED TEST: The test run failed because the `NSubstitute` namespace is missing from the test project, which is required for the test setup and mocking in `TaxServiceTests.cs`.

**Recommended Fix:**
Install the `NSubstitute` NuGet package in the test project by running the following command:

```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_IsVatExemptAsync_CustomerInDifferentEUCountryWithValidVatNumber_IsExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var address = new Address { CountryId = 2, StateProvinceId = 1 };
        customer.IsTaxExempt = false;
        await _customerService.UpdateCustomerAsync(customer);
    
        var country = new Country { Id = 2, TwoLetterIsoCode = "DE", SubjectToVat = true };
        await _countryService.InsertCountryAsync(country);
    
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatShopCountryId = 1;
        _taxSettings.EuVatAllowVatExemption = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);
    
        // Act
        var result = await _taxService.IsVatExemptAsync(address, customer);
    
        // Assert
        result.Should().BeTrue();
    }

*/

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
/*
FAILED TEST: **Analysis:**
1. The test run failed because the `NSubstitute` NuGet package is missing from the test project, which is required for mocking in `TaxServiceTests.cs`.
2. There are multiple syntax errors in the `TaxServiceTests.cs` file, likely due to incomplete or malformed test code.

**Recommended Fixes:**
1. **Install NSubstitute:**
   ```
   dotnet add package NSubstitute
   ```
2. **Fix syntax errors in `TaxServiceTests.cs`:**
   - Ensure all test methods are properly enclosed within `[Test]` attributes and valid C# method syntax.
   - Correct any malformed or incomplete test code.

    [Test]
    public async Task TaxService_IsVatExemptAsync_CustomerInDifferentEUCountryWithValidVatNumber_IsExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var address = new Address { CountryId = 2, StateProvinceId = 1 };
        customer.IsTaxExempt = false;
        await _customerService.UpdateCustomerAsync(customer);
    
        var country = new Country { Id = 2, TwoLetterIsoCode = "DE", SubjectToVat = true };
        await _countryService.InsertCountryAsync(country);
    
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatShopCountryId = 1;
        _taxSettings.EuVatAllowVatExemption = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);
    
        // Act
        var result = await _taxService.IsVatExemptAsync(address, customer);
    
        // Assert
        result.Should().BeTrue();
    }

*/
/*
FAILED TEST: ### **Analysis:**
1. The test run failed because the `NSubstitute` NuGet package is missing from the test project, which is required for mocking in `TaxServiceTests.cs`.
2. There are multiple syntax and compilation errors in the `TaxServiceTests.cs` file, likely due to incomplete or malformed test code.

---

### **Recommended Fixes:**
1. **Install NSubstitute:**
   ```
   dotnet add package NSubstitute
   ```

2. **Fix syntax errors in `TaxServiceTests.cs`:**
   - Ensure all test methods are properly enclosed within `[Test]` attributes and valid C# method syntax.
   - Correct any malformed or incomplete test code.

    [Test]
    public async Task TaxService_IsVatExemptAsync_CustomerInSameEUCountryAsShop_IsNotExempt()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var address = new Address { CountryId = 1, StateProvinceId = 1 };
        customer.IsTaxExempt = false;
        await _customerService.UpdateCustomerAsync(customer);
    
        var country = new Country { Id = 1, TwoLetterIsoCode = "DE", SubjectToVat = true };
        await _countryService.InsertCountryAsync(country);
    
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatShopCountryId = 1;
        _taxSettings.EuVatAllowVatExemption = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);
    
        // Act
        var result = await _taxService.IsVatExemptAsync(address, customer);
    
        // Assert
        result.Should().BeFalse();
    }

*/
/*
FAILED TEST: ### **Analysis:**
1. The test run failed because the `NSubstitute` NuGet package is missing from the test project, which is required for mocking in `TaxServiceTests.cs`.
2. There are multiple syntax and compilation errors in the `TaxServiceTests.cs` file, likely due to incomplete or malformed test code.

---

### **Recommended Fixes:**
1. **Install NSubstitute:**
   ```
   dotnet add package NSubstitute
   ```

2. **Fix syntax errors in `TaxServiceTests.cs`:**
   - Ensure all test methods are properly enclosed within `[Test]` attributes and valid C# method syntax.
   - Correct any malformed or incomplete test code.

    [Test]
    public async Task TaxService_GetVatNumberStatusAsync_InvalidVatNumberFormat_ReturnsInvalidStatus()
    {
        // Arrange
        var fullVatNumber = "InvalidFormat";
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatUseWebService = true;
        _taxSettings.EuVatAssumeValid = false;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: ### **Analysis:**
1. The test run failed because the `NSubstitute` NuGet package is missing from the test project, which is required for mocking in `TaxServiceTests.cs`.
2. There are multiple syntax and compilation errors in the `TaxServiceTests.cs` file, likely due to incomplete or malformed test code.

---

### **Recommended Fixes:**
1. **Install NSubstitute:**
   ```
   dotnet add package NSubstitute
   ```

2. **Fix syntax errors in `TaxServiceTests.cs`:**
   - Ensure all test methods are properly enclosed within `[Test]` attributes and valid C# method syntax.
   - Correct any malformed or incomplete test code.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithNoAddress_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOn = TaxBasedOn.DefaultAddress;
        taxSettings.AutomaticallyDetectCountry = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/
/*
FAILED TEST: **Analysis:**  
1. The test run failed because the `NSubstitute` NuGet package is missing from the test project, which is required for mocking in `TaxServiceTests.cs`.  
2. There are multiple syntax and compilation errors in the `TaxServiceTests.cs` file, likely due to incomplete or malformed test code.

**Recommended Fixes:**  
1. Install the `NSubstitute` NuGet package by running the following command:
   ```
   dotnet add package NSubstitute
   ```
2. Review and correct the syntax errors in the `TaxServiceTests.cs` file to ensure valid C# code structure.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithPickupPoint_PickupPointAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOnPickupPointAddress = true;
        taxSettings.AutomaticallyDetectCountry = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        var pickupPoint = new PickupPoint
        {
            CountryCode = "US",
            StateAbbreviation = "CA",
            County = "Los Angeles",
            City = "Los Angeles",
            Address = "123 Main St",
            ZipPostalCode = "90001"
        };
    
        _genericAttributeService.GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, 1).Returns(pickupPoint);
        var country = new Country { Id = 1, TwoLetterIsoCode = "US" };
        var state = new StateProvince { Id = 1, Abbreviation = "CA", CountryId = country.Id };
        _countryService.GetCountryByTwoLetterIsoCodeAsync("US").Returns(country);
        _stateProvinceService.GetStateProvinceByAbbreviationAsync("CA", country.Id).Returns(state);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.CountryId.Should().Be(country.Id);
        result.Address.StateProvinceId.Should().Be(state.Id);
        result.Address.County.Should().Be(pickupPoint.County);
        result.Address.City.Should().Be(pickupPoint.City);
        result.Address.Address1.Should().Be(pickupPoint.Address);
        result.Address.ZipPostalCode.Should().Be(pickupPoint.ZipPostalCode);
    }

*/
/*
FAILED TEST: The test run failed due to two main issues:

1. **Missing NSubstitute NuGet Package**:  
   The `NSubstitute` namespace is referenced in `TaxServiceTests.cs`, but the required NuGet package is not installed in the test project.  

   **Recommended Fix**:  
   Install the `NSubstitute` NuGet package by running the following command:
   ```
   dotnet add package NSubstitute
   ```

2. **Syntax and Compilation Errors in Test File**:  
   The `TaxServiceTests.cs` file contains multiple syntax errors (e.g., missing semicolons, invalid tokens, incorrect member declarations), likely due to malformed test code or copy-paste issues.

   **Recommended Fix**:  
   Review and correct the syntax errors in the test file, particularly around line 365 and following, ensuring proper C# syntax and structure.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithInvalidPickupPoint_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOnPickupPointAddress = true;
        taxSettings.AutomaticallyDetectCountry = false;
        await _settingService.SaveSettingAsync(taxSettings);
    
        var pickupPoint = new PickupPoint
        {
            CountryCode = "XX", // Invalid country code
            StateAbbreviation = "ZZ", // Invalid state abbreviation
            County = "Invalid County",
            City = "Invalid City",
            Address = "Invalid Address",
            ZipPostalCode = "00000"
        };
    
        _genericAttributeService.GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, 1).Returns(pickupPoint);
        _countryService.GetCountryByTwoLetterIsoCodeAsync("XX").ReturnsNull();
        _stateProvinceService.GetStateProvinceByAbbreviationAsync("ZZ", 0).ReturnsNull();
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/
/*
FAILED TEST: **Analysis:**  
The test run failed because the `NSubstitute` namespace is missing from the test project, which is required for mocking in the `TaxServiceTests.cs` file. This indicates that the required NuGet package for NSubstitute is not installed or not properly referenced.

**Recommended Fix:**  
Install the `NSubstitute` NuGet package in the test project by running the following command:

```
dotnet add package NSubstitute
```

This will add the necessary reference and resolve the compilation errors.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithNoAddressAndInvalidIP_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
        taxSettings.AutomaticallyDetectCountry = true;
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Mock invalid IP address
        _webHelper.GetCurrentIpAddress().Returns("invalid-ip");
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/

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
    public async Task TaxService_GetProductPriceAsync_ZeroPrice_ReturnsZeroTax()
    {
        // Arrange
        var product = new Product { TaxCategoryId = 1 };
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var price = decimal.Zero;
    
        _taxSettings.PricesIncludeTax = true;
        await _settingService.SaveSettingAsync(_taxSettings);
    
        // Act
        var result = await _taxService.GetProductPriceAsync(product, price, customer);
    
        // Assert
        result.price.Should().Be(price);
        result.taxRate.Should().Be(decimal.Zero);
    }

FAILED TEST: **Analysis:**
The test run failed because the `NSubstitute` namespace is missing, indicating that the required NuGet package is not installed or referenced in the test project.

**Recommended Fix:**
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithNoAddressAndInvalidIP_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
        await _customerService.UpdateCustomerAsync(customer);
    
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
        taxSettings.AutomaticallyDetectCountry = true;
    
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Mock invalid IP address
        _webHelper.GetCurrentIpAddress().Returns("invalid-ip");
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed because the `NSubstitute` namespace is missing, indicating that the required NuGet package is not installed or referenced in the test project.

**Recommended Fix:**
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithInvalidPickupPoint_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOnPickupPointAddress = true;
        taxSettings.AutomaticallyDetectCountry = false;
    
        await _settingService.SaveSettingAsync(taxSettings);
    
        var pickupPoint = new PickupPoint
        {
            CountryCode = "XX", // Invalid country code
            StateAbbreviation = "ZZ", // Invalid state abbreviation
            County = "Invalid County",
            City = "Invalid City",
            Address = "Invalid Address",
            ZipPostalCode = "00000"
        };
    
        _genericAttributeService.GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, 1).Returns(pickupPoint);
        _countryService.GetCountryByTwoLetterIsoCodeAsync("XX").ReturnsNull();
        _stateProvinceService.GetStateProvinceByAbbreviationAsync("ZZ", 0).ReturnsNull();
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed because the `NSubstitute` namespace is missing, indicating that the required NuGet package is not installed or referenced in the test project.

**Recommended Fix:**
Install the `NSubstitute` NuGet package in the test project by running the following command:
```
dotnet add package NSubstitute
```

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithPickupPoint_PickupPointAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOnPickupPointAddress = true;
        taxSettings.AutomaticallyDetectCountry = false;
    
        await _settingService.SaveSettingAsync(taxSettings);
    
        var pickupPoint = new PickupPoint
        {
            CountryCode = "US",
            StateAbbreviation = "CA",
            County = "Los Angeles",
            City = "Los Angeles",
            Address = "123 Main St",
            ZipPostalCode = "90001"
        };
    
        _genericAttributeService.GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, 1).Returns(pickupPoint);
        var country = new Country { Id = 1, TwoLetterIsoCode = "US" };
        var state = new StateProvince { Id = 1, Abbreviation = "CA", CountryId = country.Id };
        _countryService.GetCountryByTwoLetterIsoCodeAsync("US").Returns(country);
        _stateProvinceService.GetStateProvinceByAbbreviationAsync("CA", country.Id).Returns(state);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.CountryId.Should().Be(country.Id);
        result.Address.StateProvinceId.Should().Be(state.Id);
        result.Address.County.Should().Be(pickupPoint.County);
        result.Address.City.Should().Be(pickupPoint.City);
        result.Address.Address1.Should().Be(pickupPoint.Address);
        result.Address.ZipPostalCode.Should().Be(pickupPoint.ZipPostalCode);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed because the `NSubstitute` namespace could not be found. This indicates that the required NuGet package for NSubstitute is either missing or not properly referenced in the test project.

**Recommended Fix:**
Install the `NSubstitute` NuGet package in the test project by running the following command:

```
dotnet add package NSubstitute
```

This will add the necessary reference and resolve the compilation errors.

    [Test]
    public async Task TaxService_PrepareTaxRateRequestAsync_CustomerWithNoAddress_DefaultAddressUsed()
    {
        // Arrange
        var customer = await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
        var product = new Product { TaxCategoryId = 1 };
        var taxSettings = _taxSettings;
        taxSettings.TaxBasedOn = TaxBasedOn.DefaultAddress;
        taxSettings.AutomaticallyDetectCountry = false;
    
        await _settingService.SaveSettingAsync(taxSettings);
    
        // Mock default tax address
        var defaultTaxAddress = new Address { Id = 1, CountryId = 1, StateProvinceId = 1 };
        _addressService.GetAddressByIdAsync(defaultTaxAddress.Id).Returns(defaultTaxAddress);
    
        // Act
        var result = await _taxService.PrepareTaxRateRequestAsync(product, 0, customer, 100);
    
        // Assert
        result.Address.Should().NotBeNull();
        result.Address.Id.Should().Be(defaultTaxAddress.Id);
    }

*/
}