module MiniPhys.Simulator

open Fable.Core.JS
open MiniPhys.Types
open MiniPhys.Types.Units
//open FSharp.Data.UnitSystems.SI.UnitSymbols

// huge shoutout to
// http://buildnewgames.com/gamephysics/

let guardFloatInstability<[<Measure>] 'u> =
    mapFloatTyped<'u>
        (fun value ->
            if isNaN value || Math.abs value = Infinity
            then 0.
            else value)

let guardForceTorqueInstability (f, t) =
    Vec2.map guardFloatInstability f, guardFloatInstability t

let calcForcesAndTorques gObj timeStep globalTime =
    match gObj.forces with
    | BatchFC bfc -> bfc gObj timeStep globalTime
    | SingleFCs sfcs -> List.map (fun (_, f) -> f gObj timeStep globalTime) sfcs
    
    // remove NaN and +-Infinity forces and torques
    |> List.map guardForceTorqueInstability
    // sum all forces and torques
    |> List.reduce (fun (f1, t1) (f2, t2) -> (f1 + f2, t1 + t2))

let updateObjectPos gObj timeStep globalTime =
    // update transform from last tick info
    let newPos = gObj.pos + (gObj.velocity * timeStep) + (gObj.accel * 0.5 * (timeStep * timeStep))
    let newAngle = gObj.angle + (gObj.angVelocity * timeStep) + (gObj.angAccel * 0.5 * (timeStep * timeStep))
    
    // calculate forces and torques
    let force, torque = calcForcesAndTorques {gObj with pos = newPos(*; angle = newAngle*)} timeStep globalTime
    
    // update this tick acceleration
    // radians are dimensionless so are not part of the torque nor moment of inertia units, so add them in with *1rad
    let newAccel = force / gObj.mass
    let newAngAccel = torque / gObj.momentOfInertia * 1.<rad>
    
    let avgAccel = (gObj.accel + newAccel) / 2.
    let avgAngAccel = (gObj.angAccel + newAngAccel) / 2.
    
    // update this tick velocity
    let newVelocity = gObj.velocity + (avgAccel * timeStep)
    let newAngVelocity = gObj.angVelocity + (avgAngAccel * timeStep)
    
    { gObj with
        pos = newPos
        accel = newAccel
        velocity = newVelocity
        
        angle = newAngle
        angAccel = newAngAccel
        angVelocity = newAngVelocity
        }