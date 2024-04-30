using FluentAssertions;
using IquraStudyBE.Classes;
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
    [Category("TaskControllerTests")]
    public class TaskControllerTests
    {
        private MyDbContext _context;
        private TaskController _controller;
        private Mock<ITokenService> _tokenServiceMock;
        [SetUp]
        public void SetUp()
        {
            SeedDatabase seed = new SeedDatabase();
            _context = seed._context;
            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock.CallBase = false;
            _tokenServiceMock.Setup(ts => ts.GetUserIdFromToken()).Returns("userId");
            _controller = new TaskController(_context, _tokenServiceMock.Object);
        }

        [Test]
        public async Task PostGroupTask_GetGroupTask_DeleteGroupTask()
        {
            // Arrange
            var createGroupTaskDto = new CreateGroupTaskDTO
            {
                Title = "Task Title",
                Description = "Task Description",
                GroupId = 1
            };

            // Act
            var result = await _controller.PostGroupTask(createGroupTaskDto);
            var group = await _controller.GetGroupTask(1);

            // Assert
            result.Result.Should().BeAssignableTo<CreatedAtActionResult>().Which.Value.Should().BeAssignableTo<GroupTask>();

            var createdAtActionResult = (CreatedAtActionResult)result.Result;
            var groupTask = (GroupTask)createdAtActionResult.Value;

            groupTask.Title.Should().Be(createGroupTaskDto.Title);
            groupTask.Description.Should().Be(createGroupTaskDto.Description);
            groupTask.GroupId.Should().Be(createGroupTaskDto.GroupId);
            groupTask.CreateByUserId.Should().Be("userId");

            group.Value[0].Title.Should().Be(createGroupTaskDto.Title);
            group.Value[0].Description.Should().Be(createGroupTaskDto.Description);
            group.Value[0].GroupId.Should().Be(createGroupTaskDto.GroupId);
            group.Value[0].CreateByUserId.Should().Be("userId");

            _tokenServiceMock.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
            
            var delete = await _controller.DeleteGroupTask(group.Value[0].Id);
            
            delete.Should().BeOfType<NoContentResult>();

            _tokenServiceMock.Verify(ts => ts.GetUserIdFromToken(), Times.Exactly(1));

            var sameGroup = await _controller.GetGroupTask(1);
            sameGroup.Value.Should().BeEmpty();
            _tokenServiceMock.Verify(ts => ts.GetUserIdFromToken(), Times.Exactly(1));
            
            await _controller.PostGroupTask(createGroupTaskDto);
            
            _tokenServiceMock.Verify(ts => ts.GetUserIdFromToken(), Times.Exactly(2));
        }
        [Test]
        public async Task PostGroupTask_HandleExceptionFromTokenService()
        {
            // Arrange
            _tokenServiceMock.Setup(ts => ts.GetUserIdFromToken()).Throws<Exception>();

            var createGroupTaskDto = new CreateGroupTaskDTO
            {
                Title = "Task Title",
                Description = "Task Description",
                GroupId = 1
            };

            // Act
            var result = await _controller.PostGroupTask(createGroupTaskDto);
            
            // Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
            result.Result.As<ObjectResult>().Value.As<ProblemDetails>().Detail.Should().Be("An error occurred while processing your request.");
        }


    }
}
