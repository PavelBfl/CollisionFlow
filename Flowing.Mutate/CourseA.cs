using System;
using System.Collections;

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

		public bool Equals(CourseA other) => V == other.V && A == other.A;
		public override bool Equals(object obj) => obj is CourseA other ? Equals(other) : false;
		public override int GetHashCode() => V.GetHashCode() ^ A.GetHashCode();
	}
}
