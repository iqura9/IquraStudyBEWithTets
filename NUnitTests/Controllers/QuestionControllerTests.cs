using FluentAssertions;
using IquraStudyBE.Classes;
using IquraStudyBE.Context;
using IquraStudyBE.Controllers;
using IquraStudyBE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit;


namespace NUnitTests
{
    [TestFixture]
    [Category("QuestionControllerTests")]
    public class QuestionControllerTests
    {
        private MyDbContext _context;
        private QuestionContoller _controller;
        
        [SetUp]
        public void SetUp()
        {
            SeedDatabase seed = new SeedDatabase();
            _context = seed._context;
            _controller = new QuestionContoller(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task PostQuestionWithAnswers_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var quizId = 1;
            var data = new CreateQuestionWithAnswersDto
            {
                Title = "Test Question",
                Answers = new List<CreateAnswerDto>
                {
                    new CreateAnswerDto { Title = "Answer 1", IsCorrect = true },
                    new CreateAnswerDto { Title = "Answer 2", IsCorrect = false },
                    new CreateAnswerDto { Title = "Answer 3", IsCorrect = false }
                }
            };

            // Act
            var result = await _controller.PostQuestionWithAnswers(quizId, data);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.ActionName.Should().Be("GetQuestion");
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.RouteValues?["id"].Should().NotBeNull();

            var createdAtActionResult = (CreatedAtActionResult)result.Result!;
            var questionId = createdAtActionResult.RouteValues!["id"].Should().BeOfType<int>().Subject;

            var questionInDatabase = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            questionInDatabase.Should().NotBeNull();
            questionInDatabase!.Title.Should().Be(data.Title);
            questionInDatabase.QuizId.Should().Be(quizId);
            questionInDatabase.Answers.Should().HaveCount(data.Answers.Count);
            questionInDatabase.isMultiSelect.Should().BeFalse();
            
        }
        [Test]
        public async Task DeleteQuestion_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            var existingQuestion = new Question { Id = 1, Title = "Existing Question" };
            _context.Questions.Add(existingQuestion);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteQuestion(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var questionInDatabase = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == 1);
            questionInDatabase.Should().BeNull();
        }

        [Test]
        public async Task DeleteQuestion_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteQuestion(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
