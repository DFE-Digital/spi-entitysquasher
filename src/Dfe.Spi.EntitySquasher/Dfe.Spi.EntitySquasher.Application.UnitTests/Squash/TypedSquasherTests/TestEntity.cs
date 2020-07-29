using System;
using Dfe.Spi.Models.Entities;

namespace Dfe.Spi.EntitySquasher.Application.UnitTests.Squash.TypedSquasherTests
{
    public class TestEntity : EntityBase
    {
        public string Address { get; set; }
        public int NumberOfPupils { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? OpenDate { get; set; }
        public string[] ContactNumbers { get; set; }
    }
}