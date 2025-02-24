using UnityEngine.UIElements;

namespace CookieJar.Editor.Toast
{
	public struct ToastArgs
	{
		public string Title { get; set; }
		public string Message { get; set; }
		public ToastPosition ToastPosition { get; set; }
		public float LifeTime { get; set; }
		public VisualElement CustomContent { get; set; }
		public ToastSeverity Severity { get; set; }
	}
}