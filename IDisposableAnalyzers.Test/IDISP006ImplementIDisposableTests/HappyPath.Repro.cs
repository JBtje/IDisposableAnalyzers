namespace IDisposableAnalyzers.Test.IDISP006ImplementIDisposableTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class Repro
        {
            [Test]
            public void Issue63()
            {
                var viewModelBaseCode = @"
namespace MVVM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications
    /// and has a DisplayName property. This class is abstract.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected ViewModelBase()
        {
        }

        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly object disposeLock = new object();
        private bool isDisposed;

        public void Dispose()
        {
            lock (disposeLock)
            {
                this.OnDispose();

                if (isDisposed)
                    return;

                foreach (var disposable in disposables)
                    disposable.Dispose();

                isDisposed = true;
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}";
                var popupViewModelCode = @"
namespace ProjectX.ViewModel
{
    using System;
    using MVVM;

    public class PopupViewModel : ViewModelBase
    {
        public PopupViewModel()
        {
            ClosePopupCommand = new ClosePopupCommand(this);
        }

        // Gives an IDISP006 warning (need to implement IDispose)
        public ClosePopupCommand ClosePopupCommand { get; }

        protected override void OnDispose()
        {
            ClosePopupCommand.Dispose();
            CloseProgramCommand.Dispose();
        }
    }
}";

                var closePopupCommandCode = @"
namespace ProjectX.Commands
{
    using System;

    public sealed class ClosePopupCommand : IDelegateCommand, IDisposable
    {
        private readonly object disposeLock = new object();
        private bool isDisposed;
        private bool isBusy = false;

        internal ClosePopupCommand()
        {
        }

        public event EventHandler CanExecuteChanged;

        public void Dispose()
        {
            lock (disposeLock)
            {
                if (isDisposed)
                    return;

                // Here we have code that actually needs to be disposed off...

                isDisposed = true;
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, viewModelBaseCode, popupViewModelCode, closePopupCommandCode);
            }
        }
    }
}
