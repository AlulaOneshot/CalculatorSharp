namespace CalculatorSharp

open System.ComponentModel
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

type MainViewModel() =
    let mutable _calculator_text = "Hello World"
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish

    member private this.OnPropertyChanged(calculator_text: string) =
        propertyChanged.Trigger(this, PropertyChangedEventArgs(calculator_text))

    member this.CalculatorText
        with get () = _calculator_text
        and set (value) =
            if _calculator_text <> value then
                _calculator_text <- value
                this.OnPropertyChanged(nameof this.CalculatorText)

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
