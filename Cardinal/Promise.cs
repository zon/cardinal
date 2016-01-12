using System;
using System.Collections;

namespace Cardinal {

	public class Promise<A> {
		Action<A> resolutions;
		Action<Exception> rejections;

		public static Action<Exception> onException;

		public State state {
			get;
			private set;
		}

		public A value {
			get;
			private set;
		}

		public Exception reason {
			get;
			private set;
		}

		public bool isFulfilled {
			get { return state == State.Fulfilled; }
		}

		public bool isRejected {
			get { return state == State.Rejected; }
		}

		public bool isPending {
			get { return state == State.Pending; }
		}

		public Promise() { }

		public Promise(A value) {
			Resolve(value);
		}

		public Promise(Promise<A> promise) {
			Resolve(promise);
		}

		public Promise(Exception reason) {
			Reject(reason);
		}

		public void Resolve(Promise<A> promise) {
			var self = this;
			promise.Map(v => {
				self.Resolve(v);
				return v;
			}).Catch(e => self.Reject(e));
		}

		public void Resolve(A value) {
			if (state == State.Pending) {
				state = State.Fulfilled;
				this.value = value;
				Resolve();
			} else {
				throw new ApplicationException("Promise is " + state + ". Only " + State.Pending + " promises can be resolved.");
			}
		}

		public void Reject(Exception reason) {
			if (state == State.Pending) {
				state = State.Rejected;
				this.reason = reason;
				Resolve();
			} else {
				throw new ApplicationException("Promise is " + state + ". Only " + State.Pending + " promises can be rejected.");
			}
		}

		public Promise<A> Then(Action onFulfilled) {
			var promise = new Promise<A>();
			resolutions += value => {
				try {
					onFulfilled();
					promise.Resolve(value);
				} catch (Exception e) {
					promise.Reject(e);
				}
			};
			rejections += promise.Reject;
			Resolve();
			return promise;
		}

		public Promise<A> Then(Action<A> onFulfilled) {
			var promise = new Promise<A>();
			resolutions += value => {
				try {
					onFulfilled(value);
					promise.Resolve(value);
				} catch (Exception e) {
					promise.Reject(e);
				}
			};
			rejections += promise.Reject;
			Resolve();
			return promise;
		}

		public Promise<B> Map<B>(Func<A, B> onFulfilled) {
			var promise = new Promise<B>();
			resolutions += value => {
				try {
					promise.Resolve(onFulfilled(value));
				} catch (Exception e) {
					promise.Reject(e);
				}
			};
			rejections += promise.Reject;
			Resolve();
			return promise;
		}

		public Promise<B> Next<B>(Func<A, Promise<B>> onFulfilled) {
			var promise = new Promise<B>();
			resolutions += value => {
				try {
					promise.Resolve(onFulfilled(value));
				} catch (Exception e) {
					promise.Reject(e);
				}
			};
			rejections += promise.Reject;
			Resolve();
			return promise;
		}

		public void Done() {
			if (onException != null)
				rejections += onException;
			Resolve(true);
		}

		public void Done(Action<A> onFulfilled) {
			resolutions += value => {
				try {
					onFulfilled(value);
				} catch (Exception e) {
					if (onException != null)
						onException(e);
				}
			};
			Done();
		}

		public Promise<A> Catch(Action<Exception> onRejected) {
			return Catch<Exception>(onRejected);
		}

		public Promise<A> Catch<E>(Action<E> onRejected) where E : Exception {
			var promise = new Promise<A>();
			resolutions += promise.Resolve;
			rejections += exp => {
				if (exp is E) {
					var e = exp as E;
					onRejected(e);
				} else {
					promise.Reject(exp);
				}
			};
			Resolve();
			return promise;
		}

		public Promise<A> Catch<E>(Func<E, bool> condition, Action<E> onRejected) where E : Exception {
			var promise = new Promise<A>();
			resolutions += promise.Resolve;
			rejections += exp => {
				var handled = false;
				if (exp is E) {
					var e = exp as E;
					if (condition(e)) {
						onRejected(e);
						handled = true;
					}
				}
				if (!handled)
					promise.Reject(exp);
			};
			Resolve();
			return promise;
		}

		public Promise<A> Recover<E>(Func<E, Promise<A>> recover) where E : Exception {
			var promise = new Promise<A>();
			resolutions += promise.Resolve;
			rejections += exp => {
				var recovered = false;
				if (exp is E) {
					var e = exp as E;
					try {
						promise.Resolve(recover(e));
						recovered = true;
					} catch (Exception x) {
						promise.Reject(x);
					}
				}
				if (!recovered)
					promise.Reject(exp);
			};
			Resolve();
			return promise;
		}

		public Promise<A> Recover<E>(Func<E, bool> condition, Func<E, Promise<A>> recover) where E : Exception {
			var promise = new Promise<A>();
			resolutions += promise.Resolve;
			rejections += exp => {
				var recovered = false;
				if (exp is E) {
					var e = exp as E;
					if (condition(e)) {
						try {
							promise.Resolve(recover(e));
							recovered = true;
						} catch (Exception x) {
							promise.Reject(x);
						}
					}
				}
				if (!recovered)
					promise.Reject(exp);
			};
			Resolve();
			return promise;
		}

		//		public void Wait() {
		//			var i = 2000;
		//			while (isPending) {
		//				if (--i > 0) {
		//					Thread.Sleep(10);
		//				} else {
		//					throw new Exception("Wait time out");
		//				}
		//			}
		//		}

		public IEnumerator WaitCoroutine() {
			while (isPending) {
				yield return null;
			}
		}

		public override string ToString() {
			return string.Format("Promise<{0}>: state={1}, value={2}, reason={3}", typeof(A), state, value, reason);
		}

		void Resolve(bool done = false) {
			if (state == State.Fulfilled) {
				if (resolutions != null)
					resolutions(value);
			} else if (state == State.Rejected) {
				if (rejections != null)
					rejections(reason);
				else if (done && onException != null)
					onException(reason);
			}
			if (state != State.Pending) {
				resolutions = null;
				rejections = null;
			}
		}

		public enum State {
			Pending,
			Fulfilled,
			Rejected
		}

	}

}
