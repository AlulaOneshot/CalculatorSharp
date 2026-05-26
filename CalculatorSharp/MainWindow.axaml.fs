namespace CalculatorSharp

open System.ComponentModel
open System.Windows.Input
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

type DelegateCommand(execute: obj -> unit) =
    let canExecuteChanged = Event<_, _>()

    interface ICommand with
        [<CLIEvent>]
        member _.CanExecuteChanged = canExecuteChanged.Publish
        member _.CanExecute(_) = true
        member _.Execute(param) = execute param

type MainViewModel() =
    let mutable display = "0"
    let mutable lhs = 0.0
    let mutable pendingOp = None
    let mutable clearNext = false
    let mutable isError = false
    
    let formatResult (n: float) =
        if System.Double.IsNaN(n) || System.Double.IsInfinity(n) then "Error"
        else sprintf "%g" n

    let applyOp op l r =
        match op with
        | "+" -> l + r
        | "-" -> l - r
        | "*" -> l * r
        | "/" -> if r = 0.0 then System.Double.NaN else l / r
        | "%" -> l % r
        | _ -> r
   
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish

    member private this.OnPropertyChanged(display: string) =
        propertyChanged.Trigger(this, PropertyChangedEventArgs(display))
                
    member this.CalculatorText
        with get() = display
        and set(v) =
            display <- v
            this.OnPropertyChanged(nameof this.CalculatorText)

    member private this.SetError() =
        this.CalculatorText <- "Error"
        isError <- true
        pendingOp <- None
        clearNext <- true

    member this.InputDigit(digit: string) =
        if isError then ()
        elif clearNext then
            this.CalculatorText <- digit
            clearNext <- false
        elif display = "0" then
            this.CalculatorText <- digit
        else
            this.CalculatorText <- display + digit
            
    member this.InputDecimal() =
        if isError then ()
        elif clearNext then
            this.CalculatorText <- "0."
            clearNext <- false
        elif not (display.Contains(".")) then
            this.CalculatorText <- display + "."

    member this.InputOperator(op: string) =
        if isError then ()
        else
            let current = double display
            match pendingOp with
            | Some existingOp ->
                let result = applyOp existingOp lhs current
                let formatted = formatResult result
                this.CalculatorText <- formatted
                if formatted = "Error" then
                    this.SetError()
                else
                    lhs <- result
                    pendingOp <- Some op
                    clearNext <- true
            | None ->
                lhs <- current
                pendingOp <- Some op
                clearNext <- true
                
    member this.Calculate() =
        if isError then ()
        else
            match pendingOp with
            | None -> ()
            | Some op ->
                let rhs = double display
                let result = applyOp op lhs rhs
                let formatted = formatResult result
                this.CalculatorText <- formatted
                if formatted = "Error" then
                    this.SetError()
                else
                    lhs <- 0.0
                    pendingOp <- None
                    clearNext <- true

    member this.ToggleSign() =
        if not isError then
            if display.StartsWith("-") then
                this.CalculatorText <- display.Substring(1)
            else
                this.CalculatorText <- "-" + display

    member this.Clear() =
        this.CalculatorText <- "0"
        lhs <- 0.0
        pendingOp <- None
        clearNext <- false
        isError <- false

    member this.DigitCommand =
        DelegateCommand(fun d -> this.InputDigit(string d))

    member this.DecimalCommand =
        DelegateCommand(fun _ -> this.InputDecimal())

    member this.OperatorCommand =
        DelegateCommand(fun op -> this.InputOperator(string op))

    member this.EqualsCommand =
        DelegateCommand(fun _ -> this.Calculate())

    member this.ToggleSignCommand =
        DelegateCommand(fun _ -> this.ToggleSign())

    member this.ClearCommand =
        DelegateCommand(fun _ -> this.Clear())

type MainWindow() as this =
    inherit Window()

    do
        AvaloniaXamlLoader.Load(this)
        this.DataContext <- MainViewModel()

    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
