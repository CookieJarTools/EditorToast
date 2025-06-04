using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CookieJar.Editor.Toast
{
	internal class Toast : EditorWindow
	{
		private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

		private enum DWM_WINDOW_CORNER_PREFERENCE
		{
			DWMWCP_DEFAULT = 0,
			DWMWCP_DONOTROUND = 1,
			DWMWCP_ROUND = 2,
			DWMWCP_ROUNDSMALL = 3
		}
		
		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref DWM_WINDOW_CORNER_PREFERENCE pref, int attrLen);
		
		private const float POSITION_SPEED = 10f;
		private const float POSITION_THRESHOLD = 0.1f;
		private const float ROOT_CORNER_RADIUS = 9.2f;
		private const float ROOT_BORDER_WIDTH = 2f;
		
		public ToastData Data { get; set; }
		public ToastPosition ToastPosition => ToastArgs.ToastPosition;
		public ToastArgs ToastArgs { get; set; }

		public event Action<Toast> OnClose;

		private Rect currentPosition;
		
		private VisualElement titleBar;
		private Label titleLabel;
		private Label messageLabel;
		private Button closeButton;
		private VisualElement contentContainer;
		private ProgressBar lifetimeBar;
		
		public void SetupWindow(ToastData toastData)
		{
			Data = toastData;
			ToastArgs = toastData.ToastArgs;
			position = this.GetEditorWindowPosition(ToastArgs.ToastPosition);
			currentPosition = position;

			titleLabel.text = ToastArgs.Title;
			messageLabel.text = ToastArgs.Message;
			contentContainer.Add(ToastArgs.CustomContent?.Invoke());
			
			var backgroundColor = ToastArgs.Severity switch
			{
				ToastSeverity.Info => new Color(0.27f, 0.38f, 0.49f),
				ToastSeverity.Warning => new Color(0.69f, 0.5f, 0.02f),
				_ => new Color(0.49f, 0f, 0f)
			};

			lifetimeBar.lowValue = 0;
			var dataToastArgs = Data.ToastArgs;
			lifetimeBar.value = lifetimeBar.highValue = dataToastArgs.NoTimeOut ? 1f : dataToastArgs.LifeTime;
			
			var lifetimeBarContainer = lifetimeBar.Children().First();
			var lifetimeBarBackground = lifetimeBarContainer.Children().First();
			var lifetimeBarFill = lifetimeBarBackground.Children().First();
			if (lifetimeBarFill != null)
			{
				lifetimeBarFill.style.backgroundColor = backgroundColor;
			}
			
			titleBar.style.backgroundColor = backgroundColor;
			
			closeButton.RegisterCallback<PointerEnterEvent>(evt => {
				closeButton.style.backgroundColor = backgroundColor;
			});

			closeButton.RegisterCallback<PointerLeaveEvent>(evt => {
				closeButton.style.backgroundColor = new Color(0.31f, 0.31f, 0.31f);
			});

			EnableRoundedCorners();
		}
		
		private void EnableRoundedCorners()
		{
			IntPtr hwnd = GetActiveWindow();
			if (hwnd != IntPtr.Zero)
			{
				DWM_WINDOW_CORNER_PREFERENCE pref = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
				DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));
			}
			else
			{
				Debug.LogWarning("Failed to get window handle.");
			}
		}

		private void CreateGUI()
		{
			rootVisualElement.style.borderTopWidth = ROOT_BORDER_WIDTH;
			rootVisualElement.style.borderBottomWidth = ROOT_BORDER_WIDTH;
			rootVisualElement.style.borderLeftWidth = ROOT_BORDER_WIDTH;
			rootVisualElement.style.borderRightWidth = ROOT_BORDER_WIDTH;
			rootVisualElement.style.borderTopColor = Color.black;
			rootVisualElement.style.borderBottomColor = Color.black;
			rootVisualElement.style.borderLeftColor = Color.black;
			rootVisualElement.style.borderRightColor = Color.black;
			rootVisualElement.style.borderTopLeftRadius = ROOT_CORNER_RADIUS;
			rootVisualElement.style.borderTopRightRadius = ROOT_CORNER_RADIUS;
			rootVisualElement.style.borderBottomLeftRadius = ROOT_CORNER_RADIUS;
			rootVisualElement.style.borderBottomRightRadius = ROOT_CORNER_RADIUS;
			
			titleBar = new VisualElement
			{
				style =
				{
					height = 20f,
					minHeight = 20f,
					width = new StyleLength(Length.Percent(100)),
					maxWidth = new StyleLength(Length.Percent(100)),
					flexDirection = FlexDirection.Row,
					borderBottomWidth = 1f,
					borderBottomColor = Color.black
				}
			};
			rootVisualElement.Add(titleBar);
			{
				titleLabel = new Label()
				{
					style =
					{
						flexGrow = 1f,
						unityFontStyleAndWeight = FontStyle.Bold,
						fontSize = 13f,
						unityTextAlign = TextAnchor.MiddleLeft
					}
				};
				titleBar.Add(titleLabel);
				
				closeButton = new Button(Close)
				{
					text = "X",
					style =
					{
						flexGrow = 0f,
						width = 20f,
						minWidth = 20f,
						borderBottomWidth = 0f,
						borderLeftWidth = 1f,
						borderRightWidth = 0f,
						borderTopWidth = 0f,
						borderTopLeftRadius = 0f,
						borderTopRightRadius = 0f,
						borderBottomRightRadius = 0f,
						borderBottomLeftRadius = 0f,
						marginRight = 0f,
						marginLeft = 0f,
						marginTop = 0f,
						marginBottom = 0f
					}
				};
				titleBar.Add(closeButton);
			}

			lifetimeBar = new ProgressBar
			{
				style =
				{
					marginBottom = 0f,
					marginLeft = 0f,
					marginRight = 0f,
					marginTop = 0f,
					height = 6f,
					minHeight = 6f,
					maxHeight = 6f
				}
			};
			rootVisualElement.Add(lifetimeBar);
			
			var messageContainer = new VisualElement();
			rootVisualElement.Add(messageContainer);
			{
				messageLabel = new Label
				{
					style =
					{
						marginTop = 2f,
						marginBottom = 2f,
						marginLeft = 2f,
						marginRight = 2f,
					}
				};
				messageContainer.Add(messageLabel);
			}
			
			contentContainer = new VisualElement();
			rootVisualElement.Add(contentContainer);
		}
		
		public float GetHeight() => position.height;

		public void UpdatePosition(Rect positionRect, float currentHeightOffset, double deltaTime)
		{
			var targetPosition = new Rect(positionRect.x, positionRect.y + currentHeightOffset, positionRect.width, positionRect.height);
			
			if (Mathf.Abs(currentPosition.y - targetPosition.y) < POSITION_THRESHOLD)
			{
				currentPosition.y = targetPosition.y;
				position = currentPosition;
			}
			else
			{
				var newY = Mathf.Lerp(
					currentPosition.y, 
					targetPosition.y, 
					(float)(POSITION_SPEED * deltaTime)
				);
				currentPosition.y = newY;
				currentPosition.x = targetPosition.x;
			
				position = currentPosition;
			}
		}

		public bool IsLifetimeOver()
		{
			if (ToastArgs.NoTimeOut) return false;
			
			var currentLifeTime = Time.time - Data.TimeCreated;
			lifetimeBar.value = ToastArgs.LifeTime - currentLifeTime;
			return currentLifeTime > ToastArgs.LifeTime;
		}

		private void OnDestroy()
		{
			OnClose?.Invoke(this);
		}
	}
}