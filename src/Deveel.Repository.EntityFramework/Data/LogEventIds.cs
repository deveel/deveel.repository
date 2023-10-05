using System;

namespace Deveel.Data {
    static class LogEventIds {
        public const int CreatingEntity = 10000;
        public const int UpdatingEntity = 10001;
        public const int DeletingEntity = 10002;
        public const int FindingById = 10010;

        public const int EntityCreated = 10020;
        public const int EntityUpdated = 10021;
        public const int EntityDeleted = 10022;
        public const int EntityFoundById = 10030;
        public const int EntityNotFoundById = 10031;

        public const int EntityNotFound = -1001;
        public const int EntityNotDeleted = -1002;
        public const int EntityNotUpdated = -1003;
        public const int UnknownError = -1000;
    }
}
