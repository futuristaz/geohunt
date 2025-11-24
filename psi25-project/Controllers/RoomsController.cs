using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Services;
using System;
using System.Threading.Tasks;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly RoomService _roomService;

        public RoomsController(RoomService roomService)
        {
            _roomService = roomService;
        }

        // Create a new room
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom()
        {
            var room = await _roomService.CreateRoomAsync();
            return Ok(room);
        }

        // Join room → user becomes player
        [HttpPost("join")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomDto dto)
        {
            var player = await _roomService.JoinRoomAsync(dto.RoomCode, dto.UserId, dto.DisplayName);
            if (player == null)
                return NotFound(new { message = "Room not found" });

            return Ok(player);
        }

        // Get players in a room
        [HttpGet("{roomCode}/players")]
        public async Task<IActionResult> GetPlayers(string roomCode)
        {
            var players = await _roomService.GetPlayersInRoomAsync(roomCode);
            return Ok(players);
        }
    }

    public class JoinRoomDto
    {
        public string RoomCode { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
