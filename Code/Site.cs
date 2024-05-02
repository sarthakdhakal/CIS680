
using Microsoft.EntityFrameworkCore;
using NewDotnet.Context;
using Database = NewDotnet.DataLayer.Database;

namespace NewDotnet.Models
{
        public partial class OODBModel
        {
     
     
        public string[] GetNames(string starId)
            {
                var localAccount = _context.Accounts.FirstOrDefault(x => x.StarId == starId);

                string firstName;
                string lastName;

                // First try to get names locally
                if (localAccount != null)
                {
                    firstName = localAccount.FirstName;
                    lastName = localAccount.LastName;
                }
                else
                {
                    try
                    {
                        // Try to get the names from the database
                        firstName = _context.Database.SqlQueryRaw<string>("SELECT [FirstName] FROM .[Account] WHERE [UserID] = {0}", starId).FirstOrDefault() ?? "FirstName";
                        lastName = _context.Database.SqlQueryRaw<string>("SELECT [LastName] FROM [Account] WHERE [UserID] = {0}", starId).FirstOrDefault() ?? "LastName";
                    }
                    catch
                    {
                        firstName = "Orientation";
                        lastName = "User";
                    }
                }
                return [firstName, lastName];
            }

            public void CreateUser(string starId)
            {   var db = new Database(_context);
                string[] names = GetNames(starId);
                db.CreateUser(starId.ToLower(), names[0], names[1]);
                return;
            }

            public void CreateUserByTechId(string techId)
            {

                var starId = GetStarId(techId);
                if (starId == null)
                {
                    throw new ArgumentException(@"Invalid or nonexistent Tech ID.", nameof(starId));
                }
                CreateUser(starId);
            }

            public bool TryGetAccountByTechId(string techId, out Account account)
            {
                account = null;

                // Look for the tech ID
                string starId = GetStarId(techId);
                if (starId == null) return false;

                // Look for the account
                account = _context.Accounts.Where(x => x.StarId == starId).FirstOrDefault();
                // note: if the account is null, this will return false.
                return (account != null);
            }

            public string GetStarId(string techId)
            {
                return _context.Database.SqlQueryRaw<string>("SELECT [UserID] FROM [dbo].[Account] WHERE [TechID] = {0}", techId).FirstOrDefault();
            }
            public string GetTechId(string starId)
            {
                try
                {
                    return _context.Database.SqlQueryRaw<string>("SELECT [TechID] FROM [dbo].[Account] WHERE [UserID] = {0}", starId.ToLower()).FirstOrDefault();
                }
                catch
                {
                    return "N/A";
                }
            }
            public string GetEmailAddress(string starId)
            {
                return _context.Database.SqlQueryRaw<string>("SELECT [Email] FROM [dbo].[Account] WHERE [UserID] = {0}", starId.ToLower()).FirstOrDefault() ?? "email@example.com";
            }
        }
 }


