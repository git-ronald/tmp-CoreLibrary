using CoreLibrary.Delegates;

namespace CoreLibrary.Helpers
{
    public static class HelperExtensions
    {
        public static Task InvokeHandlers(this EmptyAsyncHandler? handler)
        {
            if (handler == null)
            {
                return Task.CompletedTask;
            }
            return handler.Invoke();
        }
    }
}
