using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Services;
using System;
using System.Threading.Tasks;
using psi25_project.Models.Dtos;

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
        public async Task<ActionResult<Room>> CreateRoom([FromBody] RoomCreateDto dto)
        {
            var room = await _roomService.CreateRoomAsync(dto.TotalRounds);
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

        // POST /api/Players/{playerId}/ready
        [HttpPost("/api/Players/{playerId}/ready")]
        public async Task<IActionResult> SetReady(Guid playerId)
        {
            var player = await _roomService.SetReadyAsync(playerId);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpPost("/api/Players/{playerId}/toggle-ready")]
        public async Task<IActionResult> ToggleReady(Guid playerId)
        {
            var player = await _roomService.ToggleReadyAsync(playerId);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpPost("/api/Players/{playerId}/leave-room")]
        public async Task<IActionResult> LeaveRoom(Guid playerId)
        {
            var result = await _roomService.LeaveRoomAsync(playerId);
            if (!result) return NotFound();
            return Ok();
        }

    }

    public class JoinRoomDto
    {
        public string RoomCode { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
