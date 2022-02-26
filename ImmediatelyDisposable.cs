namespace CoreLibrary
{
    public abstract class ImmediatelyDisposable : IImmediatelyDisposable
    {
        public async Task ExecuteDispose()
        {
            await using var disposable = await Execute();
        }

        protected abstract Task<IAsyncDisposable> Execute();
    }
}
