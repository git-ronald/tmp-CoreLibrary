namespace CoreLibrary
{
    public static class AsyncEventHandlers
    {
        public delegate Task EmptyAsyncHandler();
        public delegate Task ArgAsyncHandler<T>(T arg);
    }
}
