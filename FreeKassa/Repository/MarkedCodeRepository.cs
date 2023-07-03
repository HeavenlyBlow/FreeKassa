using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AtolDriver.Models;
using Microsoft.Extensions.Primitives;

namespace FreeKassa.Repository
{
    public class MarkedCodeRepository
    {

        private const string _path = @"Marked\notSendMarked.txt";

        public MarkedCodeRepository()
        {
            InitDirectory();
        }
        
        public void Delete(string mark)
        {
            var collection = File.ReadAllLines(_path).Where(c => !c.Equals(mark)).ToList();
            File.WriteAllLines(_path, collection);
        }

        public List<MarkingCheckModel> Read()
        {
            var list = File.ReadAllLines(_path).ToList();

            if (list.Count == 0)
                return new List<MarkingCheckModel>();

            var answer = new List<MarkingCheckModel>();
            
            list.ForEach(c => answer.Add(
                new MarkingCheckModel()
                {
                    CheckIter = 0,
                    Marking = c
                }));

            return answer;
        }

        public void Save(string code = "" ,List<string> marks = null)
        {
            var readAllText = File.ReadAllText(_path);
            var str = new StringBuilder();

            if (marks != null && marks.Count != 0)
            {
                foreach (var c in marks)
                {
                    str.Append($"{c}\n");
                }
            }
            
            if (code != "")
            {
                str.Append(code);
            }
                        
            File.WriteAllText(_path,  readAllText + str.ToString());
        }


        private static void InitDirectory()
        {
            if (Directory.Exists("Marked")) return;
            
            var path = Directory.CreateDirectory("Marked");
            var file = File.Create(@$"{path}\notSendMarked.txt");
            file.Close();
        }
    }
}