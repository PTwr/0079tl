using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class LuaTests
    {
        [Fact]
        public void MinifyLua()
        {
            string lua = @"
aaaaa

--[[
block comment
]]

--full line comment
zzzz --inline comment at end of line

    intendation

unnecessary whitespace at the end          

equals sign surrounded by spaces = blah blah
not equals sign surrounded by spaces ~= blah blah

bbbbb
";
            string expected = @"aaaaa
zzzz
intendation
unnecessary whitespace at the end
equals sign surrounded by spaces=blah blah
not equals sign surrounded by spaces~=blah blah
bbbbb";
            string minified = lua.MinifyLua();

            Assert.Equal(expected, minified);
        }
    }
}
