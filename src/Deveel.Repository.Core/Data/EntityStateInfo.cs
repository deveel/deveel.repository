using System;

namespace Deveel.Data {
    public readonly struct EntityStateInfo<TStatus> {
        public EntityStateInfo(TStatus status, string actorId, DateTimeOffset? timeStamp = null, IDictionary<string, object>? data = null) : this() {
            Status = status;
            ActorId = actorId;
            TimeStamp = timeStamp ?? DateTimeOffset.UtcNow;
            Data = data;
        }

        public string ActorId { get; }

        public IDictionary<string, object>? Data { get; }

        public DateTimeOffset TimeStamp { get; }

        public TStatus Status { get; }
    }
}
