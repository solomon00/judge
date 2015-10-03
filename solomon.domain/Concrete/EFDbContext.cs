using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Solomon.Domain.Concrete
{
    public class EFDbContext : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<UserProfileTeam> UserProfileTeam { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentFormat> TournamentFormats { get; set; }
        public DbSet<TournamentType> TournamentTypes { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<ProblemTag> ProblemTags { get; set; }
        public DbSet<ProblemType> ProblemTypes { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<SolutionTestResult> SolutionTestResults { get; set; }
        public DbSet<ProgrammingLanguage> ProgrammingLanguages { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<UserCategory> UserCategories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Prevents table names from being pluralized.
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<SolutionTestResult>().HasRequired(str => str.TestResult).WithMany().WillCascadeOnDelete(false);

            //modelBuilder.Entity<UserProfile>().HasMany<Team>(t => t.Teams).WithMany(u => u.Members).Map(
            //    x =>
            //    {
            //        x.ToTable("UserProfileTeam", "public");
            //    }
            //);
        }

    }

    public class EFDbContextInitializer : CreateDatabaseIfNotExists<EFDbContext>
    {
        protected override void Seed(EFDbContext context)
        {
            //IList<ProgrammingLanguage> defaultLanguages = new List<ProgrammingLanguage>();

            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.C, Title = "GNU C" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.CPP, Title = "GNU C++" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.CS, Title = "C# .NET" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.VB, Title = "VB .NET" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.Java, Title = "Java" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.Pascal, Title = "Free Pascal" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.Python, Title = "Python" });

            //foreach (ProgrammingLanguage std in defaultLanguages)
            //    context.ProgrammingLanguages.Add(std);
            
            //All standards will
            base.Seed(context);
        }
    }
}
