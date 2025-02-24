using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CookieJar.Editor.Toast
{
	internal class Toast : EditorWindow
	{
		private const float POSITION_SPEED = 10f;
		private const float POSITION_THRESHOLD = 0.1f;
		
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
		
		public void SetupWindow(ToastData toastData)
		{
			Data = toastData;
			ToastArgs = toastData.ToastArgs;
			position = this.GetEditorWindowPosition(ToastArgs.ToastPosition);
			currentPosition = position;

			titleLabel.text = ToastArgs.Title;
			messageLabel.text = ToastArgs.Message;
			contentContainer.Add(ToastArgs.CustomContent);
			
			var backgroundColor = ToastArgs.Severity switch
			{
				ToastSeverity.Info => new Color(0.27f, 0.38f, 0.49f),
				ToastSeverity.Warning => new Color(0.69f, 0.5f, 0.02f),
				_ => new Color(0.49f, 0f, 0f)
			};
			titleBar.style.backgroundColor = backgroundColor;
			
			closeButton.RegisterCallback<PointerEnterEvent>(evt => {
				closeButton.style.backgroundColor = backgroundColor;
			});

			closeButton.RegisterCallback<PointerLeaveEvent>(evt => {
				closeButton.style.backgroundColor = new Color(0.31f, 0.31f, 0.31f);
			});
		}

		private void CreateGUI()
		{
			rootVisualElement.style.borderTopWidth = 2f;
			rootVisualElement.style.borderBottomWidth = 2f;
			rootVisualElement.style.borderLeftWidth = 2f;
			rootVisualElement.style.borderRightWidth = 2f;
			rootVisualElement.style.borderTopColor = Color.black;
			rootVisualElement.style.borderBottomColor = Color.black;
			rootVisualElement.style.borderLeftColor = Color.black;
			rootVisualElement.style.borderRightColor = Color.black;
			
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
			return Time.time - Data.TimeCreated > ToastArgs.LifeTime;
		}

		private void OnDestroy()
		{
			OnClose?.Invoke(this);
		}
	}
}