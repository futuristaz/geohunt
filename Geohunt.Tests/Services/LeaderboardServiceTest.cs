using Moq;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class LeaderboardServiceTest
{
    [Fact]
    public async Task GetTopPlayersAsync_ReturnRepositoryData()
    {
        var mockRepo = new Mock<ILeaderboardRepository>();

        var expectedList = new List<LeaderboardEntry>
        {
            new LeaderboardEntry { Username = "Joe", TotalScore = 1000},
            new LeaderboardEntry { Username = "Doe", TotalScore = 3000}
        };

        mockRepo.Setup(r => r.GetTopPlayersAsync(20))
                .ReturnsAsync(expectedList);
        
        var service = new LeaderboardService(mockRepo.Object);

        var result = await service.GetTopPlayersAsync();

        Assert.Equal(expectedList, result);
        mockRepo.Verify(r => r.GetTopPlayersAsync(20), Times.Once);

    }

    [Fact]
    public async Task GetTopLeaderboardAsync_ReturnsRepositoryData()
    {
        var mockRepo = new Mock<ILeaderboardRepository>();

        var expected = new List<LeaderboardEntry>
        {
            new LeaderboardEntry { Username = "Test", DistanceKm = 15 }
        };

        mockRepo.Setup(r => r.GetTopGuessesAsync(10))
                .ReturnsAsync(expected);

        var service = new LeaderboardService(mockRepo.Object);

        var result = await service.GetTopLeaderboardAsync(10);

        Assert.Equal(expected, result);
        mockRepo.Verify(r => r.GetTopGuessesAsync(10), Times.Once);
    }
}