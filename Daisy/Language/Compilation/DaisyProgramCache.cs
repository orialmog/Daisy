using Ancestry.Daisy.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ancestry.Daisy.Language.Compilation
{
    public class DaisyProgramCache
    {
        private static object semaphore = new object();
        private static IDictionary<string, object> Cache = new Dictionary<string, object>();

        public static void Stash<T>(string code, IDaisyProgram<T> item)
        {
            lock(semaphore)
                Cache[code] = item;
        }
        public static IDaisyProgram<T> Get<T>(string code)
        {
            lock (semaphore)
            {
                if (Cache.ContainsKey(code))
                    return (IDaisyProgram<T>)Cache[code];
                return null;
            }
        }
    }
}
