using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("timer")]
public class TimerController : ControllerBase
{
    private readonly TimerService _timer;

    public TimerController(TimerService timer)
    {
        _timer = timer;
    }

    [HttpGet("start")]
    public IActionResult Start() => Ok(_timer.Start());

    [HttpGet("status")]
    public IActionResult Status() => Ok(_timer.Status());

    [HttpGet("reset")]
    public IActionResult Reset() => Ok(_timer.Reset());
}
