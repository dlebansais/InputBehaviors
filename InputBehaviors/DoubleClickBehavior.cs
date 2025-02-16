namespace InputBehaviors;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

/// <summary>
/// Represents a behavior that let a <see cref="FrameworkElement"/> send either a single-click or double-click command, but not both.
/// </summary>
public class DoubleClickBehavior : Behavior<FrameworkElement>, IDisposable
{
    /// <summary>
    /// Identifies the <see cref="SingleClickCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SingleClickCommandProperty = DependencyProperty.Register(nameof(SingleClickCommand), typeof(ICommand), typeof(DoubleClickBehavior), new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> for a single click.
    /// </summary>
    public ICommand SingleClickCommand
    {
        get => (ICommand)GetValue(SingleClickCommandProperty);
        set => SetValue(SingleClickCommandProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="DoubleClickCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(nameof(DoubleClickCommand), typeof(ICommand), typeof(DoubleClickBehavior), new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> for a double click.
    /// </summary>
    public ICommand DoubleClickCommand
    {
        get => (ICommand)GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(DoubleClickBehavior), new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is FrameworkElement Element)
            Element.MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is FrameworkElement Element)
            Element.MouseLeftButtonDown -= OnMouseLeftButtonDown;

        base.OnDetaching();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount <= 1)
        {
            if (SingleClickCommand is ICommand Command)
                _ = Dispatcher.BeginInvoke(async () => await SingleClickAsync(Command, CommandParameter).ConfigureAwait(false));
        }
        else
        {
            if (DoubleClickCommand is ICommand Command)
                _ = Dispatcher.BeginInvoke(() => DoubleClick(Command, CommandParameter));
        }
    }

    private async Task SingleClickAsync(ICommand command, object commandParameter)
    {
        SelectionTokenSource?.Dispose();
        SelectionTokenSource = new CancellationTokenSource();

        await Task.Delay(GetDoubleClickTime()).ConfigureAwait(true);

        SingleClickAsyncPhase2(command, commandParameter);
    }

    private void SingleClickAsyncPhase2(ICommand command, object commandParameter)
    {
        if (SelectionTokenSource?.Token is CancellationToken Token && !Token.IsCancellationRequested)
        {
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    private void DoubleClick(ICommand command, object commandParameter)
    {
        SelectionTokenSource?.Cancel();

        if (command.CanExecute(commandParameter))
            command.Execute(commandParameter);
    }

    private CancellationTokenSource? SelectionTokenSource;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetDoubleClickTime();

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
                SelectionTokenSource?.Dispose();
                SelectionTokenSource = null;
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
