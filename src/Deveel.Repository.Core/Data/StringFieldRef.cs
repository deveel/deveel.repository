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

namespace Deveel.Data
{
    /// <summary>
    /// References a field of an entity by its name
    /// </summary>
    public sealed class StringFieldRef : IFieldRef
    {
        /// <summary>
        /// Constructs the reference with the name of the field
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the field is null or empty.
        /// </exception>
        public StringFieldRef(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or whitespace.", nameof(fieldName));

            FieldName = fieldName;
        }

        /// <summary>
        /// Gets the name of the field referenced
        /// </summary>
        public string FieldName { get; }
    }
}
