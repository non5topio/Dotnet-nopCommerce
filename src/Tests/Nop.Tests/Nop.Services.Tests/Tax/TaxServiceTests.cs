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
FAILED TEST: **Analysis:**
The test run failed due to two main issues:
1. A missing reference to the `Shouldly` library, which is used for assertions in the test file.
2. A syntax error in the file `TaxServiceTests.cs` at line 1914, column 6, where the compiler encountered the keyword `namespace` incorrectly placed or structured.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet package** in the test project (`Nop.Tests.csproj`) by running the command:
   ```
   dotnet add package Shouldly
   ```

2. **Fix the syntax error** in `TaxServiceTests.cs` by reviewing the code around line 1914. Ensure that the `namespace` keyword is used correctly and that all class and method definitions are properly structured with matching opening and closing braces (`{}`). Specifically, verify that the class and method definitions are correctly nested within the namespace block and that all braces are properly matched and placed.

    [Test]
    public async Task CanGetTaxRateWithMissingBillingAddress()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax settings to use billing address
        _taxSettings.TaxBasedOn = TaxBasedOn.BillingAddress;
        _taxSettings.AutomaticallyDetectCountry = true;
    
        // Act
        var (taxRate, isTaxable) = await _taxService.GetTaxRateAsync(product, 1, customer, 1000M);
    
        // Assert
        taxRate.Should().BeGreaterOrEqual(0M);
        isTaxable.Should().BeTrue();
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to two main issues:
1. A missing reference to the `Shouldly` library, which is used for assertions in the test file.
2. A syntax error in the file `TaxServiceTests.cs` at line 1914, column 6, where the compiler encountered the keyword `namespace` incorrectly placed or structured.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet package** in the test project (`Nop.Tests.csproj`) by running the command:
   ```
   dotnet add package Shouldly
   ```

2. **Fix the syntax error** in `TaxServiceTests.cs` by reviewing the code around line 1914. Ensure that the `namespace` keyword is used correctly and that all class and method definitions are properly structured with matching opening and closing braces (`{}`). Specifically, verify that the class and method definitions are correctly nested within the namespace block and that all braces are properly matched.

    [Test]
    public async Task CanGetTaxRateWithInvalidPickupPoint()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { TaxCategoryId = 1 };
    
        // Mock tax settings to use pickup point address
        _taxSettings.TaxBasedOnPickupPointAddress = true;
        _shippingSettings.AllowPickupInStore = true;
    
        // Act
        var (taxRate, isTaxable) = await _taxService.GetTaxRateAsync(product, 1, customer, 1000M);
    
        // Assert
        taxRate.Should().Be(0M);
        isTaxable.Should().BeTrue();
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to two main issues:
1. A missing reference to the `Shouldly` library, which is used for assertions in the test file.
2. A syntax error in the file `TaxServiceTests.cs` at line 1914, column 6, where the compiler encountered the keyword `namespace` incorrectly placed or structured.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet package** in the test project (`Nop.Tests.csproj`) by running the command:
   ```
   dotnet add package Shouldly
   ```

2. **Fix the syntax error** in `TaxServiceTests.cs` by reviewing the code around line 1914. Ensure that the `namespace` keyword is used correctly and that all class and method definitions are properly structured with matching opening and closing braces (`{}`).

    [Test]
    public async Task CanGetProductPriceWithTaxExemptProduct()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { TaxCategoryId = 1, IsTaxExempt = true };
    
        // Act
        var (price, taxRateResult) = await _taxService.GetProductPriceAsync(product, 1000M, true, customer);
    
        // Assert
        price.Should().Be(1000M);
        taxRateResult.Should().Be(0M);
    }

*/
/*
FAILED TEST: **Analysis:**  
The test run failed due to two main issues:
1. A missing reference to the `Shouldly` library, which is used for assertions in the test file.
2. A syntax error in the file `TaxServiceTests.cs` at line 1914, column 6, where the compiler encountered the keyword `namespace` incorrectly placed or structured.

**Recommended Fixes:**
1. **Install `Shouldly` NuGet package** in the test project (`Nop.Tests.csproj`) by running the command:
   ```
   dotnet add package Shouldly
   ```
2. **Fix the syntax error** in `TaxServiceTests.cs` by reviewing the code around line 1914. Ensure that the `namespace` keyword is used correctly and that all class and method definitions are properly structured with matching opening and closing braces (`{}`).

    [Test]
    public async Task CanGetProductPriceWithTaxExemptCustomerRole()
    {
        // Arrange
        var customer = new Customer();
        var taxExemptRole = new CustomerRole { Name = "TaxExemptRole", TaxExempt = true };
        customer.CustomerRoles.Add(taxExemptRole);
    
        var product = new Product { TaxCategoryId = 1 };
    
        // Act
        var (price, taxRateResult) = await _taxService.GetProductPriceAsync(product, 1000M, true, customer);
    
        // Assert
        price.Should().Be(1000M);
        taxRateResult.Should().Be(0M);
    }

*/
/*
FAILED TEST: **Analysis:**  
The test run failed due to a syntax error in the file `TaxServiceTests.cs` at line 1914, column 6. The error `CS1041: Identifier expected; 'namespace' is a keyword` indicates that the compiler encountered the keyword `namespace` where it was not expected, likely due to a misplaced or missing identifier, such as a class or method definition.

**Recommended Fix:**  
Review the code around line 1914 in `TaxServiceTests.cs` and ensure that all class and method definitions are correctly structured. Specifically, verify that the `namespace` keyword is used correctly and that all opening and closing braces (`{}`) are properly matched and placed.

    [Test]
    public async Task CanGetProductPriceWithNegativePrice()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { TaxCategoryId = 1 };
        var taxRate = 20M;
    
        // Mock tax provider to return tax rate
        var taxProvider = new Mock<ITaxProvider>();
        taxProvider.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = taxRate, Success = true });
    
        var taxPluginManager = new Mock<ITaxPluginManager>();
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
            .ReturnsAsync(taxProvider.Object);
    
        var taxService = new TaxService(
            addressSettings: new AddressSettings(),
            customerSettings: new CustomerSettings(),
            addressService: new Mock<IAddressService>().Object,
            checkVatService: new Mock<ICheckVatService>().Object,
            countryService: new Mock<ICountryService>().Object,
            customerService: new Mock<ICustomerService>().Object,
            eventPublisher: new Mock<IEventPublisher>().Object,
            genericAttributeService: new Mock<IGenericAttributeService>().Object,
            geoLookupService: new Mock<IGeoLookupService>().Object,
            logger: new Mock<ILogger>().Object,
            stateProvinceService: new Mock<IStateProvinceService>().Object,
            storeContext: new Mock<IStoreContext>().Object,
            taxPluginManager: taxPluginManager.Object,
            webHelper: new Mock<IWebHelper>().Object,
            workContext: new Mock<IWorkContext>().Object,
            shippingSettings: new ShippingSettings(),
            taxSettings: new TaxSettings()
        );
    
        // Act
        var (price, taxRateResult) = await taxService.GetProductPriceAsync(product, -100M, true, customer);
    
        // Assert
        price.Should().Be(-120M);
        taxRateResult.Should().Be(taxRate);
    }

*/
/*
FAILED TEST: The test run failed due to a missing reference to the `Shouldly` library, which is used in the test file for assertions.

**Failure Reason:**
- Error: `CS0246: The type or namespace name 'Shouldly' could not be found`
- This indicates that the `Shouldly` NuGet package is not installed or referenced in the test project.

**Recommended Fix:**
1. Install the `Shouldly` NuGet package in the test project (`Nop.Tests.csproj`) by running the following command:
   ```
   dotnet add package Shouldly
   ```
2. Rebuild and rerun the tests.

    [Test]
    public async Task CanGetProductPriceWithZeroTaxRate()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { TaxCategoryId = 0 };
        var taxRate = decimal.Zero;
    
        // Mock tax provider to return zero tax rate
        var taxProvider = new Mock<ITaxProvider>();
        taxProvider.Setup(x => x.GetTaxRateAsync(It.IsAny<TaxRateRequest>()))
            .ReturnsAsync(new TaxRateResult { TaxRate = taxRate, Success = true });
    
        var taxPluginManager = Mock.Get(_taxService._taxPluginManager);
        taxPluginManager.Setup(x => x.LoadPrimaryPluginAsync(It.IsAny<Customer>(), It.IsAny<int>()))
            .ReturnsAsync(taxProvider.Object);
    
        // Act
        var (price, taxRateResult) = await _taxService.GetProductPriceAsync(product, 1000M, true, customer);
    
        // Assert
        price.Should().Be(1000M);
        taxRateResult.Should().Be(taxRate);
    }

*/
}