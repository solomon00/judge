using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.TypesExtensions
{
    public enum RequestCodes
    {
                                                // For all socket messages: 
                                                //      first 4 bytes is a length of message,
                                                //      second 4 bytes is a RequestCode.
        MainInfo = 0,                           // []
        CPUUsage = 10,                          // []
        SolutionFile = 20,                      // [4b ProblemID][4b SolutionID][4b PL][4b Tournament format][4b FileName length][FileName][File]
        ProblemFile = 30,                       // [4b FileName length][FileName][File]
        ReadyForReceivingProblem = 31,          // []
        EndOfReceivingProblem = 32,             // []
        DeleteProblem = 40,                     // [4b ProblemID]
        ProblemsInfo = 50,                      // []
        CompilerOptionsFile = 60,               // [4b FileName length][FileName][File]
        ReadyForReceivingCompilerOptions = 61,  // []
    }

    public enum ResponseCodes
    {
                                                // For all socket messages: 
                                                //      first 4 bytes is a length of message,
                                                //      second 4 bytes is a ResponseCode.
        MainInfo = 0,                           // [4b virt proc count][4b compilers count][4b for each compiler]
        CPUUsage = 10,                          // [4b cpu usage]
        SolutionFileChecked = 20,               // [4b solution id][problem type][result][score]  [time][memory][result]...
        ProblemFileReceived = 30,               // []
        ReadyForReceivingProblem = 31,          // []
        ProblemDeleted = 40,                    // []
        ProblemsInfo = 50,                      // [4b FileName length][FileName][File]
        CompilerOptionsFileReceived = 60,       // []
        ReadyForReceivingCompilerOptions = 61,  // []
    }

    public enum ProgrammingLanguages
    {
        C = 0,
        CPP = 10,
        VCPP = 11,
        Java = 20,
        CS = 30,
        VB = 40,
        Pascal = 50,
        Delphi = 51,
        ObjPas = 52,
        TurboPas = 53,
        Python = 60,
        Open = 100
    }

    public enum TestResults
    {
        OK = 0,
        WA = 1,
        PE = 2,
        FL = 3,
        CE = 4,
        TLE = 5,
        MLE = 6,
        RTE = 7,
        PS = 8,
        CHKP = 9,

        Waiting = 100,
        Compiling = 101,
        Executing = 102,

        Disqualified = 1000
    }

    public enum ProblemTypes
    {
        Standart = 0,
        Interactive = 10,
        Open = 20,
    }

    public enum TournamentTypes
    {
        Open = 0,
        Close = 1,
    }

    public enum TournamentFormats
    {
        ACM = 0,
        IOI = 10,
    }

    public enum UserCategories
    {
        None = 0,
        School = 5,
        Student = 10,
        Teacher = 20,

        Other = 100
    }
}
