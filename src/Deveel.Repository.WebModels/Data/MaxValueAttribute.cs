using System;
using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public class MaxValueAttribute : RangeAttribute {
		public MaxValueAttribute(int value)
			: base(int.MinValue, value) {
		}
		public MaxValueAttribute(long value)
			: base(long.MinValue, value) {
		}

		public MaxValueAttribute(float value)
			: base(float.MinValue, value) {
		}

		public MaxValueAttribute(double value)
			: base(double.MinValue, value) {
		}

		public MaxValueAttribute(decimal value)
			: base(typeof(decimal), decimal.MinValue.ToString(), value.ToString()) {
		}
	}
}
