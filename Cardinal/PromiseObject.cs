using System;

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

	}

}
