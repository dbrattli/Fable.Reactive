module Tests.Bind

open Fable.Reactive

open Expecto
open Tests.Utils

[<Tests>]
let tests = testList "Filter Tests" [

    testAsync "Test flatMap empty" {
        // Arrange
        let xs = Reactive.empty ()
        let zs = xs |> Reactive.flatMap (fun x -> x)
        let obv = TestObserver<int>()

        // Act
        let! sub = zs.SubscribeAsync obv
        try
            do! obv.AwaitIgnore ()
        with
        | _ -> ()

        // Assert
        Expect.equal obv.Notifications.Count 1 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnCompleted ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test flatMap some" {
        // Arrange
        let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted]
        let zs = xs |> Reactive.flatMap (fun x -> Reactive.single x)
        let obv = TestObserver<int>()

        // Act
        let! sub = zs.SubscribeAsync obv
        do! obv.AwaitIgnore ()

        // Assert
        Expect.equal obv.Notifications.Count 4 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 1; OnNext 2; OnNext 3; OnCompleted ]
        Expect.containsAll actual expected "Should contain all"
    }

    /// return x >>= f is the same thing as f x
    testAsync "Test flatMap monad law left identity" {

        // Arrange
        let f x = Reactive.single (x * 10)
        let xs = Reactive.single 42 |> Reactive.flatMap f
        let ys = f 42
        let obv1 = TestObserver<int>()
        let obv2 = TestObserver<int>()

        // Act
        do! xs.RunAsync obv1
        let! x = obv1.Await ()

        do! ys.RunAsync obv2
        let! y = obv2.Await ()

        // Assert
        Expect.equal x y "Should be equal"
        Expect.equal x 420 "Should be equal"
    }

    /// m >>= return is no different than just m
    testAsync "Test flatMap monad law right identity" {

        // Arrange
        let m = Reactive.single 42
        let xs = m |> Reactive.flatMap Reactive.single
        let obv1 = TestObserver<int>()
        let obv2 = TestObserver<int>()

        // Act
        do! m.RunAsync obv1
        let! x = obv1.Await ()

        do! xs.RunAsync obv2
        let! y = obv2.Await ()

        // Assert
        Expect.equal x y "Should be equal"
        Expect.equal x 42 "Should be equal"
    }

    /// (m >>= f) >>= g is just like doing m >>= (\x -> f x >>= g)
    testAsync "Test flatMap monad law associativity" {
        // Arrange
        let m = Reactive.single 42
        let f x = Reactive.single (x * 1000)
        let g x = Reactive.single (x * 42)

        let xs = m |> Reactive.flatMap f |> Reactive.flatMap g
        let ys = m |> Reactive.flatMap (fun x -> f x |> Reactive.flatMap g)

        let obv1 = TestObserver<int>()
        let obv2 = TestObserver<int>()

        // Act
        do! xs.RunAsync obv1
        let! x = obv1.Await ()

        do! ys.RunAsync obv2
        let! y = obv2.Await ()

        // Assert
        Expect.equal x y "Should be equal"
        Expect.equal x 1764000 "Should be equal"
    }

    testAsync "Test flatMap expression some" {
        // Arrange
        let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted]
        let ys = reactive {
            let! x = xs
            yield x * 2
        }
        let obv = TestObserver<int>()

        // Act
        let! sub = ys.SubscribeAsync obv
        do! obv.AwaitIgnore ()

        // Assert
        Expect.equal obv.Notifications.Count 4 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 2; OnNext 4; OnNext 6; OnCompleted ]
        Expect.containsAll actual expected "Should contain all"
    }

    testAsync "Test flatMap expression some for" {
        // Arrange
        let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted]
        let ys = reactive {
            for x in xs do
                yield x * 2
        }
        let obv = TestObserver<int>()

        // Act
        let! sub = ys.SubscribeAsync obv
        do! obv.AwaitIgnore ()

        // Assert
        Expect.equal obv.Notifications.Count 4 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 2; OnNext 4; OnNext 6; OnCompleted ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test flatMap expression some return bang" {
        // Arrange
        let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted]
        let ys = reactive {
            let! x = xs
            yield! Reactive.single (x * 2)
        }
        let obv = TestObserver<int>()

        // Act
        let! sub = ys.SubscribeAsync obv
        do! obv.AwaitIgnore ()

        // Assert
        Expect.equal obv.Notifications.Count 4 "Wrong count"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnNext 2; OnNext 4; OnNext 6; OnCompleted ]
        Expect.containsAll actual expected "Should contain all"
    }
]