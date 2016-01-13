using System;
using Xunit;
using Cardinal;

namespace CardinalTests {

	public class PromiseTests {

		[Fact]
		public void can_resolve_simple_promise() {
			var promisedValue = 5;
			var promise = new Promise<int>(promisedValue);

			var completed = 0;
			promise.Then(v => {
				Assert.Equal(promisedValue, v);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void can_reject_simple_promise() {
			var ex = new Exception();
			var promise = new Promise<int>(ex);

			var errors = 0;
			promise.Catch(e => {
				Assert.Equal(ex, e);
				++errors;
			});

			Assert.Equal(1, errors);
		}

		[Fact]
		public void exception_is_thrown_for_reject_after_reject() {
			var promise = new Promise<int>();

			promise.Reject(new ApplicationException());

			Assert.Throws<ApplicationException>(() =>
				promise.Reject(new ApplicationException())
			);
		}

		[Fact]
		public void exception_is_thrown_for_reject_after_resolve() {
			var promise = new Promise<int>();

			promise.Resolve(5);

			Assert.Throws<ApplicationException>(() =>
				promise.Reject(new ApplicationException())
			);
		}

		[Fact]
		public void exception_is_thrown_for_resolve_after_reject() {
			var promise = new Promise<int>();

			promise.Reject(new ApplicationException());

			Assert.Throws<ApplicationException>(() =>
				promise.Resolve(5)
			);
		}

		[Fact]
		public void can_resolve_promise_and_trigger_then_handler() {
			var promise = new Promise<int>();

			var completed = 0;
			var promisedValue = 15;

			promise.Then(v => {
				Assert.Equal(promisedValue, v);
				++completed;
			});

			promise.Resolve(promisedValue);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void exception_is_thrown_for_resolve_after_resolve() {
			var promise = new Promise<int>();

			promise.Resolve(5);

			Assert.Throws<ApplicationException>(() =>
				promise.Resolve(5)
			);
		}

		[Fact]
		public void can_resolve_promise_and_trigger_multiple_then_handlers_in_order() {
			var promise = new Promise<int>();

			var completed = 0;

			promise.Then(v => Assert.Equal(1, ++completed));
			promise.Then(v => Assert.Equal(2, ++completed));

			promise.Resolve(1);

			Assert.Equal(2, completed);
		}

		[Fact]
		public void can_resolve_promise_and_trigger_then_handler_with_callback_registration_after_resolve() {
			var promise = new Promise<int>();

			var completed = 0;
			var promisedValue = -10;

			promise.Resolve(promisedValue);

			promise.Then(v => {
				Assert.Equal(promisedValue, v);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void can_reject_promise_and_trigger_error_handler() {
			var promise = new Promise<int>();

			var ex = new ApplicationException();
			var completed = 0;
			promise.Catch(e => {
				Assert.Equal(ex, e);
				++completed;
			});

			promise.Reject(ex);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void can_reject_promise_and_trigger_multiple_error_handlers_in_order() {
			var promise = new Promise<int>();

			var ex = new ApplicationException();
			var completed = 0;

			promise.Catch(e => {
				Assert.Equal(ex, e);
				Assert.Equal(1, ++completed);
			});
			promise.Catch(e => {
				Assert.Equal(ex, e);
				Assert.Equal(2, ++completed);
			});

			promise.Reject(ex);

			Assert.Equal(2, completed);
		}

		[Fact]
		public void can_reject_promise_and_trigger_error_handler_with_registration_after_reject() {
			var promise = new Promise<int>();

			var ex = new ApplicationException();
			promise.Reject(ex);

			var completed = 0;
			promise.Catch(e => {
				Assert.Equal(ex, e);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void error_handler_is_not_invoked_for_resolved_promised() {
			var promise = new Promise<int>();

			promise.Catch(e => {
				throw new ApplicationException("This shouldn't happen");
			});

			promise.Resolve(5);
		}

		[Fact]
		public void then_handler_is_not_invoked_for_rejected_promise() {
			var promise = new Promise<int>();

			promise.Then(v => {
				throw new ApplicationException("This shouldn't happen");
			});

			promise.Reject(new ApplicationException("Rejection!"));
		}

		[Fact]
		public void chain_multiple_promises_using_all() {
			var promise = new Promise<string>();
			var chainedPromise1 = new Promise<int>();
			var chainedPromise2 = new Promise<int>();
			var chainedResult1 = 10;
			var chainedResult2 = 15;

			var completed = 0;

			Promise.All(new Promise<int>[] { chainedPromise1, chainedPromise2 })
				.Then(results => {
					Assert.Equal(2, results.Length);
					Assert.Equal(chainedResult1, results[0]);
					Assert.Equal(chainedResult2, results[1]);

					++completed;
				});

			Assert.Equal(0, completed);

			promise.Resolve("hello");

			Assert.Equal(0, completed);

			chainedPromise1.Resolve(chainedResult1);

			Assert.Equal(0, completed);

			chainedPromise2.Resolve(chainedResult2);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void chain_multiple_promises_using_all_that_are_resolved_out_of_order() {
			var promise = new Promise<string>();
			var chainedPromise1 = new Promise<int>();
			var chainedPromise2 = new Promise<int>();
			var chainedResult1 = 10;
			var chainedResult2 = 15;

			var completed = 0;

			Promise.All(new Promise<int>[] { chainedPromise1, chainedPromise2 })
				.Then(results => {
					Assert.Equal(2, results.Length);
					Assert.Equal(chainedResult1, results[0]);
					Assert.Equal(chainedResult2, results[1]);

					++completed;
				});

			Assert.Equal(0, completed);

			promise.Resolve("hello");

			Assert.Equal(0, completed);

			chainedPromise2.Resolve(chainedResult2);

			Assert.Equal(0, completed);

			chainedPromise1.Resolve(chainedResult1);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void combined_promise_is_resolved_when_children_are_resolved() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var all = Promise.All(new Promise<int>[] { promise1, promise2 });

			var completed = 0;

			all.Then(results => {
				++completed;
				
				Assert.Equal(2, results.Length);
				Assert.Equal(1, results[0]);
				Assert.Equal(2, results[1]);
			});

			promise1.Resolve(1);
			promise2.Resolve(2);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void combined_promise_is_rejected_when_first_promise_is_rejected() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var all = Promise.All(new Promise<int>[] { promise1, promise2 });

			all.Then(v => {
				throw new ApplicationException("Shouldn't happen");
			});

			var errors = 0;
			all.Catch(e => {
				++errors;
			});

			promise1.Reject(new ApplicationException("Error!"));
			promise2.Resolve(2);

			Assert.Equal(1, errors);
		}

		[Fact]
		public void combined_promise_is_rejected_when_second_promise_is_rejected() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var all = Promise.All(new Promise<int>[] { promise1, promise2 });

			all.Then(v => {
				throw new ApplicationException("Shouldn't happen");
			});

			var errors = 0;
			all.Catch(e => {
				++errors;
			});

			promise1.Resolve(2);
			promise2.Reject(new ApplicationException("Error!"));

			Assert.Equal(1, errors);
		}

		[Fact]
		public void combined_promise_is_rejected_when_both_promises_are_rejected() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var all = Promise.All(new Promise<int>[] { promise1, promise2 });

			all.Then(v => {
				throw new ApplicationException("Shouldn't happen");
			});

			var errors = 0;
			all.Catch(e => {
				++errors;
			});

			promise1.Reject(new ApplicationException("Error!"));
			promise2.Reject(new ApplicationException("Error!"));

			Assert.Equal(1, errors);
		}

		[Fact]
		public void combined_promise_is_resolved_if_there_are_no_promises() {
			var all = Promise.All(new Promise<int>[0]);

			var completed = 0;

			all.Then(results => {
				++completed;

				Assert.Empty(results);
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void combined_promise_is_resolved_when_all_promises_are_already_resolved() {
			var promise1 = new Promise<int>(1);
			var promise2 = new Promise<int>(1);

			var all = Promise.All(new Promise<int>[] { promise1, promise2 });

			var completed = 0;

			all.Then(v => {
				++completed;

				Assert.Empty(v);
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void can_map_promise_value() {
			var promise = new Promise<int>();

			var promisedValue = 15;
			var completed = 0;

			promise
				.Map(v => v.ToString())
				.Then(v => {
					Assert.Equal(promisedValue.ToString(), v);

					++completed;
				});

			promise.Resolve(promisedValue);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void rejection_of_source_promise_rejects_mapped_promise() {
			var promise = new Promise<int>();

			var ex = new Exception();
			var errors = 0;

			promise
				.Map(v => v.ToString())
				.Catch(e => {
					Assert.Equal(ex, e);

					++errors;
				});

			promise.Reject(ex);

			Assert.Equal(1, errors);
		}

		[Fact]
		public void exception_thrown_during_transform_rejects_transformed_promise() {
			var promise = new Promise<int>();

			var promisedValue = 15;
			var errors = 0;
			var ex = new Exception();

			promise
				.Map<string>(v => {
					throw ex;
				})
				.Catch(e => {
					Assert.Equal(ex, e);

					++errors;
				});

			promise.Resolve(promisedValue);

			Assert.Equal(1, errors);
		}

		[Fact]
		public void can_chain_promise_and_convert_type_of_value() {
			var promise = new Promise<int>();
			var chainedPromise = new Promise<string>();

			var promisedValue = 15;
			var chainedPromiseValue = "blah";
			var completed = 0;

			promise
				.Next(v => chainedPromise)
				.Then(v => {
					Assert.Equal(chainedPromiseValue, v);

					++completed;
				});

			promise.Resolve(promisedValue);
			chainedPromise.Resolve(chainedPromiseValue);

			Assert.Equal(1, completed);
		}

		[Fact]
		public void exception_thrown_in_chain_rejects_resulting_promise() {
			var promise = new Promise<int>();
			var chainedPromise = new Promise<string>();

			var ex = new Exception();
			var errors = 0;

			promise
				.Next<string>(v => {
					throw ex;
				})
				.Catch(e => {
					Assert.Equal(ex, e);

					++errors;
				});

			promise.Resolve(15);

			Assert.Equal(1, errors);
		}

		[Fact]
		public void rejection_of_source_promise_rejects_chained_promise() {
			var promise = new Promise<int>();
			var chainedPromise = new Promise<string>();

			var ex = new Exception();
			var errors = 0;

			promise
				.Next(v => chainedPromise)
				.Catch(e => {
					Assert.Equal(ex, e);

					++errors;
				});

			promise.Reject(ex);

			Assert.Equal(1, errors);
		}

		[Fact]
		public void race_is_resolved_when_first_promise_is_resolved_first() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var resolved = 0;

			Promise
				.Race(promise1, promise2)
				.Then(i => resolved = i);

			promise1.Resolve(5);

			Assert.Equal(5, resolved);
		}

		[Fact]
		public void race_is_resolved_when_second_promise_is_resolved_first() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			var resolved = 0;

			Promise
				.Race(promise1, promise2)
				.Then(i => resolved = i);

			promise2.Resolve(12);

			Assert.Equal(12, resolved);
		}

		[Fact]
		public void race_is_rejected_when_first_promise_is_rejected_first() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			Exception ex = null;

			Promise
				.Race(promise1, promise2)
				.Catch(e => ex = e);

			var expected = new Exception();
			promise1.Reject(expected);

			Assert.Equal(expected, ex);
		}

		[Fact]
		public void race_is_rejected_when_second_promise_is_rejected_first() {
			var promise1 = new Promise<int>();
			var promise2 = new Promise<int>();

			Exception ex = null;

			Promise
				.Race(promise1, promise2)
				.Catch(e => ex = e);

			var expected = new Exception();
			promise2.Reject(expected);

			Assert.Equal(expected, ex);
		}

		[Fact]
		public void can_resolve_promise_via_resolver_function() {
			var promise = new Promise<int>((resolve, reject) => {
				resolve(5);
			});

			var completed = 0;
			promise.Then(v => {
				Assert.Equal(5, v);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void can_reject_promise_via_reject_function() {
			var ex = new Exception();
			var promise = new Promise<int>((resolve, reject) => {
				reject(ex);
			});

			var completed = 0;
			promise.Catch(e => {
				Assert.Equal(ex, e);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void exception_thrown_during_resolver_rejects_proimse() {
			var ex = new Exception();
			var promise = new Promise<int>((resolve, reject) => {
				throw ex;
			});

			var completed = 0;
			promise.Catch(e => {
				Assert.Equal(ex, e);
				++completed;
			});

			Assert.Equal(1, completed);
		}

		[Fact]
		public void unhandled_exception_is_propagated_via_event() {
			var promise = new Promise<int>();
			var ex = new Exception();
			var eventRaised = 0;

			Action<Exception> handler = e => {
				Assert.Equal(ex, e);

				++eventRaised;
			};

			Promise.onUnhandled += handler;

			try {
				promise
					.Then(a => {
						throw ex;
					})
					.Done();

				promise.Resolve(5);

				Assert.Equal(1, eventRaised);
			} finally {
				Promise.onUnhandled -= handler;
			}
		}

		[Fact]
		public void handled_exception_is_not_propagated_via_event() {
			var promise = new Promise<int>();
			var ex = new Exception();
			var eventCaught = 0;
			var eventRaised = 0;

			Action<Exception> handler = e => ++eventRaised;

			Promise.onUnhandled += handler;

			try {
				promise
					.Then(a => {
						throw ex;
					})
					.Catch(_ => ++eventCaught)
					.Done();

				promise.Resolve(5);

				Assert.Equal(1, eventCaught);
				Assert.Equal(0, eventRaised);
			} finally {
				Promise.onUnhandled -= handler;
			}

		}

		[Fact]
		public void can_handle_Done_onResolved() {
			var promise = new Promise<int>();
			var callback = 0;
			var expectedValue = 5;

			promise.Done(value => {
				Assert.Equal(expectedValue, value);

				++callback;
			});

			promise.Resolve(expectedValue);

			Assert.Equal(1, callback);
		}

		[Fact]
		public void can_handle_Done_onResolved_with_onReject() {
			var promise = new Promise<int>();
			var callback = 0;
			var errorCallback = 0;
			var expectedValue = 5;

			promise.Done(
				value => {
					Assert.Equal(expectedValue, value);

					++callback;
				},
				ex => {
					++errorCallback;
				}
			);

			promise.Resolve(expectedValue);

			Assert.Equal(1, callback);
			Assert.Equal(0, errorCallback);
		}
		
        [Fact]
        public void exception_during_Done_onResolved_triggers_error_hander()
        {
            var promise = new Promise<int>();
            var callback = 0;
            var errorCallback = 0;
            var expectedValue = 5;
            var expectedException = new Exception();

            promise.Done(
                value =>
                {
                    Assert.Equal(expectedValue, value);

                    ++callback;

                    throw expectedException;
                },
                ex =>
                {
                    Assert.Equal(expectedException, ex);

                    ++errorCallback;
                }
            );

            promise.Resolve(expectedValue);

            Assert.Equal(1, callback);
            Assert.Equal(1, errorCallback);
        }

		[Fact]
		public void exception_during_Then_onResolved_triggers_error_hander() {
			var promise = new Promise<int>();
			var callback = 0;
			var errorCallback = 0;
			var expectedException = new Exception();

			promise
				.Next(value => {
					throw expectedException;

					return new Promise<int>(10);
				})
				.Done(
					value => {
						++callback;
					},
					ex => {
						Assert.Equal(expectedException, ex);

						++errorCallback;
					}
				);

			promise.Resolve(6);

			Assert.Equal(0, callback);
			Assert.Equal(1, errorCallback);
		}

	}

}
