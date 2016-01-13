using System;
using System.Collections.Generic;
using System.Linq;

namespace Medley {

	public static class MedleyArray {

		public static T[] Slice<T>(this T[] arr, int begin, int end) {
			var result = new T[end - begin];
			var r = 0;
			for (var a = begin; a < end; a++)
				result[r++] = arr[a];
			return result;
		}

		public static T[] Concat<T>(this T[] arr, params T[][] others) {
			var length = arr.Length;
			for (var o = 0; o < others.Length; o++)
				length += others[o].Length;
			var result = new T[length];
			arr.CopyTo(result, 0);
			var index = arr.Length;
			for (var o = 0; o < others.Length; o++) {
				var other = others[o];
				other.CopyTo(result, index);
				index += other.Length;
			}
			return result;
		}

		public static B[] Map<A, B>(this A[] arr, Func<A, B> func) {
			var result = new B[arr.Length];
			for (var a = 0; a < arr.Length; a++) {
				result[a] = func(arr[a]);
			}
			return result;
		}

		public static B[] Map<A, B>(this A[] arr, Func<A, int, B> func) {
			var result = new B[arr.Length];
			for (var a = 0; a < arr.Length; a++) {
				result[a] = func(arr[a], a);
			}
			return result;
		}

		public static B Reduce<A, B>(this A[] arr, Func<B, A, B> iteratee, B memo) {
			for (var i = 0; i < arr.Length; i++) {
				memo = iteratee(memo, arr[i]);
			}
			return memo;
		}

		public static B Reduce<A, B>(this A[] arr, Func<B, A, B> iteratee) {
			return Reduce(arr, iteratee, default(B));
		}

		public static T[] Filter<T>(this T[] arr, Func<T, bool> test) {
			var result = new List<T>();
			for (var i = 0; i < arr.Length; i++) {
				var item = arr[i];
				if (test(item))
					result.Add(item);
			}
			return MedleyList.ToArray(result);
		}

		public static T Find<T>(this T[] arr, Func<T, bool> test) {
			for (var a = 0; a < arr.Length; a++) {
				var item = arr[a];
				if (test(item)) {
					return item;
				}
			}
			return default(T);
		}

		public static int FindIndex<T>(this T[] arr, Func<T, bool> test) {
			for (var a = 0; a < arr.Length; a++)
				if (test(arr[a]))
					return a;
			return -1;
		}

		public static T Max<T>(this T[] arr, Func<T, T, int> compare) {
			if (arr.Length < 1)
				return default(T);
			var result = arr[0];
			var previous = result;
			for (var i = 1; i < arr.Length; i++) {
				var item = arr[i];
				if (compare(previous, item) < 0) {
					result = previous = item;
				}
			}
			return result;
		}

		public static int Max(this int[] arr) {
			return Max(arr, (a, b) => a - b);
		}

		public static float Max(this float[] arr) {
			return Max(arr, (a, b) => {
				if (a > b) {
					return 1;
				} else if (a < b) {
					return -1;
				} else {
					return 0;
				}
			});
		}

		public static float Avg(this float[] arr) {
			var t = 0f;
			for (var i = 0; i < arr.Length; i++)
				t += arr[i];
			return t / arr.Length;
		}

		public static T Random<T>(this T[] arr, Random random) {
			if (arr.Length > 0) {
				return arr[random.Next(0, arr.Length)];
			} else {
				return default(T);
			}
		}

		public static T Random<T>(this T[] arr) {
			return Random(arr, new Random());
		}

		public static T First<T>(this T[] arr) {
			return arr[0];
		}

		public static T Last<T>(this T[] arr) {
			return arr[arr.Length - 1];
		}

		public static T[] Flatten<T>(this T[][] arr) {
			var len = 0;
			for (var a = 0; a < arr.Length; a++)
				len += arr[a].Length;
			var result = new T[len];
			var i = 0;
			for (var a = 0; a < arr.Length; a++) {
				var sub = arr[a];
				for (var s = 0; s < sub.Length; s++) {
					result[i++] = sub[s];
				}
			}
			return result;
		}

		public static List<T> ToList<T>(this T[] arr) {
			var list = new List<T>();
			for (var i = 0; i < arr.Length; i++) {
				list.Add(arr[i]);
			}
			return list;
		}

		public static string Inspect<T>(this T[] arr) {
			return "[" + string.Join(", ", arr.Map(i => i.ToString())) + "]";
		}

		public static T[] SortBy<T, TKey>(this T[] arr, Func<T, TKey> convert) {
			return arr.OrderBy(convert).ToArray();
		}

	}

}