using System;

namespace Deveel.Data {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MultiTenantAttribute : Attribute {
    }
}
