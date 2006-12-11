//
// Mainsoft.Web.Security.DerbyDBSchema
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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

using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text;

namespace Mainsoft.Web.Security
{
	internal class DerbyDBSchema
	{
		#region schema string array
		static string [] schemaElements = new string [] {
			// Applications table
			@"CREATE TABLE aspnet_Applications (
				ApplicationId                           char(36)            PRIMARY KEY,
				ApplicationName                         varchar(256)        NOT NULL UNIQUE,
				LoweredApplicationName                  varchar(256)        NOT NULL UNIQUE,
				Description                             varchar(256)
			)",
			@"CREATE INDEX aspnet_Applications_Index ON aspnet_Applications(LoweredApplicationName)",

			// Users table
			@"CREATE TABLE aspnet_Users (
				ApplicationId                           char(36)            NOT NULL,
				UserId                                  char(36)            NOT NULL PRIMARY KEY,
				UserName                                varchar(256)        NOT NULL,
				LoweredUserName                         varchar(256)        NOT NULL,
				MobileAlias                             varchar(16)         DEFAULT NULL,
				IsAnonymous                             int                 NOT NULL DEFAULT 0,
				LastActivityDate                        timestamp           NOT NULL,

				CONSTRAINT Users_ApplicationId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			)",
			@"CREATE UNIQUE INDEX aspnet_Users_Index ON aspnet_Users(ApplicationId, LoweredUserName)",
			@"CREATE INDEX aspnet_Users_Index2 ON aspnet_Users(ApplicationId, LastActivityDate)",

			// Membership table
			@"CREATE TABLE aspnet_Membership (
				ApplicationId                           char(36)            NOT NULL,
				UserId                                  char(36)            NOT NULL PRIMARY KEY, 
				Password                                varchar(128)        NOT NULL,
				PasswordFormat                          int                 NOT NULL DEFAULT 0,
				PasswordSalt                            varchar(128)        NOT NULL,
				MobilePIN                               varchar(16),
				Email                                   varchar(256),
				LoweredEmail                            varchar(256),
				PasswordQuestion                        varchar(256),
				PasswordAnswer                          varchar(128),
				IsApproved                              int                 NOT NULL,
				IsLockedOut                             int                 NOT NULL,
				CreateDate                              timestamp           NOT NULL,
				LastLoginDate                           timestamp           NOT NULL,
				LastPasswordChangedDate                 timestamp           NOT NULL,
				LastLockoutDate                         timestamp           NOT NULL,
				FailedPasswordAttemptCount              int                 NOT NULL,
				FailedPasswordAttemptWindowStart        timestamp           NOT NULL,
				FailedPasswordAnswerAttemptCount        int                 NOT NULL,
				FailedPasswordAnswerAttemptWindowStart  timestamp           NOT NULL,
				Comment                                 varchar(256), 

				CONSTRAINT Member_ApplicationId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId),
				CONSTRAINT UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users(UserId)
			)",
			@"CREATE INDEX aspnet_Membership_index ON aspnet_Membership(ApplicationId, LoweredEmail)",

			// Roles table
			@"CREATE TABLE aspnet_Roles (
				ApplicationId                           char(36)            NOT NULL,
				RoleId                                  char(36)            NOT NULL PRIMARY KEY,
				RoleName                                varchar(256)        NOT NULL,
				LoweredRoleName                         varchar(256)        NOT NULL,
				Description                             varchar(256),

				CONSTRAINT Roles_ApplicationId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			)",
			@"CREATE UNIQUE INDEX aspnet_Roles_index ON aspnet_Roles(ApplicationId, LoweredRoleName)",

			// UsersInRoles table
			@"CREATE TABLE aspnet_UsersInRoles (
				UserId                                  char(36)            NOT NULL, 
				RoleId                                  char(36)            NOT NULL,

				CONSTRAINT RoleId_UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId),
				CONSTRAINT UserId_RoleId_PK FOREIGN KEY (RoleId) REFERENCES aspnet_Roles (RoleId)
			)",
			@"ALTER TABLE aspnet_UsersInRoles ADD PRIMARY KEY (UserId, RoleId)",
			@"CREATE INDEX aspnet_UsersInRoles_index ON aspnet_UsersInRoles(RoleId)",

			// Profile table
			@"CREATE TABLE aspnet_Profile (
				UserId                                  char(36)            NOT NULL PRIMARY KEY,
				PropertyNames                           long varchar        NOT NULL,
				PropertyValuesString                    long varchar        NOT NULL,
				PropertyValuesBinary                    blob                NOT NULL,
				LastUpdatedDate                         timestamp           NOT NULL,

				CONSTRAINT Profile_UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId)
			)",

			// Pathes table
			@"CREATE TABLE aspnet_Paths (
				ApplicationId                           char(36)            NOT NULL,
				PathId                                  char(36)            NOT NULL PRIMARY KEY,
				Path                                    varchar(256)        NOT NULL,
				LoweredPath                             varchar(256)        NOT NULL,

				CONSTRAINT Paths_ApplicationId_FK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			)",
			@"CREATE UNIQUE INDEX aspnet_Paths_index ON aspnet_Paths(ApplicationId, LoweredPath)",

			// Personalization tables
			@"CREATE TABLE aspnet_PersonalizationAllUsers (
				PathId                                  char(36)            NOT NULL PRIMARY KEY,
				PageSettings                            blob                NOT NULL,
				LastUpdatedDate                         timestamp           NOT NULL,

				CONSTRAINT PersnlUsers_PathId_PK FOREIGN KEY (PathId) REFERENCES aspnet_Paths (PathId)
			)",
			@"CREATE TABLE aspnet_PersonalizationPerUser (
				Id                                      char(36)            NOT NULL PRIMARY KEY,
				PathId                                  char(36)            NOT NULL,
				UserId                                  char(36)            NOT NULL,
				PageSettings                            blob                NOT NULL,
				LastUpdatedDate                         timestamp           NOT NULL,

				CONSTRAINT PersnlPerUser_PathId_FK FOREIGN KEY (PathId) REFERENCES aspnet_Paths (PathId),
				CONSTRAINT PersnlPerUser_UserId_FK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId)
			)",
			@"CREATE UNIQUE INDEX aspnet_PersonalizationPerUser_index1 ON aspnet_PersonalizationPerUser(PathId,UserId)",
			@"CREATE UNIQUE INDEX aspnet_PersonalizationPerUser_ncindex2 ON aspnet_PersonalizationPerUser(UserId,PathId)"
		};
		#endregion

		public static void InitializeSchema (string connectionString)
		{
			RegisterUnloadHandler (connectionString);

			if (IsSchemaExists (connectionString))
				return;

			if (connectionString.ToLower ().IndexOf ("create=true") < 0) {
				if (!connectionString.Trim ().EndsWith (";"))
					connectionString += ";";

				connectionString += "create=true";
			}

			OleDbConnection connection = new OleDbConnection ();
			connection.ConnectionString = connectionString;

			connection.Open ();

			using (connection) {
				for (int i = 0; i < schemaElements.Length; i++) {
					try {
						OleDbCommand cmd = new OleDbCommand (schemaElements [i], connection);
						cmd.ExecuteNonQuery ();
					}
					catch (Exception) {
					}
				}
			}
		}

		public static bool IsSchemaExists (string connectionString)
		{
			OleDbConnection connection = new OleDbConnection (connectionString);

			try {
				connection.Open ();
			}
			catch (Exception) {
				return false;
			}

			using (connection) {
				OleDbCommand cmd = new OleDbCommand ("SELECT COUNT(*) FROM aspnet_Applications", connection);
				try {
					using (OleDbDataReader reader = cmd.ExecuteReader ()) {
						if (reader.Read ())
							return true;
					}
				}
				catch (Exception) {
					return false;
				}
			}

			return false;
		}

		static void RegisterUnloadHandler (string connectionString)
		{
			DerbyUnloadManager derbyMan = new DerbyUnloadManager (connectionString);
			derbyMan.RegisterUnloadHandler ();
		}
	}

	internal class DerbyUnloadManager
	{
		private string _releaseString = null;

		public DerbyUnloadManager (string connectionString)
		{
			_releaseString = connectionString;

			string [] parts = _releaseString.Split (';');
			bool found = false;

			for (int i=0; i<parts.Length; i++)
			{
				if (parts[i].ToLower().Trim().StartsWith("create"))
				{
					parts[i] = parts[i].ToLower().Trim().Replace("create", "shutdown");
					found = true;
					break;
				}
			}
			if (found)
				_releaseString = string.Join (";", parts);
			else
			{
				if (!_releaseString.Trim ().EndsWith (";"))
					_releaseString += ";";

				_releaseString += "shutdown=true";
			}
		}

		public void UnloadHandler (object sender, EventArgs e)
		{
			OleDbConnection connection = new OleDbConnection (_releaseString);

			try {
				connection.Open ();
			}
			catch (Exception) {
			}
		}

		public void RegisterUnloadHandler ()
		{
			AppDomain.CurrentDomain.DomainUnload += new EventHandler (UnloadHandler);
		}

		public void UnregisterUnloadHandler ()
		{
			AppDomain.CurrentDomain.DomainUnload -= new EventHandler (UnloadHandler);
		}
	}
}

#endif
