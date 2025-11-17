using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Payloads;
using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class GuessServiceTests
{
    private readonly Mock<IGuessRepository> _mockGuessRepository;
    private readonly GuessService _service;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<ILocationRepository> _mockLocationRepository;

    public GuessServiceTests()
    {
        _mockGuessRepository = new Mock<IGuessRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockLocationRepository = new Mock<ILocationRepository>();
        _service = new GuessService(_mockGuessRepository.Object, _mockGameRepository.Object, _mockLocationRepository.Object);
    }

    [Fact]
    public async Task CreateGuessAsync_MidGame_CreatesGuess_UpdatesGame_AndReturnsExpectedTuple()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            User = null!,
            FinishedAt = null,
            CurrentRound = 1,
            TotalRounds = 3,
            TotalScore = 0
        };

        var location = new Location
        {
            Id = dto.LocationId,
            Latitude = 50,
            Longitude = 60
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(dto.LocationId))
            .ReturnsAsync(location);

        Guess? capturedGuess = null;

        _mockGuessRepository.Setup(r => r.AddAsync(It.IsAny<Guess>()))
            .Callback<Guess>(g => capturedGuess = g)
            .Returns(Task.CompletedTask);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        // Act
        var (guessDto, finished, currentRound, totalScore) = await _service.CreateGuessAsync(dto);

        // Assert – guess entity
        Assert.NotNull(capturedGuess);
        Assert.Equal(game.Id, capturedGuess!.GameId);
        Assert.Equal(location.Id, capturedGuess.LocationId);
        Assert.Equal(dto.GuessedLatitude, capturedGuess.GuessedLatitude);
        Assert.Equal(dto.GuessedLongitude, capturedGuess.GuessedLongitude);
        Assert.Equal(dto.DistanceKm, capturedGuess.DistanceKm);
        Assert.Equal(dto.Score, capturedGuess.Score);

        // Assert – game updates
        Assert.Equal(2, game.CurrentRound);      // 1 -> 2
        Assert.Equal(100, game.TotalScore);      // 0 + 100
        Assert.Null(game.FinishedAt);

        // Assert – tuple return
        Assert.False(finished);
        Assert.Equal(game.CurrentRound, currentRound);
        Assert.Equal(game.TotalScore, totalScore);

        // Assert – mapping via MapToDto
        Assert.Equal(capturedGuess.Id, guessDto.Id);
        Assert.Equal(capturedGuess.GameId, guessDto.GameId);
        Assert.Equal(capturedGuess.LocationId, guessDto.LocationId);
        Assert.Equal(location.Latitude, guessDto.ActualLatitude);
        Assert.Equal(location.Longitude, guessDto.ActualLongitude);

        _mockGuessRepository.Verify(r => r.AddAsync(It.IsAny<Guess>()), Times.Once);
        _mockGameRepository.Verify(r => r.UpdateAsync(game), Times.Once);
    }

    
}
