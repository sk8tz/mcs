// 
// System.Web.HttpValueCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;

namespace System.Web {

   [Serializable]
   public class HttpValueCollection : NameValueCollection {
      private bool	_bHeaders;

      internal HttpValueCollection() {
         _bHeaders = false;
      }

      internal HttpValueCollection(string sData) {
         FillFromQueryString(sData, Encoding.UTF8);
         IsReadOnly = true;
      }

      internal HttpValueCollection(string sData, bool ReadOnly, Encoding encoding) {
         FillFromQueryString(sData, encoding);
         IsReadOnly = ReadOnly;
      }

      protected HttpValueCollection(SerializationInfo info, StreamingContext context) : base(info, context) {
      }
      
      // string = header1: value1\r\nheader2: value2
      internal void FillFromHeaders(string sHeaders, Encoding encoding) {
         _bHeaders = true;
         char [] arrSplitValue = new char [] {':'};
         string sKey, sValue;

         sKey = "";
         sValue = "";

         string [] arrValues = sHeaders.Split(new char [] {'\r', '\n'});
         foreach (string sLine in arrValues) {
            string [] arrKeyValue = sLine.Split(arrSplitValue);
            if (arrKeyValue.Length == 1 && arrKeyValue[0].Length == 0) {
               // Empty \r or \n is ignored
               continue;
            }

            if (arrKeyValue[0] != sKey && sKey.Length > 0) {
               Add(System.Web.HttpUtility.UrlDecode(sKey, encoding), System.Web.HttpUtility.UrlDecode(sValue, encoding));
            }

            if (arrKeyValue.Length == 1) {
               sValue += "\r\n" + arrKeyValue[0].Trim();
               continue;
            } 
            else if (arrKeyValue.Length == 2) {
               if (arrKeyValue[0].Length == 0) {
                  sValue += arrKeyValue[1].Trim();
                  continue;
               }

               sKey = arrKeyValue[0].Trim();
               sValue = arrKeyValue[1].Trim();
            } 
         }
			
         if (sKey.Length > 0) {
            Add(System.Web.HttpUtility.UrlDecode(sKey, encoding), System.Web.HttpUtility.UrlDecode(sValue, encoding));
         }
      }

      internal void FillFromHeaders(string sData) {
         FillFromHeaders(sData, Encoding.UTF8);
      }

      // String = test=aaa&kalle=nisse
      internal void FillFromQueryString(string sData, Encoding encoding) {
         _bHeaders = false;

         char [] arrSplitValue = new char [] {'='};

         string [] arrValues = sData.Split(new char [] {'&'});
         foreach (string sValue in arrValues) {
            string [] arrKeyValue = sValue.Split(arrSplitValue);
            if (arrKeyValue.Length == 1) {
               // Add key only
               Add(System.Web.HttpUtility.UrlDecode(arrKeyValue[0].Trim(), encoding), string.Empty);
            } 
            else if (arrKeyValue.Length == 2) {
               Add(System.Web.HttpUtility.UrlDecode(arrKeyValue[0].Trim(), encoding),System.Web.HttpUtility.UrlDecode(arrKeyValue[1].Trim(), encoding));
            } 
            else {
               throw new InvalidOperationException("Data is malformed");
            }
         }		
      }

      internal void FillFromQueryString(string sData) {
         FillFromQueryString(sData, Encoding.UTF8);
      }

      internal void FillFromCookieString(string sData) {
         FillFromQueryString(sData, Encoding.UTF8);
      }

      internal void MakeReadOnly() {
         IsReadOnly = true;
      }
		
      internal void MakeReadWrite() {
         IsReadOnly = false;
      }

      internal void Merge(NameValueCollection oData) {
         foreach (string sKey in oData) {
            Add(sKey, oData[sKey]);
         }
      }

      internal void Reset() {
         Clear();
      }

      [MonoTODO("string ToString(bool UrlEncode)")]
      internal string ToString(bool UrlEncode) {
         if (_bHeaders) {
         }

         // TODO: Should return a correctly formated string (different depending on header flag)
         throw new NotImplementedException();
      }

      new public string ToString() {
         return ToString(false);
      }
   }

}
