// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
