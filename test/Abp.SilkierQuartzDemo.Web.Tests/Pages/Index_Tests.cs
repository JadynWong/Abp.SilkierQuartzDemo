using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Abp.SilkierQuartzDemo.Pages;

public class Index_Tests : SilkierQuartzDemoWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
