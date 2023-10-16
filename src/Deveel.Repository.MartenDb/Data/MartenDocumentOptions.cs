namespace Deveel.Data {
	/// <summary>
	/// Provides a set of options that can be used to configure
	/// the behavior of a <see cref="MartenDocumentRepository{TDocument}"/>.
	/// </summary>
	public class MartenDocumentOptions {
		/// <summary>
		/// Gets or sets a flag indicating if the repository is read-only.
		/// </summary>
		public bool ReadOnly { get; set; } = false;
	}
}
