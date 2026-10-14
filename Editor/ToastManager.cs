using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// Raises toasts and keeps them stacked in their corners.
	/// <para>
	/// A toast is worth raising when something finished while the reader was looking somewhere
	/// else, and it is worth more when it carries the one thing they would want to do next. It is
	/// the wrong shape for anything they have to read or have to act on, because it leaves on its
	/// own.
	/// </para>
	/// </summary>
	public static class ToastManager
	{
		private const float Margin = 5f;
		private const double TargetFrameTime = 1.0 / 30.0;

		private static readonly Dictionary<ToastPosition, List<Toast>> stacks = new Dictionary<ToastPosition, List<Toast>>();

		private static AudioClip sound;
		private static MethodInfo playClip;
		private static double lastUpdate;

		/// <summary>
		/// A clip to play when a toast arrives. Nothing is played unless one is set, because a
		/// noise on every notification is the thing that makes somebody turn a tool off.
		/// </summary>
		public static AudioClip Sound
		{
			get => sound;
			set => sound = value;
		}

		/// <summary>
		/// Shows a toast, or adds to one already saying the same thing.
		/// <para>
		/// A toast carrying an action is held until it is dismissed, whatever lifetime was asked
		/// for. An Undo that takes itself away while it is being read is worse than none, because
		/// the next one will not be trusted either.
		/// </para>
		/// </summary>
		public static void ShowNotification(ToastArgs toastArgs, Vector2 windowSize = default)
		{
			EnsureUpdateHook();

			if (toastArgs.Action != null)
			{
				toastArgs.NoTimeOut = true;
			}

			var existing = Find(Toast.Grouping(toastArgs), toastArgs.ToastPosition);
			if (existing != null)
			{
				existing.Repeat();
				return;
			}

			var toast = ScriptableObject.CreateInstance<Toast>();
			toast.titleContent = new GUIContent("Notification - " + toastArgs.Title);
			toast.minSize = windowSize == default ? new Vector2(320, 64) : windowSize;
			toast.maxSize = toast.minSize;
			toast.position = new Rect(0, 0, toast.minSize.x, toast.minSize.y);
			toast.ShowPopup();

			toast.OnClose += RemoveNotification;
			toast.SetupWindow(new ToastData
			{
				ToastArgs = toastArgs,
				TimeCreated = EditorApplication.timeSinceStartup,
			});

			AddNotification(toast);
			PlayNotificationNoise();
		}

		/// <summary>Takes down every toast, whether it has an action on it or not.</summary>
		public static void DismissAll()
		{
			foreach (var stack in stacks.Values)
			{
				for (var index = stack.Count - 1; index >= 0; index--)
				{
					stack[index].Close();
				}
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeDomainReload;
			EnsureUpdateHook();
		}

		private static List<Toast> Stack(ToastPosition position)
		{
			if (!stacks.TryGetValue(position, out var stack))
			{
				stack = new List<Toast>();
				stacks[position] = stack;
			}

			return stack;
		}

		/// <summary>A toast already up in that corner saying the same thing, or null.</summary>
		private static Toast Find(string key, ToastPosition position)
		{
			foreach (var toast in Stack(position))
			{
				if (toast != null && toast.Key == key)
				{
					return toast;
				}
			}

			return null;
		}

		private static void EnsureUpdateHook()
		{
			EditorApplication.update -= CustomUpdateLoop;
			EditorApplication.update += CustomUpdateLoop;
		}

		private static void OnBeforeDomainReload()
		{
			EditorApplication.update -= CustomUpdateLoop;
			DismissAll();
		}

		private static void CustomUpdateLoop()
		{
			var now = EditorApplication.timeSinceStartup;
			var delta = now - lastUpdate;
			if (delta < TargetFrameTime)
			{
				return;
			}

			lastUpdate = now;
			CloseFinished();

			foreach (var position in stacks.Keys)
			{
				UpdateNotificationPositions(position, delta);
			}
		}

		private static void CloseFinished()
		{
			foreach (var stack in stacks.Values)
			{
				for (var index = stack.Count - 1; index >= 0; index--)
				{
					var toast = stack[index];
					if (toast == null)
					{
						stack.RemoveAt(index);
						continue;
					}

					if (toast.IsLifetimeOver())
					{
						toast.Close();
					}
				}
			}
		}

		private static void UpdateNotificationPositions(ToastPosition position, double deltaTime)
		{
			var stack = Stack(position);
			var offset = 0f;
			foreach (var toast in stack)
			{
				if (toast == null)
				{
					continue;
				}

				var anchor = toast.GetEditorWindowPosition(position);
				var direction = Downwards(position) ? 1f : -1f;
				toast.UpdatePosition(anchor, offset * direction, deltaTime);
				offset += toast.GetHeight() + Margin;
			}
		}

		private static bool Downwards(ToastPosition position)
		{
			return position == ToastPosition.TopLeft
				|| position == ToastPosition.TopRight
				|| position == ToastPosition.TopCenter;
		}

		private static void AddNotification(Toast toast)
		{
			Stack(toast.ToastPosition).Insert(0, toast);
		}

		private static void RemoveNotification(Toast toast)
		{
			Stack(toast.ToastPosition).Remove(toast);
		}

		private static void PlayNotificationNoise()
		{
			if (sound == null)
			{
				return;
			}

			if (playClip == null)
			{
				var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
				playClip = audioUtil?.GetMethod(
					"PlayPreviewClip",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new[] { typeof(AudioClip), typeof(int), typeof(bool) },
					null);
			}

			playClip?.Invoke(null, new object[] { sound, 0, false });
		}
	}
}
