using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public interface IFilterCache {
		Delegate Get(string expression);

		void Set(string expression, Delegate lambda);
	}
}
