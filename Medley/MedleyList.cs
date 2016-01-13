using System;
using System.Collections.Generic;
using System.Linq;

namespace Medley {

	public static class MedleyList {

		public static List<T> Slice<T>(this List<T> list, int begin, int end) {
			var result = new List<T>();
			for (var i = begin; i < end; i++) {
				result.Add(list[i]);
			}
			return result;
		}

		public static List<T> Slice<T>(this List<T> list, int end) {
			return list.Slice(0, end);
		}

		public static List<B> Map<A, B>(this List<A> list, Func<A, B> transform) {
			var result = new List<B>();
			for (var i = 0; i < list.Count; i++) {
				result.Add(transform(list[i]));
			}
			return result;
		}

		public static List<T> Filter<T>(this List<T> list, Func<T, bool> test) {
			var result = new List<T>();
			for (var i = 0; i < list.Count; i++) {
				var item = list[i];
				if (test(item))
					result.Add(item);
			}
			return result;
		}

		public static List<T> Compact<T>(this List<T> list) {
			var d = default(T);
			return list.Filter(v => !EqualityComparer<T>.Default.Equals(v, d));
		}

		public static List<T> Flatten<T>(this List<List<T>> list) {
			var result = new List<T>();
			for (var y = 0; y < list.Count; y++) {
				var row = list[y];
				for (var x = 0; x < row.Count; x++) {
					result.Add(row[x]);
				}
			}
			return result;
		}

		public static A Max<A, B>(this List<A> list, Func<A, B> transform, Func<B, B, int> compare) {
			if (list.Count < 1)
				return default(A);
			var result = list[0];
			var previous = transform(result);
			for (var i = 1; i < list.Count; i++) {
				var item = list[i];
				var next = transform(item);
				if (compare(previous, next) < 0) {
					result = item;
					previous = next;
				}
			}
			return result;
		}

		public static float Max(this List<float> list) {
			return Max(list, (a, b) => a.CompareTo(b));
		}

		public static A Min<A, B>(this List<A> list, Func<A, B> transform, Func<B, B, int> compare) {
			if (list.Count < 1)
				return default(A);
			var result = list[0];
			var previous = transform(result);
			for (var i = 1; i < list.Count; i++) {
				var item = list[i];
				var next = transform(item);
				if (compare(previous, next) > 0) {
					result = item;
					previous = next;
				}
			}
			return result;
		}

		public static float Min(this List<float> list) {
			return Min(list, (a, b) => a.CompareTo(b));
		}

		public static T Max<T>(this List<T> list, Func<T, T, int> compare) {
			if (list.Count < 1)
				return default(T);
			var result = list[0];
			var previous = result;
			for (var i = 1; i < list.Count; i++) {
				var item = list[i];
				if (compare(previous, item) < 0) {
					result = previous = item;
				}
			}
			return result;
		}

		public static T Min<T>(this List<T> list, Func<T, T, int> compare) {
			if (list.Count < 1)
				return default(T);
			var result = list[0];
			var previous = result;
			for (var i = 1; i < list.Count; i++) {
				var item = list[i];
				if (compare(previous, item) > 0) {
					result = previous = item;
				}
			}
			return result;
		}

		public static float Avg(this List<float> list) {
			var t = 0f;
			for (var i = 0; i < list.Count; i++)
				t += list[i];
			return t / list.Count;
		}

		public static List<T> SortBy<T, TKey>(this List<T> list, Func<T, TKey> convert) {
			return list.OrderBy(convert).ToList();
		}

		public static T First<T>(this List<T> list) {
			if (list.Count > 0) {
				return list[0];
			} else {
				return default(T);
			}
		}

		public static T Random<T>(this List<T> list) {
			if (list.Count > 0) {
				return list[UnityEngine.Random.Range(0, list.Count)];
			} else {
				return default(T);
			}
		}

		public static bool Contains<T>(this List<T> list, Func<T, bool> test) {
			for (var i = 0; i < list.Count; i++) {
				if (test(list[i]))
					return true;
			}
			return false;
		}

		public static T[] ToArray<T>(this List<T> list) {
			var arr = new T[list.Count];
			for (var i = 0; i < list.Count; i++) {
				arr[i] = list[i];
			}
			return arr;
		}

	}

}