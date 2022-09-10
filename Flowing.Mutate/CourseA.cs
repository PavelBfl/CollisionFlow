using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Flowing.Mutate
{
	public struct CourseA : IEquatable<CourseA>
	{
		public CourseA(double v, double a)
		{
			V = v;
			A = a;
		}

		/// <summary>
		/// Скорость
		/// </summary>
		public double V { get; }
		/// <summary>
		/// Ускорение
		/// </summary>
		public double A { get; }

		public CourseA SetV(double v) => new CourseA(v, A);
		public CourseA SetA(double a) => new CourseA(V, a);
		/// <summary>
		/// Сдвинуть значение ускорения на определённое время
		/// </summary>
		/// <param name="time">Значение времени для сдвига</param>
		/// <returns>Изменённое значение направления движения</returns>
		public CourseA Offset(double time) => new CourseA(V + A * time, A);
		/// <summary>
		/// Получить время через которое <see cref="V"/> будет равно значению <paramref name="expectationV"/>
		/// </summary>
		/// <param name="expectationV">Ожидаемое значение скорости</param>
		/// <returns>Значение времени через которое значение <see cref="V"/> сравняется с <paramref name="expectationV"/>, значение времени может быть отрицательным</returns>
		public double GetTime(double expectationV) => (expectationV - V) / A;
		/// <summary>
		/// Изменить значение на определённое время
		/// </summary>
		/// <param name="value">Изменяемое значение</param>
		/// <param name="time">Время на которое необходимо изменить значение</param>
		/// <returns>Изменённое значение</returns>
		public double OffsetValue(double value, double time) => value + V * time + (A * time * time) / 2;

		public static Vector<double> OffsetValue(Vector<double> v, Vector<double> a, Vector<double> value, double time)
		{
			return value + (v * time) + (a * time * time) * new Vector<double>(0.5);
		}

		public bool Equals(CourseA other) => UnitComparer.Position.Equals(V, other.V) && UnitComparer.Position.Equals(A, other.A);
		public override bool Equals(object obj) => obj is CourseA other ? Equals(other) : false;
		public override int GetHashCode() => UnitComparer.Position.GetHashCode(V) ^ UnitComparer.Position.GetHashCode(A);
	}

	public class CourseLimit
	{
		public double V { get; set; }

		public double TotalA => GetEnableLimits().Select(x => x.A).DefaultIfEmpty().Sum();

		public CourseA ToCourseA() => new CourseA(V, TotalA);

		public double GetNextTime()
		{
			return (from limitA in GetEnableLimits()
					let course = new CourseA(V, limitA.A)
					select Math.Min(course.GetTime(limitA.Limit), course.GetTime(-limitA.Limit)))
				   .DefaultIfEmpty(double.PositiveInfinity)
				   .Min();
		}

		public IEnumerable<LimitA> GetEnableLimits()
		{
			var absV = Math.Abs(V);
			return from limitA in LimitsA
				   where limitA.Limit > absV && !double.IsInfinity(limitA.Limit)
				   select limitA;
		}

		private HashSet<LimitA> LimitsAContainer { get; } = new HashSet<LimitA>();

		public ICollection<LimitA> LimitsA => LimitsAContainer;

		public LimitA Add(double a, double limit)
		{
			var item = new LimitA(a, limit);
			LimitsA.Add(item);
			return item;
		}

		public bool Remove(LimitA limit) => LimitsA.Remove(limit);
	}

	public class LimitA
	{
		public LimitA(double a, double limit)
		{
			A = a;
			Limit = limit;
		}

		public double A { get; }
		public double Limit { get; }
	}

	public struct Vector2<T>
	{
		public Vector2(T x, T y)
		{
			X = x;
			Y = y;
		}

		public T X { get; }
		public T Y { get; }

		public bool Equals(Vector2<T> other, IEqualityComparer<T> itemComparer)
		{
			if (itemComparer is null)
			{
				throw new ArgumentNullException(nameof(itemComparer));
			}

			return itemComparer.Equals(X, other.X) && itemComparer.Equals(Y, other.Y);
		}

		public int GetHashCode(IEqualityComparer<T> itemComparer)
		{
			if (itemComparer is null)
			{
				throw new ArgumentNullException(nameof(itemComparer));
			}

			return itemComparer.GetHashCode(X) ^ itemComparer.GetHashCode(Y);
		}
	}

	public static class Vector2Extensions
	{
		public static double GetNextTime(this Vector2<CourseLimit> course)
		{
			return Math.Min(course.X.GetNextTime(), course.Y.GetNextTime());
		}
	}
}
