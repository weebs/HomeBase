module Chip8.Processor

open System.IO

type Register = byte
type MemoryAddress = uint16

/// Documentation from: http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
type Instruction =
    /// 0NNN:
    /// Jump to a machine code routine at NNN.
    /// This instruction is only used on the old computers on which Chip-8 was originally implemented. It is ignored by modern interpreters.
    | JumpToMachineCode of MemoryAddress
    /// 00E0:
    ///   Clear the display
    | ClearScreen
    /// 00EE:
    ///   The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
    | Return
    /// 1NNN:
    ///   The interpreter sets the program counter to nnn.
    | JumpTo of MemoryAddress
    /// 2NNN:
    ///   The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
    | CallSubroutine of MemoryAddress
    /// 3xNN:
    ///   The interpreter compares register vX to kk, and if they are equal, increments the program counter by 2.
    | SkipNextIfRegisterEquals of vX: Register * value: byte
    /// 4xNN:
    ///   The interpreter compares register vX to kk, and if they are not equal, increments the program counter by 2.
    | SkipNextIfRegisterNotEquals of vX: Register * value: byte
    /// 5xy0:
    ///   The interpreter compares register vX to register vY, and if they are equal, increments the program counter by 2.
    | SkipNextIfRegistersEqual of vX: Register * vY: Register
    /// 6xNN:
    ///   The interpreter puts the value kk into register vX.
    | SetRegister of vX: Register * value: byte
    /// 7xNN:
    ///   Adds the value kk to the value of register vX, then stores the result in vX. 
    | IncrementRegister of vX: Register * value: byte
    /// 8xy0:
    /// Set vX = vY.
    ///   Stores the value of register vY in register vX.
    | SetvXFromvY of vX: Register * vY: Register
    /// 8xy1:
    ///   Performs a bitwise OR on the values of vX and vY, then stores the result in vX. A bitwise OR compares the corrseponding bits from two values, and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0.
    | OR of vX: Register * vY: Register
    /// 8xy2:
    ///   Performs a bitwise AND on the values of vX and vY, then stores the result in vX. A bitwise AND compares the corrseponding bits from two values, and if both bits are 1, then the same bit in the result is also 1. Otherwise, it is 0.
    | AND of vX: Register * vY: Register
    /// 8xy3:
    ///   Performs a bitwise exclusive OR on the values of vX and vY, then stores the result in vX. An exclusive OR compares the corrseponding bits from two values, and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0.
    | XOR of vX: Register * vY: Register
    /// 8xy4:
    ///   The values of vX and vY are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in vX.
    | ADD of vX: Register * vY: Register
    /// 8xy5:
    ///   If vX > vY, then VF is set to 1, otherwise 0. Then vY is subtracted from vX, and the results stored in vX.
    | SUB of vX: Register * vY: Register
    /// 8xy6 - SHR vX {, vY}
    ///   If the least-significant bit of vX is 1, then VF is set to 1, otherwise 0. Then vX is divided by 2.
    | SHR of vX: Register * vY: Register
    /// 8xy7:
    ///   If vY > vX, then VF is set to 1, otherwise 0. Then vX is subtracted from vY, and the results stored in vX.
    | SUBN of vX: Register * vY: Register
    /// 8xyE - SHL vX {, vY}
    ///   If the most-significant bit of vX is 1, then VF is set to 1, otherwise to 0. Then vX is multiplied by 2.
    | SHL of vX: Register * vY: Register
    /// 9xy0:
    ///   The values of vX and vY are compared, and if they are not equal, the program counter is increased by 2.
    | SkipNextIfRegistersNotEqual of vX: Register * vY: Register
    /// ANNN:
    ///   The value of register I is set to nnn.
    | SetI of MemoryAddress
    /// BNNN
    ///   The program counter is set to nnn plus the value of V0.
    | JumpToPlusV0 of MemoryAddress
    /// CxNN
    ///   The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in vX. See instruction 8xy2 for more information on AND.
    | SetRegisterRandom of vX: Register * value: byte
    /// DxyN
    ///   The interpreter reads n bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen at coordinates (vX, vY). Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen. See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.
    | DrawSprite of vX: Register * vY: Register * height: byte
    /// Ex9E:
    ///   Checks the keyboard, and if the key corresponding to the value of vX is currently in the down position, PC is increased by 2.
    | SkipIfKeyPressed of vX: Register
    /// ExA1:
    ///   Checks the keyboard, and if the key corresponding to the value of vX is currently in the up position, PC is increased by 2.
    | SkipIfKeyNotPressed of vX: Register
    /// Fx07:
    ///   The value of DT is placed into vX.
    | GetDelayTime of vX: Register
    /// Fx0A:
    ///   All execution stops until a key is pressed, then the value of that key is stored in vX.
    | WaitForKey of vX: Register
    /// Fx15:
    ///   DT is set equal to the value of vX.
    | SetDelayTime of vX: Register
    /// Fx18:
    ///   ST is set equal to the value of vX.
    | SetST of vX: Register
    /// Fx1E:
    ///   The values of I and vX are added, and the results are stored in I.
    | IncrementI of vX: Register
    /// Fx29:
    ///   The value of I is set to the location for the hexadecimal sprite corresponding to the value of vX. See section 2.4, Display, for more information on the Chip-8 hexadecimal font.
    | SetIFromSprite of vX: Register
    /// Fx33:
    ///   The interpreter takes the decimal value of vX, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.
    | StoreBCD of vX: Register
    /// Fx55:
    ///   The interpreter copies the values of registers V0 through vX into memory, starting at the address in I.
    | StoreRegisters of vEnd: byte
    /// Fx65:
    ///   The interpreter reads values from memory starting at location I into registers V0 through vX.
    | ReadRegisters of vEnd: byte
    /// An opcode which failed to parse
    | Unrecognized of uint16
    with
    member this.Recognized() =
        match this with
        | Unrecognized _ -> false
        | _ -> true

let parseInstruction (opcode: uint16) =
    let pullAddr opcode =
        opcode &&& 0xFFFus
    let pullRegAndByte opcode =
        let vX = byte ((opcode &&& 0x0F00us) >>> 8)
        let value = byte (opcode &&& 0x00FFus)
        (vX, value)
    let pullRegs opcode =
        let vX = byte ((opcode &&& 0x0F00us) >>> 8)
        let vY = byte ((opcode &&& 0x00F0us) >>> 4)
        (vX, vY)
    let pullRegsAndNibble opcode =
        let (vX, vY) = pullRegs opcode
        let nibble = byte (opcode &&& 0x000Fus)
        (vX, vY, nibble)

    match opcode &&& 0xF000us with
    | 0x0000us ->
        match opcode with
        | 0x00E0us -> ClearScreen
        | 0x00EEus -> Return
        | _        -> JumpToMachineCode (pullAddr opcode)

    | 0x1000us -> JumpTo (pullAddr opcode)
    | 0x2000us -> CallSubroutine (pullAddr opcode)
    | 0x3000us -> SkipNextIfRegisterEquals (pullRegAndByte opcode)
    | 0x4000us -> SkipNextIfRegisterNotEquals (pullRegAndByte opcode)
    | 0x5000us -> SkipNextIfRegistersEqual (pullRegs opcode)
    | 0x6000us -> SetRegister (pullRegAndByte opcode)
    | 0x7000us -> IncrementRegister (pullRegAndByte opcode)
    | 0x8000us ->
        let (vX, vY, nibble) = pullRegsAndNibble opcode
        match nibble with
        | 0x0uy -> SetvXFromvY (vX, vY)
        | 0x1uy -> OR (vX, vY)
        | 0x2uy -> AND (vX, vY)
        | 0x3uy -> XOR (vX, vY)
        | 0x4uy -> ADD (vX, vY)
        | 0x5uy -> SUB (vX, vY)
        | 0x6uy -> SHR (vX, vY)
        | 0x7uy -> SUBN (vX, vY)
        | 0xEuy -> SHL (vX, vY)
        | _     -> Unrecognized opcode

    | 0x9000us -> SkipNextIfRegistersNotEqual (pullRegs opcode)
    | 0xA000us -> SetI (pullAddr opcode)
    | 0xB000us -> JumpToPlusV0 (pullAddr opcode)
    | 0xC000us -> SetRegisterRandom (pullRegAndByte opcode)
    | 0xD000us -> DrawSprite <| pullRegsAndNibble opcode

    | 0xE000us ->
        let (vX, b) = pullRegAndByte opcode
        match b with // b is the last two digits of the opcode
        | 0x9Euy -> SkipIfKeyPressed vX
        | 0xA1uy -> SkipIfKeyNotPressed vX
        | _      -> Unrecognized opcode

    | 0xF000us ->
        let vX = byte ((opcode &&& 0x0F00us) >>> 8)
        match (opcode &&& 0xF0FFus) with
        | 0xF007us -> GetDelayTime vX
        | 0XF00Aus -> WaitForKey vX
        | 0xF015us -> SetDelayTime vX
        | 0xF018us -> SetST vX
        | 0xF01Eus -> IncrementI vX
        | 0xF029us -> SetIFromSprite vX
        | 0xF033us -> StoreBCD vX
        | 0xF055us -> StoreRegisters vX
        | 0xF065us -> ReadRegisters vX
        | _        -> Unrecognized opcode

    | _        -> Unrecognized opcode

let toOpcode (bs: byte[]) =
    match bs with
    | [| x; y |] -> 
        ((uint16 x) <<< 8) ||| (uint16 y)
    | _ -> 
        failwith "Invalid byte array: instructions can only be two bytes"

type Chip8 =
    {
        PC: uint16
        I: uint16
        DelayTimer: byte
        SoundTimer: byte
        Stack: uint16 list
        Registers: Map<byte, byte> 
        Memory: byte[]
        Screen: byte[]
    }
    with
    static member screenSize = 2048
    static member memorySize = 4096
    static member create(rom: byte[]) =
        let memory = Array.create Chip8.memorySize 0uy
        for i in [0..(rom.Length - 1)] do
            memory.[i + 512] <- rom.[i]
        { 
            PC = 0x200us; I = 0us; DelayTimer = 0uy; SoundTimer = 0uy;
            Stack = []
            Screen = Array.create Chip8.screenSize 0uy
            Registers = (Map.empty, [0uy..15uy]) ||> List.fold (fun regs i -> regs.Add(i, 0uy))
            Memory = memory 
        }
    // member this.NextInstruction() = { this with PC = this.PC + 2us }
    // member this.SkipInstruction() = { this with PC = this.PC + 4us }
    // member this.JumpTo(address) = { this with PC = address }
    // member this.CallSubroutine(address) = { this with PC = address; Stack = this.PC::this.Stack }

let applyInstruction (instruction: Instruction) (c8: Chip8) =
    printfn "%A" instruction
    match instruction with
    | ClearScreen ->
        { c8 with PC     = c8.PC + 2us
                  Screen = Array.create Chip8.screenSize 0uy 
        }
    | Return ->
        match c8.Stack with
        | addr::stack ->
            // { c8 with PC    = List.head c8.Stack
            //           Stack = List.tail c8.Stack // todo: Just stack?
            // }
            { c8 with PC    = addr
                      Stack = stack // todo: Just stack?
            }
        | [] ->
            failwith "Return called in program while stack was empty"
    | JumpTo address ->
        { c8 with PC = address }
    | JumpToPlusV0 address ->
        { c8 with PC = address + uint16 c8.Registers.[0uy] }
    | CallSubroutine address ->
        { c8 with PC    = address
                  Stack = c8.PC::c8.Stack 
        }
    | SkipNextIfRegisterEquals (vX, value) ->
        if c8.Registers.[vX] = value then
            { c8 with PC = c8.PC + 4us }
        else
            { c8 with PC = c8.PC + 2us }
    | SkipNextIfRegisterNotEquals (vX, value) ->
        if c8.Registers.[vX] <> value then
            { c8 with PC = c8.PC + 4us }
        else
            { c8 with PC = c8.PC + 2us }
    | SkipNextIfRegistersEqual (vX, vY) ->
        if c8.Registers.[vX] = c8.Registers.[vY] then
            { c8 with PC = c8.PC + 4us }
        else
            { c8 with PC = c8.PC + 2us}
    | SkipNextIfRegistersNotEqual (vX, vY) ->
        if c8.Registers.[vX] <> c8.Registers.[vY] then
            { c8 with PC = c8.PC + 4us }
        else
            { c8 with PC = c8.PC + 2us}
    | SetRegister (vX, value) ->
        { c8 with PC        = c8.PC + 2us
                  Registers = c8.Registers.Add(vX, value) 
        }
    | IncrementRegister (vX, value) ->
        { c8 with  PC        = c8.PC + 2us
                   Registers = c8.Registers.Add(vX, c8.Registers.[vX] + value)
        }
    | SetvXFromvY (vX, vY) ->
        { c8 with PC = c8.PC + 2us
                  Registers = c8.Registers.Add(vX, c8.Registers.[vY])
        }
    | SetI address ->
        { c8 with PC = c8.PC + 2us
                  I  = address }
    | DrawSprite (vX, vY, height) ->
        printfn "%d (%d, %d) %d" c8.I vX vY height
        let sprite = c8.Memory.[int c8.I]
        let row = 0uy
        let x = c8.Registers.[vX]
        let y = c8.Registers.[vY]
        { c8 with PC = c8.PC + 2us }
    | _ ->
        { c8 with PC = c8.PC + 2us }

[<EntryPoint>]
let main argv =
    let romPath = "/home/dave/Downloads/pong.ch8"
    let romBytes = File.ReadAllBytes(romPath)
    let instructions = 
        romBytes 
        |> Array.chunkBySize 2
        |> Array.map toOpcode
        |> Array.mapi (fun addr instr -> 
            (uint16 (addr * 2) + uint16 512, parseInstruction instr)
        )
        |> Map.ofArray
        // |> Array.filter (fun (addr, instr) -> instr.Recognized())

    let mutable c8 = Chip8.create(romBytes)
    for i in [0..99] do
        let instruction = instructions.[c8.PC]
        c8 <- c8 |> applyInstruction instruction
        // c8 <- nextC8
    // for (_addr, instruction) in instructions do
    //     let (nextC8, command) = c8 |> applyInstruction instruction
    //     c8 <- nextC8
    0 // return an integer exit code
