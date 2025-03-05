# EditorToast
Unity Editor Toast System, useful for notifying of any information or issues detected.
This will be used for CookieJar tools, but feel free to use it for your own use.

![image](https://github.com/user-attachments/assets/565bdf40-d82d-421b-8e1a-77b75a3fd875)

### Important info
Currently this only works on windows, and I've only tested on one machine so can't gurantee it'll work on your machine.
- Tested on Unity 6000.0.38f1

### Api
To use this you can just call `ToastManager.ShowNotification`:
``` c#
ToastManager.ShowNotification(new ToastArgs
{
    Title = "Hello World",
    Message = "Hello World!",
    LifeTime = 5f, //Time in seconds the notification will last
    Severity = ToastSeverity.Info,
    ToastPosition = ToastPosition.BottomRight
});
```

If you want notification sounds when a notification appears, you can put an mp3 file called `NotificationSound.mp3` at `Assets/Audio/NotificationSound.mp3`
