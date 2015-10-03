using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.TypesExtensions
{
    [Serializable]
    public class ProblemInfo : ISerializable
    {
        public int ProblemID { get; set; }
        public bool IsCorrect { get; set; }
        public string Info { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string Name { get; set; }

        public ProblemInfo() { }

        public ProblemInfo(SerializationInfo info, StreamingContext context)
        {
            //Get the values from info and assign them to the appropriate properties
            ProblemID = (int)info.GetValue("ProblemID", typeof(int));
            IsCorrect = (bool)info.GetValue("IsCorrect", typeof(bool));
            Info = (string)info.GetValue("Info", typeof(string));
            LastModifiedTime = (DateTime)info.GetValue("LastModifiedTime", typeof(DateTime));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ProblemID", ProblemID);
            info.AddValue("IsCorrect", IsCorrect);
            info.AddValue("Info", Info);
            info.AddValue("LastModifiedTime", LastModifiedTime);
        }
    }
}
