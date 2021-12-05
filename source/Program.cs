// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

class Program {
    static void Main( string[] args ) {

        void PressAThing() {
            Console.WriteLine( "\n Press a thing to continue..." );
            Console.ReadKey();
        }

        Console.Write( "\n\n--------------------------\n" );
        Console.WriteLine( "FF6 Encounter rate patcher" );
        Console.WriteLine( "github.com/JonathanDotCel" );
        Console.WriteLine( "--------------------------\n\n" );

        if ( args.Length == 0 ) {
            Console.WriteLine( "Error: Specify a filename, or just drag the .nes file onto this .exe...\n" );
            PressAThing();
            return;
        }

        string srcName = args[ 0 ];

        //
        // Read the file into RAM
        //

        Byte[] bytes;
        try {
            bytes = File.ReadAllBytes( srcName );
        } catch ( System.Exception e ) {
            Console.WriteLine( $"Error opening the file: {srcName}  Error:{e}" );
            PressAThing();
            return;
        }

        //
        // Check the file size
        //

        const int FSIZE_NOHEADER = 0x300000;
        const int FSIZE_HEADER = FSIZE_NOHEADER + 0x200;

        if (
            bytes.Length != FSIZE_HEADER
            && bytes.Length != FSIZE_NOHEADER
        ) {
            Console.WriteLine(
                $"Warning: Expected file size: 0x{FSIZE_HEADER.ToString( "X" )} or 0x{FSIZE_NOHEADER.ToString( "X" )}\n"
            );
            Console.WriteLine( $"Got: 0x{bytes.Length.ToString( "X" )}" );
        }

        // e.g. the rough area we want to write to
        if ( bytes.Length < 0xC4E0 ) {
            Console.WriteLine( "It's far too small, exiting\n" );
            PressAThing();
            return;
        } else {
            Console.WriteLine( "Trying anyway...\n" );
            PressAThing();
        }

        //
        // Check it looks vaguely like FF6
        //

        UInt16 Read16( UInt32 index, bool withOffset = false ) {

            if ( withOffset ) index += 0x200;

            // C# will promote to int, so convert back to short
            return (UInt16)(bytes[ index + 1 ] << 8 | bytes[ index ]);
        }

        void Write16( UInt32 index, UInt16 inValue, bool withOffset ) {

            if ( withOffset ) index += 0x200;

            bytes[ index + 1 ] = (byte)(inValue >> 8);
            bytes[ index ] = (byte)(inValue & 0xFF);

        }

        // skips 0xC2A5
        UInt32[] offsets = {
            0xC29F, 0xC2A1, 0xC2A3,         0xC2A7, 0xC2A9, 0xC2AB,
            0xC2BF, 0xC2C1, 0xC2C3, //0xC2C5, 0xC2C7, 0xC2C9, 0xC2CB  
            //not sure on those last ones yet, i'll let you know when i've finished the game :)
        };

        // First with no header...
        // compare 2 known byte values
        // Yep, they're totally unaligned
        UInt16 A = Read16( offsets[ 0 ] );
        UInt16 B = Read16( offsets[ 1 ] );

        bool valid = (A == 0xC0 && B == 0x60);
        bool hasHeader = false;

        if ( !valid ) {
            A = Read16( offsets[ 0 ], true );
            B = Read16( offsets[ 1 ], true );

            valid = (A == 0xC0 && B == 0x60);
            hasHeader = true;
        }

        if ( !valid ) {
            Console.WriteLine( "Doesn't seem to be FF6, or it's already patched\n" );
            Console.WriteLine( "Tried with and without a 0x200 header\n" );
            PressAThing();
            return;
        }

        Console.WriteLine( "\nSeems valid!\n" );

        //
        // Patch it!
        //
        foreach ( UInt32 addr in offsets ) {
            Write16( addr, 0x10, hasHeader );
        }

        //
        // Save it
        //

        string noExt = System.IO.Path.ChangeExtension( srcName, null );
        string ext = System.IO.Path.GetExtension( srcName );

        string outPath = noExt + "_encounterpatched" + ext;

        int counter = 2;
        while ( System.IO.File.Exists( outPath ) ) {

            outPath = noExt + "_enceounterpatched" + counter + ext;

        }

        Console.WriteLine( "Writing out to " + outPath );

        try {

            File.WriteAllBytes( outPath, bytes );

        } catch ( System.Exception e ) {
            Console.WriteLine( "Error! Encountered an error saving the file: " + e );
            PressAThing();
            return;
        }

        Console.WriteLine( "Success!" );
        PressAThing();

    }

}

