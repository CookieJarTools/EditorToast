using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CookieJar.Editor.Toast
{
	public static class ToastManager
	{
		//This is Just for testing
		[MenuItem("Tools/Cookie Jar Tools/Example Notification")]
		private static void ShowNot()
		{
			ShowNotification(new ToastArgs
			{
				Title = "Example 1",
				Message = "An example info notification",
				ToastPosition = ToastPosition.TopRight,
				LifeTime = 9f,
				Severity = ToastSeverity.Info
			});
			
			ShowNotification(new ToastArgs
			{
				Title = "Example 2",
				Message = "An example warning notification",
				ToastPosition = ToastPosition.TopRight,
				LifeTime = 6f,
				Severity = ToastSeverity.Warning
			});
			
			ShowNotification(new ToastArgs
			{
				Title = "Example 3",
				Message = "An example error notification",
				ToastPosition = ToastPosition.TopRight,
				LifeTime = 3f,
				Severity = ToastSeverity.Error
			});
		}
		
		private const float NOTIFICATION_MARGIN = 5f;
		private const double TARGET_FRAME_TIME = 1.0 / 30.0; // 120Hz
		
		private static readonly List<Toast> topLeftNotifications = new();
		private static readonly List<Toast> topRightNotifications = new();
		private static readonly List<Toast> topCenterNotifications = new();
		private static readonly List<Toast> bottomLeftNotifications = new();
		private static readonly List<Toast> bottomRightNotifications = new();
		private static readonly List<Toast> bottomCenterNotifications = new();
		private static readonly Stopwatch stopwatch = new Stopwatch();
		
		private static int notificationCount = 0;
		private static long lastTick;
		private static double accumulator;
		private static AudioClip audioClip;
		private static MethodInfo playClipMethod;

		[InitializeOnLoadMethod]
		private static void Init()
		{
			stopwatch.Start();
			EditorApplication.update += CustomUpdateLoop;
			EditorApplication.QueuePlayerLoopUpdate();
			
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeDomainReload;
			
			audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/NotificationSound.mp3");
		}
		
		private static void OnBeforeDomainReload()
		{
			//close all notifications on domain reload
			EditorApplication.update -= CustomUpdateLoop;
			
			for (var index = topLeftNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topLeftNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = topRightNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topRightNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = topCenterNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topCenterNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomLeftNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomLeftNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomRightNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomRightNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomCenterNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomCenterNotifications[index];
				RemoveNotification(notification);
				notification.Close();
			}
		}

		private static void CustomUpdateLoop()
		{
			if (notificationCount <= 0) return;
			
			var elapsedTicks = stopwatch.ElapsedTicks - lastTick;
			var deltaTime = (double)elapsedTicks / Stopwatch.Frequency;
			lastTick = stopwatch.ElapsedTicks;
			accumulator += deltaTime;

			while (accumulator >= TARGET_FRAME_TIME)
			{
				UpdateNotifications(TARGET_FRAME_TIME);
				accumulator -= TARGET_FRAME_TIME;
			}

			if (!EditorApplication.isPlaying || notificationCount > 0)
			{
				EditorApplication.delayCall += () => 
				{
					EditorWindow.focusedWindow?.Repaint();
					SceneView.RepaintAll();
				};
			}
		}

		private static void UpdateNotifications(double deltaTime)
		{
			CheckNotificationLifetimes();

			UpdateNotificationPositions(deltaTime);
		}

		private static void CheckNotificationLifetimes()
		{
			for (var index = topLeftNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topLeftNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = topRightNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topRightNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = topCenterNotifications.Count - 1; index >= 0; index--)
			{
				var notification = topCenterNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomLeftNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomLeftNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomRightNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomRightNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}

			for (var index = bottomCenterNotifications.Count - 1; index >= 0; index--)
			{
				var notification = bottomCenterNotifications[index];
				if (!notification.IsLifetimeOver()) continue;
				RemoveNotification(notification);
				notification.Close();
			}
		}

		private static void UpdateNotificationPositions(double deltaTime)
		{
			UpdateNotificationPositions(ToastPosition.TopLeft, deltaTime);
			UpdateNotificationPositions(ToastPosition.TopRight, deltaTime);
			UpdateNotificationPositions(ToastPosition.TopCenter, deltaTime);
			UpdateNotificationPositions(ToastPosition.BottomLeft, deltaTime);
			UpdateNotificationPositions(ToastPosition.BottomRight, deltaTime);
			UpdateNotificationPositions(ToastPosition.BottomCenter, deltaTime);
		}
		
		public static void ShowNotification(
			ToastArgs toastArgs,
			Vector2 windowSize = default)
		{
			//Create the window
			var notification = ScriptableObject.CreateInstance<Toast>();
			
			notification.titleContent = new GUIContent($"Notification - {toastArgs.Title}");
			notification.minSize = windowSize == default ? new Vector2(250, 100) : windowSize;
			notification.maxSize = notification.minSize;
			notification.position = new Rect(0, 0, notification.minSize.x, notification.minSize.y);
			notification.ShowPopup();

			notification.OnClose += RemoveNotification;
			
			//Set position
			notification.SetupWindow(new ToastData
			{
				ToastArgs = toastArgs,
				TimeCreated = Time.time,
			});
			
			//Update other notifications
			AddNotification(notification);
			
			PlayNotificationNoise();
		}

		private static void PlayNotificationNoise()
		{
			if (audioClip == null) return;
			
			if (playClipMethod == null)
			{
				var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
				playClipMethod = audioUtilType.GetMethod(
					"PlayPreviewClip",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new[] { typeof(AudioClip), typeof(int), typeof(bool) },
					null
				);
			}
			playClipMethod?.Invoke(null, new object[] { audioClip, 0, false });
		}

		private static void AddNotification(
			Toast toast)
		{
			switch (toast.ToastPosition)
			{
				case ToastPosition.TopLeft:
					topLeftNotifications.Insert(0, toast);
					break;
				case ToastPosition.TopRight:
					topRightNotifications.Insert(0, toast);
					break;
				case ToastPosition.TopCenter:
					topCenterNotifications.Insert(0, toast);
					break;
				case ToastPosition.BottomLeft:
					bottomLeftNotifications.Insert(0, toast);
					break;
				case ToastPosition.BottomRight:
					bottomRightNotifications.Insert(0, toast);
					break;
				case ToastPosition.BottomCenter:
					bottomCenterNotifications.Insert(0, toast);
					break;
				default:
					throw new ArgumentOutOfRangeException(null);
			}

			notificationCount++;
		}

		private static void RemoveNotification(
			Toast toast)
		{
			toast.OnClose -= RemoveNotification;
			switch (toast.ToastPosition)
			{
				case ToastPosition.TopLeft:
					topLeftNotifications.Remove(toast);
					break;
				case ToastPosition.TopRight:
					topRightNotifications.Remove(toast);
					break;
				case ToastPosition.TopCenter:
					topCenterNotifications.Remove(toast);
					break;
				case ToastPosition.BottomLeft:
					bottomLeftNotifications.Remove(toast);
					break;
				case ToastPosition.BottomRight:
					bottomRightNotifications.Remove(toast);
					break;
				case ToastPosition.BottomCenter:
					bottomCenterNotifications.Remove(toast);
					break;
				default:
					throw new ArgumentOutOfRangeException(null);
			}

			notificationCount--;
		}

		private static void UpdateNotificationPositions(ToastPosition toastPosition, double deltaTime)
		{
			var currentHeightOffset = 0f;
			switch (toastPosition)
			{
				case ToastPosition.TopLeft:
					foreach (var currentNotification in topLeftNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset += currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				case ToastPosition.TopRight:
					foreach (var currentNotification in topRightNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset += currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				case ToastPosition.TopCenter:
					foreach (var currentNotification in topCenterNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset += currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				case ToastPosition.BottomLeft:
					foreach (var currentNotification in bottomLeftNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset -= currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				case ToastPosition.BottomRight:
					foreach (var currentNotification in bottomRightNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset -= currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				case ToastPosition.BottomCenter:
					foreach (var currentNotification in bottomCenterNotifications)
					{
						var positionRect = currentNotification.GetEditorWindowPosition(toastPosition);
						currentNotification.UpdatePosition(positionRect, currentHeightOffset, deltaTime);
						currentHeightOffset -= currentNotification.GetHeight() + NOTIFICATION_MARGIN;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(toastPosition), toastPosition, null);
			}
		}
	}
}