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
FAILED TEST: **Analysis of Test Failure:**

1. **Missing `Shouldly` NuGet Package:**
   - The test file references the `Shouldly` assertion library, but it is not installed in the test project.
   - This causes the compiler error: `CS0246: The type or namespace name 'Shouldly' could not be found`.

2. **Incorrect or Duplicate Using Directives:**
   - There are multiple warnings about duplicate `using` directives (e.g., `NUnit.Framework`, `Nop.Core.Domain.Catalog`).
   - These are not causing the test to fail but indicate a cleanup opportunity.

3. **Namespace Mismatch:**
   - The test file references a non-existent namespace `Nop.Services.Tests`, which is not part of the solution.
   - This may be due to incorrect project or namespace configuration.

---

**Recommended Fixes:**

1. **Install the `Shouldly` NuGet Package:**
   - Run the following command in the terminal or Package Manager Console:
     ```
     dotnet add package Shouldly
     ```

2. **Remove or Correct Duplicate Using Directives:**
   - Remove duplicate `using` statements from the test file to eliminate warnings.

3. **Correct Namespace References:**
   - Ensure that the namespace in the test file (`Nop.Tests.Nop.Services.Tests.Tax`) matches the actual project structure.
   - Remove or correct any incorrect references to `Nop.Services.Tests`.

4. **Ensure Project References Are Correct:**
   - Confirm that the test project has a reference to the `Nop.Services` project or its compiled DLL.

    [Test]
    public async Task CanGetProductPriceWithEuVatExemption()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
    
        // Enable EU VAT
        _taxSettings.EuVatEnabled = true;
        _taxSettings.EuVatAllowVatExemption = true;
        _taxSettings.EuVatShopCountryId = 1;
    
        // Set a valid VAT number for the customer
        customer.VatNumber = "DE123456789";
        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
    
        // Create an address in a different EU country
        var address = new Address
        {
            CountryId = 2,
            CreatedOnUtc = DateTime.UtcNow
        };
    
        // Mock the country service to return a country that is subject to VAT
        var country = new Country { Id = 2, SubjectToVat = true };
        _countryService = new CountryServiceMock(country);
    
        // Re-inject the services into the tax service
        _taxService = new TaxService(
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
            _taxPluginManager,
            _webHelper,
            _workContext,
            _shippingSettings,
            _taxSettings
        );
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0M);
        price.Should().Be(1000M);
    }

*/
/*
FAILED TEST: ### **Analysis of Test Run Failure**

The test run failed due to the following key issues:

1. **Missing `Nop.Services.Tests` Namespace**:
   - The test file references a non-existent namespace `Nop.Services.Tests`, which is not part of the solution.
   - This leads to multiple `CS0234` errors indicating that the compiler cannot find the `Tests` namespace within `Nop.Services`.

2. **Missing `Shouldly` NuGet Package**:
   - The test file uses `Shouldly` assertions (e.g., `Should().Be(...)`), but the package is not installed.
   - This results in a `CS0246` error indicating that the type or namespace `Shouldly` could not be found.

---

### **Recommended Fixes**

1. **Remove or Correct the `Nop.Services.Tests` References**:
   - Either remove the incorrect `using Nop.Services.Tests;` directives or correct them to reference valid namespaces.
   - Ensure the project structure and namespace declarations are consistent.

2. **Install the `Shouldly` NuGet Package**:
   - Run the following command in the test project directory:
     ```bash
     dotnet add package Shouldly
     ```
   - Or via the NuGet Package Manager:
     - Search for `Shouldly` and install it into the test project.

    [Test]
    public async Task CanGetProductPriceWithAutoDetectedCountry()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
    
        // Simulate no billing or shipping address
        customer.BillingAddressId = null;
        customer.ShippingAddressId = null;
    
        // Enable automatic country detection
        _taxSettings.AutomaticallyDetectCountry = true;
    
        // Simulate a valid IP address lookup
        var ipAddress = "8.8.8.8";
        var countryIsoCode = "US";
        var country = new Country { TwoLetterIsoCode = countryIsoCode, SubjectToVat = true };
    
        // Mock the web helper and geo lookup service
        var webHelperMock = new Mock<IWebHelper>();
        webHelperMock.Setup(x => x.GetCurrentIpAddress()).Returns(ipAddress);
    
        var geoLookupServiceMock = new Mock<IGeoLookupService>();
        geoLookupServiceMock.Setup(x => x.LookupCountryIsoCode(ipAddress)).Returns(countryIsoCode);
    
        var countryServiceMock = new Mock<ICountryService>();
        countryServiceMock.Setup(x => x.GetCountryByTwoLetterIsoCodeAsync(countryIsoCode)).ReturnsAsync(country);
    
        // Re-inject the services into the tax service
        _taxService = new TaxService(
            _addressSettings,
            _customerSettings,
            _addressService.Object,
            _checkVatService.Object,
            countryServiceMock.Object,
            _customerService.Object,
            _eventPublisher,
            _genericAttributeService.Object,
            geoLookupServiceMock.Object,
            _logger.Object,
            _stateProvinceService.Object,
            _storeContext.Object,
            _taxPluginManager.Object,
            webHelperMock.Object,
            _workContext.Object,
            _shippingSettings,
            _taxSettings
        );
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0M);
        price.Should().BeGreaterThanOrEqualTo(0M);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to missing dependencies and incorrect project structure. Specifically:
1. The `Shouldly` NuGet package is missing, which is required for the `Should().Be()` assertions.
2. There is a reference to a non-existent namespace `Nop.Services.Tests`, indicating a possible incorrect project or namespace setup.

**Recommended Fixes:**
1. Install the `Shouldly` NuGet package:
   ```
   dotnet add package Shouldly
   ```
2. Correct the namespace or project reference for `Nop.Services.Tests`. Ensure the project structure and namespaces align correctly.

    [Test]
    public async Task CanGetProductPriceWithPickupPointAddress()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
        var pickupPoint = new PickupPoint
        {
            CountryCode = "US",
            StateAbbreviation = "CA",
            County = "Los Angeles",
            City = "Los Angeles",
            Address = "123 Main St",
            ZipPostalCode = "90001"
        };
    
        // Set the pickup point for the customer
        var store = new Store { Id = 1 };
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, store.Id);
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0M);
        price.Should().BeGreaterThanOrEqualTo(0M);
    }

*/
/*
FAILED TEST: **Analysis:**
The test run failed due to a missing dependency. The `Shouldly` NuGet package is not installed in the test project, which is required for the `Should().Be()` and similar assertion methods used in the test file.

**Recommended Fix:**
Install the `Shouldly` NuGet package in the test project using the following command:
```
dotnet add package Shouldly
```

    [Test]
    public async Task CanCheckVatNumberWithInvalidCountryCode()
    {
        // Arrange
        var fullVatNumber = "XX123456789";
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` package is missing from the test project. This is indicated by the error:

```
CS0246: The type or namespace name 'Shouldly' could not be found (are you missing a using directive or an assembly reference?)
```

**Recommended Fix:**
Install the `Shouldly` NuGet package in the test project using the command:

```
dotnet add package Shouldly
```

    [Test]
    public async Task CanCheckVatNumberWithInvalidFormat()
    {
        // Arrange
        var fullVatNumber = "123456789";
    
        // Act
        var result = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
    
        // Assert
        result.vatNumberStatus.Should().Be(VatNumberStatus.Invalid);
        result.name.Should().BeEmpty();
        result.address.Should().BeEmpty();
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` package is missing from the test project. This is indicated by the error:

```
error CS0246: The type or namespace name 'Shouldly' could not be found (are you missing a using directive or an assembly reference?)
```

**Recommended Fix:**
Install the `Shouldly` NuGet package in the test project using the following command:

```
dotnet add package Shouldly
```

    [Test]
    public async Task CanGetProductPriceWithInvalidPickupPoint()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0M);
        price.Should().BeGreaterThanOrEqualTo(0M);
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` package is missing from the test project. This is indicated by the error:

```
CS0246: The type or namespace name 'Shouldly' could not be found (are you missing a using directive or an assembly reference?)
```

**Recommended Fix:**
- Install the `Shouldly` NuGet package in the test project using the command:
  ```
  dotnet add package Shouldly
  ```

    [Test]
    public async Task CanGetProductPriceWithTaxExemptProduct()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product { IsTaxExempt = true };
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
            taxPluginManager: new TaxPluginManager(new Nop.Core.Plugins.IPluginFinder[] { new PluginFinder() }),
            webHelper: new WebHelper(),
            workContext: new WorkContext(new Nop.Core.Data.IRepository<Customer>(new Nop.Core.Data.NullRepository<Customer>())),
            shippingSettings: new ShippingSettings(),
            taxSettings: new TaxSettings()
        );
    
        // Act
        var (price, taxRate) = await taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0M);
        price.Should().Be(1000M);
    }

*/
/*
FAILED TEST: ### **Test Run Failure Analysis:**

The test run failed because the `Shouldly` library, used for assertions in the test file `TaxServiceTests.cs`, is missing from the project.

### **Failure Reason:**
- The compiler cannot find the `Shouldly` namespace, indicating that the `Shouldly` NuGet package is not installed or referenced.

### **Recommended Fix:**
Install the `Shouldly` NuGet package in the test project using the following command:

```bash
dotnet add package Shouldly
```

    [Test]
    public async Task CanGetProductPriceWithTaxExemptCustomer()
    {
        // Arrange
        var customer = new Customer { IsTaxExempt = true };
        var product = new Product();
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0M);
        price.Should().Be(1000M);
    }

*/
/*
FAILED TEST: The test run failed because the `Shouldly` library, used for assertions in the test file `TaxServiceTests.cs`, is missing from the project.

### **Failure Reason:**
- The compiler cannot find the `Shouldly` namespace, indicating that the `Shouldly` NuGet package is not installed or referenced.

### **Recommended Fix:**
Install the `Shouldly` package using the following command:

```bash
dotnet add package Shouldly
```

Or manually add the package reference to the `.csproj` file:

```xml
<PackageReference Include="Shouldly" Version="x.x.x" />
```

Replace `x.x.x` with the appropriate version number.

    [Test]
    public async Task CanGetProductPriceWithNegativePrice()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 1, -100M, true, customer, false);
    
        // Assert
        taxRate.Should().BeGreaterThanOrEqualTo(0M);
        price.Should().BeLessThan(0M);
    }

*/
/*
FAILED TEST: The test run failed due to a missing reference to the `Shouldly` library, which is used in the test file for assertions.

### **Failure Reason:**
- **Error:** `CS0246: The type or namespace name 'Shouldly' could not be found`
- This indicates that the `Shouldly` NuGet package is not installed or referenced in the test project.

### **Recommended Fix:**
Install the `Shouldly` NuGet package in the test project using the following command:

```bash
dotnet add package Shouldly
```

Or manually add the package reference to the `.csproj` file:

```xml
<PackageReference Include="Shouldly" Version="x.x.x" />
```

Replace `x.x.x` with the appropriate version number.

    [Test]
    public async Task CanGetProductPriceWithZeroTaxRate()
    {
        // Arrange
        var customer = new Customer();
        var product = new Product();
    
        // Act
        var (price, taxRate) = await _taxService.GetProductPriceAsync(product, 0, 1000M, true, customer, true);
    
        // Assert
        taxRate.Should().Be(0M);
        price.Should().Be(1000M);
    }

*/
}