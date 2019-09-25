using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.Hubbub
{
    public class EventMap : List<EventField>
    {
        const string fileName = "eventmap.csv";
        public void Load()
        {
            this.Clear();
            using (StreamReader reader = new StreamReader(fileName))
            {
                reader.ReadLine(); // read header
                while(reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] spilits = line.Split(',');
                    EventField ef = new EventField() { Code = int.Parse(spilits[1]), Register = int.Parse(spilits[2]), BitValue = ushort.Parse(spilits[4]) };
                    this.Add(ef);
                }
            }
        }
    }

    public class EventField
    {
        public int Code { get; set; }
        public int Register { get; set; }
        public ushort BitValue { get; set; }
    }
}
