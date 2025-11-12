using psi25_project.Models.Dtos;
using psi25_project.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Services.Interfaces
{
    public interface IGuessService
    {
        Task<(GuessResponseDto guess, bool finished, int currentRound, int totalScore)> CreateGuessAsync(CreateGuessDto dto);
        Task<List<GuessResponseDto>> GetGuessesForGameAsync(Guid gameId);
    }
}
