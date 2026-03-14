module Tests.Timeshift

open Fable.Reactive

open Expecto
open Tests.Utils

[<Tests>]
let tests =
    testList
        "Timeshift Tests"
        [ testAsync "Test delay values are delayed" {
              // Arrange
              let xs = fromNotification [ OnNext 1; OnNext 2; OnCompleted ]
              let delayed = xs |> AsyncRx.delay 50
              let obv = TestObserver<int>()

              // Act
              let! sub = delayed.SubscribeAsync obv
              do! obv.AwaitIgnore()

              // Assert
              let actual = obv.Notifications |> Seq.toList
              let expected: Notification<int> list = [ OnNext 1; OnNext 2; OnCompleted ]
              Expect.equal actual expected "Should be equal"
          }

          testAsync "Test delay can resubscribe after dispose" {
              // Arrange - each subscription should get its own CTS
              let xs = fromNotification [ OnNext 1; OnCompleted ]
              let delayed = xs |> AsyncRx.delay 50

              // Act - first subscription
              let obv1 = TestObserver<int>()
              let! sub1 = delayed.SubscribeAsync obv1
              do! obv1.AwaitIgnore()
              do! sub1.DisposeAsync()

              // Second subscription should work (not broken by first dispose)
              let obv2 = TestObserver<int>()
              let! sub2 = delayed.SubscribeAsync obv2
              do! obv2.AwaitIgnore()

              // Assert
              let actual = obv2.Notifications |> Seq.toList
              let expected: Notification<int> list = [ OnNext 1; OnCompleted ]
              Expect.equal actual expected "Second subscription should work after first is disposed"
          }

          testAsync "Test debounce emits last value after timeout" {
              // Arrange
              let dispatch, source = AsyncRx.subject<int> ()
              let debounced = source |> AsyncRx.debounce 50
              let obv = TestObserver<int>()

              // Act
              let! sub = debounced.SubscribeAsync obv
              do! dispatch.OnNextAsync 1
              do! Async.Sleep 100
              do! dispatch.OnCompletedAsync()
              do! obv.AwaitIgnore()

              // Assert
              let actual = obv.Notifications |> Seq.toList
              let expected: Notification<int> list = [ OnNext 1; OnCompleted ]
              Expect.equal actual expected "Should emit debounced value"
          }

          testAsync "Test debounce dispose cancels pending timers" {
              // Arrange
              let dispatch, source = AsyncRx.subject<int> ()
              let debounced = source |> AsyncRx.debounce 200
              let obv = TestObserver<int>()

              // Act
              let! sub = debounced.SubscribeAsync obv
              do! dispatch.OnNextAsync 1
              // Dispose before the debounce timer (200ms) fires
              do! Async.Sleep 50
              do! sub.DisposeAsync()
              // Wait past what would have been the debounce period
              do! Async.Sleep 300

              // Assert - no emissions since we disposed before timer fired
              let actual = obv.Notifications |> Seq.toList
              Expect.isEmpty actual "Should have no emissions when disposed before debounce timer fires"
          } ]
