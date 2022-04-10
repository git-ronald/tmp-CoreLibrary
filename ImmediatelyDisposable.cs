namespace CoreLibrary
{
    // TODO: enable return value (perhaps generic out variable for Execute)
    // This way HubClient can indicate restart
    public abstract class ImmediatelyDisposable : IImmediatelyDisposable
    {
        public async Task ExecuteDispose()
        {
            await using var disposable = await Execute();
        }

        protected abstract Task<IAsyncDisposable> Execute();
    }
}
