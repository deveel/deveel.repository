using Deveel.Data.Entities;
using Deveel.Data.WebModels;

using Riok.Mapperly.Abstractions;

namespace Deveel.Data.Mapping {
	[Mapper]
	public static partial class UserMapper {
		[MapperIgnoreSource(nameof(UserModel.Id))]
		public static partial UserEntity ToEntity(this UserModel model);

		public static partial UserModel ToModel(this UserEntity entity);
	}
}
