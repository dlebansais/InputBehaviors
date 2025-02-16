namespace InputBehaviors;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

/// <summary>
/// Chapter.
/// </summary>
public class DelayedEditBehavior : Behavior<TextBox>, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedEditBehavior"/> class.
    /// </summary>
    public DelayedEditBehavior()
    {
        DelayTimer = new(new TimerCallback(DelayTimerCallback));
    }

    /// <summary>
    /// Identifies the <see cref="DelayedEditCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DelayedEditCommandProperty = DependencyProperty.Register(nameof(DelayedEditCommand), typeof(ICommand), typeof(DelayedEditBehavior), new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> for a delayed edit.
    /// </summary>
    public ICommand DelayedEditCommand
    {
        get => (ICommand)GetValue(DelayedEditCommandProperty);
        set => SetValue(DelayedEditCommandProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Delay"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(DelayedEditBehavior), new UIPropertyMetadata(TimeSpan.FromSeconds(0.2)));

    /// <summary>
    /// Gets or sets the delay.
    /// </summary>
    public TimeSpan Delay
    {
        get => (TimeSpan)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is TextBox Element)
        {
            Element.Loaded += OnLoaded;
            Element.TextChanged += OnTextChanged;
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is TextBox Element)
        {
            Element.Loaded -= OnLoaded;
            Element.TextChanged -= OnTextChanged;
        }

        base.OnDetaching();
    }

    private void OnLoaded(object sender, EventArgs args)
    {
        if (sender is TextBox Edit && Edit.Text is string InitText)
            LastText = InitText;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        if (sender is TextBox Edit && Edit.Text is string Text)
        {
            _ = DelayTimer.Change(Delay, Timeout.InfiniteTimeSpan);
            NewText = Text;
        }
    }

    private void DelayTimerCallback(object? parameter)
    {
        string OldText = LastText;
        _ = Dispatcher.BeginInvoke(new Action(() => DelayedEdit(OldText, NewText)));
        LastText = NewText;
    }

    private void DelayedEdit(string oldText, string newText)
    {
        if (DelayedEditCommand is ICommand Command)
        {
            object Parameter = new Tuple<string, string>(oldText, newText);

            if (Command.CanExecute(Parameter))
                Command.Execute(Parameter);
        }
    }

    private readonly Timer DelayTimer;
    private string LastText = string.Empty;
    private string NewText = string.Empty;

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="True"/> if the method should dispose of resources; Otherwise, <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                DelayTimer.Dispose();
            }

            IsDisposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private bool IsDisposed;
}
