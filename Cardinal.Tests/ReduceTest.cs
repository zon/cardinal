using System;
using Xunit;
using Cardinal;

namespace CardinalTests {

	public class ReduceTest {

		Promise<int> TotalPromise(int total, int value) {
			return Promise.Resolve(total + value);
		}

		[Theory]
		[InlineData(new int[0])]
		[InlineData(new int[] { 7 })]
		[InlineData(new int[] { 2, 5, 7 })]
		public void works_with_arrays(int[] data) {
			var resolved = 0;
			var total = 0;
			for (var d = 0; d < data.Length; d++)
				total += data[d];
			Promise.Reduce(data, TotalPromise, 0)
				.Then(t => {
					Assert.Equal(total, t);
					resolved++;
				});
			Assert.Equal(1, resolved);
		}

		[Fact]
		public void handles_accumulator_exceptions() {
			var exception = new Exception("Fail");
			var input = new int[] { 2, 5, 7 };
			var resolved = 0;
			var rejected = 0;
			Promise.Reduce(input, (last, value) => {
				if (value == 5)
					throw exception;
				else
					return Promise.Resolve(last + value);
			}, 0)
				.Then(t => resolved++)
				.Catch(e => {
					Assert.Equal(exception, e);
					rejected++;
				});
			Assert.Equal(0, resolved);
			Assert.Equal(1, rejected);
		}

		[Fact]
		public void handles_chained_accumulator_exceptions() {
			var exception = new Exception("Fail");
			var input = new int[] { 2, 5, 7 };
			var resolved = 0;
			var rejected = 0;
			Promise.Reduce(input, (last, value) => {
				if (value == 5)
					return Promise.Reject<int>(exception);
				else
					return Promise.Resolve(last + value);
			}, 0)
				.Then(t => resolved++)
				.Catch(e => {
					Assert.Equal(exception, e);
					rejected++;
				});
			Assert.Equal(0, resolved);
			Assert.Equal(1, rejected);
		}

	}

}
