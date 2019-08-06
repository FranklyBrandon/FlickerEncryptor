using NUnit.Framework;
using System;

namespace FlickerEncryptor.Test
{
    [TestFixture]
    public abstract class Specification
    {
        [SetUp]
        public void SetUp()
        {
            Arrange();
            Act();
            Assert();
            TearDown();
        }

        public abstract void Arrange();
        public abstract void Act();
        public abstract void Assert();
        public abstract void TearDown();

    }
}
