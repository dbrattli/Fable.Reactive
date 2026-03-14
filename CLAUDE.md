# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Fable.Reactive is a lightweight Async Reactive library for F# implementing Async Observables (ReactiveX pattern). Designed for Fable compatibility, enabling the same F# code to run both server-side and transpiled to JavaScript.

## Build Commands

Use `just` as the task runner (see `justfile` for all recipes):

```bash
just              # List available recipes
just build        # Build F# project
just build-release # Build in Release mode
just test         # Run all tests
just format       # Format with Fantomas
just pack         # Create NuGet package
just restore      # Restore dependencies and tools
just shipit       # Run EasyBuild.ShipIt for release management
```

Tools are managed via `.config/dotnet-tools.json` (Paket, Fantomas, EasyBuild.ShipIt).

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/). PR titles are enforced via CI.

Allowed types: `feat`, `fix`, `chore`, `docs`, `style`, `refactor`, `perf`, `test`, `ci`, `build`, `revert`.

## Test Framework

Uses **Expecto** test framework. Tests are in `test/` directory.

Test utilities in `test/Utils.fs`:

- `TestObserver<'a>` - Captures OnNext/OnError/OnCompleted notifications
- `fromNotification` - Creates observables from notification sequences

Example test pattern:

```fsharp
testAsync "Test name" {
    let xs = Reactive.single 42 |> Reactive.map (fun x -> x * 10)
    let obv = TestObserver<int>()
    let! sub = xs.SubscribeAsync obv
    let! latest = obv.Await()
    Expect.equal latest 420 "Should be equal"
}
```

## Architecture

### Core Types (`src/Types.fs`)

- `IReactiveDisposable` - Async disposable for subscriptions
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
| AsyncObservable.fs | Main API module exporting all operators via `Reactive`                                            |
| Builder.fs         | Query/computation expression builder (`reactive { }`)                                             |

### Key Patterns

- All operators are async-aware using F# `Async<'T>`
- `safeObserver` uses MailboxProcessor to serialize notifications and enforce Rx grammar
- Operators compose via the `AsyncStream` function type
- Conditional compilation (`#if FABLE_COMPILER`) for Fable compatibility

## Formatting

Fantomas with settings in `.editorconfig`:

- Max line length: 120 characters
- Max infix operator expression: 50
