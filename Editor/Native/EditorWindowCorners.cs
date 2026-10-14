using System;
using System.Runtime.InteropServices;
using UnityEditor;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// Rounds the corners of a popup window on Windows 11, so the card's own radius is not sitting
	/// inside a square frame.
	/// <para>
	/// Everywhere else this does nothing at all. The call lives behind a platform check because
	/// dwmapi exists only on Windows, and a DllImport of a missing library throws the moment it is
	/// reached rather than when it is declared.
	/// </para>
	/// </summary>
	internal static class EditorWindowCorners
	{
		private const int CornerPreference = 33;
		private const int RoundedPreference = 2;

		public static void Round(EditorWindow window)
		{
#if UNITY_EDITOR_WIN
			RoundOnWindows(window);
#endif
		}

#if UNITY_EDITOR_WIN
		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

		private static void RoundOnWindows(EditorWindow window)
		{
			// GetActiveWindow returns whatever has focus, which is the toast only because it was
			// just shown. Anything that steals focus first and this rounds the wrong window.
			var handle = GetActiveWindow();
			if (handle == IntPtr.Zero)
			{
				return;
			}

			var preference = RoundedPreference;
			DwmSetWindowAttribute(handle, CornerPreference, ref preference, sizeof(int));
		}
#endif
	}
}
