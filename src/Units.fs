module MiniPhys.Types.Units

open System

let inline floatToTyped<[<Measure>] 'u> x : float<'u> = LanguagePrimitives.FloatWithMeasure x
    
let inline typedToFloat<[<Measure>] 'u> (x: float<'u>) = float x

let inline typedToTyped<[<Measure>] 'u, [<Measure>] 'v> = typedToFloat<'u> >> floatToTyped<'v>

let inline mapFloatTyped<[<Measure>] 'u> f = (typedToFloat<'u> >> f >> floatToTyped<'u>)

[<Measure>] type rad
[<Measure>] type deg

let degPerRad = 180.<deg/rad> / Math.PI

let radToDeg (r: float<rad>) = r * degPerRad
let degToRad (d: float<deg>) = d / degPerRad