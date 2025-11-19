using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using psi25_project.Services;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly RoomService _service;

        public RoomsController(RoomService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom()
        {
            var room = await _service.CreateRoomAsync();
            return Ok(new { roomCode = room.RoomCode });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest req)
        {
            var player = await _service.JoinRoomAsync(req.RoomCode, req.UserId, req.DisplayName);

            if (player == null)
                return NotFound("Room not found");

            return Ok(player);
        }
    }

    public class JoinRoomRequest
    {
        public string RoomCode { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; }
    }
}