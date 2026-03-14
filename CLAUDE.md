# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Fable.Reactive is a lightweight Async Reactive library for F# implementing Async Observables (ReactiveX pattern). Designed for Fable compatibility, enabling the same F# code to run both server-side and transpiled to JavaScript.

## Build Commands

```bash
dotnet restore                    # Restore dependencies
dotnet build --configuration Release  # Build
dotnet test                       # Run tests with coverage
dotnet pack                       # Create NuGet package
```

## Tools

```bash
dotnet fantomas src/ test/        # Format F# code (max 120 char lines)
```

Tools are managed via `.config/dotnet-tools.json` (Paket, Fantomas).

## Test Framework

Uses **Expecto** test framework. Tests are in `test/` directory.

Test utilities in `test/Utils.fs`:

- `TestObserver<'a>` - Captures OnNext/OnError/OnCompleted notifications
- `fromNotification` - Creates observables from notification sequences

Example test pattern:

```fsharp
testAsync "Test name" {
    let xs = AsyncRx.single 42 |> AsyncRx.map (fun x -> x * 10)
    let obv = TestObserver<int>()
    let! sub = xs.SubscribeAsync obv
    let! latest = obv.Await()
    Expect.equal latest 420 "Should be equal"
}
```

## Architecture

### Core Types (`src/Types.fs`)

- `IAsyncRxDisposable` - Async disposable (Fable-compatible name)
- `IAsyncObserver<'T>` - Async observer with OnNextAsync/OnErrorAsync/OnCompletedAsync
- `IAsyncObservable<'T>` - Async observable with SubscribeAsync
- `Notification<'T>` - OnNext/OnError/OnCompleted discriminated union
- `AsyncStream<'TSource, 'TResult>` - Function type for operator composition

### Source Modules (`src/`)

|       Module       |                                              Purpose                                              |
| ------------------ | ------------------------------------------------------------------------------------------------- |
| Create.fs          | Observable creation: `single`, `empty`, `never`, `interval`, `timer`, `ofSeq`, `ofAsync`, `defer` |
| Transform.fs       | `map`, `flatMap`, `concatMap`, `catch`, `retry`, `switch`                                         |
| Filter.fs          | `filter`, `take`, `skip`, `choose`, `distinctUntilChanged`, `takeUntil`                           |
| Combine.fs         | `merge`, `concat`, `combineLatest`, `withLatestFrom`, `zip`                                       |
| Aggregate.fs       | `scan`, `reduce`, `groupBy`, `min`, `max`                                                         |
| Timeshift.fs       | `delay`, `debounce`, `sample`                                                                     |
| Subject.fs         | Hot/cold stream subjects for multicast                                                            |
| AsyncObservable.fs | Main API module exporting all operators via `AsyncRx`                                             |
| Builder.fs         | Query/computation expression builder (`asyncRx { }`)                                              |

### Key Patterns

- All operators are async-aware using F# `Async<'T>`
- `safeObserver` uses MailboxProcessor to serialize notifications and enforce Rx grammar
- Operators compose via the `AsyncStream` function type
- Conditional compilation (`#if FABLE_COMPILER`) for Fable compatibility

## Formatting

Fantomas with settings in `.editorconfig`:

- Max line length: 120 characters
- Max infix operator expression: 50
