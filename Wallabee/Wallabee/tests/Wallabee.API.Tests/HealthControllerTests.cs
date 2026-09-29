using Microsoft.AspNetCore.Mvc;
using Wallabee.API.Controllers;

namespace Wallabee.API.Tests;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyResponse()
    {
        var result = new HealthController().Get();

        Assert.IsType<OkObjectResult>(result);
    }
}