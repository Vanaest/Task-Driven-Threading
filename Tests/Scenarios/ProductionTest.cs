﻿using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Obscurum.TDT.Tests.Scenarios
{
    /// <summary>
    /// Test class for a scenario that uses many features of this package. This class includes:
    /// <ul>
    ///     <li>Multiple types of task interfaces (<see cref="Task{T}"/>, <see cref="MultiTask"/>) working together.
    ///     </li>
    ///     <li>Task dependency.</li>
    ///     <li>Dynamically calculated <see cref="MultiTask"/> single tasks amount.</li>
    ///     <li><see cref="Task{T}"/> with a return type.</li>
    ///     <li>Safe <see cref="Tracker.Wait(int)"/> with automatically timeout.</li>
    /// </ul>
    /// </summary>
    /// <author>Vanaest</author>
    /// <version>1.0.0</version>
    internal sealed class ProductionTest
    {
        /// <summary>
        /// Class to generate a <see cref="List{T}"/> of integers by the <see cref="amount"/>, starting at 1.
        /// </summary>
        private class Generator : Task
        {
            private readonly List<int> numbers;
            private readonly int amount;

            public Generator(List<int> numbers, int amount)
            {
                this.numbers = numbers;
                this.amount = amount;
            }

            public void Execute()
            {
                for (var i = 1; i <= amount; i++)
                {
                    numbers.Add(i);
                }
            }
        }

        /// <summary>
        /// Class to convert all integers in a <see cref="List{T}"/> to its power equivalent. This
        /// <see cref="MultiTask"/> takes some time.
        /// </summary>
        private class Power : MultiTask
        {
            private readonly List<int> numbers;

            public Power(List<int> numbers) => this.numbers = numbers;
            
            public void Execute(int i)
            {
                Thread.Sleep(100);
                numbers[i] *= numbers[i];
            }
        }

        /// <summary>
        /// Class to take the sum of a <see cref="List{T}"/> of integers
        /// </summary>
        private class Sum : Task<int>
        {
            private readonly List<int> numbers;

            public Sum(List<int> numbers) => this.numbers = numbers;
            
            public int Execute()
            {
                var result = 0;
                
                numbers.ForEach(i => result += i);

                return result;
            }
        }

        [Test]
        public void TestProduction()
        {
            // Arrange
            const int amount = 5;
            
            const int expected = 55;
            var actual = 0;

            var numbers = new List<int>();

            Task task1 = new Generator(numbers, amount);
            MultiTask task2 = new Power(numbers);
            Task<int> task3 = new Sum(numbers);

            // Act
            var tracker1 = task1.Schedule();
            var tracker2 = task2.Schedule(numbers, tracker1);
            var tracker3 = task3.Schedule(tracker2);

            tracker3.result += result => actual = result;
            
            tracker3.Wait(1000);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}