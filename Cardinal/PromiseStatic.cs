using System;
using Medley;

namespace Cardinal {

	public static class Promise {

		public static Action<Exception> onUnhandled;

		public static Promise<T> Resolve<T>(T value) {
			var promise = new Promise<T>();
			promise.Resolve(value);
			return promise;
		}

		public static Promise<T> Reject<T>(Exception reason) {
			var promise = new Promise<T>();
			promise.Reject(reason);
			return promise;
		}

		public static Promise<T[]> All<T>(params Promise<T>[] promises) {
			if (promises.Length < 1)
				return new Promise<T[]>(new T[0]);
			var result = new Promise<T[]>();
			var values = new T[promises.Length];
			var remaining = promises.Length;
			for (var p = 0; p < promises.Length; p++) {
				var i = p;
				var promise = promises[p];
				promise.Then(v => {
					values[i] = v;
					remaining--;
					if (remaining < 1)
						result.Resolve(values);
				});
				promise.Catch(e => {
					if (result.isPending)
						result.Reject(e);
				});
			}
			return result;
		}

		public static Promise<T> Race<T>(params Promise<T>[] promises) {
			var result = new Promise<T>();
			for (var p = 0; p < promises.Length; p++) {
				var promise = promises[p];
				promise.Then(v => {
					if (result.isPending)
						result.Resolve(v);
				});
				promise.Catch(e => {
					if (result.isPending)
						result.Reject(e);
				});
			}
			return result;
		}

		public static Promise<B[]> Map<A, B>(A[] arr, Func<A, int, Promise<B>> mapper) {
			try {
				return All(arr.Map((a, i) => mapper(a, i)));
			} catch (Exception ex) {
				return new Promise<B[]>(ex);
			}
		}

		public static Promise<B[]> Map<A, B>(A[] arr, Func<A, Promise<B>> mapper) {
			return Map(arr, (v, i) => mapper(v));
		}

		public static Promise<B[]> Map<A, B>(Promise<A>[] promises, Func<A, int, B> mapper) {
			return All(promises).Map(values => values.Map(mapper));
		}

		public static Promise<B[]> Map<A, B>(Promise<A>[] promises, Func<A, B> mapper) {
			return Map(promises, (v, i) => mapper(v));
		}

		public static Promise<B[]> Map<A, B>(Promise<A>[] promises, Func<A, int, Promise<B>> mapper) {
			return All(promises).Next(values => Map(values, mapper));
		}

		public static Promise<B[]> Map<A, B>(Promise<A>[] promises, Func<A, Promise<B>> mapper) {
			return Map(promises, (v, i) => mapper(v));
		}

		public static Promise<B> Reduce<A, B>(A[] arr, Func<B, A, Promise<B>> accumulator, B initial) {
			return arr.Reduce((last, a) => last.Next(b => accumulator(b, a)), Resolve(initial));
		}

		public static Promise<B> Reduce<A, B>(A[] arr, Func<B, A, Promise<B>> accumulator) {
			return Reduce(arr, accumulator, default(B));
		}

		public static Promise<Tuple<T1, T2>> Join<T1, T2>(Promise<T1> promise1, Promise<T2> promise2) {
			var results = new Promise<Tuple<T1, T2>>();
			var resolve = new Action(delegate {
				if (!promise1.isPending && !promise2.isPending) {
					results.Resolve(new Tuple<T1, T2>(promise1.value, promise2.value));
				}
			});
			promise1.Then(resolve);
			promise2.Then(resolve);
			return results;
		}

		public static Promise<Tuple<T1, T2, T3>> Join<T1, T2, T3>(
			Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3
		) {
			var results = new Promise<Tuple<T1, T2, T3>>();
			var resolve = new Action(delegate {
				if (!promise1.isPending && !promise2.isPending && !promise3.isPending) {
					results.Resolve(new Tuple<T1, T2, T3>(promise1.value, promise2.value, promise3.value));
				}
			});
			promise1.Then(resolve);
			promise2.Then(resolve);
			promise3.Then(resolve);
			return results;
		}

	}

}
