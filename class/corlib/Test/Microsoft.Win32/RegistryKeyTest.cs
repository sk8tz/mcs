//
// RegistryKeyTest.cs - NUnit Test Cases for Microsoft.Win32.RegistryKey
//
// Authors:
//	mei (mei@work.email.ne.jp)
//	Robert Jordan (robertj@gmx.net)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 mei
// (C) 2004, 2005 Novell (http://www.novell.com)
//

using System;
using System.IO;

using Microsoft.Win32;

using NUnit.Framework;

namespace MonoTests.Microsoft.Win32
{
	[TestFixture]
	public class RegistryKeyTest
	{
		private const string mimeroot = @"MIME\Database\Content Type";
		[Test]
		[Category("NotWorking")]
		// This will not work on Linux ever
		public void TestGetValue ()
		{
			RegistryKey root = Registry.ClassesRoot;
			RegistryKey key;
			
			key = root.OpenSubKey (mimeroot + @"\audio/wav");
			Assert.AreEqual (".wav", key.GetValue ("Extension"), "GetValue #1");
			key = root.OpenSubKey (mimeroot + @"\text/x-scriptlet");
			Assert.AreEqual (null, key.GetValue ("Extension"), "GetValue #2");
		}

		//
		// Unit test for bug #77212
		//
		[Test]
		public void TestHandle ()
		{
			// this test is for Windows only
			if (RunningOnUnix)
				return;

			// this regpath always exists under windows
			RegistryKey k = Registry.CurrentUser
				.OpenSubKey ("Software", false)
				.OpenSubKey ("Microsoft", false)
				.OpenSubKey ("Windows", false);
			
			Assert.IsNotNull (k, "#01");
		}

		[Test]
		public void OpenSubKey ()
		{
			RegistryKey key = Registry.LocalMachine;

			// HKEY_LOCAL_MACHINE\software should always exist on Windows
			// and is automatically created on Linux
			Assert.IsNotNull (key.OpenSubKey ("Software"), "#A1");
			Assert.IsNotNull (key.OpenSubKey ("soFtware"), "#A2");

			key = Registry.CurrentUser;

			// HKEY_CURRENT_USER\software should always exist on Windows
			// and is automatically created on Linux
			Assert.IsNotNull (key.OpenSubKey ("Software"), "#B1");
			Assert.IsNotNull (key.OpenSubKey ("soFtware"), "#B2");

			key = Registry.Users;

			// HKEY_USERS\software should not exist on Windows, and should not
			// be created automatically on Linux
			Assert.IsNull (key.OpenSubKey ("Software"), "#C1");
			Assert.IsNull (key.OpenSubKey ("soFtware"), "#C2");
		}

		[Test]
		public void OpenSubKey_Key_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();
			Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#1"); // read-only
			Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName, true), "#2"); // writable
		}

		[Test]
		public void OpenSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					// read-only
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					Assert.IsNull (createdKey.OpenSubKey ("monotemp"), "#5"); // read-only
					// writable
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName, true), "#6");
					Assert.IsNull (createdKey.OpenSubKey ("monotemp", true), "#7"); 
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OpenSubKey_Name_Null ()
		{
			Registry.CurrentUser.OpenSubKey (null);
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void OpenSubKey_Name_Empty ()
		{
			// read-only
			using (RegistryKey emptyKey = Registry.CurrentUser.OpenSubKey (string.Empty)) {
				Assert.IsNotNull (emptyKey, "#1");
			}
			// writable
			using (RegistryKey emptyKey = Registry.CurrentUser.OpenSubKey (string.Empty, true)) {
				Assert.IsNotNull (emptyKey, "#1");
			}
		}

		[Test]
		public void Close_Local_Hive ()
		{
			RegistryKey hive = Registry.CurrentUser;
			hive.Close ();

			Assert.IsNotNull (hive.GetSubKeyNames (), "#1");
			Assert.IsNull (hive.GetValue ("doesnotexist"), "#2");
			Assert.IsNotNull (hive.GetValueNames (), "#3");
			Assert.IsNull (hive.OpenSubKey ("doesnotexist"), "#4");
			Assert.IsNotNull (hive.SubKeyCount, "#5");
			Assert.IsNotNull (hive.ToString (), "#6");

			// closing key again does not have any effect
			hive.Close ();
		}

		[Test]
		public void Close_Local_Key ()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey ("SOFTWARE");
			key.Close ();

			// closing a key twice does not have any effect
			key.Close ();

			try {
				key.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed key does not have any effect
			key.Flush ();

			try {
				key.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				key.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				key.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				key.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Close_Remote_Hive ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			hive.Close ();

			// closing a remote hive twice does not have any effect
			hive.Close ();

			try {
				hive.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed hive does not have any effect
			hive.Flush ();

			try {
				hive.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = hive.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				hive.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = hive.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Close_Remote_Key ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			RegistryKey key = hive.OpenSubKey ("SOFTWARE");
			key.Close ();

			// closing a remote key twice does not have any effect
			key.Close ();

			try {
				key.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed key does not have any effect
			key.Flush ();

			try {
				key.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				key.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				key.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				key.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}

			hive.Close ();
		}

		[Test]
		public void CreateSubKey ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// check if key was successfully created
				Assert.IsNotNull (createdKey, "#1");
				// software subkey should not be created automatically
				Assert.IsNull (createdKey.OpenSubKey ("software"), "#2");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void CreateSubKey_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
				RegistryKey createdKey = null;
				try {
					try {
						createdKey = softwareKey.CreateSubKey (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				} finally {
					if (createdKey != null)
						createdKey.Close ();
				}
			}
		}

		[Test]
		public void CreateSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.IsNull (softwareKey.OpenSubKey (subKeyName), "#1");
						try {
							createdKey.CreateSubKey ("test");
							Assert.Fail ("#2");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#3");
							Assert.IsNotNull (ex.Message, "#4");
							Assert.IsNull (ex.InnerException, "#5");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void CreateSubKey_Name_Empty ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				using (RegistryKey emptyKey = softwareKey.CreateSubKey (string.Empty)) {
					Assert.IsNotNull (emptyKey, "#1");
					emptyKey.SetValue ("name1", "value1");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateSubKey_Name_Null ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				softwareKey.CreateSubKey (null);
			}
		}

		[Test]
		public void DeleteSubKey ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#2");
					Registry.CurrentUser.DeleteSubKey (subKeyName);
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey, "#3");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_HasChildKeys ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				try {
					Registry.CurrentUser.DeleteSubKey (subKeyName);
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// Registry key has subkeys and recursive removes are not
					// supported by this method
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.InnerException, "#5");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.Close ();
				}

				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
					try {
						softwareKey.DeleteSubKey (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				}
			} finally {
				try {
					using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				Registry.CurrentUser.DeleteSubKey (subKeyName);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot delete a subkey tree because the subkey does not exist
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
			}

			try {
				Registry.CurrentUser.DeleteSubKey (subKeyName, true);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot delete a subkey tree because the subkey does not exist
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
			}

			Registry.CurrentUser.DeleteSubKey (subKeyName, false);
		}

		[Test]
		public void DeleteSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					try {
						createdKey.DeleteSubKey ("monotemp");
						Assert.Fail ("#5");
					} catch (ArgumentException ex) {
						// Cannot delete a subkey tree because the subkey does
						// not exist
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#6");
						Assert.IsNotNull (ex.Message, "#7");
						Assert.IsNull (ex.InnerException, "#8");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void DeleteSubKey_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.DeleteSubKey (string.Empty);
					createdKey.Close ();

					createdKey = softwareKey.OpenSubKey (subKeyName);
					Assert.IsNull (createdKey, "#1");
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKey_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					try {
						createdKey.DeleteSubKey (null);
						Assert.Fail ("#1");
					} catch (ArgumentNullException ex) {
						// Value cannot be null. Parameter name: subkey
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree ()
		{
			// TODO: 
			// - remove key with subkeys
			// - remove key of which some subkeys are marked for deletion
			// - remove key with values
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DeleteSubKeyTree_Key_DoesNotExist ()
		{
			// Cannot delete a subkey tree because the subkey does not exist
			string subKeyName = Guid.NewGuid ().ToString ();
			Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
		}

		[Test]
		public void DeleteSubKeyTree_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.Close ();
				}

				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
					try {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				}
			} finally {
				try {
					using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					try {
						createdKey.DeleteSubKeyTree ("monotemp");
						Assert.Fail ("#5");
					} catch (ArgumentException ex) {
						// Cannot delete a subkey tree because the subkey does
						// not exist
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#6");
						Assert.IsNotNull (ex.Message, "#7");
						Assert.IsNull (ex.InnerException, "#8");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void DeleteSubKeyTree_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.DeleteSubKeyTree (string.Empty);
					createdKey.Close ();

					createdKey = softwareKey.OpenSubKey (subKeyName);
					Assert.IsNull (createdKey, "#1");
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree_Name_Null ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					softwareKey.DeleteSubKeyTree (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					// Value cannot be null. Parameter name: subkey
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.InnerException, "#4");
				}
			}
		}

		[Test]
		public void DeleteValue ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A2");
					Assert.AreEqual (2, names.Length, "#A3");
					Assert.IsNotNull (names [0], "#A4");
					Assert.AreEqual ("name1", names [0], "#A5");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A6");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A7");
					Assert.AreEqual ("name2", names [1], "#A8");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#A10");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#B1");
					createdKey.DeleteValue ("name1");
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B2");
					Assert.AreEqual (1, names.Length, "#B3");
					Assert.IsNotNull (names [0], "#B4");
					Assert.AreEqual ("name2", names [0], "#B5");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#B6");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#B7");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#C1");
					Assert.AreEqual (1, names.Length, "#C2");
					Assert.IsNotNull (names [0], "#C3");
					Assert.AreEqual ("name2", names [0], "#C4");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#C5");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#C6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1");
						Assert.Fail ("#A1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsNull (ex.InnerException, "#A4");
					}

					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1", true);
						Assert.Fail ("#B1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
					}

					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1", false);
						Assert.Fail ("#C1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2");
						Assert.Fail ("#D1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#D2");
						Assert.IsNotNull (ex.Message, "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2", true);
						Assert.Fail ("#E1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2", false);
						Assert.Fail ("#F1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#F2");
						Assert.IsNotNull (ex.Message, "#F3");
						Assert.IsNull (ex.InnerException, "#F4");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					createdKey.SetValue ("name1", "value1");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#3");

					createdKey.DeleteValue ("name1");
					createdKey.DeleteValue ("name1", true);
					createdKey.DeleteValue ("name1", false);
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Value_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					createdKey.SetValue ("name1", "value1");

					try {
						createdKey.DeleteValue ("name2");
						Assert.Fail ("#B1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
					}

					try {
						createdKey.DeleteValue ("name2", true);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					createdKey.DeleteValue ("name2", false);
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue (string.Empty, "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
					Assert.IsNotNull (names [0], "#A3");
					/*
					Assert.AreEqual ("name1", names [0], "#A4");
					*/
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A6");
					Assert.IsNotNull (names [1], "#A7");
					/*
					Assert.AreEqual (string.Empty, names [1], "#A8");
					*/
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#A10");

					createdKey.DeleteValue (string.Empty);

					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B1");
					Assert.AreEqual (1, names.Length, "#B2");
					Assert.IsNotNull (names [0], "#B3");
					Assert.AreEqual ("name1", names [0], "#B4");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#B5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#B6");

					try {
						createdKey.DeleteValue (string.Empty);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					try {
						createdKey.DeleteValue (string.Empty, true);
						Assert.Fail ("#D1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
						Assert.IsNotNull (ex.Message, "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
					}

					createdKey.DeleteValue (string.Empty, false);

					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#E1");
					Assert.AreEqual (1, names.Length, "#E2");
					Assert.IsNotNull (names [0], "#E3");
					Assert.AreEqual ("name1", names [0], "#E4");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#E5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#E6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue (null, "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
					Assert.IsNotNull (names [0], "#A3");
					/*
					Assert.AreEqual ("name1", names [0], "#A4");
					*/
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A6");
					Assert.IsNotNull (names [1], "#A7");
					/*
					Assert.AreEqual (string.Empty, names [1], "#A8");
					*/
					Assert.IsNotNull (createdKey.GetValue (null), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue (null), "#A10");

					try {
						createdKey.DeleteValue (null);
						Assert.Fail ("#B1");
					} catch (ArgumentNullException ex) {
						// Value cannot be null. Parameter name: name
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
					}

					try {
						createdKey.DeleteValue (null, true);
						Assert.Fail ("#C1");
					} catch (ArgumentNullException ex) {
						// Value cannot be null. Parameter name: name
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					try {
						createdKey.DeleteValue (null, false);
						Assert.Fail ("#D1");
					} catch (ArgumentNullException ex) {
						// Value cannot be null. Parameter name: name
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
						Assert.IsNotNull (ex.Message, "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#1");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#2");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#3");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#4");
					Assert.IsNull (createdKey.GetValue ("name3"), "#5");
					Assert.AreEqual ("value3", createdKey.GetValue ("name3", "value3"), "#6");
					Assert.IsNull (createdKey.GetValue ("name3", null), "#7");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					Assert.IsNull (createdKey.GetValue ("name1"), "#1");
					Assert.IsNotNull (createdKey.GetValue ("name1", "default"), "#2");
					Assert.AreEqual ("default", createdKey.GetValue ("name1", "default"), "#3");
					Assert.IsNull (createdKey.GetValue ("name3"), "#3");
					Assert.IsNotNull (createdKey.GetValue ("name3", "default"), "#4");
					Assert.AreEqual ("default", createdKey.GetValue ("name3", "default"), "#5");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					Assert.IsNull (createdKey.GetValue (string.Empty), "#A1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty, "default"), "#A2");
					Assert.AreEqual ("default", createdKey.GetValue (string.Empty, "default"), "#A3");
					Assert.IsNull (createdKey.GetValue (string.Empty, null), "#A4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey.GetValue (string.Empty), "#B1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty, "default"), "#B2");
					Assert.AreEqual ("default", createdKey.GetValue (string.Empty, "default"), "#B3");
					Assert.IsNull (createdKey.GetValue (string.Empty, null), "#B4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					createdKey.SetValue (string.Empty, "value1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#C1");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#C2");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, "default"), "#C3");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, null), "#C4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#D1");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#D2");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, "default"), "#D3");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, null), "#D4");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					Assert.IsNull (createdKey.GetValue (null), "#A1");
					Assert.IsNotNull (createdKey.GetValue (null, "default"), "#A2");
					Assert.AreEqual ("default", createdKey.GetValue (null, "default"), "#A3");
					Assert.IsNull (createdKey.GetValue (null, null), "#A4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey.GetValue (null), "#B1");
					Assert.IsNotNull (createdKey.GetValue (null, "default"), "#B2");
					Assert.AreEqual ("default", createdKey.GetValue (null, "default"), "#B3");
					Assert.IsNull (createdKey.GetValue (null, null), "#B4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					createdKey.SetValue (string.Empty, "value1");
					Assert.IsNotNull (createdKey.GetValue (null), "#C1");
					Assert.AreEqual ("value1", createdKey.GetValue (null), "#C2");
					Assert.AreEqual ("value1", createdKey.GetValue (null, "default"), "#C3");
					Assert.AreEqual ("value1", createdKey.GetValue (null, null), "#C4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue (null), "#D1");
					Assert.AreEqual ("value1", createdKey.GetValue (null), "#D2");
					Assert.AreEqual ("value1", createdKey.GetValue (null, "default"), "#D3");
					Assert.AreEqual ("value1", createdKey.GetValue (null, null), "#D4");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

#if NET_2_0
		[Test]
		public void GetValue_Expand ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					Environment.SetEnvironmentVariable ("MONO_TEST1", "123");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "456");

					createdKey.SetValue ("name1", "%MONO_TEST1%/%MONO_TEST2%",
						RegistryValueKind.ExpandString);
					createdKey.SetValue ("name2", "%MONO_TEST1%/%MONO_TEST2%");
					createdKey.SetValue ("name3", "just some text",
						RegistryValueKind.ExpandString);

					Assert.AreEqual ("123/456", createdKey.GetValue ("name1"), "#A1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#A2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#A3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A6");
					Assert.AreEqual ("123/456", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#A7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#A8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#A9");

					Environment.SetEnvironmentVariable ("MONO_TEST1", "789");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "666");

					Assert.AreEqual ("789/666", createdKey.GetValue ("name1"), "#B1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#B2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#B3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B6");
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#B7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#B8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#B9");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1"), "#C1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#C2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#C3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C6");
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#C7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#C8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#C9");

					Environment.SetEnvironmentVariable ("MONO_TEST1", "123");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "456");

					Assert.AreEqual ("123/456", createdKey.GetValue ("name1"), "#D1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#D2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#D3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D6");
					Assert.AreEqual ("123/456", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#D7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#D8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#D9");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}
#endif

		[Test]
		public void GetValueNames ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (0, names.Length, "#A2");

					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
					createdKey.SetValue ("namelong", "value3");
					createdKey.SetValue ("name3", "value4");

					Assert.AreEqual (4, createdKey.ValueCount, "#B1");
					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B2");
					Assert.AreEqual (4, names.Length, "#B3");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#C1");
					Assert.AreEqual (4, names.Length, "#C2");

					// Mono's Unix registry API uses a hashtable to store the
					// values (and their names), so names are not returned in
					// order
					//
					// to test whether the names returned by GetValueNames
					// match what we expect, we use these names to remove the
					// the values from the created keys and such we should end
					// up with zero values
					for (int i = 0; i < names.Length; i++) {
						string valueName = names [i];
						createdKey.DeleteValue (valueName);
					}

					// all values should be removed now
					Assert.AreEqual (0, createdKey.ValueCount, "#C3");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValueNames_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B1");
					Assert.AreEqual (2, names.Length, "#B2");

					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						createdKey.GetValueNames ();
						Assert.Fail ("#C1");
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test] // bug #78519
		public void GetSubKeyNamesTest ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// check if key was successfully created
				Assert.IsNotNull (createdKey, "#A");

				RegistryKey subKey = createdKey.CreateSubKey ("foo");
				Assert.IsNotNull (subKey, "#B1");
				Assert.AreEqual (1, createdKey.SubKeyCount, "#B2");
				string[] subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#B3");
				Assert.AreEqual (1, subKeyNames.Length, "#B4");
				Assert.AreEqual ("foo", subKeyNames[0], "#B5");

				subKey = createdKey.CreateSubKey ("longfoo");
				Assert.IsNotNull (subKey, "#C1");
				Assert.AreEqual (2, createdKey.SubKeyCount, "#C2");
				subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#C3");
				Assert.AreEqual (2, subKeyNames.Length, "#C4");
				Assert.AreEqual ("foo", subKeyNames [0], "#C5");
				Assert.AreEqual ("longfoo", subKeyNames [1], "#C6");

				subKey = createdKey.CreateSubKey ("sfoo");
				Assert.IsNotNull (subKey, "#D1");
				Assert.AreEqual (3, createdKey.SubKeyCount, "#D2");
				subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#D3");
				Assert.AreEqual (3, subKeyNames.Length, "#D4");
				Assert.AreEqual ("foo", subKeyNames [0], "#D5");
				Assert.AreEqual ("longfoo", subKeyNames [1], "#D6");
				Assert.AreEqual ("sfoo", subKeyNames [2], "#D7");

				foreach (string name in subKeyNames) {
					createdKey.DeleteSubKeyTree (name);
				}
				Assert.AreEqual (0, createdKey.SubKeyCount, "#E");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void OpenRemoteBaseKey ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			Assert.IsNotNull (hive, "#1");

			RegistryKey key = hive.OpenSubKey ("SOFTWARE");
			Assert.IsNotNull (key, "#2");
			key.Close ();

			hive.Close ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OpenRemoteBaseKey_MachineName_Null ()
		{
			RegistryKey.OpenRemoteBaseKey (RegistryHive.CurrentUser, null);
		}

		[Test]
		public void OpenRemoteBaseKey_MachineName_DoesNotExist ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			try {
				RegistryKey.OpenRemoteBaseKey (RegistryHive.CurrentUser,
					"DOESNOTEXIST");
				Assert.Fail ("#1");
			} catch (IOException ex) {
				// The network path was not found
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void SetValue_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (null, "value1");
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (string.Empty, "value2");
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (string.Empty, "value1");
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (null, "value2");
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetValue_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// null value should result in ArgumentNullException
				createdKey.SetValue ("Name", null);
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Boolean ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Installed"), "#1");
				// create value
				createdKey.SetValue ("Installed", true);
				// get value
				object value = createdKey.GetValue ("Installed");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (true.ToString (), value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Byte ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Flags"), "#1");
				// create value
				createdKey.SetValue ("Flags", (byte) 5);
				// get value
				object value = createdKey.GetValue ("Flags");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("5", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_ByteArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Flags"), "#1");
				// create value
				createdKey.SetValue ("Flags", new byte[] { 1, 5 });
				// get value
				object value = createdKey.GetValue ("Flags");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (byte[]), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (new byte[] { 1, 5 }, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_DateTime ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				object rawValue = DateTime.Now;

				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Path"), "#1");
				// create value
				createdKey.SetValue ("Path", rawValue);
				// get value
				object value = createdKey.GetValue ("Path");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (rawValue.ToString (), value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Int32 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("RefCount"), "#1");
				// create value
				createdKey.SetValue ("RefCount", 5);
				// get value
				object value = createdKey.GetValue ("RefCount");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be int
				Assert.AreEqual (typeof (int), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (5, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Int64 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Ticks"), "#1");
				// create value
				createdKey.SetValue ("Ticks", 500L);
				// get value
				object value = createdKey.GetValue ("Ticks");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("500", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_String ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Path"), "#1");
				// create value
				createdKey.SetValue ("Path", "/usr/lib/whatever");
				// get value
				object value = createdKey.GetValue ("Path");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("/usr/lib/whatever", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_StringArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("DependsOnGroup"), "#1");
				// create value
				createdKey.SetValue ("DependsOnGroup", new string[] { "A", "B" });
				// get value
				object value = createdKey.GetValue ("DependsOnGroup");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string[]), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (new string[] { "A", "B" }, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
				try {
					softwareKey.SetValue ("name1", "value1");
					Assert.Fail ("#1");
				} catch (UnauthorizedAccessException ex) {
					// Cannot write to the registry key
					Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.InnerException, "#4");
				}
			}

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
					}

					using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName)) {
						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#1");
						} catch (UnauthorizedAccessException ex) {
							// Cannot write to the registry key
							Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
							Assert.IsNotNull (ex.Message, "#3");
							Assert.IsNull (ex.InnerException, "#4");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void SetValue_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.IsNull (softwareKey.OpenSubKey (subKeyName), "#1");
						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#2");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#3");
							Assert.IsNotNull (ex.Message, "#4");
							Assert.IsNull (ex.InnerException, "#5");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void SubKeyCount ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp1")) {
						subKey.Close ();
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#A2");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp2")) {
						subKey.Close ();
					}
					Assert.AreEqual (2, createdKey.SubKeyCount, "#A3");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (2, createdKey.SubKeyCount, "#B2");

					using (RegistryKey createdKey2 = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
						Assert.IsNotNull (createdKey2, "#B3");
						Assert.AreEqual (2, createdKey2.SubKeyCount, "#B4");
						createdKey2.DeleteSubKey ("monotemp1");
						Assert.AreEqual (1, createdKey2.SubKeyCount, "#B5");
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#B6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void SubKeyCount_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp1")) {
						subKey.Close ();
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#A2");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp2")) {
						subKey.Close ();
					}
					Assert.AreEqual (2, createdKey.SubKeyCount, "#A3");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (2, createdKey.SubKeyCount, "#B2");

					// remove created key
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						Assert.Fail ("#C1: " + createdKey.SubKeyCount);
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
						Assert.IsNotNull (ex.Message, "#15");
						Assert.IsNull (ex.InnerException, "#16");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void ValueCount ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					Assert.AreEqual (0, createdKey.ValueCount, "#A2");
					createdKey.SetValue ("name1", "value1");
					Assert.AreEqual (1, createdKey.ValueCount, "#A3");
					createdKey.SetValue ("name2", "value2");
					Assert.AreEqual (2, createdKey.ValueCount, "#A4");
					createdKey.SetValue ("name2", "value2b");
					Assert.AreEqual (2, createdKey.ValueCount, "#A5");
					createdKey.SetValue ("name3", "value3");
					Assert.AreEqual (3, createdKey.ValueCount, "#A6");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (3, createdKey.ValueCount, "#B2");

					using (RegistryKey createdKey2 = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
						Assert.IsNotNull (createdKey2, "#B3");
						Assert.AreEqual (3, createdKey2.ValueCount, "#B4");
						createdKey2.DeleteValue ("name2");
						Assert.AreEqual (2, createdKey2.ValueCount, "#B5");
					}
					Assert.AreEqual (2, createdKey.ValueCount, "#B6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void ValueCount_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					Assert.AreEqual (0, createdKey.ValueCount, "#A2");
					createdKey.SetValue ("name1", "value1");
					Assert.AreEqual (1, createdKey.ValueCount, "#A3");
					createdKey.SetValue ("name2", "value2");
					Assert.AreEqual (2, createdKey.ValueCount, "#A4");
					createdKey.SetValue ("name2", "value2b");
					Assert.AreEqual (2, createdKey.ValueCount, "#A5");
					createdKey.SetValue ("name3", "value3");
					Assert.AreEqual (3, createdKey.ValueCount, "#A6");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (3, createdKey.ValueCount, "#B2");

					// remove created key
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						Assert.Fail ("#C1: " + createdKey.ValueCount);
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
						Assert.IsNotNull (ex.Message, "#15");
						Assert.IsNull (ex.InnerException, "#16");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void bug79051 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						createdKey.SetValue ("test", "whatever");
						createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bug79059 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						using (RegistryKey softwareKey2 = Registry.CurrentUser.OpenSubKey ("software")) {
						}
						createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bugnew1 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						createdKey.SetValue ("name1", "value1");

						RegistryKey testKey = null;
						try {
							testKey = createdKey.OpenSubKey ("test", true);
							if (testKey == null)
								testKey = createdKey.CreateSubKey ("test");
							testKey.SetValue ("another", "one");
						} finally {
							if (testKey != null)
								testKey.Close ();
						}

						createdKey.SetValue ("name2", "value2");
						Assert.IsNotNull (createdKey.GetValue ("name1"), "#2");
						Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#3");
						Assert.IsNotNull (createdKey.GetValue ("name2"), "#4");
						Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#5");

						string [] names = createdKey.GetValueNames ();
						Assert.IsNotNull (names, "#6");
						Assert.AreEqual (2, names.Length, "#7");
						Assert.AreEqual ("name1", names [0], "#8");
						Assert.AreEqual ("name2", names [1], "#9");

						softwareKey.DeleteSubKeyTree (subKeyName);

						using (RegistryKey openedKey = softwareKey.OpenSubKey (subKeyName, true)) {
							Assert.IsNull (openedKey, "#10");
						}

						Assert.IsNull (createdKey.GetValue ("name1"), "#11");
						Assert.IsNull (createdKey.GetValue ("name2"), "#12");

						try {
							createdKey.GetValueNames ();
							Assert.Fail ("#13");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
							Assert.IsNotNull (ex.Message, "#15");
							Assert.IsNull (ex.InnerException, "#16");
						}

						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#17");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#18");
							Assert.IsNotNull (ex.Message, "#19");
							Assert.IsNull (ex.InnerException, "#20");
						}

						try {
							createdKey.SetValue ("newname", "value1");
							Assert.Fail ("#21");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#22");
							Assert.IsNotNull (ex.Message, "#23");
							Assert.IsNull (ex.InnerException, "#24");
						}

						Assert.IsNull (createdKey.OpenSubKey ("test"), "#25");
						Assert.IsNull (createdKey.OpenSubKey ("test", true), "#26");
						Assert.IsNull (createdKey.OpenSubKey ("new"), "#27");
						Assert.IsNull (createdKey.OpenSubKey ("new", true), "#28");

						try {
							createdKey.CreateSubKey ("new");
							Assert.Fail ("#29");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#30");
							Assert.IsNotNull (ex.Message, "#31");
							Assert.IsNull (ex.InnerException, "#32");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bugnew2 () // values cannot be written on registry root (hive)
		{
			string [] names = Registry.CurrentUser.GetValueNames ();
			Assert.IsNotNull (names, "#1");
			Registry.CurrentUser.SetValue ("name1", "value1");
			Assert.IsNotNull (Registry.CurrentUser.GetValue ("name1"), "#2");
			Assert.AreEqual ("value1", Registry.CurrentUser.GetValue ("name1"), "#3");
			string [] newNames = Registry.CurrentUser.GetValueNames ();
			Assert.IsNotNull (newNames, "#4");
			Assert.AreEqual (names.Length + 1, newNames.Length, "#5");
			Registry.CurrentUser.DeleteValue ("name1");
		}

		[Test]
		public void bugnew3 () // on Windows, key cannot be closed twice
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}

				RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName);
				openedKey.Close ();
				openedKey.Close ();
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void bugnew4 () // Key cannot be flushed once it has been closed
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}

				RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName);
				openedKey.Close ();
				openedKey.Flush ();
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		private bool RunningOnUnix {
			get {
#if NET_2_0
				return Environment.OSVersion.Platform == PlatformID.Unix;
#else
				int p = (int) Environment.OSVersion.Platform;
				return ((p == 4) || (p == 128));
#endif
			}
		}
	}
}
