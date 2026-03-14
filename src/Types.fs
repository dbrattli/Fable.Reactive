namespace Fable.Reactive

/// Async disposable interface for Fable.Reactive subscriptions.
type IReactiveDisposable =
    abstract member DisposeAsync: unit -> Async<unit>

/// Async observer with OnNextAsync/OnErrorAsync/OnCompletedAsync.
type IAsyncObserver<'T> =
    abstract member OnNextAsync: 'T -> Async<unit>
    abstract member OnErrorAsync: exn -> Async<unit>
    abstract member OnCompletedAsync: unit -> Async<unit>

type IAsyncObservable<'T> =
    abstract member SubscribeAsync: IAsyncObserver<'T> -> Async<IReactiveDisposable>

type Notification<'T> =
    | OnNext of 'T
    | OnError of exn
    | OnCompleted

type AsyncStream<'TSource, 'TResult> = IAsyncObservable<'TSource> -> IAsyncObservable<'TResult>
type AsyncStream<'TSource> = AsyncStream<'TSource, 'TSource>
