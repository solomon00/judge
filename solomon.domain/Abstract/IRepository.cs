using Solomon.Domain.Entities;
using System.Linq;

namespace Solomon.Domain.Abstract
{
    public interface IRepository
    {
        IQueryable<Comment> Comments { get; }
        IQueryable<Tournament> Tournaments { get; }
        IQueryable<Problem> Problems { get; }
        IQueryable<ProblemTag> ProblemTags { get; }
        IQueryable<UserProfile> Users { get; }
        IQueryable<Team> Teams { get; }
        IQueryable<UserProfileTeam> UserProfileTeam { get; }
        IQueryable<Solution> Solutions { get; }
        IQueryable<SolutionTestResult> SolutionTestResults { get; }
        IQueryable<ProgrammingLanguage> ProgrammingLanguages { get; }
        IQueryable<Country> Country { get; }
        IQueryable<City> City { get; }
        IQueryable<Institution> Institutions { get; }

        bool IsUserOnline(int userId);
        bool IsUserOnline(UserProfile user);

        void UpdateUserProfile(UserProfile UserProfile);

        int SaveSolution(Solution Solution);

        void MakeProgrammingLanguageAvailable(Solomon.TypesExtensions.ProgrammingLanguages PL);
        void MakeProgrammingLanguageUnavailable(Solomon.TypesExtensions.ProgrammingLanguages PL);
        void EnableProgrammingLanguage(Solomon.TypesExtensions.ProgrammingLanguages PL);
        void DisableProgrammingLanguage(Solomon.TypesExtensions.ProgrammingLanguages PL);
        void SetProgrammingLanguageName(Solomon.TypesExtensions.ProgrammingLanguages PL, string Name);
        bool IsProgrammingLanguageEnable(Solomon.TypesExtensions.ProgrammingLanguages PL);

        int AddSolutionTestResult(SolutionTestResult SolutionTestResult);
        void DeleteSolutionTestResult(SolutionTestResult SolutionTestResult);
        void DeleteSolutionTestResult(int SolutionTestResultID);
        void DeleteSolutionTestResults(Solution Solution);
        void DeleteSolutionTestResults(int SolutionID);

        int AddComment(Comment Comment);
        void DeleteComment(Comment Comment);
        void DeleteComment(int CommentID);

        int AddTournament(Tournament Tournament);
        void DeleteTournament(Tournament Tournament);
        void DeleteTournament(int TournamentID);

        int AddProblem(Problem Problem);
        void DeleteProblem(Problem Problem);
        void DeleteProblem(int ProblemID);

        int AddProblemTag(ProblemTag ProblemTag);
        void DeleteProblemTag(ProblemTag ProblemTag);
        void DeleteProblemTag(int ProblemTagID);

        int AddCity(City City);
        void DeleteCity(City City);
        void DeleteCity(int CityID);

        int AddInstitution(Institution Institution);
        void DeleteInstitution(Institution Institution);
        void DeleteInstitution(int InstitutionID);

        int AddUserProfileTeam(UserProfileTeam UserProfileTeam);
        void DeleteUserProfileTeam(UserProfileTeam UserProfileTeam);
        void DeleteUserProfileTeam(int UserProfileTeamID);

        int AddTeam(Team Team);
        void DeleteTeam(Team Team);
        void DeleteTeam(int TeamID);

        void BindProblemToTournament(int TournamentID, int ProblemID);
        void UnbindProblemFromTournament(int TournamentID, int ProblemID);
        void BindUserToTournament(int TournamentID, int UserProfileID);
        void UnbindUserFromTournament(int TournamentID, int UserProfileID);
    }
}
