<p align="center">
  <img src="Documentation~/hero.png" alt="Editor Toast" width="1000">
</p>

<h3 align="center">Toast notifications for the Unity editor, for saying something without stopping the work.</h3>

## Install

Unity 6000.0 or newer.

**In the Package Manager**

1. Open `Window > Package Manager`
2. Press **+**, then **Install package from git URL**
3. Paste this and press Add:

```
https://github.com/CookieJarTools/EditorToast.git
```

**Or by hand**, put this line in `Packages/manifest.json`:

```json
"com.cookiejartools.editortoast": "https://github.com/CookieJarTools/EditorToast.git"
```

## What a toast is for

A toast is for telling somebody that a thing has already happened while they were looking
somewhere else, and at most giving them one thing to do about it. That is the whole of it, and
knowing where the edges are is most of using it well.

**Something finished.** A build, a bake, an import, a test run. This is the main one in an editor,
because the alternatives are the console, which is missed, and a dialog, which interrupts.

**Something reversible happened, and here is the way back.** This is the case that earns a toast
its place. A line in the console cannot offer you an Undo.

**A small thing worked.** A path copied, a preset saved. Cases where you would otherwise get no
feedback at all and be left wondering.

And where it is the wrong shape:

**Not for anything they must read.** A toast takes itself away, so putting something important in
one means hurrying somebody to read it before it goes. That belongs in the console or in front of
them.

**Not for anything irreversible.** Deleting something that cannot come back wants a dialog, before
the fact, not a notice afterwards.

**Not for what they can already see.** If the thing they just did is plainly done on screen, a
toast saying so is noise.

## Raising one

```csharp
ToastManager.ShowNotification(new ToastArgs
{
    Title = "Build finished",
    Message = "Windows 64 bit, 2 minutes 14 seconds.",
    Severity = ToastSeverity.Success,
    ToastPosition = ToastPosition.BottomRight,
});
```

With the way back on it:

```csharp
ToastManager.ShowNotification(new ToastArgs
{
    Title = "12 unused assets removed",
    Message = "They were not referenced by any scene or prefab.",
    Action = new ToastAction("Undo", PutThemBack),
});
```

The examples under `Window > CookieJarTools > Editor Toast` are the four shapes worth copying.

## Worth knowing

**A toast with an action never times out.** An Undo that disappears while somebody is still
reading the sentence above it is worse than no Undo at all, because the next one will not be
trusted either. Setting an action turns the timer off for you.

**The same thing raised twice becomes one toast wearing a count.** Give repeats a `Key` to group
them, or let it fall back to matching the title and message. The words and the button never change
while a toast is on screen, because a notice that rewrites itself under the pointer is how
somebody presses the wrong thing. Only the count moves, and the clock starts again.

**The timer stops while the pointer is on it**, so a toast cannot leave in the middle of being
read.

**How long it stays is worked out from how much it says**, with a floor of four seconds, unless
you set `LifeTime` yourself.

**Nothing makes a sound unless you ask for one.** Set `ToastManager.Sound` to a clip. It is off by
default because a noise on every notification is the thing that makes somebody turn a tool off.

**`Severity` only picks the colour and the mark.** Use `Error` for something that failed and that
the reader can do nothing about from here. A failure they must act on is not a toast.

## Licence

Free to use, including in games you sell, under the PolyForm Shield License 1.0.0.
The one thing it does not allow is using it to make something that competes with the
tool itself. See [LICENSE.md](LICENSE.md).
