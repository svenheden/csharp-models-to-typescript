using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace TestNamespace
{
    /// <summary>
    /// Sample class comment.
    /// </summary>
    public class TestClass
    {
        /// <summary>
        /// Sample comment.
        /// </summary>
        public int IntProperty { get; set; }

        [Obsolete("obsolete test prop")]
        public string StringProperty { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTimeProperty { get; set; }

        public bool BooleanProperty { get; set; }
    }

    public enum TestEnum {
        A = 1,              // decimal: 1
        B = 1_002,          // decimal: 1002
        C = 0b011,          // binary: 3 in decimal
        D = 0b_0000_0100,   // binary: 4 in decimal
        E = 0x005,          // hexadecimal: 5 in decimal
        F = 0x000_01a,      // hexadecimal: 26 in decimal
        [Obsolete("obsolete test enum")]
        G                   // 27 in decimal
    }
}
