using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Configuration;

namespace Api.Test.Modules.Configuration
{
    public sealed class ApplicationConfigurationControllerTest
    {
        [Fact]
        public void Controller_Should_Expose_Configuration_History_As_Read_Only()
        {
            var publicActions = typeof(ApplicationConfigurationController)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.Contains(publicActions, method => method.GetCustomAttribute<HttpGetAttribute>() is not null);
            Assert.DoesNotContain(publicActions, method => method.GetCustomAttribute<HttpPostAttribute>() is not null);
            Assert.DoesNotContain(publicActions, method => method.GetCustomAttribute<HttpPutAttribute>() is not null);
            Assert.DoesNotContain(publicActions, method => method.GetCustomAttribute<HttpDeleteAttribute>() is not null);
        }
    }
}
