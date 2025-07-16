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
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxWithVatExemptionForEuCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            VatNumber = "DE123456789",
            VatNumberStatusId = (int)VatNumberStatus.Valid
        };
        var product = new Product();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxBasedOnDefaultAddress()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 1;
    
        // Ensure billing and shipping addresses are not set
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxWithNegativeTaxRate()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Public Modifier on Test Method**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure test methods are not marked as `public` unless necessary. In NUnit, test methods should typically be `public` only if they are to be discoverable by the test runner, but ensure they are correctly placed within the test class.

    [Test]
    public async Task CanCalculateTaxWithNoTaxPlugin()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCheckInvalidVatNumber()
    {
        // Arrange
        var invalidVatNumber = "InvalidVATNumber";
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(invalidVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: **Analysis Summary:**

1. **Missing Shouldly Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Update the using directives to correct namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`). Ensure the project has references to the correct assemblies for these namespaces.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxBasedOnAutoDetectedCountry()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 1;
    
        // Ensure billing and shipping addresses are not set
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**:  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Ensure correct namespace references (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`) or update the using directives if the namespaces have been moved or renamed.

3. **Duplicate Using Directives**:  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**:  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxBasedOnPickupPointAddress()
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
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint);
    
        var product = new Product();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` NuGet package using:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**:  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Ensure the correct namespaces are referenced (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`) or update the using directives if the namespaces have been moved or renamed.

3. **Duplicate Using Directives**:  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Method Modifier**:  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

    [Test]
    public async Task CanCalculateTaxWithTaxExemptProduct()
    {
        // Arrange
        var product = new Product { IsTaxExempt = true };
        var customer = new Customer();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
/*
FAILED TEST: **Analysis of Test Run Failure:**

1. **Missing `Shouldly` NuGet Package**  
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`  
   - **Fix**: Install the `Shouldly` package using:  
     ```bash
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**  
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`  
   - **Error**: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`  
   - **Fix**: Ensure the project has references to the correct assemblies for `Nop.Services.Address` and `Nop.Services.GenericAttributes`. If these namespaces have been moved or renamed, update the using directives accordingly.

3. **Duplicate Using Directives**  
   - **Error**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`  
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

4. **Invalid Modifier on Test Method**  
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`  
   - **Fix**: Ensure test methods are not incorrectly marked as `public`. In test classes, test methods should be marked with `[Test]` and not have the `public` modifier unless necessary.

---

**Recommended Fixes Summary:**

1. Install the `Shouldly` NuGet package.
2. Verify and correct references for `Nop.Services.Address` and `Nop.Services.GenericAttributes`.
3. Remove duplicate `using` directives.
4. Remove the `public` modifier from test methods where it is not valid.

    [Test]
    public async Task CanCalculateTaxWithTaxExemptCustomer()
    {
        // Arrange
        var customer = new Customer { IsTaxExempt = true };
        var product = new Product();
        var taxCategoryId = 1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
/*
FAILED TEST: **Analysis of Test Run Failure:**

1. **Missing `Shouldly` NuGet Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespace References**:
   - **Error**: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`
   - **Fix**: Ensure the project has references to the correct assemblies for `Nop.Services.Address` and `Nop.Services.GenericAttributes`. If these namespaces have been moved or renamed, update the using directives accordingly.

3. **Duplicate Using Directives**:
   - **Warning**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`
   - **Fix**: Remove duplicate `using` directives from the file.

4. **Invalid Method Modifier**:
   - **Error**: `CS0106: The modifier 'public' is not valid for this item`
   - **Fix**: Ensure that method declarations are correctly formatted and that the `public` modifier is only used where valid (e.g., not on test methods in a test class without proper setup).

---

**Recommended Fixes Summary:**

1. Install the `Shouldly` NuGet package.
2. Fix or remove missing/incorrect using directives for `Nop.Services.Address` and `Nop.Services.GenericAttributes`.
3. Remove duplicate `using` directives.
4. Correct the invalid method modifier in the test file.

    [Test]
    public async Task CanCalculateTaxWithInvalidTaxCategoryId()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = -1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
        var result = await _taxService.GetVatNumberStatusAsync(vatNumber);

        result.vatNumberStatus.Should().Be(canBeStatus);
    }
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - **Warning**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

3. **Missing Namespace References**:
   - **Fix**: Ensure the project has references to the correct assemblies for `Nop.Services.Address` and `Nop.Services.GenericAttributes`. If these namespaces have been moved or renamed, update the using directives accordingly.

    [Test]
    public async Task CanCalculateTaxWithExcludingTaxDisplayType()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 0;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, false, customer, false);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - **Warning**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

3. **General Recommendations**:
   - Ensure all missing namespaces (e.g., `Nop.Services.Address`, `Nop.Services.GenericAttributes`) are either referenced correctly or removed if no longer used.
   - Clean up the file by removing unused or duplicate `using` directives for clarity and maintainability.

    [Test]
    public async Task CanCalculateTaxWithEuVatExemption()
    {
        // Arrange
        var customer = new Customer
        {
            VatNumber = "DE123456789",
            VatNumberStatusId = (int)VatNumberStatus.Valid
        };
        var product = new Product();
        var taxCategoryId = 0;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0);
        price.Should().Be(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - **Warning**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

3. **Missing Namespace References**:
   - **Error**: Missing references for `Nop.Services.Address` and `Nop.Services.GenericAttributes`
   - **Fix**: Ensure the project has references to the correct assemblies for these namespaces. If the namespaces have been moved or renamed, update the using directives accordingly.

    [Test]
    public async Task CanCalculateTaxWithInvalidTaxCategoryId()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = -1;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - **Warning**: `CS0105: The using directive for 'NUnit.Framework' appeared previously in this namespace`
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

3. **Missing Namespace References**:
   - **Error**: Missing references for `Nop.Services.Address` and `Nop.Services.GenericAttributes`
   - **Fix**: Ensure the project has references to the correct assemblies for these namespaces. If the namespaces have been moved or renamed, update the using directives accordingly.

    [Test]
    public async Task CanCalculateTaxBasedOnDefaultAddress()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 0;
    
        // Ensure billing and shipping addresses are not set
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Shouldly Package**:
   - **Error**: `CS0246: The type or namespace name 'Shouldly' could not be found`
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - **Warnings**: Multiple `using` directives are duplicated (e.g., `Nop.Core.Domain.Catalog`, `Nop.Core.Domain.Customers`, `NUnit.Framework`).
   - **Fix**: Remove the duplicate `using` directives from `TaxServiceTests.cs`.

3. **Missing Namespace References**:
   - **Error**: `Nop.Services.GenericAttributes` and `Nop.Services.Address` are referenced but not found.
   - **Fix**: Ensure the project has references to the correct assemblies for these namespaces. If the namespaces have been moved or renamed, update the using directives accordingly.

    [Test]
    public async Task CanCalculateTaxWithAutoDetectedCountry()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var taxCategoryId = 0;
    
        // Mock IP address and country lookup
        var ipAddress = "192.0.2.1"; // Test IP address
        var countryIsoCode = "US";
        var country = new Country { Id = 1, Name = "United States", TwoLetterIsoCode = countryIsoCode, SubjectToVat = true };
    
        _webHelper.Setup(x => x.GetCurrentIpAddress()).Returns(ipAddress);
        _geoLookupService.Setup(x => x.LookupCountryIsoCode(ipAddress)).Returns(countryIsoCode);
        _countryService.Setup(x => x.GetCountryByTwoLetterIsoCodeAsync(countryIsoCode)).ReturnsAsync(country);
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: # Test Run Failure Analysis

**Reason for Failure:**
1. **Missing Shouldly Package**: The `Shouldly` assertion library is referenced but not installed, causing a compilation error.
2. **Missing Namespace**: The `Nop.Services.GenericAttributes` namespace is missing, causing a compilation error.
3. **Duplicate Using Directives**: Multiple duplicate `using` directives are present, causing compiler warnings.

**Recommended Fixes:**
1. **Install Shouldly**: Install the `Shouldly` NuGet package using: `dotnet add package Shouldly`
2. **Fix Missing Namespace**: Ensure the project has a reference to the correct assembly for `Nop.Services.GenericAttributes` or update the using directive if the namespace has been moved/renamed.
3. **Remove Duplicates**: Remove the duplicate `using` directives from `TaxServiceTests.cs` to eliminate compiler warnings.

    [Test]
    public async Task CanCalculateTaxBasedOnPickupPointAddress()
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
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint);
    
        var product = new Product();
        var taxCategoryId = 0;
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, taxCategoryId, 100, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0);
        price.Should().BeGreaterOrEqual(100);
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Assembly Reference**:
   - The `Shouldly` namespace is referenced but the corresponding NuGet package is not installed.
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Duplicate Using Directives**:
   - Duplicate `using` directives for `NUnit.Framework` are present, causing compiler warnings.
   - **Fix**: Remove the duplicate `using NUnit.Framework;` directive from the file.

3. **Missing Namespace References**:
   - The namespaces `Nop.Services.Address` and `Nop.Services.GenericAttributes` are referenced but do not exist or are not properly referenced.
   - **Fix**: Ensure the project has references to the correct assemblies for these namespaces. If these namespaces have been moved or renamed, update the using directives accordingly.

    [Test]
    public async Task CanCheckInvalidVatNumber()
    {
        // Arrange
        var invalidVatNumber = "InvalidVATNumber";
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(invalidVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed due to the following issues:

1. **Missing Assembly Reference**:
   - The `Shouldly` assertion library is referenced but not installed.
   - **Fix**: Install the `Shouldly` NuGet package using the command:  
     ```
     dotnet add package Shouldly
     ```

2. **Missing Namespaces**:
   - `Nop.Services.Address` and `Nop.Services.GenericAttributes` are referenced but not found.
   - **Fix**: Ensure the project has references to the correct assemblies for these namespaces. If the namespaces have been moved or renamed, update the using directives accordingly.

3. **Duplicate Using Directives**:
   - Duplicate `using` directives are present, causing compiler warnings.
   - **Fix**: Remove duplicate `using` directives from the file for cleaner and error-free compilation.

    [Test]
    public async Task CanGetProductPriceForCustomerWithTaxExemptRole()
    {
        // Arrange
        var customer = new Customer();
        var taxExemptRole = new CustomerRole { Name = "TaxExemptRole", TaxExempt = true };
        customer.CustomerRoles.Add(taxExemptRole);
        var product = new Product();
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 100, 0, true, customer, true);
    
        // Assert
        price.Should().Be(100);
        taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: ### **Analysis of Test Run Failure**

The test run failed due to the following issues:

1. **Missing Assembly References**:
   - `Nop.Services.Address`: The namespace `Nop.Services.Address` is missing, causing a compilation error.
   - `Nop.Services.GenericAttributes`: The namespace `Nop.Services.GenericAttributes` is missing, causing a compilation error.

2. **Missing NuGet Package**:
   - `Shouldly`: The `Shouldly` assertion library is referenced but not installed, causing a compilation error.

3. **Duplicate Using Directives**:
   - Several `using` directives are duplicated, causing compiler warnings. While not fatal, they should be cleaned up for clarity and maintainability.

---

### **Recommended Fixes**

1. **Add Missing References**:
   - Ensure the project has a reference to the correct assemblies for `Nop.Services.Address` and `Nop.Services.GenericAttributes`. If these namespaces have been moved or renamed, update the using directives accordingly.

2. **Install `Shouldly`**:
   - Install the `Shouldly` NuGet package using the following command:
     ```bash
     dotnet add package Shouldly
     ```

3. **Remove Duplicate Using Directives**:
   - Remove or consolidate duplicate `using` directives in the test file to eliminate compiler warnings.

    [Test]
    public async Task CanGetProductPriceForTaxExemptProduct()
    {
        // Arrange
        var product = new Product { IsTaxExempt = true };
        var customer = new Customer();
        var taxService = new TaxService(
            addressSettings: new AddressSettings(),
            customerSettings: new CustomerSettings(),
            addressService: new AddressService(new Nop.Data.NopObjectContext()),
            checkVatService: new CheckVatService(),
            countryService: new CountryService(new Nop.Data.NopObjectContext()),
            customerService: new CustomerService(new Nop.Data.NopObjectContext()),
            eventPublisher: new EventPublisher(),
            genericAttributeService: new GenericAttributeService(new Nop.Data.NopObjectContext()),
            geoLookupService: new GeoLookupService(),
            logger: new NullLogger(),
            stateProvinceService: new StateProvinceService(new Nop.Data.NopObjectContext()),
            storeContext: new StoreContext(new Nop.Data.NopObjectContext()),
            taxPluginManager: new TaxPluginManager(new PluginFinder()),
            webHelper: new WebHelper(),
            workContext: new WorkContext(new CustomerService(new Nop.Data.NopObjectContext())),
            shippingSettings: new ShippingSettings(),
            taxSettings: new TaxSettings()
        );
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 100, 0, true, customer, true);
    
        // Assert
        price.Should().Be(100);
        taxRate.Should().Be(0);
    }

*/
/*
FAILED TEST: ### **Analysis of Test Run Failure**

The test run failed due to the following key issues:

1. **Missing Assembly Reference or Namespace**:
   - `Nop.Services.Address` is missing (error: `The type or namespace name 'Address' does not exist in the namespace 'Nop.Services'`).
   - `Nop.Services.GenericAttributes` is missing (error: `The type or namespace name 'GenericAttributes' does not exist in the namespace 'Nop.Services'`).

2. **Missing Assembly Reference for Shouldly**:
   - `Shouldly` is referenced but not available (error: `The type or namespace name 'Shouldly' could not be found`).

---

### **Recommended Fixes**

1. **Add Missing Using Directives or References**:
   - Ensure the project has references to the correct assemblies for `Nop.Services.Address` and `Nop.Services.GenericAttributes`. If these are moved or renamed, update the using directives accordingly.

2. **Install or Reference Shouldly**:
   - Install the `Shouldly` NuGet package in the test project:
     ```bash
     dotnet add package Shouldly
     ```
   - Or ensure it is already referenced in the project file (`Nop.Tests.csproj`).

3. **Fix Duplicate Using Directives**:
   - Remove or consolidate duplicate `using` directives (e.g., `Nop.Core.Domain.Catalog`, `Nop.Core.Domain.Customers`, etc.) to eliminate compiler warnings.

    [Test]
    public async Task CanGetProductPriceWithZeroPrice()
    {
        // Arrange
        var product = new Product();
        var customer = new Customer();
        var taxService = new TaxService(
            addressSettings: new AddressSettings(),
            customerSettings: new CustomerSettings(),
            addressService: new AddressService(new Nop.Core.Data.IRepository<Address>(new Nop.Core.Data.NullRepository<Address>())),
            checkVatService: new CheckVatService(),
            countryService: new CountryService(new Nop.Core.Data.IRepository<Country>(new Nop.Core.Data.NullRepository<Country>())),
            customerService: new CustomerService(new Nop.Core.Data.IRepository<Customer>(new Nop.Core.Data.NullRepository<Customer>())),
            eventPublisher: new EventPublisher(),
            genericAttributeService: new GenericAttributeService(new Nop.Core.Data.IRepository<GenericAttribute>(new Nop.Core.Data.NullRepository<GenericAttribute>())),
            geoLookupService: new GeoLookupService(),
            logger: new NullLogger(),
            stateProvinceService: new StateProvinceService(new Nop.Core.Data.IRepository<StateProvince>(new Nop.Core.Data.NullRepository<StateProvince>())),
            storeContext: new StoreContext(new Nop.Core.Data.IRepository<Store>(new Nop.Core.Data.NullRepository<Store>())),
            taxPluginManager: new TaxPluginManager(new Nop.Services.Plugins.IPluginFinder[] { new PluginFinder() }),
            webHelper: new WebHelper(),
            workContext: new WorkContext(new Nop.Core.Data.IRepository<Customer>(new Nop.Core.Data.NullRepository<Customer>())),
            shippingSettings: new ShippingSettings(),
            taxSettings: new TaxSettings()
        );
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 0, 0, true, customer, true);
    
        // Assert
        price.Should().Be(0);
        taxRate.Should().Be(0);
    }

*/
}