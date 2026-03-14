module Tests.Merge

open System.Threading.Tasks

open Fable.Reactive

open Expecto
open Tests.Utils

exception MyError of string

[<Tests>]
let tests = testList "Merge Tests" [

    testAsync "Test merge non empty emtpy" {
        // Arrange
        let xs = seq { 1..5 } |> Reactive.ofSeq
        let ys = Reactive.empty<int> ()
        let zs = Reactive.ofSeq [ xs; ys ] |> Reactive.mergeInner
        let obv = TestObserver<int>()

        // Act
        let! sub = zs.SubscribeAsync obv
        let! latest= obv.Await ()

        // Assert
        Expect.equal latest 5 "Should be equal"
        Expect.equal obv.Notifications.Count 6 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 1; OnNext 2; OnNext 3; OnNext 4; OnNext 5; OnCompleted ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test merge empty non emtpy" {
        // Arrange
        let xs = Reactive.empty<int> ()
        let ys = seq { 1..5 } |> Reactive.ofSeq
        let zs = Reactive.ofSeq [ xs; ys ] |> Reactive.mergeInner
        let obv = TestObserver<int>()

        // Act
        let! sub = zs.SubscribeAsync obv
        let! latest= obv.Await ()

        // Assert
        Expect.equal latest 5 "Should be equal"
        Expect.equal obv.Notifications.Count 6 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 1; OnNext 2; OnNext 3; OnNext 4; OnNext 5; OnCompleted ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test merge error error" {
        // Arrange
        let error = MyError "error"
        let xs = Reactive.fail error
        let ys = Reactive.fail error
        let zs = Reactive.ofSeq [ xs; ys ] |> Reactive.mergeInner
        let obv = TestObserver<int> ()

        // Act
        let! sub = zs.SubscribeAsync obv

        try
            do! obv.Await () |> Async.Ignore
        with
        | _ -> ()

        // Assert
        Expect.equal obv.Notifications.Count 1 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnError error ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test merge two" {
        // Arrange
        let xs  = seq { 1..3 } |> Reactive.ofSeq
        let ys = seq { 4..5 } |> Reactive.ofSeq
        let zs = Reactive.ofSeq [ xs; ys ] |> Reactive.mergeInner
        let obv = TestObserver<int> ()

        // Act
        let! sub = zs.SubscribeAsync obv
        do! obv.AwaitIgnore ()

        // Assert
        Expect.equal obv.Notifications.Count 6 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        Expect.contains actual (OnNext 1) "Should contain the element"
        Expect.contains actual (OnNext 2) "Should contain the element"
        Expect.contains actual (OnNext 3) "Should contain the element"
        Expect.contains actual (OnNext 4) "Should contain the element"
        Expect.contains actual (OnNext 5) "Should contain the element"
        Expect.contains actual (OnCompleted) "Should contain the element"
    }

    testAsync "Test subscribe immediately" {
         // Arrange
         let obv, stream = Reactive.singleSubject<int> ()
         let msgs =
            Create.ofSeq [stream;  Reactive.empty () ] |> Reactive.mergeInner

         let testObv = TestObserver<int>()

         // Act
         do! async {
            let! subscription = msgs.SubscribeAsync testObv
            do! obv.OnNextAsync 1
            do! Async.Sleep 100
            do! obv.OnCompletedAsync ()
            do! testObv.AwaitIgnore ()
            do! subscription.DisposeAsync ()
         }
         // Assert
         Expect.sequenceEqual testObv.Notifications [OnNext 1; OnCompleted] "Should have received one OnNext and OnCompleted notifications"
     }
]
