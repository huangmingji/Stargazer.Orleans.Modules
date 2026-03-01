namespace Stargazer.Orleans.Utility.Snowflake
{
    internal class DisposableAction : IDisposable
    {
        readonly Action _action;

        public DisposableAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}