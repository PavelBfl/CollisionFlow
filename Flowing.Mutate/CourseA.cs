using System;
using System.Collections;
using System.Linq;
using System.Numerics;

namespace Flowing.Mutate
{
	public struct CourseA : IEquatable<CourseA>
	{
		public static CourseA Zero { get; } = new CourseA(0, 0);

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
}
