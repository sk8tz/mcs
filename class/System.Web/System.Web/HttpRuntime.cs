// 
// System.Web.HttpRuntime
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Gaurav Vaish (gvaish@iitk.ac.in)
//
using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.Utils;
using System.Web.Caching;

namespace System.Web {
   [MonoTODO("Make corrent right now this is a simple impl to give us a base for testing... the methods here are not complete or valid")]
   public class HttpRuntime {
      // Security permission helper objects
      private static IStackWalk appPathDiscoveryStackWalk;
      private static IStackWalk ctrlPrincipalStackWalk;
      private static IStackWalk sensitiveInfoStackWalk;
      private static IStackWalk unmgdCodeStackWalk;
      private static IStackWalk unrestrictedStackWalk;
      private static IStackWalk reflectionStackWalk;

      private static HttpRuntime _Runtime;
      private Cache _Cache;

      // TODO: Temp to test the framework..
      IHttpHandler   _Handler;

      static HttpRuntime() {
         appPathDiscoveryStackWalk = null;
         ctrlPrincipalStackWalk    = null;
         sensitiveInfoStackWalk    = null;
         unmgdCodeStackWalk        = null;
         unrestrictedStackWalk     = null;
         
         _Runtime = new HttpRuntime();
      }

      public HttpRuntime() {
         Init();
      }

      private void Init() {
         _Cache = new Cache();
      }

      public static IHttpHandler Handler {
         get {
            return _Runtime._Handler;
         }

         set {
            _Runtime._Handler = value;
         }
      }

      public static void ProcessRequest(HttpWorkerRequest Request) {
         if (null == _Runtime._Handler) {
            throw new ArgumentException("No handler");
         }

         // just a test method to test the framework

         HttpContext oContext = new HttpContext(Request);
         _Runtime._Handler.ProcessRequest(oContext);

         oContext.Response.FlushAtEndOfRequest();
         Request.EndOfRequest();
      }

      public static Cache Cache {
         get {
            return _Runtime._Cache;
         }
      }      

      [MonoTODO]
      public static string AppDomainAppId {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string AppDomainAppPath {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string AppDomainAppVirtualPath {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string AppDomainId {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string AspInstallDirectory {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string BinDirectory {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string ClrInstallDirectory {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string CodegenDir {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static bool IsOnUNCShare {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static string MachineConfigurationDirectory {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO]
      public static void Close() {
         throw new NotImplementedException();
      }

      internal static string FormatResourceString(string key) {         return GetResourceString(key);      }
      internal static string FormatResourceString(string key, string arg0) {
         string format = GetResourceString(key);
         if(format==null)
            return null;
         return String.Format(format, arg0);
      }

      [MonoTODO("FormatResourceString(string, string, string)")]
      internal static string FormatResourceString(string key, string arg0, string type) {
         // String.Format(string, object, object);
         throw new NotImplementedException();
      }

      [MonoTODO("FormatResourceString(string, string, string, string)")]
      internal static string FormatResourceString(string key, string arg0, string arg1, string arg2) {
         // String.Format(string, object, object, object);
         throw new NotImplementedException();
      }

      [MonoTODO("FormatResourceString(string, string[]")]
      internal static string FormatResourceString(string key, string[] args) {
         // String.Format(string, object[]);
         throw new NotImplementedException();
      }

      private static string GetResourceString(string key) {
         return _Runtime.GetResourceStringFromResourceManager(key);
      }

      [MonoTODO("GetResourceStringFromResourceManager(string)")]
      private string GetResourceStringFromResourceManager(string key) {
         throw new NotImplementedException();
      }

      [MonoTODO("Get Application path from the appdomain object")]
      internal static IStackWalk AppPathDiscovery {
         get {
            if(appPathDiscoveryStackWalk == null) {
               appPathDiscoveryStackWalk = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, "<apppath>");
            }
            return appPathDiscoveryStackWalk;
         }
      }

      internal static IStackWalk ControlPrincipal {
         get {
            if(ctrlPrincipalStackWalk == null) {
               ctrlPrincipalStackWalk = new SecurityPermission(SecurityPermissionFlag.ControlPrincipal);
            }
            return ctrlPrincipalStackWalk;
         }
      }

      internal static IStackWalk Reflection {
         get {
            if(reflectionStackWalk == null) {
               reflectionStackWalk = new ReflectionPermission(ReflectionPermissionFlag.TypeInformation | ReflectionPermissionFlag.MemberAccess);
            }
            return reflectionStackWalk;
         }
      }

      internal static IStackWalk SensitiveInformation {
         get {
            if(sensitiveInfoStackWalk == null) {
               sensitiveInfoStackWalk = new EnvironmentPermission(PermissionState.Unrestricted);
            }
            return sensitiveInfoStackWalk;
         }
      }

      internal static IStackWalk UnmanagedCode {
         get {
            if(unmgdCodeStackWalk == null) {
               unmgdCodeStackWalk = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
            return unmgdCodeStackWalk;
         }
      }

      internal static IStackWalk Unrestricted {
         get {
            if(unrestrictedStackWalk == null) {
               unrestrictedStackWalk = new PermissionSet(PermissionState.Unrestricted);
            }
            return unrestrictedStackWalk;
         }
      }

      internal static IStackWalk FileReadAccess(string file) {
         return new FileIOPermission(FileIOPermissionAccess.Read, file);
      }

      internal static IStackWalk PathDiscoveryAccess(string path) {
         return new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
      }
   }
}
