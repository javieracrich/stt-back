using Api.Controllers;
using AutoMapper;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class DeviceControllerTest : SttBaseTest
    {
        [Fact]
        public async Task Post()
        {
            var service = new Mock<IGenericService>().Object;
            var logger = new Mock<ILogger<DeviceController>>().Object;
            var mapper = new Mock<IMapper>().Object;
            var principalProvider = new Mock<IPrincipalProvider>().Object;

            var controller = new DeviceController(mapper, principalProvider, service, logger);
            var model = new PostDeviceModel();

            var result = await controller.PostDevice(model);

            Assert.IsType<ActionResult<PostDeviceModel>>(result);
        }
    }
}
