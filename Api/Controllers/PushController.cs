using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PushController : ControllerBase
    {
        private readonly ILogger<PushController> _logger;
        private readonly IPushNotificationService _notificationSender;

        public PushController(IPushNotificationService notificationSender, ILogger<PushController> logger)
        {
            _logger = logger;
            _notificationSender = notificationSender;
        }

        /// <summary>
        /// Send a push notification using Firebase (only for testing purposes)
        /// </summary>
        [HttpPost("notification")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> Notification()
        {
            try
            {
                var code1 = new Guid("36A60156-D0D7-420C-AB96-AC6AB686DEBC");
                var code2 = new Guid("36A60156-D0D7-420C-AB96-AC6AB686DEBC");

                return await _notificationSender.SendPush(code1, "this is a test", code2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when pushing notifications with FCM: {0}", ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        //[HttpPost("subscribe/{token}/{topic}")]
        //[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<bool>> Subscribe(string token, string topic)
        //{
        //    try
        //    {
        //        httpCli
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error subscribing to topic");
        //        return BadRequest(ex.InnermostMsg());
        //    }
        //}
    }
}
