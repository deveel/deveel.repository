using Deveel.Data;

namespace Deveel.Repository.TestApi.Data {
    public class PersonEntity : IDataEntity {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? BirthDate { get; set; }
    }
}
