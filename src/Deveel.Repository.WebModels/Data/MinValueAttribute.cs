using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public class MinValueAttribute : RangeAttribute {

		public MinValueAttribute(int value)
			: base(value, int.MaxValue) {
		}

		public MinValueAttribute(long value)
			: base(value, long.MaxValue) {
		}

		public MinValueAttribute(float value)
			: base(value, float.MaxValue) {
		}

		public MinValueAttribute(double value)
			: base(value, double.MaxValue) {
		}

		public MinValueAttribute(decimal value)
			: base(typeof(decimal), value.ToString(), decimal.MaxValue.ToString()) {
		}
	}
}
