using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace FreeKassa.Repository
{
    public class MarkedCodeRepository
    {
        private object _locker = new();
        
        public MarkedCodeRepository()
        {
            InitDirectory();
        }

        
        public T MarkedWorker<T> (Work work ,string code = "",List<string> codeList = null) where T : new()
        {
            lock (_locker)
            {

                var path = @"Marked\notSendMarked.txt";
                
                switch (work)
                {
                    case Work.Save:

                        var readStr = File.ReadAllText(path);

                        var str = new StringBuilder();

                        if (codeList != null && codeList.Count != 0)
                        {
                            foreach (var c in codeList)
                            {
                                str.Append($"{c}\n");
                            }
                        }

                        if (code != "")
                        {
                            str.Append(code);
                        }
                        
                        File.WriteAllText(path,  readStr + str.ToString());

                        return (T)(object)true;

                    case Work.Read:
                        
                        return (T)(object)File.ReadAllLines(path).ToList();
                        
                    case Work.Delete:
                        
                        var re = File.ReadAllLines(path).Where(s => !s.Contains(code));
                        File.WriteAllLines(path,re);
                        break;
                        
                    default:
                        throw new ArgumentOutOfRangeException(nameof(work), work, null);
                }
            }

            return new T();
        }
            
        
        private void InitDirectory()
        {
            if (Directory.Exists("Marked")) return;
            
            var path = Directory.CreateDirectory("Marked");
            var file = File.Create(@$"{path}\notSendMarked.txt");
            file.Close();
        }
    }

    public enum Work
    {
        Save,
        Read,
        Delete
    }
}