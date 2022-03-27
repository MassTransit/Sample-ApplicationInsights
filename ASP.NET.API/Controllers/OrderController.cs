using System.Threading.Tasks;
using MassTransit;
using Messaging.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IBus _bus;

    public OrderController(IBus bus)
    {
        _bus = bus;
    }

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