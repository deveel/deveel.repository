namespace Deveel.Data
{
	/// <summary>
	/// The contract used to define an object that 
	/// has an owner.
	/// </summary>
	public interface IHaveOwner<TKey>
	{
		/// <summary>
		/// Gets the identifier of the owner of 
		/// the object.
		/// </summary>
		TKey Owner { get; }

		/// <summary>
		/// Sets the owner of the object, eventually
		/// overriding the current owner.
		/// </summary>
		/// <param name="owner">
		/// The identifier of the new owner.
		/// </param>
		void SetOwner(TKey owner);
	}
}
