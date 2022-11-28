using System;

using MongoDB.Bson;

namespace Deveel.Data {
	public static class ObjectIdExtensions {
		public static string ToEntityId(this ObjectId objectId) 
			=> objectId == ObjectId.Empty ? string.Empty : objectId.ToString();
	}
}
