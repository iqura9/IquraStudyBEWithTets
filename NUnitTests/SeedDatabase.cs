using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using IquraStudyBE.Classes;
using IquraStudyBE.Context;
using IquraStudyBE.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace NUnit
{
    internal class SeedDatabase
    {
        public MyDbContext _context;
        public Mock<IDBContextFactory> _contextFactoryMock;

        public SeedDatabase()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
               .UseInMemoryDatabase(databaseName: "TestDatabase")
               .Options;
            _context = new MyDbContext(options);
        }
    }
}
