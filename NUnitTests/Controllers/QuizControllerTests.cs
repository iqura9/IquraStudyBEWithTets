using FluentAssertions;
using IquraStudyBE.Context;
using IquraStudyBE.Controllers;
using IquraStudyBE.Models;
using Moq;
using IquraStudyBE.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit;

namespace NUnitTests
{
    [TestFixture]
    [Category("QuizControllerTests")]
    public class QuizControllerTests
    {
        private MyDbContext _context;
        private QuizController _controller;
        private Mock<ITokenService> _tokenServiceMock;
        [SetUp]
        public void SetUp()
        {
            SeedDatabase seed = new SeedDatabase();
            _context = seed._context;
            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock.Setup(ts => ts.GetUserIdFromToken()).Returns("2");
            
            _controller = new QuizController(_context, _tokenServiceMock.Object);
        }
        
        [Test]
        public async Task GetQuizzes_ReturnsListOfQuizzes()
        {
            // Arrange
            var user = new User
            {
                Id = "2",
                UserName = "exampleUser",
                Email = "user@example.com",
                Image = "profile.jpg",
                Description = "This is a sample user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RefreshToken = "refreshTokenValue",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            
            var fakeUser = new User
            {
                Id = "3",
                UserName = "exampleUser2",
                Email = "user@example.com",
                Image = "profile.jpg",
                Description = "This is a sample user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RefreshToken = "refreshTokenValue",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            var expectedQuizzes = new List<Quiz>
            {
                new Quiz { Id = 1, Title = "Quiz 1", CreatedByUserId = user.Id, CreatedByUser = user },
                new Quiz { Id = 2, Title = "Quiz 2", CreatedByUserId = user.Id, CreatedByUser = user },
                new Quiz { Id = 3, Title = "Quiz 3", CreatedByUserId = fakeUser.Id, CreatedByUser = fakeUser }
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _context.Quizzes.AddRangeAsync(expectedQuizzes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuizzes();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>() 
                .Which.Value.Should().BeAssignableTo<IEnumerable<Quiz>>();

            var quizzes = (IEnumerable<Quiz>)((OkObjectResult)result.Result!).Value!;
            quizzes.Should().HaveCount(2);

            var quiz = await _controller.GetQuiz(3);
            quiz.Value.Should().NotBeNull();
            quiz.Value?.Title.Should().Be("Quiz 3");
            quiz.Value?.CreatedByUser?.UserName.Should().Be("exampleUser2");
            _tokenServiceMock.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }
    }
}
