using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cantonese.Model
{
    //用于调用百度翻译api的jason反解析
    class LanguageModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<TranslationResult> Trans_result { get; set; }
    }
    class TranslationResult
    {
        public string src { get; set; }
        public string dst { get; set; }
    }
}
