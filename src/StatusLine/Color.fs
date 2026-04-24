module StatusLine.Color

open System.Drawing

let percentageToColor (percentage: float) : Color =
    let p = System.Math.Clamp(percentage, 0.0, 100.0)
    let r = int (p / 100.0 * 255.0)
    let g = int ((100.0 - p) / 100.0 * 255.0)
    Color.FromArgb(r, g, 0)
