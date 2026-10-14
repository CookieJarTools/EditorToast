using System;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// One thing the reader can do about a toast, such as undoing what just happened or opening
	/// what it is talking about.
	/// <para>
	/// A toast carrying an action never times out. An Undo that disappears while somebody is
	/// reading the sentence above it is worse than no Undo at all, because they stop trusting it.
	/// </para>
	/// </summary>
	public sealed class ToastAction
	{
		public ToastAction(string label, Action perform, bool closesToast = true)
		{
			Label = label;
			Perform = perform;
			ClosesToast = closesToast;
		}

		/// <summary>What the button says. Two or three words, naming the result: "Undo", "Show me".</summary>
		public string Label { get; }

		/// <summary>What pressing it does.</summary>
		public Action Perform { get; }

		/// <summary>Whether the toast goes away once it has been pressed. Usually it should.</summary>
		public bool ClosesToast { get; }
	}
}
