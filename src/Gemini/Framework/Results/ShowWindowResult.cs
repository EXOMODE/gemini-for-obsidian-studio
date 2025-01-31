using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework.Threading;

namespace Gemini.Framework.Results
{
    public class ShowWindowResult<TWindow> : OpenResultBase<TWindow>
        where TWindow : IWindow
    {
        private readonly Func<TWindow> _windowLocator = () => IoC.Get<TWindow>();

        [Import]
        public IWindowManager WindowManager { get; set; }

        public ShowWindowResult()
        {
            
        }

        public ShowWindowResult(TWindow window)
        {
            _windowLocator = () => window;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            var window = _windowLocator();

            if (_setData != null)
                _setData(window);

            if (_onConfigure != null)
                _onConfigure(window);

            window.Deactivated += (s, e) =>
            {
                if (!e.WasClosed)
                    return TaskUtility.Completed;

                if (_onShutDown != null)
                    _onShutDown(window);

                OnCompleted(null, false);

                return TaskUtility.Completed;
            };

            WindowManager.ShowWindowAsync(window);
        }
    }
}
