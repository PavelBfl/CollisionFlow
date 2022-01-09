using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public class CollisionSpace : CollisionDispatcher
	{
		public CollisionSpace()
		{
			Expectations = new SortedList<double, List<IFlowEvent>>(Comparer);
		}

		public IComparer<double> Comparer { get; } = Comparer<double>.Default;
		public double Time { get; private set; } = 0;
		private SortedList<double, List<IFlowEvent>> Expectations { get; }

		public void AddExpectationOffset(IFlowEvent flowEvent, double offset)
		{
			var time = Time + offset;
			if (!Expectations.TryGetValue(time, out var events))
			{
				events = new List<IFlowEvent>();
				Expectations.Add(time, events);
			}

			events.Add(flowEvent);
		}
		public void Remove(IFlowEvent flowEvent)
		{
			if (flowEvent is null)
			{
				throw new ArgumentNullException(nameof(flowEvent));
			}

			foreach (var expection in Expectations.ToArray())
			{
				expection.Value.Remove(flowEvent);
				if (!expection.Value.Any())
				{
					Expectations.Remove(expection.Key);
				}
			}
		}

		protected override double Planning(double offset)
		{
			if (Expectations.Any())
			{
				var key = Expectations.Keys[0];
				if (Comparer.Compare(key, Time + offset) < 0)
				{
					var expectation = Expectations.Values[0];
					Expectations.RemoveAt(0);

					foreach (var flowEvent in expectation)
					{
						flowEvent.Handle();
					}

					var step = key - Time;
					Time = key;
					return step;
				}
			}

			Time += offset;
			return offset;
		}
	}
}
