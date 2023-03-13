using System;

namespace Deveel.Data {
    public interface IMultiTenantRepository : IRepository {
        string? TenantId { get; }
    }
}
