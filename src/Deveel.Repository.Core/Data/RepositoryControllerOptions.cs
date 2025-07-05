// Copyright 2023-2025 Antonello Provenzano
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
	/// <summary>
	/// Provides a set of options that can be used to control
	/// the behavior of a <see cref="IRepositoryController"/>.
	/// </summary>
	public class RepositoryControllerOptions {
		/// <summary>
		/// Instructs the controller to delete pre-existing repositories,
		/// or otherwise fail
		/// </summary>
		public bool DeleteIfExists { get; set; } = true;

		/// <summary>
		/// Skips any operation if the repository is not controllable
		/// </summary>
		public bool IgnoreNotControllable { get; set; } = true;

		/// <summary>
		/// Instructs the controller to not create a
		/// repository if already exists
		/// </summary>
		public bool DontCreateExisting { get; set; } = true;
	}
}
