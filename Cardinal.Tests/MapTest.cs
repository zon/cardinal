using System;
using Xunit;
using Cardinal;

namespace CardinalTests {

	public class MapTest {

		int Mapper(int v) {
			return v * 2;
		}

		Promise<int> MapperPromise(int v) {
			return Promise.Resolve(Mapper(v));
		}

		[Fact]
		public void should_map_input_values_array() {
			var input = new Promise<int>[] { Promise.Resolve(1), Promise.Resolve(2), Promise.Resolve(3) };
			var result = new int[] { 2, 4, 6 };
			var resolved = 0;

			Promise.Map(input, Mapper).Then(v => {
				Assert.Equal(result, v);
				resolved++;
			});

			Assert.Equal(1, resolved);
		}

		[Fact]
		public void should_map_input_promies_array() {
			var input = new Promise<int>[] { Promise.Resolve(1), Promise.Resolve(2), Promise.Resolve(3) };
			var result = new int[] { 2, 4, 6 };
			var resolved = 0;

			Promise.Map(input, Mapper).Then(v => {
				Assert.Equal(result, v);
				resolved++;
			});

			Assert.Equal(1, resolved);
		}

		[Fact]
		public void should_map_input_when_mapper_returns_a_promise() {
			var input = new int[] { 1, 2, 3 };
			var result = new int[] { 2, 4, 6 };
			var resolved = 0;

			Promise.Map(input, MapperPromise).Then(v => {
				Assert.Equal(result, v);
				resolved++;
			});

			Assert.Equal(1, resolved);
		}

		[Fact]
		public void should_reject_when_input_contains_rejection() {
			var exception = new Exception("Fail");
			var input = new Promise<int>[] { Promise.Resolve(1), Promise.Reject<int>(exception), Promise.Resolve(3) };
			var resolved = 0;
			var rejected = 0;

			Promise.Map(input, MapperPromise)
				.Then(v => resolved++)
				.Catch(e => {
					Assert.Equal(exception, e);
					rejected++;
				});

			Assert.Equal(0, resolved);
			Assert.Equal(1, rejected);
		}

	}

}
