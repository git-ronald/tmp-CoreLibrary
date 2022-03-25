namespace CoreLibrary.Helpers
{
    public static class HelperExtensions
    {
        public static Task InvokeHandlers(this AsyncEventHandlers.EmptyAsyncHandler? handler)
        {
            if (handler is null)
            {
                return Task.CompletedTask;
            }
            return handler.Invoke();
        }
        public static Task InvokeHAndlers<T>(this AsyncEventHandlers.ArgAsyncHandler<T>? handler, T arg)
        {
            if (handler is null)
            {
                return Task.CompletedTask;
            }
            return handler.Invoke(arg);
        }
    }
}
