//
// System.Web.UI.WebControls.ProfileBase.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Security;
using System.Web.Configuration;

namespace System.Web.Profile
{
	public class ProfileBase : SettingsBase
	{
		bool _propertiyValuesLoaded = false;
		bool _dirty = false;
		DateTime _lastActivityDate = DateTime.MinValue;
		DateTime _lastUpdatedDate = DateTime.MinValue;
		SettingsContext _settingsContext = null;
		SettingsPropertyValueCollection _propertiyValues = null;
		const string Profiles_SettingsPropertyCollection = "Profiles.SettingsPropertyCollection";

#if TARGET_J2EE
		static SettingsPropertyCollection _properties
		{
			get
			{
				object o = AppDomain.CurrentDomain.GetData (Profiles_SettingsPropertyCollection);
				return (SettingsPropertyCollection) o;
			}
			set
			{
				AppDomain.CurrentDomain.SetData (Profiles_SettingsPropertyCollection, value);
			}
		}
#else
		static SettingsPropertyCollection _properties = null;
#endif

		static void InitProperties ()
		{
			SettingsPropertyCollection properties = new SettingsPropertyCollection ();

			ProfileSection config = (ProfileSection) WebConfigurationManager.GetSection ("system.web/profile");
			RootProfilePropertySettingsCollection ps = config.PropertySettings;

			for (int i = 0; i < ps.GroupSettings.Count; i++) {
				ProfileGroupSettings pgs = ps.GroupSettings [i];
				ProfilePropertySettingsCollection ppsc = pgs.PropertySettings;

				for (int s = 0; s < ppsc.Count; s++) {
					SettingsProperty settingsProperty = CreateSettingsPropery (pgs, ppsc [s]);
					ValidateProperty (settingsProperty, ppsc [i].ElementInformation);
					properties.Add (settingsProperty);
				}
			}

			for (int s = 0; s < ps.Count; s++) {
				SettingsProperty settingsProperty = CreateSettingsPropery (null, ps [s]);
				ValidateProperty (settingsProperty, ps [s].ElementInformation);
				properties.Add (settingsProperty);
			}

			properties.SetReadOnly ();
			lock (Profiles_SettingsPropertyCollection) {
				if (_properties == null)
					_properties = properties;
			}
		}
		
		public ProfileBase ()
		{
		}

		public static ProfileBase Create (string username)
		{
			return Create (username, true);
		}

		public static ProfileBase Create (string username, bool isAuthenticated)
		{
			ProfileBase profile = null;
			Type profileType = ProfileParser.GetProfileCommonType (HttpContext.Current);
			if (profileType != null)
				profile = (ProfileBase) Activator.CreateInstance (profileType);
			else
				profile = (ProfileBase) new DefaultProfile ();

			profile.Initialize (username, isAuthenticated);
			return profile;
		}

		public ProfileGroupBase GetProfileGroup (string groupName)
		{
			ProfileGroupBase group = null;
			Type groupType = ProfileParser.GetProfileGroupType (HttpContext.Current, groupName);
			if (groupType != null)
				group = (ProfileGroupBase) Activator.CreateInstance (groupType);
			else
				throw new ProviderException ("Group '" + groupName + "' not found");

			group.Init (this, groupName);
			return group;
		}

		public object GetPropertyValue (string propertyName)
		{
			if (!_propertiyValuesLoaded)
				InitPropertiesValues ();

			_lastActivityDate = DateTime.UtcNow;

			return ((SettingsPropertyValue) _propertiyValues [propertyName]).PropertyValue;
		}

		public void SetPropertyValue (string propertyName, object propertyValue)
		{
			if (!_propertiyValuesLoaded)
				InitPropertiesValues ();

			if (_propertiyValues [propertyName] == null)
				throw new SettingsPropertyNotFoundException ("The settings property '" + propertyName + "' was not found.");

			((SettingsPropertyValue) _propertiyValues [propertyName]).PropertyValue = propertyValue;
			_dirty = true;
			_lastActivityDate = DateTime.UtcNow;
			_lastUpdatedDate = _lastActivityDate;
		}

		public override object this [string propertyName]
		{
			get
			{
				return GetPropertyValue (propertyName);
			}
			set
			{
				SetPropertyValue (propertyName, value);
			}
		}

		void InitPropertiesValues ()
		{
			if (!_propertiyValuesLoaded) {
				_propertiyValues = ProfileManager.Provider.GetPropertyValues (_settingsContext, Properties);
				_propertiyValuesLoaded = true;
			}
		}

		static Type GetPropertyType (ProfileGroupSettings pgs, ProfilePropertySettings pps)
		{
			Type type = Type.GetType (pps.Type);
			if (type != null)
				return type;

			Type profileType = null;
			if (pgs == null)
				profileType = ProfileParser.GetProfileCommonType (HttpContext.Current);
			else
				profileType = ProfileParser.GetProfileGroupType (HttpContext.Current, pgs.Name);

			if (profileType == null)
				return null;

			System.Reflection.PropertyInfo pi = profileType.GetProperty (pps.Name);
			if (pi != null)
				return pi.PropertyType;

			return null;
		}

		static void ValidateProperty (SettingsProperty settingsProperty, ElementInformation elementInfo)
		{
			string exceptionMessage = string.Empty;
			if (!AnonymousIdentificationModule.Enabled && 
				(bool) settingsProperty.Attributes ["AllowAnonymous"])
				exceptionMessage = "Profile property '{0}' allows anonymous users to store data. " +
					"This requires that the AnonymousIdentification feature be enabled.";

			if (settingsProperty.PropertyType == null)
				exceptionMessage = "The type specified for a profile property '{0}' could not be found.";

			if (settingsProperty.SerializeAs == SettingsSerializeAs.Binary &&
				!settingsProperty.PropertyType.IsSerializable)
				exceptionMessage = "The type for the property '{0}' cannot be serialized " +
					"using the binary serializer, since the type is not marked as serializable.";

			if (exceptionMessage.Length > 0)
				throw new ConfigurationErrorsException (string.Format (exceptionMessage, settingsProperty.Name),
					elementInfo.Source, elementInfo.LineNumber);
		}

		static SettingsProperty CreateSettingsPropery (ProfileGroupSettings pgs, ProfilePropertySettings pps)
		{
			string name = ((pgs == null) ? "" : pgs.Name + ".") + pps.Name;
			SettingsProperty sp = new SettingsProperty (name);

			sp.Attributes.Add ("AllowAnonymous", pps.AllowAnonymous);
			sp.DefaultValue = pps.DefaultValue;
			sp.IsReadOnly = pps.ReadOnly;
			sp.Provider = ProfileManager.Provider;
			sp.ThrowOnErrorDeserializing = false;
			sp.ThrowOnErrorSerializing = true;

			if (pps.Type.Length == 0 || pps.Type == "string")
				sp.PropertyType = typeof (string);
			else
				sp.PropertyType = GetPropertyType (pgs, pps);

			switch (pps.SerializeAs) {
				case SerializationMode.Binary:
					sp.SerializeAs = SettingsSerializeAs.Binary;
					break;
				case SerializationMode.ProviderSpecific:
					sp.SerializeAs = SettingsSerializeAs.ProviderSpecific;
					break;
				case SerializationMode.String:
					sp.SerializeAs = SettingsSerializeAs.String;
					break;
				case SerializationMode.Xml:
					sp.SerializeAs = SettingsSerializeAs.Xml;
					break;
			}

			return sp;
		}


		public void Initialize (string username, bool isAuthenticated)
		{
			_settingsContext = new SettingsContext ();
			_settingsContext.Add ("UserName", username);
			_settingsContext.Add ("IsAuthenticated", isAuthenticated);
		}

		public override void Save ()
		{
			if (IsDirty) {
				ProfileManager.Provider.SetPropertyValues (_settingsContext, _propertiyValues);
			}
		}

		public bool IsAnonymous {
			get {
				return !(bool) _settingsContext ["IsAuthenticated"];
			}
		}

		public bool IsDirty {
			get {
				return _dirty;
			}
		}

		public DateTime LastActivityDate {
			get {
				return _lastActivityDate;
			}
		}

		public DateTime LastUpdatedDate {
			get {
				return _lastUpdatedDate;
			}
		}

		public static SettingsPropertyCollection Properties {
			get {
				if (_properties == null)
					InitProperties ();

				return _properties;
			}
		}

		public string UserName {
			get {
				return (string) _settingsContext ["UserName"];
			}
		}
	}

}

#endif
