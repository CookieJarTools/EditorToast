namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// How much the toast matters, which decides the colour of its mark and its stripe.
	/// <para>
	/// Error is for something that failed and that the reader can do nothing about from here. A
	/// failure they must act on belongs in the console or in front of them, not in something that
	/// takes itself away.
	/// </para>
	/// </summary>
	public enum ToastSeverity
	{
		Info,
		Success,
		Warning,
		Error
	}
}
