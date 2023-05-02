using Deveel.Data.Entities;
using Deveel.Data.WebModels;

using Riok.Mapperly.Abstractions;

namespace Deveel.Data.Mapping {
    [Mapper]
    public static partial class PersonMapper {
        public static partial PersonModel ToModel(this PersonEntity person);

		[MapperIgnoreSource(nameof(PersonModel.Id))]
		public static partial PersonEntity ToEntity(this PersonModel model);
    }
}
