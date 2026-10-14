using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CookieJar.Editor.Toast
{
	/// <summary>
	/// One toast: a borderless popup window holding a card. The card is styled entirely from
	/// EditorToast.uss so the look lives in one file rather than in a hundred inline styles.
	/// </summary>
	internal class Toast : EditorWindow
	{
		private const float PositionSpeed = 10f;
		private const float PositionThreshold = 0.1f;
		private const float MinimumLifeTime = 4f;
		private const float SecondsPerWord = 0.3f;
		private const float CardBorder = 1f;

		public event Action<Toast> OnClose;

		private Rect currentPosition;
		private bool paused;
		private int count = 1;
		private double lastTick;

		private VisualElement card;
		private VisualElement body;
		private Image icon;
		private Label titleLabel;
		private Label countLabel;
		private Label messageLabel;
		private VisualElement content;
		private VisualElement actions;
		private VisualElement life;

		public ToastData Data { get; set; }

		public ToastArgs ToastArgs { get; set; }

		public ToastPosition ToastPosition => ToastArgs.ToastPosition;

		/// <summary>What groups repeats together. Two toasts sharing it become one with a count.</summary>
		public string Key { get; private set; }

		/// <summary>How long a toast should sit there: long enough to read what it actually says.</summary>
		public static float Lifetime(ToastArgs args)
		{
			if (args.LifeTime > 0f)
			{
				return args.LifeTime;
			}

			var words = CountWords(args.Title) + CountWords(args.Message);
			return Mathf.Max(MinimumLifeTime, words * SecondsPerWord);
		}

		/// <summary>What groups two toasts together as the same thing happening twice.</summary>
		public static string Grouping(ToastArgs args)
		{
			return string.IsNullOrEmpty(args.Key) ? args.Title + "\n" + args.Message : args.Key;
		}

		public void SetupWindow(ToastData toastData)
		{
			Data = toastData;
			ToastArgs = toastData.ToastArgs;
			Key = Grouping(ToastArgs);
			lastTick = EditorApplication.timeSinceStartup;
			position = this.GetEditorWindowPosition(ToastArgs.ToastPosition);
			currentPosition = position;

			titleLabel.text = ToastArgs.Title;
			messageLabel.text = ToastArgs.Message;
			messageLabel.style.display = string.IsNullOrEmpty(ToastArgs.Message) ? DisplayStyle.None : DisplayStyle.Flex;

			card.AddToClassList(SeverityClass(ToastArgs.Severity));
			icon.image = EditorGUIUtility.IconContent(SeverityIcon(ToastArgs.Severity)).image;

			if (ToastArgs.CustomContent != null)
			{
				content.Add(ToastArgs.CustomContent.Invoke());
			}

			BuildAction();
			EditorWindowCorners.Round(this);

			// The card knows how tall it needs to be only once it has been laid out, and asking
			// before then gives NaN. Fit the window to it on the first pass, and again whenever
			// the content changes height under it.
			body.RegisterCallback<GeometryChangedEvent>(_ => FitToContent());
		}

		/// <summary>
		/// Grows the window to whatever the card turned out to need, so a longer message is not
		/// quietly cut off at the bottom of a fixed box.
		/// </summary>
		private void FitToContent()
		{
			// Neither the card nor the body can be asked how tall they want to be: both stretch to
			// the window, so both answer with the height we are trying to work out. Measure where
			// the last visible thing inside the body ends instead.
			var wanted = ContentBottom() + body.resolvedStyle.paddingBottom + CardBorder * 2f;
			if (float.IsNaN(wanted) || wanted <= 1f)
			{
				return;
			}

			var height = Mathf.Ceil(wanted);
			if (Mathf.Abs(minSize.y - height) < 1f)
			{
				return;
			}

			minSize = new Vector2(minSize.x, height);
			maxSize = new Vector2(maxSize.x, height);
			currentPosition.height = height;
			position = currentPosition;
		}

		/// <summary>
		/// Says the same thing happened again. The words and the button are left alone on purpose:
		/// a toast that rewrites itself while it is being read is how somebody presses the wrong
		/// thing. Only the count changes, and the clock starts again.
		/// </summary>
		public void Repeat()
		{
			count++;
			countLabel.text = "x" + count;
			countLabel.AddToClassList("toast__count--shown");
			Data = new ToastData { ToastArgs = ToastArgs, TimeCreated = EditorApplication.timeSinceStartup };
		}

		public float GetHeight() => position.height;

		public void UpdatePosition(Rect positionRect, float currentHeightOffset, double deltaTime)
		{
			var targetPosition = new Rect(positionRect.x, positionRect.y + currentHeightOffset, positionRect.width, positionRect.height);
			if (Mathf.Abs(currentPosition.y - targetPosition.y) < PositionThreshold)
			{
				currentPosition.y = targetPosition.y;
				position = currentPosition;
				return;
			}

			currentPosition.y = Mathf.Lerp(currentPosition.y, targetPosition.y, (float)(PositionSpeed * deltaTime));
			currentPosition.x = targetPosition.x;
			position = currentPosition;
		}

		/// <summary>
		/// Whether its time is up. Also draws the hairline along the bottom, which is the only
		/// sign of the clock, because a bar you watch counting down asks to be watched.
		/// </summary>
		public bool IsLifetimeOver()
		{
			var now = EditorApplication.timeSinceStartup;
			if (paused)
			{
				// Hold the clock still for as long as somebody is reading it.
				Data = new ToastData { ToastArgs = ToastArgs, TimeCreated = Data.TimeCreated + now - lastTick };
			}

			lastTick = now;
			if (ToastArgs.NoTimeOut)
			{
				return false;
			}

			var lived = now - Data.TimeCreated;
			var total = Lifetime(ToastArgs);
			life.style.width = new StyleLength(Length.Percent(Mathf.Clamp01((float)(1.0 - lived / total)) * 100f));
			return lived > total;
		}

		/// <summary>How far down the body the last thing that is actually shown reaches.</summary>
		private float ContentBottom()
		{
			var bottom = 0f;
			foreach (var child in body.Children())
			{
				if (child.resolvedStyle.display == DisplayStyle.None)
				{
					continue;
				}

				bottom = Mathf.Max(bottom, child.layout.yMax);
			}

			return bottom;
		}

		private void CreateGUI()
		{
			var sheet = Stylesheet();
			if (sheet != null)
			{
				rootVisualElement.styleSheets.Add(sheet);
			}

			card = new VisualElement();
			card.AddToClassList("toast");
			rootVisualElement.Add(card);

			var stripe = new VisualElement();
			stripe.AddToClassList("toast__stripe");
			card.Add(stripe);

			body = new VisualElement();
			body.AddToClassList("toast__body");
			card.Add(body);

			var head = new VisualElement();
			head.AddToClassList("toast__head");
			body.Add(head);

			var iconFrame = new VisualElement();
			iconFrame.AddToClassList("toast__icon");
			head.Add(iconFrame);

			icon = new Image();
			icon.AddToClassList("toast__icon-image");
			iconFrame.Add(icon);

			titleLabel = new Label();
			titleLabel.AddToClassList("toast__title");
			head.Add(titleLabel);

			countLabel = new Label();
			countLabel.AddToClassList("toast__count");
			head.Add(countLabel);

			messageLabel = new Label();
			messageLabel.AddToClassList("toast__message");
			body.Add(messageLabel);

			content = new VisualElement();
			content.AddToClassList("toast__content");
			body.Add(content);

			actions = new VisualElement();
			actions.AddToClassList("toast__actions");
			body.Add(actions);

			var close = new Button(Close) { text = "✕" };
			close.AddToClassList("toast__close");
			card.Add(close);

			life = new VisualElement();
			life.AddToClassList("toast__life");
			card.Add(life);

			card.RegisterCallback<PointerEnterEvent>(_ => Pause(true));
			card.RegisterCallback<PointerLeaveEvent>(_ => Pause(false));
		}

		private void BuildAction()
		{
			if (ToastArgs.Action == null)
			{
				return;
			}

			var action = ToastArgs.Action;
			var button = new Button(() => Run(action)) { text = action.Label };
			button.AddToClassList("toast__action");
			actions.Add(button);
			actions.AddToClassList("toast__actions--shown");
			life.style.display = DisplayStyle.None;
		}

		private void Run(ToastAction action)
		{
			action.Perform?.Invoke();
			if (action.ClosesToast)
			{
				Close();
			}
		}

		private void Pause(bool on)
		{
			paused = on;
			life.EnableInClassList("toast__life--paused", on);
		}

		private void OnDestroy()
		{
			OnClose?.Invoke(this);
		}

		private static StyleSheet Stylesheet()
		{
			foreach (var guid in AssetDatabase.FindAssets("EditorToast t:StyleSheet"))
			{
				var found = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		private static string SeverityClass(ToastSeverity severity)
		{
			return severity switch
			{
				ToastSeverity.Success => "toast--success",
				ToastSeverity.Warning => "toast--warning",
				ToastSeverity.Error => "toast--error",
				_ => "toast--info"
			};
		}

		private static string SeverityIcon(ToastSeverity severity)
		{
			return severity switch
			{
				ToastSeverity.Success => "d_Progress",
				ToastSeverity.Warning => "console.warnicon",
				ToastSeverity.Error => "console.erroricon",
				_ => "console.infoicon"
			};
		}

		private static int CountWords(string text)
		{
			return string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ').Length;
		}
	}
}
