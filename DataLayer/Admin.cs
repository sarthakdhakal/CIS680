using Microsoft.EntityFrameworkCore;
using NewDotnet.Context;
using NewDotnet.Models;

namespace NewDotnet.DataLayer
{
    public partial class Database
    {
 
        public UserResult[] ListUsers()
        {
            return _context.Users.Select(x => new UserResult { id = x.Id, userId = x.UserId, fullName = x.FirstName + " " + x.LastName, isAdmin = x.IsAdmin }).ToArray();
        }

        public void AddUser(string userId, string firstName, string lastName, string passwordClearText, bool IsAdmin)
        {
            
            // Add new user to database
            _context.Users.Add(new User
            {
                UserId = userId.ToLower().Trim(),
                FirstName = firstName,
                LastName = lastName,
                Password =BCrypt.Net.BCrypt.HashPassword(passwordClearText, 12, true),
                IsAdmin = IsAdmin
            });
            _context.Accounts.Add(new Account
            {
                StarId = userId.ToLower().Trim(),
                FirstName = firstName,
                LastName = lastName
            });
            _context.SaveChanges();
            if (IsAdmin)
            {
                m.SetUserFlag(userId.ToLower().Trim(), "admin");
            }
            _context.SaveChanges();
        }

        public void SetUserPassword(int id, string passwordClearText)
        {
            // Try to get user account
            var acct = _context.Users.FirstOrDefault(x => x.Id == id);
            if (acct == null) return;
            acct.Password = BCrypt.Net.BCrypt.HashPassword(passwordClearText, 12, true);
            _context.SaveChanges();
        }

        public void DeleteUser(int id)
        {

            // Get the user account associated with this user
            var acct = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (acct == null) return; // No user, can't delete.

            string userId = acct.UserId;
            // Delete any quizzes the user might have recorded
            _context.AccountQuizzes.RemoveRange(_context.AccountQuizzes.Where(x => x.StarId == userId));
            // Delete any assignments the user has
            _context.Assignments.RemoveRange(_context.Assignments.Where(x => x.StarId == userId));
            // Get account
            var thisAcct = _context.Accounts.Where(x => x.StarId == userId).First();
            thisAcct.Flags.Clear();
            // Delete the account
            _context.Accounts.Remove(thisAcct);
            // Delete the local account info 
            _context.Users.Remove(acct);

            _context.SaveChanges();
        }

        public void ResetUser(string userId, int playlistId)
        {
            // Delete items
            _context.AccountQuizzes.RemoveRange(_context.AccountQuizzes.Where(x => x.StarId == userId));

            // Get current assignment
            var thisAssignment = _context.Assignments.Where(x => x.StarId == userId && x.AssignedPlaylist == playlistId).FirstOrDefault();
            if (thisAssignment != null)
            {
                _context.Assignments.Remove(thisAssignment);
            }

            _context.Assignments.Add(new Assignment
            {
                StarId = userId,
                AssignedPlaylist = playlistId,
                StartTime = DateTime.Now,
                EndTime = null,
                CurrentPosition = 0,
                CurrentProgress = 0
            });

            // workaround for failed save
            bool saveFailed;
            do
            {
                saveFailed = false;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update the values of the entity that failed to save from the store 
                    ex.Entries.Single().Reload();
                }

            } while (saveFailed);
        }

        public AssignmentResult[] AssignUsers(string[] ids, int playlist, bool remove)
        {
            return ids.Select(x => AssignUser(x, playlist, remove)).ToArray();
        }

        public AssignmentResult[] EnrollUsers(string[] ids, int flagId, bool remove)
        {
            return ids.Select(x => EnrollUser(x, flagId, remove)).ToArray();
        }

        public Account findAccount(string starIdOrTechId)
        {
            
            // Look for a user account in the database matching this user.
            Account thisAccount =  _context.Accounts.FirstOrDefault(x => x.StarId == starIdOrTechId);

            // If we did not get an account, call out to the site to see if there's another way to get the account.
            // (e.g. lookup StarID by TechID)
            if (thisAccount == null)
            {
                bool success = m.TryGetAccountByTechId(starIdOrTechId, out thisAccount);
            }

            // If we STILL have no account, see if we can create a brand new one for the user.
            if (thisAccount == null)
            {
                try
                {
                    m.CreateUser(starIdOrTechId);
                    thisAccount = _context.Accounts.Where(x => x.StarId == starIdOrTechId).FirstOrDefault();
                }
                catch (Exception)
                {
                    thisAccount = null;
                }
            }

            // If we STILL!! don't have an account, try creating one by Tech ID.
            if (thisAccount == null)
            {
                try
                {
                    m.CreateUserByTechId(starIdOrTechId);
                    thisAccount =  _context.Accounts.Where(x => x.StarId == starIdOrTechId).FirstOrDefault();
                }
                catch (Exception)
                {
                    thisAccount = null;
                }
            }

            // We give up. For whatever reason this account refuses to be created.
            return thisAccount;
        }

        private AssignmentResult AssignUser(string id, int playlist, bool remove)
        {
           
            Account thisAccount = findAccount(id);

            // We give up.
            if (thisAccount == null)
            {
                // We did not find either a StarID or TechID that matched.
                return new AssignmentResult
                {
                    id = id,
                    firstName = "",
                    lastName = "",
                    resultText = $"Could not find '{id}' as either a TechID or StarID. No operation performed."
                };
            }

            // We now have the account. 
            // Let's see if their assignment already exists for this playlist.
            var thisAssignment = _context.Assignments.Where(x => x.StarId == thisAccount.StarId && x.AssignedPlaylist == playlist).FirstOrDefault();

            // Are we removing ?
            if (remove)
            {

                // If the assignment exists, just remove it.
                if (thisAssignment != null)
                {
                    _context.Assignments.Remove(thisAssignment);
                    _context.SaveChanges();
                    return new AssignmentResult
                    {
                        id = id,
                        firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                        lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                        resultText = "Removed assignment successfully."
                    };
                }

                // The assignment does not exist. Report it, but do nothing to the DB.
                return new AssignmentResult
                {
                    id = id,
                    firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                    lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                    resultText = "User not assigned to this orientation - no operation necessary."
                };

            }
            else
            {
                // Assigning
                // Does an assignment already exist?
                if (thisAssignment != null)
                {
                    return new AssignmentResult
                    {
                        id = id,
                        firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                        lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                        resultText = "User already assigned to this orientation - no operation necessary."
                    };
                }

                // Assign user.
                 _context.Assignments.Add(new Assignment
                {
                    StarId = thisAccount.StarId,
                    StartTime = DateTime.Now,
                    AssignedPlaylist = playlist,
                    EndTime = null,
                    CurrentPosition = 0,
                    CurrentProgress = 0
                });
                _context.SaveChanges();
                return new AssignmentResult
                {
                    id = id,
                    firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                    lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                    resultText = "Assignment added successfully."
                };
            }
        }

        private AssignmentResult EnrollUser(string id, int flagId, bool remove)
        {
          
            Flag f =   _context.Flags.FirstOrDefault(x => x.FlagId == flagId && x.FlagType == "cohort");

            var thisAccount = findAccount(id);

            if (thisAccount == null)
            {
                // We did not find either a StarID or TechID that matched.
                return new AssignmentResult
                {
                    id = id,
                    firstName = "",
                    lastName = "",
                    resultText = $"Could not find '{id}' as either a TechID or StarID. No operation performed."
                };
            }

            // We now have the account. 
            if (f == null)
            {
                return new AssignmentResult
                {
                    id = id,
                    firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                    lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                    resultText = $"Could not locate cohort ID {id}. Please report this error to the developers."
                };
            }

            // Are we removing ?
            if (remove)
            {

                // If the cohort is enrolled in, just remove it.
                if (thisAccount.Flags.Contains(f))
                {
                    thisAccount.Flags.Remove(f);
                    _context.SaveChanges();
                    return new AssignmentResult
                    {
                        id = id,
                        firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                        lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                        resultText = "Removed cohort successfully."
                    };
                }

                // The cohort is not enrolled. Report it, but do nothing to the DB.
                return new AssignmentResult
                {
                    id = id,
                    firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                    lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                    resultText = "User not enrolled in this cohort - no operation necessary."
                };

            }
            else
            {
                if (thisAccount.Flags.Contains(f))
                {
                    return new AssignmentResult
                    {
                        id = id,
                        firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                        lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                        resultText = "User already enrolled in this cohort - no operation necessary."
                    };
                }

                thisAccount.Flags.Add(f);
                _context.SaveChanges();

                return new AssignmentResult
                {
                    id = id,
                    firstName = m.GetNames(thisAccount.StarId)[0] ?? "N/A",
                    lastName = m.GetNames(thisAccount.StarId)[1] ?? "N/A",
                    resultText = "Enrolled into cohort successfully."
                };
            }
        }
    }

    public class AssignmentResult
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string resultText { get; set; }
    }

    public class UserCohortResult
    {
        public string userId { get; set; }
        public string techId { get; set; }
        public string fullName { get; set; }
        public int[] cohorts { get; set; }
    }

    public class UserResult
    {
        public int id { get; set; }
        public string userId { get; set; }
        public string fullName { get; set; }
        public bool isAdmin { get; set; }
    }
}
