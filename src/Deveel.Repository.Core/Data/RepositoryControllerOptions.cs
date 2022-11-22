﻿using System;

namespace Deveel.Data {
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