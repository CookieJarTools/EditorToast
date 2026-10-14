namespace CookieJar.Editor.Toast
{
	/// <summary>A toast and the clock it is running against.</summary>
	public struct ToastData
	{
		/// <summary>Editor time it started counting from, which moves on when it is paused.</summary>
		public double TimeCreated { get; set; }

		public ToastArgs ToastArgs { get; set; }
	}
}
