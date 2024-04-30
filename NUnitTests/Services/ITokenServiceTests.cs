using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IquraStudyBE.Services;

namespace NUnitTests
{
    [TestFixture]
    [Category("Tokens")]
    public class Tests
    {
        private TokenServices _tokenServices;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var configurationMock = ConfigureConfigurationMock();

            _tokenServices = new TokenServices(configurationMock.Object, _httpContextAccessorMock.Object);
        }

        private Mock<IConfiguration> ConfigureConfigurationMock()
        {
            var configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(config => config["Jwt:Key"]).Returns("someJadfaWfcr43xqsqf23xKLSjd3ladm");
            configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("IquraStudy");
            configurationMock.Setup(config => config["Jwt:Audience"]).Returns("IquraStudy");
            configurationMock.Setup(config => config["Jwt:TokenValidityInMinutes"]).Returns("600000");
            configurationMock.Setup(config => config["Jwt:RefreshTokenValidityInDays"]).Returns("7");

            return configurationMock;
        }

        private void SetupHttpContextWithBearerToken(string token)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer " + token;
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        }

        [Test]
        public void CreateToken_ReturnsValidJwtSecurityToken()
        {
            // Arrange
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };

            // Act
            var token = _tokenServices.CreateToken(authClaims);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.InstanceOf<JwtSecurityToken>());
        }

        [Test]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            // Act
            var refreshToken = _tokenServices.GenerateRefreshToken();
            // Assert
            Assert.That(refreshToken, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetEmailFromToken_WithValidToken_ReturnsEmail()
        {
            // Arrange
            var expectedEmail = "test@example.com";
            var expectedId = "expectedId";

            // Create authentication claims with email claim
            var authClaims = new List<Claim>
            {
                new Claim("id", expectedId),
                new Claim("email", expectedEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create token with the provided authentication claims
            var token = _tokenServices.CreateToken(authClaims);
            var headerToken = new JwtSecurityTokenHandler().WriteToken(token);
            
            // Set up HttpContext with bearer token
            SetupHttpContextWithBearerToken(headerToken);

            // Act
            var userEmail = _tokenServices.GetEmailFromToken();
            var userId = _tokenServices.GetUserIdFromToken();
            // Assert
            Assert.That(userEmail, Is.EqualTo(expectedEmail));
            Assert.That(userId, Is.EqualTo(expectedId));
        }
    }
}
