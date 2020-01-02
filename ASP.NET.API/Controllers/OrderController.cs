namespace ASP.NET.API.Controllers
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using Messaging.Contracts;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        public OrderController(IBus bus)
        {
            _bus = bus;
        }

        private readonly IBus _bus;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> SubmitOrder()
        {
            await _bus.Publish<SubmitOrder>(new
                                            {
                                                OrderId = InVar.Id
                                            },
                                            x => x.ResponseAddress = _bus.Address);
            return Ok();
        }
    }
}