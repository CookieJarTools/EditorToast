using System;
using UnityEngine.UIElements;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// What a toast says and how it behaves.
	/// <para>
	/// A toast is for telling somebody that a thing has already happened while they were looking
	/// somewhere else, and at most giving them one thing to do about it. Anything they have to
	/// read, or have to act on, wants the console or a window instead, because this goes away on
	/// its own.
	/// </para>
	/// </summary>
	public struct ToastArgs
	{
		/// <summary>The headline, in a few words. "Build finished", not "Notification".</summary>
		public string Title { get; set; }

		/// <summary>One sentence under the title. Leave it empty when the title says enough.</summary>
		public string Message { get; set; }

		public ToastPosition ToastPosition { get; set; }

		/// <summary>
		/// Seconds on screen. Zero takes the default, which is long enough to read what was given.
		/// Ignored when there is an action, since those wait to be dismissed.
		/// </summary>
		public float LifeTime { get; set; }

		/// <summary>Stays until it is dismissed. Set for you when there is an action.</summary>
		public bool NoTimeOut { get; set; }

		public ToastSeverity Severity { get; set; }

		/// <summary>
		/// The one thing to do about it, such as undoing what happened or showing what it names.
		/// A toast with an action does not time out.
		/// </summary>
		public ToastAction Action { get; set; }

		/// <summary>
		/// Groups repeats. Two toasts raised with the same key while the first is still up become
		/// one toast wearing a count, rather than a pile saying the same thing. Falls back to the
		/// title and message when it is left empty.
		/// </summary>
		public string Key { get; set; }

		/// <summary>Anything else to put in the body, under the message.</summary>
		public Func<VisualElement> CustomContent { get; set; }
	}
}
