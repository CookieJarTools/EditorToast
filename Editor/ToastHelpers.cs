using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace CookieJar.Editor.Toast
{
	public static class ToastHelpers
	{
		//															right, bottom, top, left
		private static readonly Vector4 WINDOW_PADDING = new Vector4(10f, 30f, 80f, 10f);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

		[StructLayout(LayoutKind.Sequential)]
		private struct Rect
		{
			public int Left { get; set; }
			public int Top { get; set; }
			public int Right { get; set; }
			public int Bottom { get; set; }

			public UnityEngine.Rect ToUnityRect()
			{
				return new UnityEngine.Rect(Left, Top, Right - Left, Bottom - Top);
			}
		}

		public static UnityEngine.Rect GetEditorWindowPosition(this EditorWindow window, ToastPosition corner)
		{
			Vector2 windowSize = window.position.size;

			Rect editorRect = new Rect();
			GetWindowRect(Process.GetCurrentProcess().MainWindowHandle, ref editorRect);
			UnityEngine.Rect unityEditorRect = editorRect.ToUnityRect();

			Vector2 position = CalculatePosition(corner, unityEditorRect, windowSize);
			return new UnityEngine.Rect(position.x, position.y, windowSize.x, windowSize.y);
		}

		private static Vector2 CalculatePosition(ToastPosition corner, UnityEngine.Rect editorRect, Vector2 windowSize)
		{
			return corner switch
			{
				ToastPosition.TopLeft => new Vector2(
					editorRect.x + WINDOW_PADDING.w,
					editorRect.y + WINDOW_PADDING.z),
				ToastPosition.TopRight => new Vector2(
					editorRect.xMax - windowSize.x - WINDOW_PADDING.x,
					editorRect.y + WINDOW_PADDING.z),
				ToastPosition.TopCenter => new Vector2(
					editorRect.center.x - windowSize.x / 2 - WINDOW_PADDING.x,
					editorRect.y + WINDOW_PADDING.z),
				ToastPosition.BottomLeft => new Vector2(
					editorRect.x + WINDOW_PADDING.w,
					editorRect.yMax - windowSize.y - WINDOW_PADDING.y),
				ToastPosition.BottomRight => new Vector2(
					editorRect.xMax - windowSize.x - WINDOW_PADDING.x,
					editorRect.yMax - windowSize.y - WINDOW_PADDING.y),
				ToastPosition.BottomCenter => new Vector2(
					editorRect.center.x - windowSize.x / 2 - WINDOW_PADDING.x,
					editorRect.yMax - windowSize.y - WINDOW_PADDING.y),
				_ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
			};
		}
	}
}