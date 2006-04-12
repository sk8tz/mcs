//
// System.Web.Security.SqlMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005,2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Web.Configuration;
using System.Security.Cryptography;

namespace System.Web.Security {
	public class SqlMembershipProvider : MembershipProvider {

		const int SALT_BYTES = 16;
		DateTime DefaultDateTime = new DateTime (1754,1,1).ToUniversalTime();

		bool enablePasswordReset;
		bool enablePasswordRetrieval;
		int maxInvalidPasswordAttempts;
		MembershipPasswordFormat passwordFormat;
		bool requiresQuestionAndAnswer;
		bool requiresUniqueEmail;
		int minRequiredNonAlphanumericCharacters;
		int minRequiredPasswordLength;
		int passwordAttemptWindow;
		string passwordStrengthRegularExpression;
		TimeSpan userIsOnlineTimeWindow;

		ConnectionStringSettings connectionString;
		DbProviderFactory factory;
		DbConnection connection;

		string applicationName;
		
		byte[] init_vector;

		static object lockobj = new object();

		void InitConnection ()
		{
			if (connection == null) {
				lock (lockobj) {
					if (connection != null)
						return;

					factory = ProvidersHelper.GetDbProviderFactory (connectionString.ProviderName);
					connection = factory.CreateConnection();
					connection.ConnectionString = connectionString.ConnectionString;

					connection.Open ();
				}
			}
		}

		void AddParameter (DbCommand command, string parameterName, string parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = ParameterDirection.Input;
			command.Parameters.Add (dbp);
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (",") != -1)
				throw new ArgumentException (String.Format ("invalid format for {0}", pName));
		}

		string HashAndBase64Encode (string s, byte[] salt)
		{
			byte[] tmp = Encoding.UTF8.GetBytes (s);

			byte[] hashedBytes = new byte[salt.Length + tmp.Length];
			Array.Copy (salt, hashedBytes, salt.Length);
			Array.Copy (tmp, 0, hashedBytes, salt.Length, tmp.Length);

			MembershipSection section = (MembershipSection)WebConfigurationManager.GetSection ("system.web/membership");
			string alg_type = section.HashAlgorithmType;
			if (alg_type == "")
				alg_type = "SHA1";
			HashAlgorithm alg = HashAlgorithm.Create (alg_type);
			hashedBytes = alg.ComputeHash (hashedBytes);

			return Convert.ToBase64String (hashedBytes);
		}
		
		string EncryptAndBase64Encode (string s)
		{
			MachineKeySection section = (MachineKeySection)WebConfigurationManager.GetSection ("system.web/machineKey");

			if (section.DecryptionKey.StartsWith ("AutoGenerate"))
				throw new Exception ("You must explicitly specify a decryption key in the <machineKey> section when using encrypted passwords.");

			string alg_type = section.Decryption;
			if (alg_type == "Auto")
				alg_type = "AES";

			SymmetricAlgorithm alg = null;
			if (alg_type == "AES")
				alg = Rijndael.Create ();
			else if (alg_type == "3DES")
				alg = TripleDES.Create ();
			else
				throw new Exception (String.Format ("Unsupported decryption attribute '{0}' in <machineKey> configuration section", alg_type));

			ICryptoTransform encryptor = alg.CreateEncryptor (section.DecryptionKey192Bits, init_vector);

			byte[] result = Encoding.UTF8.GetBytes (s);
			result = encryptor.TransformFinalBlock (result, 0, result.Length);

			return Convert.ToBase64String (result);
		}

		[MonoTODO]
		public override bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPwdQuestion, string newPwdAnswer)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUser CreateUser (string username,
							   string password,
							   string email,
							   string pwdQuestion,
							   string pwdAnswer,
							   bool isApproved,
							   object providerUserKey,
							   out MembershipCreateStatus status)
		{
			if (username != null) username = username.Trim ();
			if (password != null) password = password.Trim ();
			if (email != null) email = email.Trim ();
			if (pwdQuestion != null) pwdQuestion = pwdQuestion.Trim ();
			if (pwdAnswer != null) pwdAnswer = pwdAnswer.Trim ();

			/* some initial validation */
			if (username == null || username.Length == 0 || username.Length > 256 || username.IndexOf (",") != -1) {
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}
			if (password == null || password.Length == 0 || password.Length > 128) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}
			if (RequiresUniqueEmail && (email == null || email.Length == 0)) {
				status = MembershipCreateStatus.InvalidEmail;
				return null;
			}
			if (RequiresQuestionAndAnswer &&
			    (pwdQuestion == null ||
			     pwdQuestion.Length == 0 || pwdQuestion.Length > 256)) {
				status = MembershipCreateStatus.InvalidQuestion;
				return null;
			}
			if (RequiresQuestionAndAnswer &&
			    (pwdAnswer == null ||
			     pwdAnswer.Length == 0 || pwdAnswer.Length > 128)) {
				status = MembershipCreateStatus.InvalidAnswer;
				return null;
			}
			if (providerUserKey != null && ! (providerUserKey is Guid)) {
				status = MembershipCreateStatus.InvalidProviderUserKey;
				return null;
			}

			/* encode our password/answer using the
			 * "passwordFormat" configuration option */
			string passwordSalt = "";

			RandomNumberGenerator rng = RandomNumberGenerator.Create ();

			switch (PasswordFormat) {
			case MembershipPasswordFormat.Hashed:
				byte[] salt = new byte[16];
				rng.GetBytes (salt);
				passwordSalt = Convert.ToBase64String (salt);
				password = HashAndBase64Encode (password, salt);
				if (RequiresQuestionAndAnswer)
					pwdAnswer = HashAndBase64Encode (pwdAnswer, salt);
				break;
			case MembershipPasswordFormat.Encrypted:
				password = EncryptAndBase64Encode (password);
				break;
			case MembershipPasswordFormat.Clear:
			default:
				break;
			}

			/* make sure the hashed/encrypted password and
			 * answer are still under 128 characters. */
			if (password.Length > 128) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (RequiresQuestionAndAnswer) {
				if (pwdAnswer.Length > 128) {
					status = MembershipCreateStatus.InvalidAnswer;
					return null;
				}
			}

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			try {

				Guid applicationId;
				Guid userId;

				/* get the application id since it seems that inside transactions we
				   can't insert using subqueries.. */

				commandText = @"
SELECT ApplicationId
  FROM dbo.aspnet_Applications
 WHERE dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
";
				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				reader.Read ();
				applicationId = reader.GetGuid (0);
				reader.Close ();

				/* check for unique username, email and
				 * provider user key, if applicable */

				commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE u.LoweredUserName = LOWER(@UserName)
   AND u.ApplicationId = a.ApplicationId
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (0 != (int)command.ExecuteScalar()) {
					status = MembershipCreateStatus.DuplicateUserName;
					trans.Rollback ();
					return null;
				}


				if (requiresUniqueEmail) {
					commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Membership, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.Email = @Email
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "Email", email);
					AddParameter (command, "ApplicationName", ApplicationName);

					if (0 != (int)command.ExecuteScalar()) {
						status = MembershipCreateStatus.DuplicateEmail;
						trans.Rollback ();
						return null;
					}
		 		}

				if (providerUserKey != null) {
					commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Membership, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.UserId = @ProviderUserKey
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "Email", email);
					AddParameter (command, "ApplicationName", ApplicationName);

					if (0 != (int)command.ExecuteScalar()) {
						status = MembershipCreateStatus.DuplicateProviderUserKey;
						trans.Rollback ();
						return null;
					}
				}

				/* first into the Users table */
				commandText = @"
INSERT into dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, LastActivityDate)
VALUES (@ApplicationId, NEWID(), @UserName, LOWER(@UserName), GETDATE())
";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationId", applicationId.ToString());

				if (command.ExecuteNonQuery() != 1) {
					status = MembershipCreateStatus.UserRejected; /* XXX */
					trans.Rollback ();
					return null;
				}

				/* then get the newly created userid */

				commandText = @"
SELECT UserId
  FROM dbo.aspnet_Users
 WHERE dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
";
				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);

				reader = command.ExecuteReader ();
				reader.Read ();
				userId = reader.GetGuid (0);
				reader.Close ();

				/* then insert into the Membership table */
				commandText = String.Format (@"
INSERT into dbo.aspnet_Membership
VALUES (@ApplicationId,
        @UserId,
        @Password, @PasswordFormat, @PasswordSalt,
        NULL,
        {0}, {1},
        {2}, {3},
        0, 0,
        GETDATE(), GETDATE(), @DefaultDateTime,
        @DefaultDateTime,
        0, @DefaultDateTime, 0, @DefaultDateTime, NULL)",
							     email == null ? "NULL" : "@Email",
							     email == null ? "NULL" : "LOWER(@Email)",
							     pwdQuestion == null ? "NULL" : "@PasswordQuestion",
							     pwdAnswer == null ? "NULL" : "@PasswordAnswer");

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "ApplicationId", applicationId.ToString());
				AddParameter (command, "UserId", userId.ToString());
				if (email != null)
					AddParameter (command, "Email", email);
				AddParameter (command, "Password", password);
				AddParameter (command, "PasswordFormat", ((int)PasswordFormat).ToString());
				AddParameter (command, "PasswordSalt", passwordSalt);
				if (pwdQuestion != null)
					AddParameter (command, "PasswordQuestion", pwdQuestion);
				if (pwdAnswer != null)
					AddParameter (command, "PasswordAnswer", pwdAnswer);
				AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

				if (command.ExecuteNonQuery() != 1) {
					status = MembershipCreateStatus.UserRejected; /* XXX */
					return null;
				}

				trans.Commit ();

				status = MembershipCreateStatus.Success;

				return GetUser (username, false);
			}
			catch {
				status = MembershipCreateStatus.ProviderError;
				trans.Rollback ();
				return null;
			}
		}
		
		[MonoTODO]
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			CheckParam ("username", username, 256);

			if (deleteAllRelatedData) {
				/* delete everything from the
				 * following features as well:
				 *
				 * Roles
				 * Profile
				 * WebParts Personalization
				 */
			}

			DbTransaction trans = connection.BeginTransaction ();

			DbCommand command;
			string commandText;

			InitConnection();

			try {
				/* delete from the Membership table */
				commandText = @"
DELETE dbo.aspnet_Membership
  FROM dbo.aspnet_Membership, dbo.aspnet_Users, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.LoweredUserName = LOWER (@UserName)
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (1 != command.ExecuteNonQuery())
					throw new ProviderException ("failed to delete from Membership table");

				/* delete from the User table */
				commandText = @"
DELETE dbo.aspnet_Users
  FROM dbo.aspnet_Users, dbo.aspnet_Applications
 WHERE dbo.aspnet_Users.LoweredUserName = LOWER (@UserName)
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (1 != command.ExecuteNonQuery())
					throw new ProviderException ("failed to delete from User table");

				trans.Commit ();

				return true;
			}
			catch {
				trans.Rollback ();
				return false;
			}
		}
		
		[MonoTODO]
		public virtual string GeneratePassword ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			CheckParam ("emailToMatch", emailToMatch, 256);

			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND m.Email LIKE @Email
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "Email", emailToMatch);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
		}

		[MonoTODO]
		public override MembershipUserCollection FindUsersByName (string nameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			CheckParam ("nameToMatch", nameToMatch, 256);

			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserName LIKE @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", nameToMatch);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
		}
		
		[MonoTODO]
		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
		}

		MembershipUserCollection BuildMembershipUserCollection (DbCommand command, int pageIndex, int pageSize, out int totalRecords)
		{
			DbDataReader reader = null;
			try {
				int num_read = 0;
				int num_added = 0;
				int num_to_skip = pageIndex * pageSize;
				MembershipUserCollection users = new MembershipUserCollection ();
				reader = command.ExecuteReader ();
				while (reader.Read()) {
					if (num_read >= num_to_skip) {
						if (num_added < pageSize) {
							users.Add (GetUserFromReader (reader));
							num_added ++;
						}
						num_read ++;
					}
				}
				totalRecords = num_read;
				return users;
			}
			catch {
				totalRecords = 0;
				return null; /* should we let the exception through? */
			}
			finally {
				if (reader != null)
					reader.Close();
			}
		}
		
		
		[MonoTODO]
		public override int GetNumberOfUsersOnline ()
		{
			string commandText;

			InitConnection();

			commandText = @"
SELECT COUNT (*)
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND DATEADD(m,@UserIsOnlineTimeWindow,dbo.aspnet_Users.LastActivityDate) >= GETDATE()
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserIsOnlineTimeWindow", userIsOnlineTimeWindow.Minutes.ToString());
			AddParameter (command, "ApplicationName", ApplicationName);

			try { 
				return (int)command.ExecuteScalar ();
			}
			catch {
				return -1;
			}
		}
		
		[MonoTODO]
		public override string GetPassword (string username, string answer)
		{
			/* do the actual validation */

			/* if the validation succeeds:
			   
			   set LastLoginDate to DateTime.Now
			   set FailedPasswordAnswerAttemptCount to 0
			   set FailedPasswordAnswerAttemptWindowStart to DefaultDateTime
			*/

			/* if validation fails:

			   if (FailedPasswordAnswerAttemptWindowStart - DateTime.Now < PasswordAttemptWindow)
			     increment FailedPasswordAnswerAttemptCount
			   FailedPasswordAnswerAttemptWindowStart = DateTime.Now
			   if (FailedPasswordAnswerAttemptCount > MaxInvalidPasswordAttempts)
			     set IsLockedOut = true.
			     set LastLockoutDate = DateTime.Now
			*/
			throw new NotImplementedException ();
		}

		MembershipUser GetUserFromReader (DbDataReader reader)
		{
			return new MembershipUser (this.Name, /* XXX is this right?  */
						   reader.GetString (0), /* name */
						   reader.GetGuid (1), /* providerUserKey */
						   reader.IsDBNull (2) ? null : reader.GetString (2), /* email */
						   reader.IsDBNull (3) ? null : reader.GetString (3), /* passwordQuestion */
						   reader.IsDBNull (4) ? null : reader.GetString (4), /* comment */
						   reader.GetBoolean (5), /* isApproved */
						   reader.GetBoolean (6), /* isLockedOut */
						   reader.GetDateTime (7), /* creationDate */
						   reader.GetDateTime (8), /* lastLoginDate */
						   reader.GetDateTime (9), /* lastActivityDate */
						   reader.GetDateTime (10), /* lastPasswordChangedDate */
						   reader.GetDateTime (11) /* lastLockoutDate */);
		}

		MembershipUser BuildMembershipUser (DbCommand query, bool userIsOnline)
		{
			DbDataReader reader = null;
			try {
				reader = query.ExecuteReader ();
				if (!reader.Read ())
					return null;

				MembershipUser user = GetUserFromReader (reader);

				if (user != null && userIsOnline) {

					string commandText;
					DbCommand command;

					commandText = @"
UPDATE dbo.aspnet_Users u, dbo.aspnet_Application a
   SET u.LastActivityDate = GETDATE()
 WHERE u.ApplicationId = a.ApplicationId
   AND u.UserName = @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);

					command.ExecuteNonQuery();
				}

				return user;
			}
			catch {
				return null; /* should we let the exception through? */
			}
			finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[MonoTODO]
		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			CheckParam ("username", username, 256);

			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserName = @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUser u = BuildMembershipUser (command, userIsOnline);

			return u;
		}
		
		[MonoTODO]
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserId = @UserKey
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserKey", providerUserKey.ToString());
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUser u = BuildMembershipUser (command, userIsOnline);

			return u;
		}
		
		[MonoTODO]
		public override string GetUserNameByEmail (string email)
		{
			CheckParam ("email", email, 256);

			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND m.Email = @Email
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "Email", email);
			AddParameter (command, "ApplicationName", ApplicationName);

			try {
				DbDataReader reader = command.ExecuteReader ();
				string rv = null;
				while (reader.Read())
					rv = reader.GetString(0);
				reader.Close();
				return rv;
			}
			catch {
				return null; /* should we allow the exception through? */
			}
		}

		bool GetBoolConfigValue (NameValueCollection config, string name, bool def)
		{
			bool rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = Boolean.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be true or false", name), e); }
			}
			return rv;
		}

		int GetIntConfigValue (NameValueCollection config, string name, int def)
		{
			int rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = Int32.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be an integer", name), e); }
			}
			return rv;
		}

		int GetEnumConfigValue (NameValueCollection config, string name, Type enumType, int def)
		{
			int rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = (int)Enum.Parse (enumType, val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be one of the following values: {1}", name, String.Join (",", Enum.GetNames (enumType))), e); }
			}
			return rv;
		}

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string rv = def;
			string val = config[name];
			if (val != null)
				rv = val;
			return rv;
		}
		
		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			base.Initialize (name, config);

			applicationName = GetStringConfigValue (config, "applicationName", "/");
			enablePasswordReset = GetBoolConfigValue (config, "enablePasswordReset", true);
			enablePasswordRetrieval = GetBoolConfigValue (config, "enablePasswordRetrieval", false);
			requiresQuestionAndAnswer = GetBoolConfigValue (config, "requiresQuestionAndAnswer", true);
			requiresUniqueEmail = GetBoolConfigValue (config, "requiresUniqueEmail", false);
			passwordFormat = (MembershipPasswordFormat)GetEnumConfigValue (config, "passwordFormat", typeof (MembershipPasswordFormat),
										       (int)MembershipPasswordFormat.Hashed);
			maxInvalidPasswordAttempts = GetIntConfigValue (config, "maxInvalidPasswordAttempts", 5);
			minRequiredPasswordLength = GetIntConfigValue (config, "minRequiredPasswordLength", 7);
			minRequiredNonAlphanumericCharacters = GetIntConfigValue (config, "minRequiredNonAlphanumericCharacters", 1);
			passwordAttemptWindow = GetIntConfigValue (config, "passwordAttemptWindow", 10);
			passwordStrengthRegularExpression = GetStringConfigValue (config, "passwordStrengthRegularExpression", "");

			MembershipSection section = (MembershipSection)WebConfigurationManager.GetSection ("system.web/membership");
			
			userIsOnlineTimeWindow = section.UserIsOnlineTimeWindow;

			/* come up with an init_vector for encryption algorithms */
			// IV is 8 bytes long for 3DES
			init_vector = new byte[8];
			int len = applicationName.Length;
			for (int i = 0; i < 8; i++) {
				if (i >= len)
					break;

				init_vector [i] = (byte) applicationName [i];
			}

			string connectionStringName = config["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName];
		}
		
		[MonoTODO]
		public override string ResetPassword (string username, string answer)
		{
			throw new NotImplementedException ();
		}
		
		public override void UpdateUser (MembershipUser user)
		{
			if (user == null) throw new ArgumentNullException ("user");
			if (user.UserName == null) throw new ArgumentNullException ("user.UserName");
			if (RequiresUniqueEmail && user.Email == null) throw new ArgumentNullException ("user.Email");

			CheckParam ("user.UserName", user.UserName, 256);
			if (user.Email.Length > 256 || (RequiresUniqueEmail && user.Email.Length == 0))
				throw new ArgumentException ("invalid format for user.Email");

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			InitConnection();

			try {
				DateTime now = DateTime.Now.ToUniversalTime ();

				commandText = String.Format (@"
UPDATE m
   SET Email = {0},
       Comment = {1},
       IsApproved = @IsApproved,
       LastLoginDate = @Now
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)",
							     user.Email == null ? "NULL" : "@Email",
							     user.Comment == null ? "NULL" : "@Comment");

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				if (user.Email != null)
					AddParameter (command, "Email", user.Email);
				if (user.Comment != null)
					AddParameter (command, "Comment", user.Comment);
				AddParameter (command, "IsApproved", user.IsApproved.ToString());
				AddParameter (command, "UserName", user.UserName);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "Now", now.ToString ());

				if (0 == command.ExecuteNonQuery())
					throw new ProviderException ("failed to membership table");


				commandText = @"
UPDATE dbo.aspnet_Users
   SET LastActivityDate = @Now
  FROM dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE a.ApplicationId = a.ApplicationId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", user.UserName);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "Now", now.ToString ());

				if (0 == command.ExecuteNonQuery())
					throw new ProviderException ("failed to user table");

				trans.Commit ();
			}
			catch (ProviderException e) {
				trans.Rollback ();
				throw e;
			}
			catch (Exception e) {
				trans.Rollback ();
				throw new ProviderException ("failed to update user", e);
			}
		}
		
		[MonoTODO ("flesh out the case where validation fails")]
		public override bool ValidateUser (string username, string password)
		{
			MembershipUser user = GetUser (username, false);

			/* if the user is locked out, return false immediately */
			if (user.IsLockedOut)
				return false;

			/* if the user is not yet approved, return false */
			if (!user.IsApproved)
				return false;

			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, password, false);
			OnValidatingPassword (args);

			if (args.Cancel)
				throw new ProviderException ("Password validation failed");
			if (args.FailureInformation != null)
				throw args.FailureInformation;

			/* get the password/salt from the db */
			string db_password;
			MembershipPasswordFormat db_passwordFormat;
			string db_salt;

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			InitConnection();

			try {
				commandText = @"
SELECT m.Password, m.PasswordFormat, m.PasswordSalt
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", user.UserName);
				AddParameter (command, "ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				reader.Read ();
				db_password = reader.GetString (0);
				db_passwordFormat = (MembershipPasswordFormat)reader.GetInt32 (1);
				db_salt = reader.GetString (2);
				reader.Close();

				/* do the actual validation */
				switch (db_passwordFormat) {
				case MembershipPasswordFormat.Hashed:
					byte[] salt = Convert.FromBase64String (db_salt);
					password = HashAndBase64Encode (password, salt);
					break;
				case MembershipPasswordFormat.Encrypted:
					password = EncryptAndBase64Encode (password);
					break;
				case MembershipPasswordFormat.Clear:
					break;
				}

				bool valid = (password == db_password);

				if (valid) {
					DateTime now = DateTime.Now.ToUniversalTime ();

					/* if the validation succeeds:
					   set LastLoginDate to DateTime.Now
					   set FailedPasswordAttemptCount to 0
					   set FailedPasswordAttemptWindow to DefaultDateTime
					   set FailedPasswordAnswerAttemptCount to 0
					   set FailedPasswordAnswerAttemptWindowStart to DefaultDateTime
					*/

					commandText = @"
UPDATE dbo.aspnet_Membership
   SET LastLoginDate = @Now,
       FailedPasswordAttemptCount = 0,
       FailedPasswordAttemptWindowStart = @DefaultDateTime,
       FailedPasswordAnswerAttemptCount = 0,
       FailedPasswordAnswerAttemptWindowStart = @DefaultDateTime
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "Now", now.ToString ());
					AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update Membership table");

					commandText = @"
UPDATE dbo.aspnet_Users
   SET LastActivityDate = @Now
  FROM dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE u.ApplicationId = a.ApplicationId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "Now", now.ToString ());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update User table");
				}
				else {
					/* if validation fails:
					   if (FailedPasswordAttemptWindowStart - DateTime.Now < PasswordAttemptWindow)
					     increment FailedPasswordAttemptCount
					   FailedPasswordAttemptWindowStart = DateTime.Now
					   if (FailedPasswordAttemptCount > MaxInvalidPasswordAttempts)
					     set IsLockedOut = true.
					     set LastLockoutDate = DateTime.Now
					*/
				}

				trans.Commit ();

				return valid;
			}
			catch {
				trans.Rollback ();

				return false; /* should we allow the exception through? */
			}
		}

		[MonoTODO]
		public override bool UnlockUser (string userName)
		{
			string commandText = @"
UPDATE dbo.aspnet_Membership, dbo.aspnet_Users, dbo.aspnet_Application
   SET dbo.aspnet_Membership.IsLockedOut = 0
 WHERE dbo.aspnet_Membership.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.LoweredUserName = LOWER (@UserName)
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

			CheckParam ("userName", userName, 256);

			InitConnection();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", userName);
			AddParameter (command, "ApplicationName", ApplicationName);

			try {
				return command.ExecuteNonQuery() == 1;
			}
			catch {
				return false;
			}
		}
		
		[MonoTODO]
		public override string ApplicationName {
			get { return applicationName; }
			set { applicationName = value; }
		}
		
		public override bool EnablePasswordReset {
			get { return enablePasswordReset; }
		}
		
		public override bool EnablePasswordRetrieval {
			get { return enablePasswordRetrieval; }
		}
		
		public override MembershipPasswordFormat PasswordFormat {
			get { return passwordFormat; }
		}
		
		public override bool RequiresQuestionAndAnswer {
			get { return requiresQuestionAndAnswer; }
		}
		
		public override bool RequiresUniqueEmail {
			get { return requiresUniqueEmail; }
		}
		
		public override int MaxInvalidPasswordAttempts {
			get { return maxInvalidPasswordAttempts; }
		}
		
		public override int MinRequiredNonAlphanumericCharacters {
			get { return minRequiredNonAlphanumericCharacters; }
		}
		
		public override int MinRequiredPasswordLength {
			get { return minRequiredPasswordLength; }
		}
		
		public override int PasswordAttemptWindow {
			get { return passwordAttemptWindow; }
		}
		
		public override string PasswordStrengthRegularExpression {
			get { return passwordStrengthRegularExpression; }
		}
	}
}
#endif

