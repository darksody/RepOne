using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarcasmDetection
{
    class Pattern
    {
        public string pattern;
        public int level;
        public string sentence;

        public Pattern(string pattern, int level)
        {
            this.pattern = pattern;
            this.level = level;
        }
    }
}
