using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	[CollectionDefinition(nameof(SqlConnectionCollection))]
	public class SqlConnectionCollection : ICollectionFixture<SqlTestConnection> {
	}
}
