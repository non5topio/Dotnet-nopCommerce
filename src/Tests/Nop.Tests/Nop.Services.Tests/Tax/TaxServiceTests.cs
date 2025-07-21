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

}