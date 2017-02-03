using Commons.Json;
using NUnit.Framework;

namespace Commons.Tests.Json
{
    [TestFixture]
    public class JsonConvertTest
    {
        [Test]
        public void PrivateSetter()
        {
            var x = new Dto1 {A = 10};
            x.SetB(20);
            var s = x.ToJson();
            Assert.That(s, Does.Contain("\"B\":20"));
            var y = s.FromJson<Dto1>();
            Assert.That(y.A, Is.EqualTo(10));
            Assert.That(y.B, Is.EqualTo(0));
        }

        [Test]
        public void CtorParam_NoDefaultCtor()
        {
            var x = new Dto2(30);
            var s = x.ToJson();
            Assert.That(s, Does.Contain("\"C\":30"));
            var y = s.FromJson<Dto2>();
            Assert.That(y.C, Is.EqualTo(30));
        }

        [Test]
        public void CtorParam_WithDefaultCtor()
        {
            var x = new Dto3(30);
            var s = x.ToJson();
            Assert.That(s, Does.Contain("\"C\":30"));
            var y = s.FromJson<Dto3>();
            Assert.That(y.C, Is.EqualTo(0));
        }

        private class Dto1
        {
            public int A { get; set; }
            public int B { get; private set; }

            public void SetB(int b)
            {
                B = b;
            }
        }

        private class Dto2
        {
            public Dto2(int c)
            {
                C = c;
            }

            public int C { get; private set; }
        }

        private class Dto3
        {
            public Dto3()
            {
            }

            public Dto3(int c)
            {
                C = c;
            }

            public int C { get; private set; }
        }
    }
}