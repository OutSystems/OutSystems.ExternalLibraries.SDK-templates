using Xunit;
using ConsumeSOAPExample.Structures;

namespace ConsumeSOAPExample.Tests
{
    public class CalculatorTests
    {
        /// <summary>
        /// Tests if the Calculator's Sum method correctly adds two numbers.
        /// The method accepts a struct with two numbers and returns their sum.
        /// </summary>
        [Fact]
        public void Add_Returns()
        {
           int sum =  new Calculator().Sum(new Numbers { NumberA = 2, NumberB = 3 });

            // Assert: Verify the returned sum is correct.
            Assert.Equal(5, sum);
        }

        /// <summary>
        /// Tests if the Calculator's Subtract method correctly subtracts one
        /// number from another. The method accepts a struct with two numbers and 
        /// returns their difference.
        /// </summary>
        [Fact]
        public void Sub_Returns()
        {
            int sub = new Calculator().Subtract(new Numbers { NumberA = 3, NumberB = 2 });

            // Assert: Verify the returned difference is correct.
            Assert.Equal(1, sub);
        }
    }
}