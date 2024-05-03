using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flip.Translation.Api
{
    public interface ITranslateApi
    {
        public Task<TranslationModel?> GetTranslate(string text,string from,string to);
        public void CancelOperation();
        public long GetPingToApi();
        public Task<bool?> CheckApi();
    }
}
