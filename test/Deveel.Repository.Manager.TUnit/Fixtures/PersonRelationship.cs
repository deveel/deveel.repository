namespace Deveel.Data {
    public sealed class PersonRelationship : IRelationship {
        public string Type { get; set; } = null!;

        public string FullName { get; set; } = null!;
    }
}

