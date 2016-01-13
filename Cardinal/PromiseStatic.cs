using System;
using Medley;

namespace Cardinal {

	public static class Promise {

		public static Action<Exception> onUnhandled;

		public static Promise<T> Default<T>() {
			return new Promise<T>(default(T));
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

		public static Promise<B[]> Map<A, B>(A[] arr, Func<A, Promise<B>> iterator) {
			try {
				return All(arr.Map(a => iterator(a)));
			} catch (Exception ex) {
				return new Promise<B[]>(ex);
			}
		}

		public static Promise<B[]> Map<A, B>(A[] arr, Func<A, int, Promise<B>> iterator) {
			try {
				return All(arr.Map((a, i) => iterator(a, i)));
			} catch (Exception ex) {
				return new Promise<B[]>(ex);
			}
		}

		public static Promise<bool> Each<A, B>(A[] arr, Func<A, Promise<B>> iterator) {
			return arr.Reduce<A, Promise<bool>>(
				(last, v) => last.Next<bool>(ok => iterator(v).Map<bool>(b => ok)),
				new Promise<bool>(true)
			);
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
