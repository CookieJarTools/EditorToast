using System;
using UnityEditor;
using UnityEngine;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// Works out where in the editor a toast should sit.
	/// <para>
	/// The main window's rectangle comes from Unity rather than from user32, so this holds up on
	/// macOS and Linux as well. The old call into user32 worked on Windows and threw everywhere
	/// else, which made the package Windows only without saying so.
	/// </para>
	/// </summary>
	public static class ToastHelpers
	{
		private const float EdgeMargin = 10f;
		private const float TopMargin = 80f;
		private const float BottomMargin = 30f;

		public static Rect GetEditorWindowPosition(this EditorWindow window, ToastPosition corner)
		{
			var size = window.position.size;
			var editor = EditorGUIUtility.GetMainWindowPosition();
			var place = CalculatePosition(corner, editor, size);
			return new Rect(place.x, place.y, size.x, size.y);
		}

		private static Vector2 CalculatePosition(ToastPosition corner, Rect editor, Vector2 size)
		{
			return corner switch
			{
				ToastPosition.TopLeft => new Vector2(
					editor.x + EdgeMargin,
					editor.y + TopMargin),
				ToastPosition.TopRight => new Vector2(
					editor.xMax - size.x - EdgeMargin,
					editor.y + TopMargin),
				ToastPosition.TopCenter => new Vector2(
					editor.center.x - size.x / 2f,
					editor.y + TopMargin),
				ToastPosition.BottomLeft => new Vector2(
					editor.x + EdgeMargin,
					editor.yMax - size.y - BottomMargin),
				ToastPosition.BottomRight => new Vector2(
					editor.xMax - size.x - EdgeMargin,
					editor.yMax - size.y - BottomMargin),
				ToastPosition.BottomCenter => new Vector2(
					editor.center.x - size.x / 2f,
					editor.yMax - size.y - BottomMargin),
				_ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
			};
		}
	}
}
