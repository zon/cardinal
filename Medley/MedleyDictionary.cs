using System;
using System.Collections.Generic;

namespace Medley {

	public static class MedleyDictionary {

		public static void Each<K, V>(this Dictionary<K, V> dict, Action<V> iteratee) {
			var enumerator = dict.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					iteratee(enumerator.Current.Value);
				}
			} finally {
				enumerator.Dispose();
			}
		}

		public static void Each<K, V>(this Dictionary<K, V> dict, Action<V, K> iteratee) {
			var enumerator = dict.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					var current = enumerator.Current;
					iteratee(current.Value, current.Key);
				}
			} finally {
				enumerator.Dispose();
			}
		}

		public static V Max<K, V>(this Dictionary<K, V> dict, Func<V, V, int> compare) {
			if (dict.Count < 1)
				return default(V);
			V result;
			var enumerator = dict.GetEnumerator();
			try {
				enumerator.MoveNext();
				result = enumerator.Current.Value;
				var previous = result;
				while (enumerator.MoveNext()) {
					var item = enumerator.Current.Value;
					if (compare(previous, item) < 0)
						result = previous = item;
				}
			} finally {
				enumerator.Dispose();
			}
			return result;
		}

	}

}