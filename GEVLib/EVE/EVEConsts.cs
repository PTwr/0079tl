using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.EVE
{
    internal static class EVEConsts
    {
        /// <summary>
        /// followed by 16bit sequential lineId
        /// </summary>
        public const ushort LineStartWord = 0x0001;
        /// <summary>
        /// Indicates end of self-contained chain of operands
        /// </summary>
        public const uint LineEndDword = 0x00040000;
        /// <summary>
        /// returns to previous block (eg. jumptable)
        /// </summary>
        public const uint ReturnDword = 0x0005FFFF;
        /// <summary>
        /// Should be final operand in EVE block
        /// </summary>
        public const uint FinalTerminatorDword = 0x0006FFFF;

        /// <summary>
        /// Followed by 16bit offset to 32bit segment to which execution will jump
        /// Then by 16bit sequential jumpid, followed by padding 0xFFFF (label for returns?)
        /// </summary>
        public const ushort JumpCode = 0x0013;
        /// <summary>
        /// followed by offset to 32bit segment on which jump table ends
        /// </summary>
        public const ushort JumpTableEndPointer = 0x0014;

        /// <summary>
        /// seems to indicate start of some functional segment of line
        /// </summary>
        public const uint EVECommandStartDword = 0x00030000;

        public const ushort PaddingCode = 0xFFFF;
        public const ushort ZeroCode = 0x0000;

        public static class EvcBlock
        {
            /// <summary>
            /// Followed by 16bit OFS id for EVC name
            /// </summary>
            public const ushort PlayEvcA = 0x00F9;
            /// <summary>
            /// Followed by 16bit OFS id for EVC name
            /// </summary>
            public const ushort PlayEvcB = 0x00FD;
        }

        public static class VoiceBlock
        {
            public const uint Opcode1 = EVECommandStartDword;
            /// <summary>
            /// followed by 16bit OFS Id to name of audio file
            /// </summary>
            public const ushort Opcode2 = 0x0115;
        }

        public static class TextBlock
        {
            /// <summary>
            /// followed by 16bit OFS id
            /// </summary>
            public const ushort OFSReferenceOpcode = 0x00C1;
            /// <summary>
            /// followed by 16bit unknown identical to other occurences of this opcode in codeblock
            /// maybe some sort of variable id?
            /// </summary>
            public const ushort OFSReferenceOpcode_B = 0x00C0;


            //Unknown, always after TextReferenceLine? Maybe display call
            //present in other lines, with way more opcodes
            public const uint TextLineB = 0x00290000;
            //Unknown, always after TextLineB? Maybe dispose call?
            public const uint TextLineC = 0x00C3FFFF;
        }
    }
}
