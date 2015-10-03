using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using System;
using System.Data;
using System.Linq;
using System.Threading;

namespace Solomon.Domain.Concrete
{
    public class EFRepository : IRepository
    {
        private EFDbContext context = new EFDbContext();

        public IQueryable<Comment> Comments
        {
            get { return context.Comments; }
        }
        public IQueryable<Tournament> Tournaments
        {
            get { return context.Tournaments; }
        }
        public IQueryable<Problem> Problems
        {
            get { return context.Problems; }
        }
        public IQueryable<ProblemTag> ProblemTags
        {
            get { return context.ProblemTags; }
        }
        public IQueryable<UserProfile> Users
        {
            get { return context.UserProfiles; }
        }
        public IQueryable<Team> Teams
        {
            get { return context.Teams; }
        }
        public IQueryable<UserProfileTeam> UserProfileTeam
        {
            get { return context.UserProfileTeam; }
        }
        public IQueryable<Solution> Solutions
        {
            get { return context.Solutions; }
        }
        public IQueryable<SolutionTestResult> SolutionTestResults
        {
            get { return context.SolutionTestResults; }
        }
        public IQueryable<ProgrammingLanguage> ProgrammingLanguages
        {
            get { return context.ProgrammingLanguages; }
        }
        public IQueryable<Country> Country
        {
            get { return context.Country; }
        }
        public IQueryable<City> City
        {
            get { return context.City; }
        }
        public IQueryable<Institution> Institutions
        {
            get { return context.Institutions; }
        }

        public bool IsUserOnline(UserProfile user)
        {
            if (user.LastAccessTime.HasValue && DateTime.Now.Subtract(user.LastAccessTime.Value).TotalMinutes < 10)
                return true;

            return false;
        }
        public bool IsUserOnline(int userId)
        {
            var user = context.UserProfiles.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                return IsUserOnline(user);
            }

            return false;
        }

        public void UpdateUserProfile(UserProfile UserProfile)
        {
            if (UserProfile.UserId == 0)
            {
                return;
            }
            else
            {
                context.Entry(UserProfile).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(UserProfile).Reload();

            return;
        }

        public int SaveSolution(Solution Solution)
        {
            if (Solution.SolutionID == 0)
            {
                Solution = context.Solutions.Add(Solution);
            }
            else
            {
                context.Entry(Solution).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Solution).Reload();

            return Solution.SolutionID;
        }

        public void MakeProgrammingLanguageAvailable(Solomon.TypesExtensions.ProgrammingLanguages PL)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            if (pl != null)
            {
                context.Entry(pl).Reload();
                pl.Available = true;
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }
            
            return;
        }
        public void MakeProgrammingLanguageUnavailable(Solomon.TypesExtensions.ProgrammingLanguages PL)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            
            if (pl != null)
            {
                context.Entry(pl).Reload();
                pl.Available = false;
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }

            return;
        }
        public void EnableProgrammingLanguage(Solomon.TypesExtensions.ProgrammingLanguages PL)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            if (pl != null)
            {
                context.Entry(pl).Reload();
                pl.Enable = true;
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }

            return;
        }
        public void DisableProgrammingLanguage(Solomon.TypesExtensions.ProgrammingLanguages PL)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            if (pl != null)
            {
                context.Entry(pl).Reload();
                pl.Enable = false;
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }

            return;
        }
        public void SetProgrammingLanguageName(Solomon.TypesExtensions.ProgrammingLanguages PL, string Name)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            if (pl != null)
            {
                context.Entry(pl).Reload();
                pl.Title = Name;
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }

            return;
        }
        public bool IsProgrammingLanguageEnable(Solomon.TypesExtensions.ProgrammingLanguages PL)
        {
            ProgrammingLanguage pl = context.ProgrammingLanguages.FirstOrDefault(l => l.ProgrammingLanguageID == PL);
            if (pl != null)
            {
                context.Entry(pl).Reload();
                return pl.Enable;
            }

            return false;
        }

        public int AddSolutionTestResult(SolutionTestResult SolutionTestResult)
        {
            if (SolutionTestResult.SolutionTestResultID == 0)
            {
                SolutionTestResult = context.SolutionTestResults.Add(SolutionTestResult);
            }
            else
            {
                context.Entry(SolutionTestResult).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(SolutionTestResult).Reload();
            
            return SolutionTestResult.SolutionTestResultID;
        }
        public void DeleteSolutionTestResult(SolutionTestResult SolutionTestResult)
        {
            context.Entry(SolutionTestResult).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteSolutionTestResult(int SolutionTestResultID)
        {
            SolutionTestResult solutionTestResult = context.SolutionTestResults.FirstOrDefault(str => str.SolutionTestResultID == SolutionTestResultID);

            if (solutionTestResult != null)
            {
                DeleteSolutionTestResult(solutionTestResult);
            }

            return;
        }
        public void DeleteSolutionTestResults(Solution Solution)
        {
            foreach (var item in context.SolutionTestResults.Where(str => str.SolutionID == Solution.SolutionID))
            {
                context.Entry(item).State = EntityState.Deleted;
            }

            context.SaveChanges();

            return;
        }
        public void DeleteSolutionTestResults(int SolutionID)
        {
            foreach (var item in context.SolutionTestResults.Where(str => str.SolutionID == SolutionID))
            {
                context.Entry(item).State = EntityState.Deleted;
            }

            context.SaveChanges();

            return;
        }

        public int AddComment(Comment Comment)
        {
            if (Comment.CommentID == 0)
            {
                context.Comments.Add(Comment);
            }
            else
            {
                context.Entry(Comment).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Comment).Reload();

            return Comment.CommentID;
        }
        public void DeleteComment(Comment Comment)
        {
            context.Entry(Comment).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteComment(int CommentID)
        {
            Comment comment = context.Comments.FirstOrDefault(c => c.CommentID == CommentID);

            if (comment != null)
            {
                DeleteComment(comment);
            }

            return;
        }

        public int AddTournament(Tournament Tournament)
        {
            if (Tournament.TournamentID == 0)
            {
                context.Tournaments.Add(Tournament);
            }
            else
            {
                context.Entry(Tournament).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Tournament).Reload();

            return Tournament.TournamentID;
        }
        public void DeleteTournament(Tournament Tournament)
        {
            context.Entry(Tournament).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteTournament(int TournamentID)
        {
            Tournament tournament = context.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament != null)
            {
                DeleteTournament(tournament);
            }

            return;
        }

        public int AddProblem(Problem Problem)
        {
            if (Problem.ProblemID == 0)
            {
                Problem = context.Problems.Add(Problem);
            }
            else
            {
                context.Entry(Problem).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Problem).Reload();

            return Problem.ProblemID;
        }
        public void DeleteProblem(Problem Problem)
        {
            context.Entry(Problem).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteProblem(int ProblemID)
        {
            Problem problem = context.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);

            if (problem != null)
            {
                DeleteProblem(problem);
            }

            return;
        }

        public int AddProblemTag(ProblemTag ProblemTag)
        {
            if (ProblemTag.ProblemTagID == 0)
            {
                ProblemTag = context.ProblemTags.Add(ProblemTag);
            }
            else
            {
                context.Entry(ProblemTag).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(ProblemTag).Reload();

            return ProblemTag.ProblemTagID;
        }
        public void DeleteProblemTag(ProblemTag ProblemTag)
        {
            context.Entry(ProblemTag).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteProblemTag(int ProblemTagID)
        {
            ProblemTag problemTag = context.ProblemTags.FirstOrDefault(p => p.ProblemTagID == ProblemTagID);

            if (problemTag != null)
            {
                DeleteProblemTag(problemTag);
            }

            return;
        }

        public int AddCity(City City)
        {
            if (City.CityID == 0)
            {
                City = context.City.Add(City);
            }
            else
            {
                context.Entry(City).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(City).Reload();

            return City.CityID;
        }
        public void DeleteCity(City City)
        {
            var users = context
                .UserProfiles
                .Where(u => u.CityID == City.CityID);

            foreach (var item in users)
            {
                item.CityID = null;
                context.Entry(item).State = EntityState.Modified;
            }

            context.Entry(City).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteCity(int CityID)
        {
            City city = context.City.FirstOrDefault(c => c.CityID == CityID);

            if (city != null)
            {
                DeleteCity(city);
            }

            return;
        }

        public int AddInstitution(Institution Institution)
        {
            if (Institution.InstitutionID == 0)
            {
                Institution = context.Institutions.Add(Institution);
            }
            else
            {
                context.Entry(Institution).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Institution).Reload();

            return Institution.InstitutionID;
        }
        public void DeleteInstitution(Institution Institution)
        {
            var users = context
                .UserProfiles
                .Where(u =>  u.InstitutionID == Institution.InstitutionID);

            foreach (var item in users)
            {
                item.InstitutionID = null;
                context.Entry(item).State = EntityState.Modified;
            }

            context.Entry(Institution).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteInstitution(int InstitutionID)
        {
            Institution institution = context.Institutions.FirstOrDefault(i => i.InstitutionID == InstitutionID);

            if (institution != null)
            {
                DeleteInstitution(institution);
            }

            return;
        }

        public int AddUserProfileTeam(UserProfileTeam UserProfileTeam)
        {
            if (UserProfileTeam.UserProfileTeamID == 0)
            {
                UserProfileTeam = context.UserProfileTeam.Add(UserProfileTeam);
            }
            else
            {
                context.Entry(UserProfileTeam).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(UserProfileTeam).Reload();

            return UserProfileTeam.UserProfileTeamID;
        }
        public void DeleteUserProfileTeam(UserProfileTeam UserProfileTeam)
        {
            context.Entry(UserProfileTeam).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteUserProfileTeam(int UserProfileTeamID)
        {
            UserProfileTeam userProfileTeam = context.UserProfileTeam.FirstOrDefault(ut => ut.UserProfileTeamID == UserProfileTeamID);

            if (userProfileTeam != null)
            {
                DeleteUserProfileTeam(userProfileTeam);
            }

            return;
        }

        public int AddTeam(Team Team)
        {
            if (Team.TeamID == 0)
            {
                Team = context.Teams.Add(Team);
            }
            else
            {
                context.Entry(Team).State = EntityState.Modified;
            }
            context.SaveChanges();
            context.Entry(Team).Reload();

            return Team.TeamID;
        }
        public void DeleteTeam(Team Team)
        {
            var users = context
                .UserProfileTeam
                .Where(ut => ut.TeamID == Team.TeamID);

            foreach (var item in users)
            {
                DeleteUserProfileTeam(item);
            }

            context.Entry(Team).State = EntityState.Deleted;

            context.SaveChanges();

            return;
        }
        public void DeleteTeam(int TeamID)
        {
            Team team = context.Teams.FirstOrDefault(t => t.TeamID == TeamID);

            if (team != null)
            {
                DeleteTeam(team);
            }

            return;
        }

        #region Bind methods
        public void BindProblemToTournament(int TournamentID, int ProblemID)
        {
            Tournament tournament = context.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                throw new EntityException("Invalid id of tournament");
            }
            Problem problem = context.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            if (problem == null)
            {
                throw new EntityException("Invalid id of problem");
            }

            tournament.Problems.Add(problem);
            context.SaveChanges();
        }

        public void UnbindProblemFromTournament(int TournamentID, int ProblemID)
        {
            Tournament tournament = context.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                throw new EntityException("Invalid id of tournament");
            }
            Problem problem = context.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            if (problem == null)
            {
                throw new EntityException("Invalid id of problem");
            }

            tournament.Problems.Remove(problem);
            context.SaveChanges();
        }

        public void BindUserToTournament(int TournamentID, int UserProfileID)
        {
            Tournament tournament = context.Tournaments.Include("Users").FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                throw new EntityException("Invalid id of tournament");
            }
            UserProfile user = context.UserProfiles.FirstOrDefault(u => u.UserId == UserProfileID);
            if (user == null)
            {
                throw new EntityException("Invalid id of user");
            }

            tournament.Users.Add(user);
            context.SaveChanges();
        }

        public void UnbindUserFromTournament(int TournamentID, int UserProfileID)
        {
            Tournament tournament = context.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                throw new EntityException("Invalid id of tournament");
            }
            UserProfile user = context.UserProfiles.FirstOrDefault(u => u.UserId == UserProfileID);
            if (user == null)
            {
                throw new EntityException("Invalid id of user");
            }

            tournament.Users.Remove(user);
            context.SaveChanges();
        }
        #endregion
    }
}
