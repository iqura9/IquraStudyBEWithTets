using FluentAssertions;
using IquraStudyBE.Classes;
using IquraStudyBE.Context;
using IquraStudyBE.Controllers;
using IquraStudyBE.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit;


namespace NUnitTests
{
    [TestFixture]
    [Category("ParallelTests")]
    [Parallelizable]
    public class AnswerControllerTests
    {
        private MyDbContext _context;
        private AnswerController _controller;
 
        
        [SetUp]
        public void SetUp()
        {
            SeedDatabase seed = new SeedDatabase();
            _context = seed._context;
            _controller = new AnswerController(_context);
        }
        
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        
        [Test]
        [TestCase("Answer 1", true)]
        [TestCase("Answer 2", false)]
        [TestCase("Answer 3", false)]
        [TestCase("Answer 4", false)]
        public async Task PostAnswer_WithValidData_ReturnsCreatedAtAction_WithParams(string title, bool isCorrect)
        {
            // Arrange
            var questionId = 1;
            var answersDto = new List<CreateAnswerDto>
            {
                new CreateAnswerDto { Title = title, IsCorrect = isCorrect }
            };
            var question = new Question { Id = questionId, Title = "Difficult question"};
            _context.Questions.Add(question);
            _context.SaveChanges();
            // Act
            var result = await _controller.PostAnswer(questionId, answersDto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.ActionName.Should().Be("GetAnswer");
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.Value.Should().BeOfType<List<Answer>>();
        }

        [Test]
        public async Task PostAnswer_WithValidData_ReturnsCreatedAtAction()
        {
           var questionId = 1;
            // Arrange
            var answersDto = new List<CreateAnswerDto>
            {
                new CreateAnswerDto { Title = "Answer 1", IsCorrect = true },
                new CreateAnswerDto { Title = "Answer 2", IsCorrect = false },
                new CreateAnswerDto { Title = "Answer 3", IsCorrect = false },
                new CreateAnswerDto { Title = "Answer 4", IsCorrect = false }
            };
            var question = new Question { Id = questionId, Title = "Difficult question"};
            _context.Questions.Add(question);
            _context.SaveChanges();
            
            // Act
            var result = await _controller.PostAnswer(questionId, answersDto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.ActionName.Should().Be("GetAnswer");
            result.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.Value.Should().BeOfType<List<Answer>>();
            
            // Verify if the answers exist in the database
            var answersInDatabase = _context.Answers.ToList();
            answersInDatabase.Should().HaveCount(answersDto.Count); // Ensure all answers are added

            // Ensure the titles and correctness of the answers match the DTOs
            foreach (var dto in answersDto)
            {
                answersInDatabase.Should().Contain(a => a.Title == dto.Title && a.IsCorrect == dto.IsCorrect);
            }
        }
        [Test]
        public async Task PostAnswer_WithNonExistingQuestion_ReturnsNotFound()
        {
            // Arrange
            var nonExistingQuestionId = 999; // Assuming this ID doesn't exist
            var answersDto = new List<CreateAnswerDto>
            {
                new CreateAnswerDto { Title = "Answer 1", IsCorrect = true },
                new CreateAnswerDto { Title = "Answer 2", IsCorrect = false }
            };

            // Act
            var result = await _controller.PostAnswer(nonExistingQuestionId, answersDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }
        
        [Test]
        public async Task PostAnswer_WithEmptyAnswersDto_ReturnsBadRequest()
        {
            // Arrange
            var question = new Question { Id = 3, Title = "Difficult question"};
            _context.Questions.Add(question);
            _context.SaveChanges();
            var emptyAnswersDto = new List<CreateAnswerDto>();

            // Act
            var result = await _controller.PostAnswer(3, emptyAnswersDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task PostAnswer_WithNullAnswersDto_ReturnsBadRequest()
        {
            // Arrange
            var question = new Question { Id = 4, Title = "Difficult question"};
            _context.Questions.Add(question);
            _context.SaveChanges();
            // Act
            var result = await _controller.PostAnswer(4, null);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Test]
        public async Task PostAnswer_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var question = new Question { Id = 5, Title = "Difficult question"};
            _context.Questions.Add(question);
            _context.SaveChanges();
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.PostAnswer(5, new List<CreateAnswerDto>());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
