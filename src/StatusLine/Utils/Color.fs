module StatusLine.Utils.Color

open System.Drawing
open Wacton.Unicolour

let private green = Unicolour(ColourSpace.Rgb255, 0.0, 255.0, 0.0)
let private red = Unicolour(ColourSpace.Rgb255, 255.0, 0.0, 0.0)

let percentageToColor (percentage: float) : Color =
    let t = System.Math.Clamp(percentage, 0.0, 100.0) / 100.0
    let mixed = green.Mix(red, ColourSpace.Oklch, amount = t).MapToRgbGamut()
    let rgb = mixed.Rgb.Byte255
    Color.FromArgb(rgb.R, rgb.G, rgb.B)
