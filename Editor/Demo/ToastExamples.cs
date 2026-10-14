using UnityEditor;
using UnityEngine;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// The four shapes a toast is actually good for, as something you can press and look at.
	/// <para>
	/// These are here to be read as much as run. Each one is a case where a toast beats both the
	/// console, which is missed, and a dialog, which interrupts.
	/// </para>
	/// </summary>
	internal static class ToastExamples
	{
		/// <summary>
		/// The main one. Something long finished while the reader was looking somewhere else, and
		/// the toast is how they find out without watching a progress bar.
		/// </summary>
		[MenuItem("Window/CookieJarTools/Editor Toast/Something finished")]
		private static void Finished()
		{
			ToastManager.ShowNotification(new ToastArgs
			{
				Title = "Build finished",
				Message = "Windows 64 bit, 2 minutes 14 seconds.",
				Severity = ToastSeverity.Success,
				ToastPosition = ToastPosition.BottomRight,
			});
		}

		/// <summary>
		/// The one that earns a toast its place: a reversible thing just happened, and the way
		/// back is right there. This one never times out, so the way back cannot be missed.
		/// </summary>
		[MenuItem("Window/CookieJarTools/Editor Toast/Something undoable")]
		private static void Undoable()
		{
			ToastManager.ShowNotification(new ToastArgs
			{
				Title = "12 unused assets removed",
				Message = "They were not referenced by any scene or prefab.",
				Severity = ToastSeverity.Info,
				ToastPosition = ToastPosition.BottomRight,
				Action = new ToastAction("Undo", () => Debug.Log("Put them back.")),
			});
		}

		/// <summary>
		/// Something went wrong that the reader cannot do anything about from here, so it says so
		/// and offers to take them to where they can.
		/// </summary>
		[MenuItem("Window/CookieJarTools/Editor Toast/Something failed")]
		private static void Failed()
		{
			ToastManager.ShowNotification(new ToastArgs
			{
				Title = "Import failed",
				Message = "oak_bark.png could not be read.",
				Severity = ToastSeverity.Error,
				ToastPosition = ToastPosition.BottomRight,
				Action = new ToastAction("Show me", () => Debug.Log("Ping the asset.")),
			});
		}

		/// <summary>
		/// The same thing happening over and over becomes one toast wearing a count, rather than a
		/// pile of identical ones burying everything else.
		/// </summary>
		[MenuItem("Window/CookieJarTools/Editor Toast/The same thing five times")]
		private static void Repeated()
		{
			for (var i = 0; i < 5; i++)
			{
				ToastManager.ShowNotification(new ToastArgs
				{
					Title = "Shader compiled",
					Message = "Water/Surface, 1 variant.",
					Severity = ToastSeverity.Info,
					ToastPosition = ToastPosition.BottomRight,
					Key = "shader-compiled",
				});
			}
		}

		[MenuItem("Window/CookieJarTools/Editor Toast/Dismiss all")]
		private static void DismissAll()
		{
			ToastManager.DismissAll();
		}
	}
}
