using Xunit;
using ConsumeSOAPExample.Structures;

namespace ConsumeSOAPExample.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void Add_Returns()
        {
           int sum =  new Calculator().Sum(new Numbers { NumberA = 2, NumberB = 3 });

            // Assert
            Assert.Equal(5, sum);
        }
        [Fact]
        public void Sub_Returns()
        {
            int sub = new Calculator().Subtract(new Numbers { NumberA = 3, NumberB = 2 });

            // Assert
            Assert.Equal(1, sub);
        }
    }
}