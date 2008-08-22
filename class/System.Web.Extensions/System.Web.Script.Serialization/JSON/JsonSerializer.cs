#region License
// Copyright (c) 2007 James Newton-King
// Copyright 2007 Konstantin Triger <kostat@mainsoft.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json.Utilities;
using System.Web.Script.Serialization;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies reference loop handling options for the <see cref="JsonWriter"/>.
	/// </summary>
	enum ReferenceLoopHandling
	{
		/// <summary>
		/// Throw a <see cref="JsonSerializationException"/> when a loop is encountered.
		/// </summary>
		Error = 0,
		/// <summary>
		/// Ignore loop references and do not serialize.
		/// </summary>
		Ignore = 1,
		/// <summary>
		/// Serialize loop references.
		/// </summary>
		Serialize = 2
	}

	/// <summary>
	/// Serializes and deserializes objects into and from the Json format.
	/// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into Json.
	/// </summary>
	sealed class JsonSerializer
	{
		sealed internal class DeserializerLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly JsonReader _reader;
			readonly JsonSerializer _serializer;
			IEnumerator<KeyValuePair<string, object>> _innerEnum;
			object _firstValue;
			public DeserializerLazyDictionary (JsonReader reader, JsonSerializer serializer) {
				_reader = reader;
				_serializer = serializer;
			}

			public object PeekFirst () {
				if (_innerEnum != null)
					throw new InvalidOperationException ("first already taken");

				_innerEnum = _serializer.PopulateObject (_reader);

				if (_innerEnum.MoveNext ())
					_firstValue = _innerEnum.Current;

				return _firstValue;
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				if (_innerEnum == null)
					_innerEnum = _serializer.PopulateObject (_reader);

				if (_firstValue != null)
					yield return (KeyValuePair<string, object>) _firstValue;

				while (_innerEnum.MoveNext ())
					yield return _innerEnum.Current;
			}
		}

		sealed class SerializerLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly object _source;

			public SerializerLazyDictionary (object source) {
				_source = source;
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				KeyValuePair <string, object> kvpret;
				
				foreach (MemberInfo member in ReflectionUtils.GetFieldsAndProperties (_source.GetType (), BindingFlags.Public | BindingFlags.Instance)) {
					if (ReflectionUtils.CanReadMemberValue (member) && !member.IsDefined (typeof (ScriptIgnoreAttribute), true))
						if (!ReflectionUtils.IsIndexedProperty (member)) {
							// A temporary hack to prevent situations
							// when a type member cannot be serialized
							// for some reason (e.g. when serializing a
							// CultureInfo for 'en-US', processing its
							// Parent property which returns CultureInfo
							// for 'en' - asking for the Calendar
							// property value for that one will throw an
							// exception). Until a better solution is
							// devised, this has to stay in.
							try {
								kvpret = new KeyValuePair<string, object> (member.Name, ReflectionUtils.GetMemberValue (member, _source));
							} catch (Exception ex) {
								Console.Error.WriteLine ("HACK WARNING! NOT YIELDING THE VALUE!  Serializing {0}.{1} threw an exception:", _source.GetType (), member.Name);
								Console.Error.WriteLine (ex);
								continue;
							}
							yield return kvpret;
						}
				}
			}
		}

		sealed class GenericDictionaryLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly object _source;
			readonly PropertyInfo _piKeys;
			readonly PropertyInfo _piValues;


			public GenericDictionaryLazyDictionary (object source, Type dictType) {
				_source = source;
				_piKeys = dictType.GetProperty ("Keys");
				_piValues = dictType.GetProperty ("Values");
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				
				IEnumerable eKeys = (IEnumerable) _piKeys.GetValue (_source, null);
				IEnumerator eValues = ((IEnumerable) _piValues.GetValue (_source, null)).GetEnumerator();
				foreach (object key in eKeys) {
					string keyString = key == null ? null : key.ToString ();
					if (!eValues.MoveNext ())
						throw new IndexOutOfRangeException (keyString);


					yield return new KeyValuePair<string, object> (keyString, eValues.Current);
				}

				if (eValues.MoveNext ())
					throw new IndexOutOfRangeException (eValues.Current != null ? eValues.Current.ToString () : String.Empty);
			}
		}

		private int _maxJsonLength;
		private int _recursionLimit;
		private int _currentRecursionCounter;
		private ReferenceLoopHandling _referenceLoopHandling;
		readonly JavaScriptSerializer _context;
		readonly JavaScriptTypeResolver _typeResolver;

		public int MaxJsonLength 
		{
			get { return _maxJsonLength; }
			set { _maxJsonLength = value; }
		}

		public int RecursionLimit {
			get { return _recursionLimit; }
			set { _recursionLimit = value; }
		}

		/// <summary>
		/// Get or set how reference loops (e.g. a class referencing itself) is handled.
		/// </summary>
		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get { return _referenceLoopHandling; }
			set
			{
				if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_referenceLoopHandling = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonSerializer"/> class.
		/// </summary>
		public JsonSerializer(JavaScriptSerializer context, JavaScriptTypeResolver resolver)
		{
			_context = context;
			_typeResolver = resolver;
			_referenceLoopHandling = ReferenceLoopHandling.Error;
		}

		#region Deserialize
		public object Deserialize (TextReader reader) {
			return Deserialize (new JsonReader (reader, MaxJsonLength, RecursionLimit));
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>
		/// into an instance of the specified type.
		/// </summary>
		/// <param name="reader">The type of object to create.</param>
		/// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
		/// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
		object Deserialize (JsonReader reader)
		{
			if (!reader.Read())
				return null;

			return GetObject(reader);
		}

		private object GetObject (JsonReader reader/*, Type objectType*/) {
			if (RecursionLimit > 0 && reader.CurrentRecursionLevel >= RecursionLimit) {
				throw new ArgumentException ("RecursionLimit exceeded.");
			}

			object value;

			switch (reader.TokenType) {
			// populate a typed object or generic dictionary/array
			// depending upon whether an objectType was supplied
			case JsonToken.StartObject:
				//value = PopulateObject(reader/*, objectType*/);
				value = new DeserializerLazyDictionary (reader, this);
				break;
			case JsonToken.StartArray:
				value = PopulateList (reader/*, objectType*/);
				break;
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Date:
				//value = EnsureType(reader.Value, objectType);
				value = reader.Value;
				break;
			case JsonToken.Constructor:
				value = reader.Value.ToString ();
				break;
			case JsonToken.Null:
			case JsonToken.Undefined:
				value = null;
				break;
			default:
				throw new JsonSerializationException ("Unexpected token whil deserializing object: " + reader.TokenType);
			}

			return value;
		}

		private IEnumerable<object> PopulateList(JsonReader reader/*, Type objectType*/)
		{

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.EndArray:
						yield break;
					case JsonToken.Comment:
						break;
					default:
						yield return GetObject(reader/*, elementType*/);

						break;
				}
			}

			throw new JsonSerializationException("Unexpected end when deserializing array.");
		}

		private IEnumerator<KeyValuePair<string, object>> PopulateObject (JsonReader reader/*, Type objectType*/)
		{
			reader.IncrementRecursionLevel ();
			while (reader.Read ())
			{
				switch (reader.TokenType)
				{
					case JsonToken.PropertyName:
						string memberName = reader.Value.ToString();

						if (!reader.Read ())
							throw new JsonSerializationException (string.Format ("Unexpected end when setting {0}'s value.", memberName));
						yield return new KeyValuePair<string, object> (memberName, GetObject (reader));
						break;
					case JsonToken.EndObject:
						reader.DecrementRecursionLevel ();
						yield break;
					default:
						throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
				}
			}

			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}
		#endregion

		#region Serialize
		/// <summary>
		/// Serializes the specified <see cref="Object"/> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="TextWriter"/>. 
		/// </summary>
		/// <param name="textWriter">The <see cref="TextWriter"/> used to write the Json structure.</param>
		/// <param name="value">The <see cref="Object"/> to serialize.</param>
		public void Serialize(TextWriter textWriter, object value)
		{
			Serialize(new JsonWriter(textWriter, MaxJsonLength), value);
		}

		/// <summary>
		/// Serializes the specified <see cref="Object"/> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="JsonWriter"/>. 
		/// </summary>
		/// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the Json structure.</param>
		/// <param name="value">The <see cref="Object"/> to serialize.</param>
		void Serialize(JsonWriter jsonWriter, object value)
		{
			SerializeValue (jsonWriter, value, true, null);
		}


		private void SerializeValue(JsonWriter writer, object value)
		{
			SerializeValue (writer, value, false, null);
		}

		private void WritePropertyName (JsonWriter writer, string name)
		{
			if (String.IsNullOrEmpty (name))
				return;

			writer.WritePropertyName (name);
		}
		
		private void SerializeValue (JsonWriter writer, object value, bool topLevelObject, string propertyName)
		{
			//JsonConverter converter;
			_currentRecursionCounter++;
			if (RecursionLimit > 0 && _currentRecursionCounter > RecursionLimit) {
				throw new ArgumentException ("RecursionLimit exceeded.");
			}

			if (value == null) {
				WritePropertyName (writer, propertyName);
				writer.WriteNull ();
			}
			else {
				Type valueType = value.GetType ();
				JavaScriptConverter jsconverter = _context.GetConverter (valueType);
				if (jsconverter != null) {
					value = jsconverter.Serialize (value, _context);
					if (value == null) {
						WritePropertyName (writer, propertyName);
						writer.WriteNull ();
						_currentRecursionCounter--;
						return;
					}
				}

				switch (Type.GetTypeCode (valueType)) {
				case TypeCode.String:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((string) value);
					break;
				case TypeCode.Char:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((char) value);
					break;
				case TypeCode.Boolean:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((bool) value);
					break;
				case TypeCode.SByte:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((sbyte) value);
					break;
				case TypeCode.Int16:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((short) value);
					break;
				case TypeCode.UInt16:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((ushort) value);
					break;
				case TypeCode.Int32:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((int) value);
					break;
				case TypeCode.Byte:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((byte) value);
					break;
				case TypeCode.UInt32:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((uint) value);
					break;
				case TypeCode.Int64:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((long) value);
					break;
				case TypeCode.UInt64:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((ulong) value);
					break;
				case TypeCode.Single:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((float) value);
					break;
				case TypeCode.Double:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((double) value);
					break;
				case TypeCode.DateTime:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((DateTime) value);
					break;
				case TypeCode.Decimal:
					WritePropertyName (writer, propertyName);
					writer.WriteValue ((decimal) value);
					break;
				default:

					ThrowOnReferenceLoop (writer, value);
					writer.SerializeStack.Push (value);
					try {
						Type genDictType;
						if (value is IDictionary) {
							WritePropertyName (writer, propertyName);
							SerializeDictionary (writer, (IDictionary) value);
						} else if (value is IDictionary<string, object>) {
							WritePropertyName (writer, propertyName);
							SerializeDictionary (writer, (IDictionary<string, object>) value, null);
						} else if ((genDictType = ReflectionUtils.GetGenericDictionary (valueType)) != null) {
							WritePropertyName (writer, propertyName);
							SerializeDictionary (writer, new GenericDictionaryLazyDictionary (value, genDictType), null);
						} else if (value is IEnumerable) {
							WritePropertyName (writer, propertyName);
							SerializeEnumerable (writer, (IEnumerable) value);
						} else if (topLevelObject) {
							SerializeCustomObject (writer, value, valueType);
						}
					}
					finally {

						object x = writer.SerializeStack.Pop ();
						if (x != value)
							throw new InvalidOperationException ("Serialization stack is corrupted");
					}

					break;
				}
			}

			_currentRecursionCounter--;
		}

		private void ThrowOnReferenceLoop (JsonWriter writer, object value)
		{
			switch (_referenceLoopHandling) {
			case ReferenceLoopHandling.Error:
				if (writer.SerializeStack.Contains (value))
					throw new JsonSerializationException ("Self referencing loop");
				break;
			case ReferenceLoopHandling.Ignore:
				// return from method
				return;
			case ReferenceLoopHandling.Serialize:
				// continue
				break;
			default:
				throw new InvalidOperationException (string.Format ("Unexpected ReferenceLoopHandling value: '{0}'", _referenceLoopHandling));
			}
		}

		private void SerializeEnumerable (JsonWriter writer, IEnumerable values) {
			writer.WriteStartArray ();

			foreach (object value in values)
				SerializeValue (writer, value);

			writer.WriteEndArray ();
		}

		private void SerializeDictionary(JsonWriter writer, IDictionary values)
		{
			writer.WriteStartObject();
			
			foreach (DictionaryEntry entry in values)
				SerializePair (writer, entry.Key.ToString (), entry.Value);
			
			writer.WriteEndObject();
		}

		private void SerializeDictionary (JsonWriter writer, IDictionary<string, object> values, string typeID) {

			writer.WriteStartObject ();
			
			if (typeID != null) {
				SerializePair (writer, JavaScriptSerializer.SerializedTypeNameKey, typeID);
					}

			foreach (KeyValuePair<string, object> entry in values)
				SerializePair (writer, entry.Key, entry.Value);

			writer.WriteEndObject ();
		}

		private void SerializeCustomObject (JsonWriter writer, object value, Type valueType) 
		{
			if (value is Uri) {
				Uri uri = value as Uri;
				writer.WriteValue (uri.GetComponents (UriComponents.AbsoluteUri, UriFormat.UriEscaped));
				return;
			}
			if (valueType == typeof (Guid)) {
				writer.WriteValue (((Guid) value).ToString ());
				return;
			}

			string typeID = null;
			if (_typeResolver != null) {
				typeID = _typeResolver.ResolveTypeId (valueType);
			}

			SerializeDictionary (writer, new SerializerLazyDictionary (value), typeID);
		}

		private void SerializePair (JsonWriter writer, string key, object value) {
			SerializeValue (writer, value, false, key);
		}

		#endregion
	}
}
