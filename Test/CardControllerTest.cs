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
    public class CardControllerTest : SttBaseTest
    {
        [Fact]
        public async Task Post()
        {
            var service = new Mock<IGenericService>().Object;
            var logger = new Mock<ILogger<CardController>>().Object;
            var cardService = new Mock<ICardService>().Object;
            var mapper = new Mock<IMapper>().Object;

            var model = new PostCardModel();

            var controller = new CardController(service, logger, cardService, mapper);
            var result = await controller.PostCard(model);

            Assert.IsType<CreatedResult>(result);
        }
    }
}
