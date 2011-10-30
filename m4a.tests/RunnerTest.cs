using System;
using NUnit.Framework;

namespace m4a.tests {
	
	[TestFixture]
	public class RunnerTest {
		
		[Test]
		public void Pass ()
		{
			Assert.True (true);
		}

		[Test]
		public void Fail ()
		{
			Assert.False (true);
		}

		[Test]
		[Ignore ("another time")]
		public void Ignore ()
		{
			Assert.True (false);
		}
	}
}